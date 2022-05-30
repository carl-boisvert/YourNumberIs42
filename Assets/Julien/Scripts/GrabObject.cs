using UnityEngine;

//---NOTES:
//-Fade Object when grabbed (Not yet added)
//-Recenter/snap to grab with physics when x distance away from grab point / fix ReCenterGrabbedObject(). Better ways?
//-Give a dropped object the camera's velocity (and other physics parameters?) so it can keep its momentum.
//-Change AoE grab to raycast?

public class GrabObject : MonoBehaviour
{
    #region Variables

    //References
    [SerializeField] private GameObject _grabPoint = null;
    private Rigidbody _rb = null;

    //Main Parameters
    [SerializeField] private float _throwForce = 10.0f;
    [SerializeField] private float _maxGrabDistance = 2.0f;
    private float _distanceToObject = 0f;

    //Recenter
    //[SerializeField] private float _recenterTolerance = 0.1f;
    //[SerializeField] private float _recenterForce = 0.5f;
    //[SerializeField] private float _grabForce = 1.0f;

    //GetKey State
    private bool _grabbedObject = false;

    //Loop States
    private bool _runningStates = false;
    private enum _states
    {
        enterNormal,
        updateNormal,
        exitNormal,
        enterGrabbed,
        updateGrabbed,
        exitGrabbed
    }
    private _states _State = _states.enterNormal;

    #endregion

    //

    #region Main

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //_grabPoint = GameObject.FindGameObjectWithTag("GrabPoint"); //Have a GameManager singleton for these instead.
    }

    private void Update()
    {
        GetDistance();
        GetInput();
        RunStates();

        //Debug.Log($"Current State Is: {_State}!");
    }

    private void LateUpdate()
    {
        //ReCenterGrabbedObject(); //Kinda weird-acting.
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.tag != "Player" && collision.gameObject.layer != LayerMask.NameToLayer("Ground")) _State = _states.enterNormal;
    }*/

    #endregion

    //

    #region Functions

    private void GetDistance()
    {
        _distanceToObject = Vector3.Distance(transform.position, _grabPoint.transform.position);

        if (_distanceToObject >= 1.0f) _grabbedObject = false;
    }

    private void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool canGrab = false;

            if (_distanceToObject < _maxGrabDistance) canGrab = true;

            if (_grabbedObject == false && canGrab == true) _grabbedObject = true;
            else if (_grabbedObject == true) _grabbedObject = false;
            
        }
    }

    private void NormalMode()
    {
        _rb.useGravity = true;
        _rb.drag = 1;
        _rb.constraints = RigidbodyConstraints.None;
    }

    private void GrabMode()
    {
        _rb.useGravity = false;
        _rb.drag = 10;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Throw()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _rb.AddForce(_grabPoint.transform.forward * _throwForce, ForceMode.Impulse);
            _grabbedObject = false;
        }
    }

    private void ParentObject()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetParent(_grabPoint.transform);
    }

    private void UnParentObject()
    {
        Vector3 objectPosition = transform.position;
        transform.SetParent(null);
        transform.position = objectPosition;
    }

    /*private void ReCenterGrabbedObject()
    {
        if (_grabbedObject == true)
        {
            if (CompareFloat(gameObject.transform.position.magnitude, _grabPoint.transform.position.magnitude, _recenterTolerance) == false)
            {
                //Vector3 _towardGrabPoint = _grabPoint.transform.position - transform.position;
                //_rb.AddForce(_towardGrabPoint.normalized * _recenterForce, ForceMode.Force);

                //Vector3 objectMoveDirection = (_grabPoint.transform.position - transform.position);
                //_rb.AddForce(objectMoveDirection * _grabForce);

                gameObject.transform.position = _grabPoint.transform.position;
            }
        }
    }*/

    #endregion

    //

    #region Utilities

    private bool CompareFloat(float a, float b, float tolerance)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    #endregion

    //

    #region States

    private void RunStates()
    {
        if (_State == _states.enterNormal) EnterNormalState();
        if (_State == _states.updateNormal) UpdateNormalState();
        if (_State == _states.exitNormal) ExitNormalState();

        if (_State == _states.enterGrabbed) EnterGrabbedState();
        if (_State == _states.updateGrabbed) UpdateGrabbedState();
        if (_State == _states.exitGrabbed) ExitGrabbedState();
    }

    //
    //---NORMAL STATE
    //

    private void EnterNormalState()
    {
        //Debug.Log("Entering Normal State.");

        _runningStates = true;

        NormalMode();
        UnParentObject();

        _State = _states.updateNormal;
    }

    private void UpdateNormalState()
    {
        //Debug.Log("Updating Normal State.");

        if (_grabbedObject == true && _runningStates == true) _State = _states.exitNormal;
    }

    private void ExitNormalState()
    {
        //Debug.Log("Exiting Normal State.");

        _runningStates = false;
        _State = _states.enterGrabbed;
    }

    //
    //---GRABBED STATE
    //

    private void EnterGrabbedState()
    {
        //Debug.Log("Entering Grab State");

        _runningStates = true;

        GrabMode();
        ParentObject();

        _State = _states.updateGrabbed;
    }

    private void UpdateGrabbedState()
    {
        //Debug.Log("Updating Grab State");

        Throw();
        if (_grabbedObject == false && _runningStates == true) _State = _states.exitGrabbed;
    }

    private void ExitGrabbedState()
    {
        //Debug.Log("Exiting Grab State");
        
        _runningStates = false;
        _State = _states.enterNormal;
    }

    #endregion
}
