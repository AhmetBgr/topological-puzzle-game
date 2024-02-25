using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddNewItem : Item
{
    public delegate void OnMouseEnterDelegate(Item item);
    public static event OnMouseEnterDelegate OnMouseEnter;

    protected override void Start()
    {
        base.Start();
        col.enabled = true;
    }

    private void OnMouseUp()
    {
        if(OnMouseEnter!= null)
        {
            OnMouseEnter(this);
        }
    }
}
