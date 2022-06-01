using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private float _endTimeSeconds;
    [SerializeField] private int _currentPatientNumber;
    [SerializeField] private int _targetPatientNumber;

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
    }
    
    private float _currentTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        _timeManager.StartTime();
        _currentTime = Time.time;
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
    }

    public void IncreaseCounter(int increase)
    {
        _currentPatientNumber += increase;
        if (_currentPatientNumber == _targetPatientNumber)
        {
            _currentPatientNumber -= Random.Range(1, 10);
        }
        
        Debug.Log($"Patient number is: {_currentPatientNumber}");
        if (Events.OnPatientNumberChanged != null)
        {
            Events.OnPatientNumberChanged(_currentPatientNumber);
        }
    }
}
