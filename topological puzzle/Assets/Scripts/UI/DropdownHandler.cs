using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public delegate void OnValueChangedDelegate(int value);
    public event OnValueChangedDelegate OnValueChanged;

    private void Awake(){
        dropdown.onValueChanged.AddListener(delegate { InvokeOnValueChagened(); });
    }

    private void OnEnable(){
        LevelManager.OnLevelPoolChanged += AddOptions;
    }

    private void OnDisable(){
        LevelManager.OnLevelPoolChanged -= AddOptions;
    }

    public void UpdateCurrentValue(int value, bool invokeOnValueChanged = true){
        if (!invokeOnValueChanged)
            dropdown.onValueChanged.RemoveAllListeners();

        dropdown.value = value;

        if (!invokeOnValueChanged)
            dropdown.onValueChanged.AddListener(delegate { InvokeOnValueChagened(); });
    }

    private void InvokeOnValueChagened()
    {
        if(OnValueChanged != null)
            OnValueChanged(dropdown.value);
    }

    public void SetNextValue() {
        int value = dropdown.value + 1;

        if (value >= dropdown.options.Count) return;

        UpdateCurrentValue(value);
    }
    public void SetPrevValue() {
        int value = dropdown.value - 1;

        if (value < 0) return;

        UpdateCurrentValue(value);
    }
    public void AddOption(string option){
        dropdown.options.Add(new TMP_Dropdown.OptionData() { text = option });
    }

    public void AddOptions(List<string> options){
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }
}
