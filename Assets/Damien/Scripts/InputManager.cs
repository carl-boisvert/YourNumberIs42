using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour {

    #region Variables
    private static InputManager instance;
    public static InputManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<InputManager>();
                if (instance == null) {
                    GameObject managerClone = new GameObject();
                    managerClone.AddComponent<InputManager>();
                    managerClone.name = "InputManager";
                }
            }
            return instance;
        }
    }

    private PlayerInput _playerInput;

    public static Vector2 LookDelta {get; private set;}
    public static Vector3 MoveDirection { get; private set; }
    public static bool IsSprintPressed { get; private set; }
    public static bool IsCrouchPressed { get; private set; }
    public static bool IsInteractPressed { get; private set; }
    #endregion

    #region Unity Events
    private void Awake() {
        instance = this;
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start() {
        LookDelta = Vector2.zero;
        MoveDirection = Vector3.zero;
    }

    private void OnEnable() {
        _playerInput.actions.Enable();
    }

    private void OnDisable() {
        _playerInput.actions.Disable();
    }
    #endregion

    #region Public Methods
    public void SwitchMap(string map) {
        _playerInput.SwitchCurrentActionMap(map);
        Debug.Log(_playerInput.currentActionMap);
    }
    #endregion

    #region PlayerInput Events
    public void Move(InputAction.CallbackContext context) {
        if (context.performed) {
            Vector2 vec = context.ReadValue<Vector2>();
            MoveDirection = new Vector3(vec.x, 0, vec.y);
        }
        else if (context.canceled) {
            MoveDirection = Vector3.zero;
        }
    }

    public void Sprint(InputAction.CallbackContext context) {
        if (context.performed) {
            IsSprintPressed = true;
        }
        else if (context.canceled) {
            IsSprintPressed = false;
        }
    }

    public void Crouch(InputAction.CallbackContext context) {
        if (context.performed) {
            IsCrouchPressed = true;
        }
        else if (context.canceled) {
            IsCrouchPressed = false;
        }
    }
    
    public void Look(InputAction.CallbackContext context) {
        if (context.performed) {
            LookDelta = context.ReadValue<Vector2>();
        }
        else if (context.canceled) {
            LookDelta = Vector2.zero;
        }
    }

    public void Interact(InputAction.CallbackContext context) {
        if (context.started) {
            IsInteractPressed = true;
        }
        else if (context.canceled) {
            IsInteractPressed = false;
        }
    }

    public void Pause(InputAction.CallbackContext context) {
        if (context.started) {
            Debug.Log("Trigger UIManager Pause Menu");
        }
    }
    #endregion
}
