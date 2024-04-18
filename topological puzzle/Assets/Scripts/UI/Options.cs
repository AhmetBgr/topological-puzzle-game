using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using DG.Tweening;

public class Options : MonoBehaviour{
    public InfoIndicator infoIndicator;
    public Toggle disableActionInfoTextToggle;
    public Toggle fullScreenToggle;
    public Toggle vsyncToggle;
    public TMP_Dropdown resolutionDropdown;
    public Slider audioSlider;
    public TextMeshProUGUI savedText;
    public AudioMixer mixer;

    public static OptionsData optionsData;

    private List<Resolution> resolutions  = new List<Resolution>();
    private IEnumerator saveDataCor;
    private Tween savedTextTween;

    private string optionsDataPath;

    private string dataFileName = "options.txt";

    private void OnEnable() {
        Grid.OnGridSizeChanged += SetGridSize;
    }

    private void OnDisable() {
        Grid.OnGridSizeChanged -= SetGridSize;
    }

    void Start(){
        optionsDataPath = Application.persistentDataPath + "/" + dataFileName;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            optionsDataPath = "/idbfs/" + dataFileName; 


        UpdateResolutionDropdown();

        if(Application.platform == RuntimePlatform.WebGLPlayer) {
            resolutionDropdown.interactable = false;
        }

        if (!File.Exists(optionsDataPath)) {
            // Set default options
            optionsData = new OptionsData();
            audioSlider.value = 0.2f;
            resolutionDropdown.value = resolutions.Count - 1;
            fullScreenToggle.isOn = true;
            vsyncToggle.isOn = true;
            disableActionInfoTextToggle.isOn = false;
            SetGridSize(4);

            // Save options data
            SaveOptionsData();
        }
        else {
            // Load options data
            LoadOptionsData();

            audioSlider.value = optionsData.masterVolume;
            resolutionDropdown.value = GetResIndex(optionsData.resolution);
            fullScreenToggle.isOn = optionsData.isFulscreen;
            vsyncToggle.isOn = optionsData.vsync;
            disableActionInfoTextToggle.isOn = optionsData.disableActionInfoText;
            SetGridSize(optionsData.gridSize);
        }
    }

    public void SaveOptionsData() {
        Utility.SaveAsJson(optionsDataPath, optionsData);
    }

    public void LoadOptionsData() {
        optionsData = Utility.LoadDataFromJson<OptionsData>(optionsDataPath);
    }

    public void SetFullScreen(bool isFullscreen){
        Screen.fullScreen = isFullscreen;
        int val = isFullscreen ? 1 : 0 ;
        optionsData.isFulscreen = isFullscreen;
        OnOptionChanged();
    }

    public void SetDisableActionInfoText(bool value) {
        Debug.Log("info text indcator enable status : " + infoIndicator.enabled);
        infoIndicator.enabled = !value;
        optionsData.disableActionInfoText = value;
        OnOptionChanged();
    }

    public void SetGridSize(float value, float minGridSize) {
        int index = (int) (value / minGridSize);

        SetGridSize(index);
    }
    public void SetGridSize(int index) {
        index = index < 1 ? 1 : index;
        index = index > 8 ? 8 : index;

        optionsData.gridSize = index;

        OnOptionChanged();
    }
    public void SetMasterVolume(float value){
        Debug.Log("master volume level saved.");
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        optionsData.masterVolume = value;

        OnOptionChanged();
    }

    public void SetResolution(TMP_Dropdown dropdown){
        Resolution resolution = resolutions[dropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        optionsData.resolution = new int[2] { resolution.width, resolution.height };

        OnOptionChanged();
    }

    public void SetVsync(bool value) {
        optionsData.vsync = value;
        QualitySettings.vSyncCount = Convert.ToInt32(value);

        OnOptionChanged();
    }

    private int GetResIndex(int[] res) {
        int resIndex = resolutions.Count - 1;
        for (int i = 0; i < resolutions.Count; i++) {

            if (resolutions[i].width == res[0] && resolutions[i].height == res[1]) {
                return i;
            }
        }
        return resIndex;
    }

    private void UpdateResolutionDropdown(){
        Resolution[] allRes = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < allRes.Length; i++){   
            if(allRes[i].width > 640){
                string option = allRes[i].width.ToString() + "x" + allRes[i].height.ToString();
                options.Add(option);
                resolutions.Add(allRes[i]);
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }

    private void OnOptionChanged() {
        if(saveDataCor != null) {
            StopCoroutine(saveDataCor);
        }
        saveDataCor = _SaveData();
        StartCoroutine(saveDataCor);
    }

    private IEnumerator _SaveData() {
        yield return new WaitForSeconds(0.1f);

        SaveOptionsData();

        if (savedTextTween != null)
            savedTextTween.Kill();

        savedText.alpha = 1;
        savedText.gameObject.SetActive(true);
        savedTextTween = savedText.DOFade(0, 1f).SetDelay(0.5f);
    }

    /*public void ChangeGraphicQuality(){
        string qualityLevelText = qualityLevelButtonText.text;
        int qualityLevel = QualitySettings.GetQualityLevel();

        if(qualityLevel == 2){
            qualityLevelButtonText.text = "Low";
            QualitySettings.SetQualityLevel(0);
        }
        else if(qualityLevel == 0){
            qualityLevelButtonText.text = "Medium";
            QualitySettings.SetQualityLevel(1);
        }
        else if(qualityLevel == 1){
            qualityLevelButtonText.text = "High";
            QualitySettings.SetQualityLevel(2);
        }
        qualityLevel = QualitySettings.GetQualityLevel();

        PlayerPrefs.SetInt(qualityLevelKey, qualityLevel);
    }*/

    /*private void SavedQualityLevel(){
        int val = 2;
        if(PlayerPrefs.HasKey(qualityLevelKey)){
            val = PlayerPrefs.GetInt(qualityLevelKey);
        }
        
        QualitySettings.SetQualityLevel(val);
        if(val == 0){
            qualityLevelButtonText.text = "Low";
        }
        else if(val == 1){
            qualityLevelButtonText.text = "Medium";
        }
        else if(val == 2){
            qualityLevelButtonText.text = "High";
        }

    }*/
}
