using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "event", menuName = "Data/Event")]
public class EventData : ScriptableObject
{
    public Guid Id;
    public GameEvent Event;
}
