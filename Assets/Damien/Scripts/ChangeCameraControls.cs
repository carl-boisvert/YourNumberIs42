using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class ChangeCameraControls : MonoBehaviour
{
    [SerializeField] private bool _listenToFovChanges = true;
    private CinemachineVirtualCamera _cam = null;
    private CinemachinePOV _aimControls = null;

    private float _fov;
    private float _aimedFOV;
    private float _verticalSensitivity;
    private float _aimedVerticalSensitivity;
    private float _horizontalSensitivity;
    private float _aimedHorizontalSensitivity;
    
    private void Start() {
        InitCam();
        Events.OnInvertedControlsChange += ChangeInvertedControls;
        Events.OnLookSensitivityChange += ChangeLookSensitity;
        if (_listenToFovChanges) {
            Events.OnFOVChange += ChangeFOV;
        }
    }

    private void OnDestroy() {
        Events.OnInvertedControlsChange -= ChangeInvertedControls;
        Events.OnLookSensitivityChange -= ChangeLookSensitity;
        if (_listenToFovChanges) {
            Events.OnFOVChange -= ChangeFOV;
        }
    }

    private void InitCam() {
        _cam = GetComponent<CinemachineVirtualCamera>();
        _aimControls = _cam.GetCinemachineComponent<CinemachinePOV>();

        if (_listenToFovChanges)
        {
            _fov = PreferenceManager.Instance.GetFloatPref("FieldOfView"); 
            _aimedFOV = _fov - 20;
        }

        _verticalSensitivity = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
        _aimedVerticalSensitivity = _verticalSensitivity / 3;
        _horizontalSensitivity = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
        _aimedHorizontalSensitivity = _horizontalSensitivity / 3;

        _aimControls.m_VerticalAxis.m_InvertInput = PreferenceManager.Instance.GetBoolPref("InvertedControls");
        _aimControls.m_VerticalAxis.m_MaxSpeed = _verticalSensitivity;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = _horizontalSensitivity;
        if (_listenToFovChanges)
        {
            _cam.m_Lens.FieldOfView = _fov;
        }
    }

    private void ChangeInvertedControls(bool state) {
        _aimControls.m_VerticalAxis.m_InvertInput = state;
    }

    private void ChangeLookSensitity(float value)
    {
        _verticalSensitivity = value;
        _horizontalSensitivity = value;
        _aimedVerticalSensitivity = _verticalSensitivity / 3;
        _aimedHorizontalSensitivity = _horizontalSensitivity / 3;
        
        _aimControls.m_VerticalAxis.m_MaxSpeed = _verticalSensitivity;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = _horizontalSensitivity;
    }

    private void ChangeFOV(float value)
    {
        if (_listenToFovChanges)
        {
            _fov = value;
            _cam.m_Lens.FieldOfView = _fov;
        }
    }

    public void SetRestFOV()
    {
        if (_listenToFovChanges)
        {
            _cam.m_Lens.FieldOfView = _fov;
        }
    }
    
    public void SetRestSensitivity()
    {
        _aimControls.m_VerticalAxis.m_MaxSpeed = _verticalSensitivity;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = _horizontalSensitivity;
    }
    
    public void SetAimedFOV()
    {
        if (_listenToFovChanges)
        {
            _cam.m_Lens.FieldOfView = _aimedFOV;
        }
    }
    
    public void SetAimedSensitivity()
    {
        _aimControls.m_VerticalAxis.m_MaxSpeed = _aimedVerticalSensitivity;
        _aimControls.m_HorizontalAxis.m_MaxSpeed = _aimedHorizontalSensitivity;
    }
}
