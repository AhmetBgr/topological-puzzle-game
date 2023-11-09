using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemContainer : MonoBehaviour
{
    public Pivot pivot;
    public Style style;
    public List<Item> items = new List<Item>();

    private Vector3 containerPos;

    Sequence sequence;

    public float gap = 0.3f;

    public delegate void OnContainerChangedDelegate(List<Item> items);
    public event OnContainerChangedDelegate OnContainerChanged;

    void Start()
    {
        FindContainerPos();
    }

    public void FindContainerPos()
    {
        if (transform.parent == null)
        {
            containerPos = transform.localPosition;
            return;
        }

        containerPos = transform.parent.localPosition + transform.localPosition;
    }

    public void AddItem(Item addedItem, int index, bool skipFix = false, bool setInstantAnim = false)
    {
        if (items.Contains(addedItem)) return;

        if(index < 0 | index >= items.Count)
        {
            items.Add(addedItem);
        }
        else
        {
            items.Insert(index, addedItem);
        }

        if (OnContainerChanged != null)
        {
            OnContainerChanged(items);
        }

        if (skipFix) return;

        FixItemPositions(setInstantAnim: setInstantAnim);
    }

    /*public void AddItems(List<Item> addedItems)
    {
        items.AddRange(addedItems);

        foreach(var item in addedItems)
        {
            item.transform.SetParent(transform);
        }
        FixItemPositions();
    }

    public void RemoveItems(List<Item> removedItems)
    {
        items.RemoveRange(removedItems);

        foreach (var item in addedItems)
        {
            item.transform.SetParent(transform);
        
        FixItemPositions();
    }*/

    public void RemoveItem(Item item, bool setInactive = false, bool skipFix = false)
    {
        items.Remove(item);
        item.transform.SetParent(LevelManager.curLevel.transform);
        if(setInactive)
            item.gameObject.SetActive(false);

        if (OnContainerChanged != null)
        {
            OnContainerChanged(items);
        }

        if (skipFix) return;

        FixItemPositions();
    }

    public void ClearAll()
    {
        items.Clear();

        if(OnContainerChanged != null)
        {
            OnContainerChanged(items);
        }
    }

    public int GetItemIndex(Item item)
    {
        return items.IndexOf(item);
    }
    public Item GetLastItem()
    {
        if (items.Count == 0) return null;

        return items[items.Count - 1];
    }

    public void FixItemPositions(bool setDelayBetweenFixes = false, bool setInstantAnim = false)
    {
        Vector3 pivot3 = Vector3.right * ((int)pivot);
        Vector3 nextItemPos = (-(items.Count - 1) * gap) * ( (pivot3 + 1*Vector3.right) / 2);
        float delay = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];
            Vector3 pos;
            /*if(sequence != null)
            {
                //sequence.Complete();
                sequence.Kill();

            }*/

            if (setInstantAnim)
            {
                pos = containerPos + nextItemPos;
                item.transform.position = pos;
                item.transform.localScale = style == Style.Main ? Vector3.one * 2 : Vector3.one;
                item.transform.SetParent(style == Style.Main ? LevelManager.curLevel.transform : transform);
                nextItemPos += Vector3.right * gap;
                continue;
            }

            float dur = setInstantAnim ? 0f : 0.5f;
            setDelayBetweenFixes = setInstantAnim ? false : setDelayBetweenFixes;
            sequence = DOTween.Sequence();
            if (setDelayBetweenFixes)
            {
                sequence.SetDelay(delay);
                delay += 0.15f;
            }



            if(style == Style.Main)
            {
                pos = containerPos + nextItemPos;
                sequence.Append(item.transform.DOMove(pos, dur));
                sequence.Append(item.transform.DOScale(2f, dur)
                    .OnComplete(() => item.transform.SetParent(LevelManager.curLevel.transform))
                    .SetDelay(-dur));
            }
            else if (style == Style.Node)
            {
                pos = containerPos + nextItemPos;
                sequence.Append(item.transform.DOMove(pos, dur));
                sequence.Append(item.transform.DOScale(1f, dur)
                    .OnComplete(() => item.transform.SetParent(transform))
                    .SetDelay(-dur));
            }
            sequence.OnComplete(() => sequence.Kill());
            item.PlayAnimSequence(sequence);
            nextItemPos += Vector3.right * gap;
        }
    }
    public enum Pivot
    {
        Center = 0,
        Right = 1,
        Left = -1
    }
    public enum Style
    {
        Node, Main
    }
}
