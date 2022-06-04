using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PatientNumberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private AudioSource _audioSource;
    
    private void OnEnable()
    {
        Events.OnPatientNumberChanged += OnPatientNumberChanged;
    }

    private void OnPatientNumberChanged(int patientnumber)
    {
        _text.text = $"00{patientnumber.ToString()}";
        _audioSource.Play();
    }

    private void OnDestroy()
    {
        Events.OnPatientNumberChanged -= OnPatientNumberChanged;
    }
}
