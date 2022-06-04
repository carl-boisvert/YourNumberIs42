using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Tooltip("To make sure raycasts don't interact with everything")]
    [SerializeField] private LayerMask _interactableLayer;
    [Tooltip("The position of the held Interactable")]
    [SerializeField] private Transform _holdInteractablePosition = null;
    [Tooltip("How fast you throw the Interactable object")]
    [SerializeField, Range(1f, 10f)] private float _throwForce = 1f;
    [Tooltip("How fast it snaps to the _holdInteractablePosition")]
    [SerializeField, Range(1f, 300f)] private float _pickupForce = 1f;
    [Tooltip("How far the interactable")]
    [SerializeField, Range(1f, 50f)] private float _pickupDistance = 1f;
    [SerializeField] private List<AudioClip> _throwSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _pickupSounds = new List<AudioClip>();

    private ThrowableObject _throwableObject;
    private GameObject _heldInteractable = null;
    private Transform _cameraPosition;

    private void Awake() {
        _cameraPosition = Camera.main.transform;
    }

    private void Update() {
        if (InputManager.IsInteractPressed) {
            if (_heldInteractable == null) {
                //Debug.Log("PickupInteractable");
                PickupInteractable();
            }
            else if (_heldInteractable != null) {
                //Debug.Log("MoveInteractable");
                MoveInteractable();
            }
        }
        else if (!InputManager.IsInteractPressed && _heldInteractable != null) {
            //Debug.Log("ThrowInteractable");
            ThrowInteractable();
        }
    }

    private void PickupInteractable() {
        RaycastHit hitInfo;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out hitInfo, _pickupDistance,_interactableLayer)) {
            //Debug.Log("Found Interactable");
            _throwableObject = hitInfo.collider.GetComponent<ThrowableObject>();
            _throwableObject.PickObject();

            _heldInteractable = hitInfo.transform.gameObject;
            _heldInteractable.transform.parent = _holdInteractablePosition;
            
            int index = Random.Range(0, _pickupSounds.Count);
            AudioManager.Instance.PlayEffect(_pickupSounds[index], AudioManager.AudioType.SFX, true);
        }       
    }

    private void ThrowInteractable() {
        _heldInteractable.transform.parent = null;
        _heldInteractable = null;        

        _throwableObject.ThrowObject(_throwForce * _holdInteractablePosition.forward);
        _throwableObject = null;
        int index = Random.Range(0, _throwSounds.Count);
        AudioManager.Instance.PlayEffect(_throwSounds[index], AudioManager.AudioType.SFX, true);
    }

    private void MoveInteractable() {
        if (Vector3.Distance(_heldInteractable.transform.position, _holdInteractablePosition.position) > 0.1f) {
            //Debug.Log("Moving Interactable");
            Vector3 moveDirection = _pickupForce * (_holdInteractablePosition.position - _heldInteractable.transform.position);
            _throwableObject.MoveObject(moveDirection);
        }
    }

    private void OnDrawGizmos() {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(Camera.main.transform.position, _pickupDistance * ray.direction);
    }
}
