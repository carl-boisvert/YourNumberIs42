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
        string number = patientnumber.ToString("0000");
        _text.text = number;
        _audioSource.Play();
    }

    private void OnDestroy()
    {
        Events.OnPatientNumberChanged -= OnPatientNumberChanged;
    }
}
