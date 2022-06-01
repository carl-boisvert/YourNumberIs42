using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChangeObjectEvent : GameEvent
{
    [Serializable]
    private struct ChangeObject
    {
        public GameObject ParticleSystem;
        public GameObject Prefab;
    }

    [SerializeField] private List<ChangeObject> _meshChanges;
    [SerializeField] private GameObject _currentMesh;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private int _increasePatientNumber = 0;
    
    private bool _isEnabled = true;
    private int _currentMeshChangeIndex = 0;

    public override void Execute()
    {
        if (_currentMeshChangeIndex < _meshChanges.Count)
        {
            GameObject temp = CopyObjectAttributes();
            Destroy(_currentMesh);
            _currentMesh = temp;

            ThrowableObject throwableObject = _currentMesh.GetComponent<ThrowableObject>();
            throwableObject.Init(_interactable);
            GameObject ps = Instantiate(_meshChanges[_currentMeshChangeIndex].ParticleSystem, _currentMesh.transform);
            Destroy(ps, 5);
            _currentMeshChangeIndex++;
            if (_currentMeshChangeIndex == _meshChanges.Count)
            {
                GameManager.Instance.IncreaseCounter(_increasePatientNumber);
                _isEnabled = false;
            }
        }
    }

    private GameObject CopyObjectAttributes()
    {
        GameObject to = Instantiate(_meshChanges[_currentMeshChangeIndex].Prefab, transform);
        to.transform.position = _currentMesh.transform.position;
        Rigidbody rbFrom = _currentMesh.GetComponent<Rigidbody>();
        Rigidbody rbTo = to.GetComponent<Rigidbody>();
        rbTo.velocity = rbFrom.velocity;
        return to;
    }

    private void OnValidate()
    {
        foreach (var meshChange in _meshChanges)
        {
            if (meshChange.Prefab.GetComponent<ThrowableObject>() == null)
            {
                Debug.LogError($"Object ${meshChange.Prefab.name} is missing the throwable object script");
            }
        }
    }
}
