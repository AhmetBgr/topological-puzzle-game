using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowPoint : MonoBehaviour
{
    public Arrow arrow;
    public int index;

    private void OnMouseEnter()
    {
        arrow.col.enabled = false;
        transform.localScale *= 1.2f;
    }

    private void OnMouseExit()
    {
        arrow.col.enabled = true;
        transform.localScale /= 1.2f;
    }
}
