using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;

    public static AudioManager instance;


    void Awake(){
        // if the singleton hasn't been initialized yet
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable(){
    }

    void OnDisable(){
        
    }

    public void PlaySound(SoundEffect sound){
        if(sound != null){
            sound.Play(audioSource);
        }
    }

    public void PlaySoundWithDelay(SoundEffect sound, float delay){
        StartCoroutine(IEPlaySoundWithDelay(sound, delay));
    }

    private IEnumerator IEPlaySoundWithDelay(SoundEffect sound, float delay){
        yield return new WaitForSeconds(delay);
        PlaySound(sound);
    }

}
