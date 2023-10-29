using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.PlayerLoop;

public class KeyManager : MonoBehaviour
{
    public Transform keyContainer;
    public Transform usedKeyContainer;
    public Transform nextKeyToUseImage;

    private Transform lastKey;
    private Vector3 nextKeyPos;
    private Vector3 inKeyContainerPos;
    private Tween highlightTween;
    private Tween moveToContainerTween;
    private Sequence moveToContainerSeq;
    private Sequence unlockNodeSeq;

    public static int keyCount = 0;
    
    private float gap = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        keyCount = 0;
        nextKeyPos = Vector3.zero;
        inKeyContainerPos = keyContainer.localPosition;
    }

    void OnEnable()
    {
        LockController.OnUnlock += UnlockNode;
        LockController.OnLock += UndoUnlock;
        LockController.OnRemoveKey += MoveToContainer;
        LockController.OnAddKey += MoveToNode;
        LockController.OnNodeWithPadlockHighlihtChanged += ChangeKeyHighlightStatus;
        LevelManager.OnLevelLoad += ResetContainers;
        LevelEditor.OnEnter += ResetContainers;
    }
    void OnDisable()
    {
        LockController.OnUnlock -= UnlockNode;
        LockController.OnLock -= UndoUnlock;
        LockController.OnRemoveKey -= MoveToContainer;
        LockController.OnAddKey -= MoveToNode;
        LockController.OnNodeWithPadlockHighlihtChanged -= ChangeKeyHighlightStatus;
        LevelManager.OnLevelLoad -= ResetContainers;
        LevelEditor.OnEnter -= ResetContainers;
    }

    public void MoveToNode(LockController node, Transform key)
    {
        if(keyContainer.childCount == 0) return;
        
        lastKey = key;
        //if (lastKey.CompareTag("PermanentKey")) 
            //return;
        
        keyCount--;
        UpdateNextKeyPos();
        //moveToContainerTween.Kill();
        moveToContainerSeq.Kill();
        //Vector3 pos = node.transform.TransformPoint(node.keyPos);
        //key.position =  pos ; //node.transform.InverseTransformPoint( pos );
        //Debug.Log("key pos : " + node.keyPos);
        key.SetParent(LevelManager.curLevel.transform, true);
        //pos = keyContainer.InverseTransformPoint(pos);
        lastKey.DOMove(node.transform.localPosition + node.keyPos, 0.3f);
        lastKey.DOScale(1f, 0.3f);
        //nextKeyPos -= Vector3.right * gap;
        //keyCount--;
        FixContainerPos();
    }

    public  void MoveToContainer(Transform key)
    {
        keyCount++;
        UpdateNextKeyPos();
        
        key.SetParent(keyContainer);
        //moveToContainerTween = key.DOLocalMove(nextKeyPos, 0.5f);
        moveToContainerSeq = DOTween.Sequence();
        moveToContainerSeq.Append(key.DOLocalMove(nextKeyPos, 0.5f));
        moveToContainerSeq.Append(key.DOScale(2f, 0.5f).SetDelay(-0.5f));
        //key.DOScale(2f, 0.5f);
        //nextKeyPos += Vector3.right * gap;
        //keyCount++;
        FixContainerPos();
        Debug.Log("should move to key container, key tag:" + key.tag);
    }

    public  void UnlockNode(LockController node)
    {
        
        if(keyContainer.childCount <= 0)
        {
            Debug.Log("no keys in container! container: " + keyContainer.childCount);
            return; 
        };
        lastKey = keyContainer.GetChild(keyContainer.childCount - 1);
        
        //lastKey.SetParent(node.transform);
        highlightTween.Kill();
        lastKey.SetParent(LevelManager.curLevel.transform);
        //pos = keyContainer.InverseTransformPoint(pos);
        //lastKey.DOMove(node.transform.localPosition + node.padLockPos, 0.3f);
        
        //nextKeyPos -= Vector3.right * gap;
        keyCount--;
        UpdateNextKeyPos();

        unlockNodeSeq = DOTween.Sequence();
        unlockNodeSeq.Append(lastKey.DOLocalMove(node.transform.position + node.padLockPos, 0.5f));
        unlockNodeSeq.Append(lastKey.DOScale(1f, 0.5f)
            .SetDelay(-0.5f)
            .OnComplete(() => { 
                lastKey.DOScale(0f, 0.2f);
                lastKey.SetParent(usedKeyContainer);
            }));
        unlockNodeSeq.OnKill( () =>
        {
            
            lastKey.SetParent(usedKeyContainer);
        });
        
        /*lastKey.DOLocalMove( node.transform.position + node.padLockPos, 0.5f);
        lastKey.DOScale(1f, 0.5f).OnComplete(() =>
        {
            lastKey.DOScale(0f, 0.2f);
            lastKey.SetParent(usedKeyContainer);
        });*/

        FixContainerPos();
    }

    public void UndoUnlock()
    {
        if(usedKeyContainer.childCount <= 0)
        {
            Debug.Log("no keys in used key container! container: " + usedKeyContainer.childCount);
            return; 
        };
        
        if(unlockNodeSeq.IsPlaying())
            unlockNodeSeq.Kill();
        
        Transform lastUsedKey = usedKeyContainer.GetChild(usedKeyContainer.childCount - 1);
        //if (lastUsedKey.CompareTag("PermanentKey")) return;
        
        keyCount++;
        UpdateNextKeyPos();
        
        lastUsedKey.SetParent(keyContainer);
        lastUsedKey.DOLocalMove(nextKeyPos, 0.3f);
        lastUsedKey.DOScale(2f, 0.3f);
        
        FixContainerPos();
    }

    private void FixContainerPos()
    {
        nextKeyPos = new Vector3(0f, -keyContainer.localPosition.y + 0.5f, 0f);
        keyContainer.DOLocalMoveX( - (keyCount - 1)*gap, 0.5f).SetDelay(0.3f);

        for (int i = 0; i < keyContainer.childCount - 1; i++)
        {
            Transform key = keyContainer.GetChild(i);
            key.DOLocalMove(nextKeyPos, 0.5f);
            nextKeyPos += Vector3.right * gap;
        }
        UpdateNextKeyPos();

        //if (keyContainer.childCount == 0) return;
        //nextKeyToUseImage.DOMove(keyContainer.GetChild(keyContainer.childCount - 1).position + Vector3.down * 0.5f, 0.5f).SetDelay(0.6f);
    }
    
    private void ChangeKeyHighlightStatus(bool isHighlighted)
    {
        if (keyContainer.childCount < 1) return;
        
        lastKey = keyContainer.GetChild(keyContainer.childCount - 1);
        if (isHighlighted)
        {
             highlightTween = lastKey.DOLocalMoveY(nextKeyPos.y + 0.3f, 0.3f);
        }
        else
        {
            highlightTween = lastKey.DOLocalMoveY(nextKeyPos.y , 0.3f);
        }
    }

    private void UpdateNextKeyPos()
    {
        nextKeyPos = new Vector3(0f, -keyContainer.localPosition.y + 0.5f, 0f) + Vector3.right * gap * (keyCount - 1);
    }

    private void ResetContainers()
    {
        keyCount = 0;
        int childCount = keyContainer.childCount;
        keyContainer.localPosition = inKeyContainerPos;
        nextKeyPos = new Vector3(0f, -keyContainer.localPosition.y + 0.5f, 0f);
        List<GameObject> keysToDestroy = new List<GameObject>();
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++){
                GameObject obj = keyContainer.GetChild(i).gameObject;
            
                keysToDestroy.Add(obj);
            }
        }

        childCount = usedKeyContainer.childCount;
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++){
                GameObject obj = usedKeyContainer.GetChild(i).gameObject;
                keysToDestroy.Add(obj);
            }
        }

        foreach (var obj in keysToDestroy){
            DestroyImmediate(obj);
        }
    }

    public Transform GetLastKeyVar()
    {
        return lastKey;
    }
    
}
