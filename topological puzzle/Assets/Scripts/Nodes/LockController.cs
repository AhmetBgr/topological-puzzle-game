using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LockController : MonoBehaviour
{
    [HideInInspector] public Transform key = null;
    [HideInInspector] public Transform padLock = null;
    
    public bool hasPadLock = false;
    public bool hasKey = false;

    private SpriteRenderer padLockImage;
    private SpriteRenderer keyImage;

    //private Node node;
    [HideInInspector] public Vector3 padLockPos;
    [HideInInspector] public Vector3 keyPos;

    public delegate void OnUnlockDelegate(LockController node);
    public static event OnUnlockDelegate OnUnlock;
    
    public delegate void OnLockDelegate();
    public static event OnLockDelegate OnLock;
    
    public delegate void OnRemoveKeyDelegate(Transform key);
    public static event OnRemoveKeyDelegate OnRemoveKey;
    
    public delegate void OnAddKeyDelegate(LockController node, Transform key);
    public static event OnAddKeyDelegate OnAddKey;
    
    public delegate void OnNodeWithPadlockHighlihtChangedDelegate(bool isHighlighted);
    public static event OnNodeWithPadlockHighlihtChangedDelegate OnNodeWithPadlockHighlihtChanged;
    
    void Start()
    {
        padLockPos = new Vector3(0.24f, -0.20f, 0);
        keyPos = new Vector3(-0.24f, -0.20f, 0);
        /*if (hasPadLock && !padLock)
        {
            GeneratePadLock();
        }

        if (hasKey && !key)
        {
            GenerateKey();
        }*/

        //node = GetComponent<Node>();
    }

    private void OnMouseEnter()
    {
        if (hasPadLock && OnNodeWithPadlockHighlihtChanged != null)
        {
            OnNodeWithPadlockHighlihtChanged(true);
        }
    }

    private void OnMouseExit()
    {
        if (hasPadLock && OnNodeWithPadlockHighlihtChanged != null)
        {
            OnNodeWithPadlockHighlihtChanged(false);
        }
    }

    public void Unlock()
    {
        Transform padLockImageObj = padLock.Find("Image");
        if (padLockImageObj)
        {
            padLockImage = padLockImageObj.GetComponent<SpriteRenderer>();
        }
        padLockImage.DOFade(0f, 0.1f).SetDelay(0.1f).OnComplete(() =>
        {
            padLock.gameObject.SetActive(false);
        });
        
        /*if (OnUnlock != null)
        {
            OnUnlock(this);
        }*/
        hasPadLock = false;
    }

    public void Lock()
    {
        /*if (OnLock != null)
        {
            OnLock();
        }*/
        
        if (padLock) // && !padLock.CompareTag("PermanentPadLock")
        {
            Debug.Log("no permanent lock");
            padLock.gameObject.SetActive(true);
            padLockImage.DOFade(1f, 0.2f);
            hasPadLock = true;
        }
        else if (padLock)
        {
            padLock.gameObject.SetActive(false);
            Debug.Log("permanent lock");
        }
    }
    
    
    public void RemoveKey()
    {
        if (OnRemoveKey != null)
        {
            OnRemoveKey(key);
        }
        hasKey = false;
    }

    public void AddKey()
    {
        if (OnAddKey != null)
        {
            OnAddKey(this, key);
        }
        if(key) // && !key.CompareTag("PermanentKey")
            hasKey = true;
    }
    
    public void GeneratePadLock(GameObject prefab)
    {
        padLock = Instantiate(prefab, padLockPos, Quaternion.identity).transform;
        padLock.SetParent(this.transform);
        padLock.localPosition = padLockPos;
        Transform padLockImageObj = padLock.Find("Image");
        if (padLockImageObj)
        {
            padLockImage = padLockImageObj.GetComponent<SpriteRenderer>();
        }
        hasPadLock = true;
    }

    public void DestroPadLock()
    {
        Destroy(padLock.gameObject);
        padLock = null;
        hasPadLock = false;
    }

    public void GenerateKey(GameObject prefab)
    {
        key = Instantiate(prefab, keyPos, Quaternion.identity).transform;
        key.SetParent(this.transform);
        key.localPosition = keyPos;
        Transform keyImageObj = key.Find("Image");
        if (keyImageObj)
        {
            padLockImage = keyImageObj.GetComponent<SpriteRenderer>();
        }
        hasKey = true;
    }

    public void DestroyKey()
    {
        Destroy(key.gameObject);
        key = null;
        hasKey = false;
    }
}
