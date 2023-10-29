using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CamController : MonoBehaviour
{
    void OnEnable(){
        //PaletteSwapper.On_Palette_Change += ChangeColor;
        //LevelManager.OnNodeCountChange += CenterCamWithDelay;
    }
    void OnDisable(){
        //PaletteSwapper.On_Palette_Change -= ChangeColor;
        //LevelManager.OnNodeCountChange -= CenterCamWithDelay;
    }


    public void ChangeColor(Color[] colors){
        Camera.main.DOColor(colors[0], 1f);
        
    }

    private void CenterCamWithDelay(Transform curLevel){
        StartCoroutine(CenterCam(curLevel, 0.5f));
    }

    private IEnumerator CenterCam(Transform curLevel, float delay = 0f){
        yield return new WaitForSeconds(delay);
        Vector3 center = FindCenter(curLevel);
        Debug.Log("should center to:" + center);
        transform.DOMove(center, 0.5f);
    }

    private Vector3 FindCenter(Transform curLevel){

        float totalX = 0f;
        float totalY = 0f;
        Transform level = LevelManager.curLevel.transform;
        int childCount = level.childCount;

        for (int i = 0; i < childCount; i++){
            Transform child = level.GetChild(i);
            if(child.gameObject.activeSelf && ( ( (1<<child.gameObject.layer) & LayerMask.GetMask("Node") ) != 0 ) ){
                totalX += child.position.x;
                totalY += child.position.y;
            }
        }

        if(childCount == 0) return new Vector3(0f, 0f, -10f);

        float centerX = totalX / childCount;
        float centerY = totalY / childCount;
        return new Vector3(centerX, centerY, -10f);

    }
}
