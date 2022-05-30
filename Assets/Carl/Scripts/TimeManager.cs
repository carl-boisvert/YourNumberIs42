using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Serializable]
    private struct TimeIncrease
    {
        public float Time;
        public int Increase;
    }
    
    [SerializeField] private List<TimeIncrease> _timeIncreases;
    [SerializeField] private float _increaseTriggerPaddingSeconds;
    [SerializeField] private GameManager _gameManager;
    
    private float _startTime;

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void StartTime()
    {
        _startTime = Time.time;
    }

    private void ListenToEvent()
    {
    }

    private void TriggerIncrease()
    {
    }

    private void OnEventTriggered()
    {
    }
}