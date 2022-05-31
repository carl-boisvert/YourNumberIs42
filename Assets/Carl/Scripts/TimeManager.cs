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
    private int _increaseIndex = 0;

    void Start()
    {
        
    }

    public void StartTime()
    {
        _startTime = Time.time;
        StartCoroutine(WaitForNextIncrease());
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

    IEnumerator WaitForNextIncrease()
    {
        while (_increaseIndex < _timeIncreases.Count)
        {
            yield return new WaitForSeconds(_timeIncreases[_increaseIndex].Time - Time.time);
            _gameManager.IncreaseCounter(_timeIncreases[_increaseIndex].Increase);
            _increaseIndex++;
        }
    }
}