using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private float _endTimeSeconds;
    [SerializeField] private int _startPatientNumber;
    [SerializeField] private int _currentPatientNumber;
    [SerializeField] private int _targetPatientNumber;
    [SerializeField] private Volume _volume;
    [SerializeField] private float _closeEyesSpeed = 0.0001f;
    [SerializeField] private GameObject _blackScreen;

    private bool _gameEnded = false;
    private Vignette _vignette;
    private Coroutine _eyesClosingCoroutine;
    
    public static GameManager instance;
    public static GameManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<GameManager>();
                if (instance == null) {
                    GameObject managerClone = new GameObject();
                    managerClone.AddComponent<GameManager>();
                    managerClone.name = "GameManager";
                }
            }
            return instance;
        }
    }

    private void Awake() {
        instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        Events.OnGameStart += OnGameStart;
    }

    private void OnDestroy()
    {
        Events.OnGameStart -= OnGameStart;
    }

    private void OnGameStart()
    {
        StartGame();
    }

    private float _currentTime = 0f;
    // Start is called before the first frame update
    void StartGame()
    {
        if (!_gameEnded)
        {
            _timeManager.StartTime();
            _currentTime = Time.time;
            _volume.profile.TryGet<Vignette>(out _vignette);
            _currentPatientNumber = _startPatientNumber;
            Events.OnPatientNumberChanged(_currentPatientNumber);
        }
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
        //Debug.Log(_gameEnded);
        if (_currentTime >= _endTimeSeconds && !_gameEnded)
        {
            _gameEnded = true;
            Debug.Log("Transition to end game");
            _timeManager.StopTime();
            _eyesClosingCoroutine = StartCoroutine(CloseEyes());
        }
    }

    public void IncreaseCounter(int increase)
    {
        if (!_gameEnded)
        {
            _currentPatientNumber += increase;
            if (_currentPatientNumber >= _targetPatientNumber)
            {
                _currentPatientNumber = _targetPatientNumber - Random.Range(1, 10);
            }
            else if (_currentPatientNumber < 0)
            {
                _currentPatientNumber = Random.Range(1, 10);
            }

            Debug.Log($"Patient number is: {_currentPatientNumber}");
            if (Events.OnPatientNumberChanged != null)
            {
                Events.OnPatientNumberChanged(_currentPatientNumber);
            }
        }
    }

    IEnumerator CloseEyes()
    {
        float _speed = _closeEyesSpeed;
        while (_vignette.intensity.value < 1)
        {
            _vignette.intensity.value += _speed;
            yield return new WaitForEndOfFrame();
        }
        _blackScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        _blackScreen.SetActive(false);
        while (_vignette.intensity.value > 0)
        {
            _vignette.intensity.value -= _speed;
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(1);
        _speed *= 2;
        
        while (_vignette.intensity.value < 1)
        {
            _vignette.intensity.value += _speed;
            yield return new WaitForEndOfFrame();
        }
        _blackScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        _blackScreen.SetActive(false);
        _speed /= 2;
        while (_vignette.intensity.value > 0)
        {
            _vignette.intensity.value -= _speed*100;
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(1);
        
        _speed /= 10;
        
        while (_vignette.intensity.value < 1)
        {
            _vignette.intensity.value += _speed;
            yield return new WaitForEndOfFrame();
        }
        _blackScreen.SetActive(true);

        SceneManager.sceneLoaded += OnEndingSceneLoaded;
        SceneManager.LoadSceneAsync("Ending");
        Debug.Log("Loading Ending Scene");
        
        StopCoroutine(_eyesClosingCoroutine);
        yield return null;
    }

    private void OnEndingSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SceneManager.sceneLoaded -= OnEndingSceneLoaded;
        _currentTime = 0;
        _gameEnded = true;
        _currentPatientNumber = _startPatientNumber;
        if (Events.OnPatientNumberChanged != null)
        {
            Events.OnPatientNumberChanged(_currentPatientNumber);
        }
        Debug.Log("Yeaaah");
        Invoke("EndGame", 5);

    }

    private void EndGame()
    {
        UIManager.Instance.GoToCredits();
    }

    public bool IsGameEnded()
    {
        return _gameEnded;
    }
}
