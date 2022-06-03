using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMeshEvent : GameEvent
{
    [SerializeField] private string _eventId;

    public override void Execute()
    {
        if (Events.OnExecuteEvent != null)
        {
            Events.OnExecuteEvent(_eventId);
        }
    }
}
