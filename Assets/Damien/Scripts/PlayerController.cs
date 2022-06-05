using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody), typeof(PlayerAudio))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera _mainCam = null;

    [Header("Movement")]
    [SerializeField] [Range(1f, 20f)] private float _movementSpeed = 10f;
    [SerializeField] [Range(0f, 2f)] private float _sprintMultiplier = 1.5f;
    private Rigidbody _rigidbody = null;
    private Vector3 _moveDirection = Vector3.zero;
    private bool _canMove = true;

    [Header("Audio")]
    private PlayerAudio _audio = null;
    #endregion

    #region Unity Events
    private void Awake() {
        Init();
    }
    private void OnEnable() {
        Events.OnPauseGame += CanPlayerMove;
    }

    private void OnDisable() {
        Events.OnPauseGame -= CanPlayerMove;
    }    

    private void Update() {
        _moveDirection = InputManager.MoveDirection;
    }

    private void FixedUpdate() {
        if (_moveDirection == Vector3.zero || !_canMove) {
            return;
        }

        MovePlayer();
    }

    private void LateUpdate() {
        if(InputManager.LookDelta == Vector2.zero) {
            return;
        }

        RotatePlayer();
    }
    #endregion

    #region Private Methods
    private void Init() {
        //Get references
        _rigidbody = GetComponent<Rigidbody>();
        _audio = GetComponent<PlayerAudio>();

        //Set default
        _moveDirection = InputManager.MoveDirection;
    }

    private void CanPlayerMove(bool state) {
        _canMove = state;
    }

    private void MovePlayer() {
        //Debug.Log(_moveDirection);

        float currentSpeed = _movementSpeed;
        Vector3 forceDirection = _moveDirection.x * transform.right + _moveDirection.z * transform.forward;

        if (InputManager.IsSprintPressed) {
            currentSpeed *= _sprintMultiplier;
            _audio.PlayPlayerSFX(PlayerAudioType.Sprinting);
        }
        else {
            _audio.PlayPlayerSFX(PlayerAudioType.Walking);
        }

        _rigidbody.AddForce(currentSpeed * forceDirection, ForceMode.Force);
    }

    private void RotatePlayer() {
        //Debug.Log(_mainCam.name + _mainCam.transform.eulerAngles.y);
        transform.rotation = Quaternion.Euler(0f, _mainCam.transform.eulerAngles.y, 0f);
    }
    #endregion
}
