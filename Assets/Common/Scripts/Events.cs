
public static class Events
{
    public delegate void PatientNumberChanged(int patientNumber);
    public static PatientNumberChanged OnPatientNumberChanged;
    
    public delegate void ExecuteEvent(string eventId);
    public static ExecuteEvent OnExecuteEvent;

    #region UI Events
    public delegate void BloomChange(float value);
    public static BloomChange OnBloomChange;

    public delegate void LookSensitivityChange(float value);
    public static LookSensitivityChange OnLookSensitivityChange;

    public delegate void InvertedControlsChange(bool state);
    public static InvertedControlsChange OnInvertedControlsChange;

    public delegate void CameraShakechange(bool state);
    public static CameraShakechange OnCameraShakeChange;

    public delegate void FOVChange(float value);
    public static FOVChange OnFOVChange;

    public delegate void HoldProgress(float progress);
    public static HoldProgress OnHoldProgress;
    #endregion
}
