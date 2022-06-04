using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    [SerializeField] private List<AudioClip> _collisionSounds = new List<AudioClip>();

    private Rigidbody _rb;
    private Collider _collider;
    private bool _isPickedUp = false;
    private bool _isThrown= false;
    private float _originalDrag = 0f;

    public void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _originalDrag = _rb.drag;
        _collider = GetComponent<Collider>();
    }

    public void Init(Interactable interactable)
    {
        _interactable = interactable;
    }

    public void PickObject()
    {
        _isPickedUp = true;
        _rb.useGravity = false;
        _rb.drag = 10f;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void MoveObject(Vector3 moveDirection) {
        _rb.AddForce(moveDirection);
    }

    public void ThrowObject(Vector3 force)
    {
        _rb.useGravity = true;
        _rb.drag = _originalDrag;
        _rb.constraints = RigidbodyConstraints.None;
        _rb.AddTorque(force, ForceMode.Impulse);
        _rb.AddForce(force, ForceMode.Impulse);
        _isThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("FloorAndWall") && _isThrown)
        {
            _isPickedUp = false;
            _interactable.OnWallCollision();
            int index = Random.Range(0, _collisionSounds.Count);
            AudioManager.Instance.PlayEffect(_collisionSounds[index], AudioManager.AudioType.SFX, true);
        }
    }
}
