using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PreferenceType {
    boolType,
    intType,
    floatType
}

public class PreferenceManager : MonoBehaviour
{
    #region Variables
    // All public functions in this class can be accessed by typing PreferenceManager.Instance.<nameOfFunction> from any class
    private static PreferenceManager instance;
    public static PreferenceManager Instance { 
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<PreferenceManager>();
                if (instance == null) {
                    GameObject managerClone = new GameObject();
                    managerClone.AddComponent<PreferenceManager>();
                    managerClone.name = "PreferenceManager";
                }
            }
            return instance;
        }
        //setter in Awake
    }

    [Tooltip("Enable to unlock the ability to reset all PlayerPrefs with the [DELETE] key")]
    [SerializeField] private bool _debugMode = false;
    [SerializeField] private FloatPref[] _floatPrefs;
    [SerializeField] private BoolPref[] _boolPrefs;
    [SerializeField] private IntPref[] _intPrefs;
    #endregion

    #region Unity Events
    private void Awake() {
        instance = this;
        InitPrefs();
    }

    private void Update() {
        //DEBUG
        if (_debugMode) {
            if (Keyboard.current.deleteKey.wasPressedThisFrame) {
                ResetPreferences();
            }
        }        
    }

    private void OnApplicationQuit() {
        PlayerPrefs.Save();
    }
    #endregion

    private void InitPrefs() {
        foreach (var pref in _floatPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetFloat(pref.Name, pref.DefaultValue);
                Debug.Log($"PlayerPref {pref.Name} created and set to default value {pref.DefaultValue}");
            }
        }

        foreach (var pref in _boolPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetInt(pref.Name, pref.DefaultValue ? 1 : 0);
                Debug.Log($"PlayerPref {pref.Name} created and set to default value {pref.DefaultValue}");
            }
        }

        foreach (var pref in _intPrefs) {
            if (!PlayerPrefs.HasKey(pref.Name)) {
                PlayerPrefs.SetInt(pref.Name, pref.DefaultValue);
                Debug.Log($"PlayerPref {pref.Name} created and set to default value {pref.DefaultValue}");
            }
        }

        PlayerPrefs.Save();
    }    

    public void UpdatePreferences<T>(string name, PreferenceType type, T value) {
        string prefix = name.Substring(0, name.IndexOf("_"));
        Debug.Log($"PreferenceManager()::UpdatePreferences() name: {prefix}, value: {value}");

        if (!PlayerPrefs.HasKey(prefix)) {
            Debug.LogError($"Couldn't find {prefix}");
            return;
        }

        Debug.Log($"{prefix} Key Found");

        switch (type) {
            case PreferenceType.boolType:
                bool b = (bool)Convert.ChangeType(value, typeof(bool));
                PlayerPrefs.SetInt(prefix, b ? 1 : 0);
                break;
            case PreferenceType.intType:
                int i = (int)Convert.ChangeType(value, typeof(int));
                PlayerPrefs.SetInt(prefix, i);
                break;
            case PreferenceType.floatType:
                float f = (float)Convert.ChangeType(value, typeof(float));
                PlayerPrefs.SetFloat(prefix, f);
                break;
            default:
                break;
        }
    }

    public void ResetPreferences() {
        Debug.Log("Deleting all preferences");
        PlayerPrefs.DeleteAll();
        InitPrefs();
    }

    public bool GetBoolPrefDefault(string name) {
        foreach (var pref in _boolPrefs) {
            if (pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return false;
    }

    public float GetFloatPrefDefault(string name) {
        foreach (var pref in _floatPrefs) {
            if (pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return 0f;
    }

    public int GetIntPrefDefault(string name) {
        foreach (var pref in _intPrefs) {
            if(pref.Name == name) {
                return pref.DefaultValue;
            }
        }

        return 0;
    }

    public float GetFloatPref(string name) {
        return PlayerPrefs.GetFloat(name, GetFloatPrefDefault(name));
    }

    public bool GetBoolPref(string name) {
        return PlayerPrefs.GetInt(name, GetBoolPrefDefault(name) == true ? 1 : 0) == 1 ? true : false;
    }

    public int GetIntPref(string name) {
        return PlayerPrefs.GetInt(name, GetIntPrefDefault(name));
    }
}

[System.Serializable]
public struct BoolPref {
    public string Name;
    public bool DefaultValue;

    public BoolPref(string name, bool defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}

[System.Serializable]
public struct FloatPref {
    public string Name;
    public float DefaultValue;

    public FloatPref(string name, float defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}

[System.Serializable]
public class IntPref {
    public string Name;
    public int DefaultValue;

    public IntPref(string name, int defaultValue) {
        this.Name = name;
        this.DefaultValue = defaultValue;
    }
}