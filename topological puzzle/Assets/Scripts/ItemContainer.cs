using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemContainer : MonoBehaviour{
    public Pivot pivot;
    public Style style;
    public List<Item> items = new List<Item>();

    private Sequence sequence;
    public Vector3 containerPos;

    public float gap = 0.3f;

    public delegate bool OnContainerChangedDelegate(List<Item> items);
    public event OnContainerChangedDelegate OnContainerChanged;

    void Start(){
        UpdateContainerPos();
    }
    public void UpdateContainerPos(){
        if (transform.parent == null){
            containerPos = transform.localPosition;
            return;
        }

        containerPos = transform.parent.localPosition + transform.localPosition;
    }

    public void AddItem(Item addedItem, int index, float dur, 
        List<Vector3> itemFixPath = null, bool skipFix = false, 
        bool setInstantAnim = false, float startingDelay = 0f) {

        if (items.Contains(addedItem)) return;

        UpdateContainerPos();

        if(index < 0 | index >= items.Count)
            items.Add(addedItem);
        else
            items.Insert(index, addedItem);

        InvokeContainerChangedEvent();

        if (skipFix) return;

        FixItemPositions(dur, itemFixPath, 
            setInstantAnim: setInstantAnim, startingDelay: startingDelay);
    }

    public void RemoveItem(Item item, float dur, 
        bool setInactive = false, bool skipFix = false){

        UpdateContainerPos();

        items.Remove(item);
        item.transform.SetParent(LevelManager.curLevel.transform);
        if(setInactive)
            item.gameObject.SetActive(false);

        InvokeContainerChangedEvent();

        if (skipFix) return;

        FixItemPositions(dur);
    }

    public void SwapItems(int index1, int index2,  float itemFixDur = 0.1f) {
        Utility.Swap(items, index1, index2);

        FixItemPositions(itemFixDur);
    }

    public void ClearAll() {
        items.Clear();

        InvokeContainerChangedEvent();
    }

    public int GetItemIndex(Item item){
        return items.IndexOf(item);
    }

    public Item GetLastItem(){
        if (items.Count == 0) return null;

        return items[items.Count - 1];
    }

    public void FixItemPositions(float dur, List<Vector3> itemFixPath = null,
        List<Item> itemsWithFixPath = null,
        bool setDelayBetweenFixes = false, 
        bool setInstantAnim = false, float startingDelay= 0f){

        //gap = style == Style.Main ? 0.15f : 0.3f;

        Vector3 pivot3 = Vector3.right * ((int)pivot);
        Vector3 nextItemPos = (-(items.Count - 1) * gap) * 
            ( (pivot3 + 1*Vector3.right) / 2);

        float delay = startingDelay;

        // Fixes every item pos in item container
        for (int i = 0; i < items.Count; i++){
            Item item = items[i];
            Vector3 pos;

            if (setInstantAnim){
                pos = containerPos + nextItemPos;
                item.transform.position = pos;
                item.transform.localScale = style == Style.Main ? 
                    Vector3.one * 2 : Vector3.one;
                item.transform.SetParent(
                    style == Style.Main ? LevelManager.curLevel.transform : 
                    transform
                );
                nextItemPos += Vector3.right * gap;
                continue;
            }

            dur = setInstantAnim ? 0f : dur;
            setDelayBetweenFixes = setInstantAnim ? false : 
                setDelayBetweenFixes;
            sequence = DOTween.Sequence();
            if (setDelayBetweenFixes){
                sequence.SetDelay(delay);
                delay += dur/5;
            }

            if(style == Style.Main){
                pos = containerPos + nextItemPos;
                float scale = i == items.Count - 1 ? 2f : 1.5f;
                sequence.Append(item.transform.DOMove(pos, dur));
                sequence.Append(item.transform.DOScale(scale, dur)
                    .OnComplete(() => 
                        item.transform.
                            SetParent(LevelManager.curLevel.transform))
                    .SetDelay(-dur));
            }
            else if (style == Style.Node){
                pos = containerPos + nextItemPos;

                Tween moveTween;

                if(itemFixPath != null && itemsWithFixPath != null && itemsWithFixPath.Contains(item)) {
                    itemFixPath.Add(pos);
                    moveTween = item.transform.DOPath(itemFixPath.ToArray(), dur)
                        .SetEase(Ease.InCubic);
                }
                else {
                    moveTween = item.transform.DOMove(pos, dur);
                }

                sequence.Append(moveTween);
                sequence.Append(item.transform.DOScale(1f, dur)
                    .OnComplete(() => item.transform.SetParent(transform))
                    .SetDelay(-dur));
            }
            item.PlayAnimSequence(sequence);
            nextItemPos += Vector3.right * gap;
        }
    }

    private void InvokeContainerChangedEvent(){
        if (OnContainerChanged != null)
            OnContainerChanged(items);
    }
    public enum Pivot{
        Center = 0,
        Right = 1,
        Left = -1
    }
    public enum Style{
        Node, Main
    }
}
