using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMeshEventListener : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private GameObject _mesh;

    private void OnEnable()
    {
        Events.OnExecuteEvent += OnExecuteEvent;
    }

    private void OnExecuteEvent(string eventId)
    {
        if (eventId == _eventId)
        {
            Instantiate(_mesh, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        Events.OnExecuteEvent -= OnExecuteEvent;
    }
}
