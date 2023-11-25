using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownHandler : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public delegate void OnValueChangedDelegate(int value);
    public event OnValueChangedDelegate OnValueChanged;
    private void Awake()
    {
        //LevelManager.OnPlayerLevelsLoaded += AddOptions;
        dropdown.onValueChanged.AddListener(delegate { InvokeOnValueChagened(); });
    }

    private void OnEnable()
    {
        LevelManager.OnLevelPoolChanged += AddOptions;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelPoolChanged -= AddOptions;
    }

    void Start()
    {

    }

    public void UpdateCurrentValue(int value)
    {
        dropdown.value = value;
    }

    private void InvokeOnValueChagened()
    {
        if(OnValueChanged != null)
        {
            OnValueChanged(dropdown.value);
        }
    }

    public void AddOption(string option)
    {
        dropdown.options.Add(new TMP_Dropdown.OptionData() { text = option });
    }

    public void AddOptions(List<string> options)
    {
        Debug.Log("should add options: " + options);
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }
}
