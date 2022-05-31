
public static class Events
{
    public delegate void PatientNumberChanged(int patientNumber);
    public static PatientNumberChanged OnPatientNumberChanged;
}
