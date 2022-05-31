using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Collider _collider;

    private bool _isPickedUp = false;

    public void Init(Interactable interactable)
    {
        _interactable = interactable;
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void PickObject()
    {
        _isPickedUp = true;
        _rb.isKinematic = true;
        _collider.enabled = false;
    }

    public void ThrowObject(Vector3 force)
    {
        _rb.isKinematic = false;
        _collider.enabled = true;
        _rb.AddTorque(force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("FloorAndWall") && _isPickedUp)
        {
            _isPickedUp = false;
            _interactable.OnWallCollision();
        }
    }
}
