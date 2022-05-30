using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    private List<GameEvent> _events;

    private void Start()
    {
        _events = GetComponents<GameEvent>().ToList();
    }

    public void OnWallCollision()
    {
        foreach (var gameEvent in _events)
        {
            gameEvent.Execute();
        }
    }
}
