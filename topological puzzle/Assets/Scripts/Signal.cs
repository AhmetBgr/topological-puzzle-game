using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Signal : MonoBehaviour
{
    public GameObject waveEffect;
    [HideInInspector] public Transform owner;

    private Arrow arrow;
    private Vector3 initOwnerScale;
    private Vector3 initSignalScale;

    void Start(){
        owner?.TryGetComponent(out arrow);

        initOwnerScale = owner.localScale;
        initSignalScale = transform.localScale;
    }

    private void OnEnable() {
        GameManager.OnRewind += EnableSignal;
        GameManager.PostRewind += DisableSignal;

    }

    private void OnDisable() {
        GameManager.OnRewind -= EnableSignal;
        GameManager.PostRewind -= DisableSignal;
    }

    private void LateUpdate() {
        if (arrow) return;

        if (owner == null) return;

        transform.position = owner.position;
        Vector3 scale = owner.localScale.x < initOwnerScale.x ? initOwnerScale : (owner.localScale.x / initOwnerScale.x) * initSignalScale;
        transform.localScale = scale;
    }

    private void EnableSignal() {
        if (arrow) {
            Vector3 point0 = arrow.linePoints[0];
            Vector3 point1 = arrow.linePoints[1];


            SetPos((point0 + point1) / 2);
            Vector3 dir = (point1 -  point0).normalized;
            SetRotation(Quaternion.Euler(0f, 0f, Utility.AngleFromDir(dir)));
        }

        waveEffect.SetActive(true);
    }

    private void DisableSignal() {
        waveEffect.SetActive(false);
    }

    public void SetPos(Vector3 pos) {
        transform.position = pos;
    }

    public void SetRotation(Quaternion rotation) {
        transform.rotation = rotation;
    }
}
