using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private float _endTimeSeconds;
    [SerializeField] private int _currentPatientNumber;
    [SerializeField] private int _targetPatientNumber;
    // Start is called before the first frame update
    void Start()
    {
        _timeManager.StartTime();
    }
}
