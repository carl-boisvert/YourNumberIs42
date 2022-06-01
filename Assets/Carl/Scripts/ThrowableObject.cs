using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ThrowableObject : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    
    private Rigidbody _rb;
    private Collider _collider;
    private bool _isPickedUp = false;
    private float _originalDrag = 0f;

    public void Init(Interactable interactable)
    {
        _interactable = interactable;
        _rb = GetComponent<Rigidbody>();
        _originalDrag = _rb.drag;
        _collider = GetComponent<Collider>();
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
