using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "New Sound Effect")]
public class SoundEffect : ScriptableObject {

    public AudioClip[] clips;

    [HideInInspector] public bool useRandomVolume;
    //[DrawIf("useRandomVolume", false, ComparisonType.Equals, DisablingType.DontDraw)]
    [Range(0, 1f)]
    [HideInInspector] public float volume;
    //[DrawIf("useRandomVolume", true, ComparisonType.Equals, DisablingType.DontDraw)]
    [HideInInspector] public Vector2 volumeRandom = new Vector2(0.5f, 0.5f);

    [HideInInspector] public bool useRandomPitch;
    //[DrawIf("useRandomPitch", false, ComparisonType.Equals, DisablingType.DontDraw)]
    [Range(-3f, 3f)]
    [HideInInspector] public float pitch;
    //[DrawIf("useRandomPitch", true, ComparisonType.Equals, DisablingType.DontDraw)]
    [HideInInspector] public Vector2 pitchRandom = new Vector2(1, 1);

    [SerializeField] private SoundClipPlayOrder playOrder;
    [SerializeField] private int playIndex = 0;

    public bool loop = false;

    [HideInInspector] public AudioSource source;

#if UNITY_EDITOR
    private AudioSource previewer;

    private void OnEnable() {
        previewer = EditorUtility
            .CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave,
                typeof(AudioSource))
            .GetComponent<AudioSource>();
    }
    
    private void OnDisable() {
        DestroyImmediate(previewer.gameObject);
    }

    public void PlayPreview(bool playReverse = false) {
        Play(previewer, playReverse);
    }

    public void StopPreview() {
        previewer.Stop();
    }
#endif

    private AudioClip GetAudioClip() {
        // get current clip
        var clip = clips[playIndex >= clips.Length ? 0 : playIndex];

        // find next clip
        switch (playOrder) {
            case SoundClipPlayOrder.in_order:
            playIndex = (playIndex + 1) % clips.Length;
            break;
            case SoundClipPlayOrder.random:
            playIndex = Random.Range(0, clips.Length);
            break;
            case SoundClipPlayOrder.reverse:
            playIndex = (playIndex + clips.Length - 1) % clips.Length;
            break;
        }

        // return clip
        return clip;
    }

    public AudioSource Play(AudioSource audioSourceParam = null, bool playReverse = false) {
        if (clips.Length == 0) {
            Debug.LogWarning($"Missing sound clips for {name}");
            return null;
        }

        source = audioSourceParam;
        if (source == null) {
            var _obj = new GameObject("Sound", typeof(AudioSource));
            source = _obj.GetComponent<AudioSource>();
        }

        // set source config:
        source.clip = GetAudioClip();
        source.loop = loop;
        source.volume = useRandomVolume ? Random.Range(volumeRandom.x, volumeRandom.y): volume;
        source.pitch = useRandomPitch ? Random.Range(pitchRandom.x, pitchRandom.y) : pitch;
        source.pitch = playReverse ? -source.pitch : source.pitch;
        source.time = source.pitch < 0 ? source.clip.length - 0.001f : 0f;

        source.Play();

/*#if UNITY_EDITOR
        if (source != previewer) {
            Destroy(source.gameObject, source.clip.length / source.pitch);
        }
#else
        Destroy(source.gameObject, source.clip.length / source.pitch);
#endif*/

        return source;
    }

    public void Stop() {
        if (source && !source.isPlaying) return;

        source.Stop();
        source = null;
    }
    enum SoundClipPlayOrder {
        random,
        in_order,
        reverse
    }
}
