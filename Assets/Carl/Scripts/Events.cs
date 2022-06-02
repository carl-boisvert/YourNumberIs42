
public static class Events
{
    public delegate void PatientNumberChanged(int patientNumber);
    public static PatientNumberChanged OnPatientNumberChanged;
    
    public delegate void ExecuteEvent(string eventId);
    public static ExecuteEvent OnExecuteEvent;
}
