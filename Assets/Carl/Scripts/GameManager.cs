using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private float _endTimeSeconds;
    [SerializeField] private int _currentPatientNumber;
    [SerializeField] private int _targetPatientNumber;

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
        float percentageOfTimePassed = _currentTime/_endTimeSeconds;
        
        _currentPatientNumber += Mathf.FloorToInt(increase - (increase*percentageOfTimePassed));
        if (_currentPatientNumber == _targetPatientNumber)
        {
            _currentPatientNumber -= Random.Range(1, 5);
        }
        
        Debug.Log($"Patient number is: {_currentPatientNumber}");
        if (Events.OnPatientNumberChanged != null)
        {
            Events.OnPatientNumberChanged(_currentPatientNumber);
        }
    }
}
