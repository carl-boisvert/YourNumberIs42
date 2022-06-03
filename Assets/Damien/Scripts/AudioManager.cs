using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Variables
    // All public functions in this class can be accessed by typing AudioManager.Instance.<nameOfFunction> from any class
    private static AudioManager instance;    
    public static AudioManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<AudioManager>();
                if (instance == null) {
                    GameObject managerClone = new GameObject();
                    managerClone.AddComponent<AudioManager>();
                    managerClone.name = "AudioManager";
                }
            }
            return instance;
        }
        //setter in Awake
    }
    public static bool IsDestroyed { 
        get { return Instance != null; } 
    }

    [Header("Mixing")]
    [SerializeField] private AudioMixer _gameMixer = null;
    [SerializeField] private AudioMixerGroup[] _mixerGroups = null;
    [SerializeField] private AudioMixerSnapshot[] _musicSnapshots = null;
    [SerializeField] private AudioMixerSnapshot[] _ambienceSnapshots = null;
    [SerializeField] private AudioMixerSnapshot[] _masterSnapshots = null;

    private AudioMixerSnapshot _currentMusicSnapshot;
    private AudioMixerSnapshot _currentAmbienceSnapshot;

    [Header("Background")]
    [SerializeField] private AudioSource[] _musicSources = null;
    [SerializeField] private AudioSource[] _ambienceSources = null;
    [SerializeField] private SwapAudioScriptableObject _defaultBackgroundAudio = null;

    [Header("SFX")]
    [SerializeField] private AudioMixerSnapshot[] _soundEffectSnapshots = null;
    [SerializeField] private AudioSource _footStepSource = null;
    [SerializeField] private AudioSource _weaponSource = null;
    [SerializeField] private Transform _sfxParent = null;
    [SerializeField] private GameObject _sourcePrefab = null;
    [SerializeField] private int _numberOfSources = 1;
    [SerializeField] private float _timeForPowerGenerator = 1f;

    private List<AudioSource> _soundEffectSources = new List<AudioSource>();
    private AudioMixerSnapshot _currentSFXSnapshot = null;
    #endregion

    #region Unity Events
    private void Awake() {
        instance = this;
        InitAudio();        
    }

    private void Start() {
        InitMixer();
    }
    #endregion

    #region Private Methods
    private void InitMixer() {
        _currentAmbienceSnapshot = _ambienceSnapshots[0];
        _currentMusicSnapshot = _musicSnapshots[0];

        //Set volumes for mixers based on PlayerPrefs
        _gameMixer.SetFloat("Master", PlayerPrefs.GetFloat("Master", PreferenceManager.Instance.GetFloatPrefDefault("Master")));
        foreach (var group in _mixerGroups) {
            if(!_gameMixer.SetFloat(group.name, PlayerPrefs.GetFloat(group.name, PreferenceManager.Instance.GetFloatPrefDefault(group.name)))) {
                Debug.Log("parameter not found");
            }
        }
    }

    private void InitSource(AudioSource[] sources, AudioType audioType = AudioType.None) {
        AudioMixerGroup group = null;
        string audioTypeStr = audioType.ToString();
        bool playOnAwake = true;
        bool loop = false;

        if(audioType != AudioType.None) {
            foreach (var mixer in _mixerGroups) {
                if (mixer.name == audioTypeStr) {
                    group = mixer;
                    break;
                }
            }
        }

        switch (audioType) {
            case AudioType.Ambience:
                loop = true;
                break;
            case AudioType.Music:
                loop = true;
                break;
            case AudioType.SFX:
                break;
            default:
                break;
        }

        foreach (var source in sources) {
            source.playOnAwake = playOnAwake;
            source.loop = loop;
            source.volume = 1f;
            if(source.outputAudioMixerGroup == null) {
                source.outputAudioMixerGroup = group;
            }           
        }
    }

    private void InitAudio() {
        //Music
        InitSource(_musicSources, AudioType.Music);

        //Ambience
        InitSource(_ambienceSources, AudioType.Ambience);

        //SFX
        if(_soundEffectSources.Count == 0) {
            for (int i = 0; i < _numberOfSources; i++) {
                _soundEffectSources.Add(CreateNewSource(_sfxParent));
            }
        }

        SwapMusicSnapshots(_defaultBackgroundAudio, 1f);
    }

    private AudioSource CreateNewSource(Transform parent) {
        //Debug.Log($"CreateNewSource({parent})");
        GameObject clone = Instantiate(_sourcePrefab, transform.position, Quaternion.identity, parent);
        AudioSource source = clone.GetComponent<AudioSource>();
        
        if (source != null) {
            AudioSource[] sources = new AudioSource[] { source };
            InitSource(sources, AudioType.SFX);
        }

        clone.SetActive(false);
        return source;
    }

    private IEnumerator SourceCleanup(AudioSource source, bool attachToParent = false) {
        //Debug.Log($"SourceCleanup {source}");
        if(source != null) {
            //if (isDialogue) Debug.Log($"{source} SourceCleanup");
            float counter = 0.5f;

            while (counter > 0) {
                counter -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (source == null)
                yield break;

            if (!source.isPlaying || Time.timeScale == 0) {
                source.spatialBlend = 0f;
                source.clip = null;
                if (attachToParent) {
                    source.gameObject.transform.SetParent(_sfxParent);
                }
                source.gameObject.SetActive(false);
            }
        }

        //Debug.Log($"End of SourceCleanup {source}");
    }
    #endregion

    #region Public Functions
    /* Public functions to access private AudioManager data or perform actions */
    public void PlayPlayerEffect(AudioClip clip, AudioType sourceToUse, bool wantToPitch = false) {
        if (clip != null) {
            float pitch = wantToPitch ? UnityEngine.Random.Range(0.1f, 3f) : 1f;
            
            if (sourceToUse == AudioType.Footsteps) {
                _footStepSource.pitch = pitch;
                _footStepSource.PlayOneShot(clip);
            }
            else if (sourceToUse == AudioType.Weapon) {
                _weaponSource.pitch = pitch;
                _weaponSource.PlayOneShot(clip);
            }
        }        
    }

    public void PlayEffect(AudioClip clip, AudioType sourceToUse = AudioType.None, bool wantToPitch = false, Transform worldPosition = null, bool attachToParent = false) {
        if (clip != null) {
            if (sourceToUse == AudioType.None || sourceToUse == AudioType.SFX) {
                int clipAlreadyInUse = 0;
                float pitch = wantToPitch ? UnityEngine.Random.Range(0.1f, 3f) : 1f;

                // Object pooling through some AudioSources to find an unused one
                foreach (var source in _soundEffectSources) {
                    if(source != null) {
                        if (source.clip == clip) {
                            clipAlreadyInUse++;
                        }

                        //Debug.Log($"{clip} in use {clipAlreadyInUse} time(s).");

                        if (!source.gameObject.activeInHierarchy) { //if a source is inactive, use it.
                            if (worldPosition != null) {
                                source.spatialBlend = 1f;
                                source.gameObject.transform.position = worldPosition.position;

                                if (attachToParent) {
                                    source.gameObject.transform.SetParent(worldPosition);
                                }                                    
                            }

                            // Load the AudioClip and play it, cleaning itself up once it has completed
                            source.enabled = true;
                            source.pitch = pitch;
                            source.clip = clip;
                            source.gameObject.SetActive(true);
                            StartCoroutine(SourceCleanup(source, attachToParent));
                            return;
                        }
                    }                    
                }

                if (clipAlreadyInUse < 9999) { //As long as the clip is not being played more than 9999 times, create a new source and play it.
                    // If all AudioSources are in use, we need to create a new source to play the clip with
                    // We'll store the new source with the others to use later if necessary
                    AudioSource newSource = CreateNewSource(_sfxParent);
                    _soundEffectSources.Add(newSource);

                    if (worldPosition != null) {
                        newSource.spatialBlend = 1f;
                        newSource.gameObject.transform.position = worldPosition.position;

                        if (attachToParent)
                            newSource.gameObject.transform.SetParent(worldPosition);
                    }

                    newSource.enabled = true;
                    newSource.pitch = pitch;
                    newSource.clip = clip;
                    newSource.gameObject.SetActive(true);
                    StartCoroutine(SourceCleanup(newSource));
                }                
            }
        }
    }

    public void SwapMusicSnapshots(SwapAudioScriptableObject so, float delay = 1f) {
        //Debug.Log("SwapSnapshots");
        if(_currentMusicSnapshot == _musicSnapshots[0]) {
            _currentMusicSnapshot = _musicSnapshots[1];
            _currentAmbienceSnapshot = _ambienceSnapshots[1];

            _musicSources[1].clip = so.Music;
            _ambienceSources[1].clip = so.Ambience;

            _musicSnapshots[1].TransitionTo(delay);
            _musicSources[1].Play();
            _ambienceSnapshots[1].TransitionTo(delay);
            _ambienceSources[1].Play();

            _musicSources[0].Stop();
            _ambienceSources[0].Stop();

            
        }
        else {
            _musicSources[0].clip = so.Music;
            _ambienceSources[0].clip = so.Ambience;

            _musicSnapshots[0].TransitionTo(delay);
            _musicSources[0].Play();
            _ambienceSnapshots[0].TransitionTo(delay);
            _ambienceSources[0].Play();

            _musicSources[1].Stop();
            _ambienceSources[1].Stop();

            _currentMusicSnapshot = _musicSnapshots[0];
            _currentAmbienceSnapshot = _ambienceSnapshots[0];
        }
    }

    public void PowerGeneratorSnapshot() {
        if (_currentSFXSnapshot == _soundEffectSnapshots[0]) {
            _currentSFXSnapshot = _soundEffectSnapshots[1];
        }
        else {
            _currentSFXSnapshot = _soundEffectSnapshots[0];
        }

        _currentSFXSnapshot.TransitionTo(_timeForPowerGenerator);
    }

    public void PauseAudio(bool state) {
            _musicSources[0].ignoreListenerPause = state;
            _musicSources[1].ignoreListenerPause = state;
            _ambienceSources[0].ignoreListenerPause = state;
            _ambienceSources[1].ignoreListenerPause = state;

            AudioListener.pause = state;
    }

    public void SetVolume(string name, float value, bool updatePref = true) {
        string prefix = name.Substring(0, name.IndexOf("_"));
        AudioType type = (AudioType)Enum.Parse(typeof(AudioType), prefix);        

        switch (type) {
            case AudioType.Master:               
            case AudioType.Music:
            case AudioType.Ambience:
            case AudioType.SFX:
                _gameMixer.SetFloat(type.ToString(), value);
                break;
            case AudioType.None:
            default:
                break;
        }

        if (updatePref) {
            PreferenceManager.Instance.UpdatePreferences<float>(name, PreferenceType.floatType, value);
        }
    }

    public void Reset(bool resetToMain = false) {        
        if (resetToMain && _defaultBackgroundAudio != null) {
            SwapMusicSnapshots(_defaultBackgroundAudio, 0.1f);
        }

        AudioListener.pause = false;
    }

    public void ReturnSourceToParent(AudioSource[] sources) {
        foreach (var source in sources) {
            source.transform.parent = _sfxParent;
            StartCoroutine(SourceCleanup(source));
        }
    }
    public void ReturnSourceToParent(AudioSource source) {
        source.transform.parent = _sfxParent;
        StartCoroutine(SourceCleanup(source));
    }
    #endregion

    public enum AudioType {
        Master,
        Music,
        Ambience,
        SFX,
        Footsteps,
        Weapon,
        None
    }

    #region UTILITIES

        ///<summary>Plays a random clip from the given array at the given source.</summary>
        ///<param name="source">The audio source the sound will be play from.</param>
        ///<param name="clips">The array of clips to choose a clip from.</param>
        ///<param name="wantToPitch">Whether to randomize the pitch or not. False by default.</param>
        ///<param name="minPitch">The minimum pitch used if wantToPitch is true. 0 by default. This should generally not be lower than 0.</param>
        ///<param name="maxPitch">The maximum pitch used if wantToPitch is true. 3 by default. This should not be higher than 3.</param>
        ///<param name="volume">The volume scale to play the sound at. 1 by default. Values should be contained from 0 to 1.</param>
        public static void PlaySFXRandom(AudioSource source, AudioClip[] clips, bool wantToPitch = false, float minPitch = 0.1f, float maxPitch = 3f, float volume = 1f){
            if (source == null || clips.Length <= 0){
                return;
            }
            
            float pitch = wantToPitch ? UnityEngine.Random.Range(minPitch, maxPitch) : 1f;
            source.pitch = pitch;

            int index = UnityEngine.Random.Range(0, clips.Length);
            source.PlayOneShot(clips[index]);
        }

    #endregion
}
