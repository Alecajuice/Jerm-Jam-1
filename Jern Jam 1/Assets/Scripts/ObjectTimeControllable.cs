using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class ObjectTimeControllable : MonoBehaviour
{
    public static Color outlineControlling = new Color(0, 212, 0);
    public static Color outlinePaused = new Color(227, 216, 0);

    [SerializeField] private LineRenderer _pastPath = null;
    [SerializeField] private LineRenderer _futurePath = null;

    private ObjectGrabbable _grabbable;
    private Outline _outline;

    private Rigidbody _rb; // Used for objects
    private FirstPersonController _controller; // Used for player

    private List<Pose> _history;
    private bool _controlling = false;
    private float _lookSum = 0f;
    private bool _colliding = false;

    private LayerMask _tempLayerMask;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controller = GetComponent<FirstPersonController>();
        _grabbable = GetComponent<ObjectGrabbable>();
        _outline = GetComponentInChildren<Outline>();
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _history = new List<Pose>();
        _history.Add(new Pose(transform.position, transform.rotation));
        AddToPath(_pastPath, transform.position);
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (!_controlling) {
            // Add to history
            float minDistance = 0.1f;
            float minAngle = 5; // degrees
            Pose lastPose = _history[_history.Count - 1];
            Quaternion rotation = _controller == null ? transform.rotation : _controller.GetCameraRotation();
            if (Vector3.Distance(transform.position, lastPose.position) >= minDistance ||
                Quaternion.Angle(rotation, lastPose.rotation) >= minAngle)
            {
                _history.Add(new Pose(transform.position, rotation));
                AddToPath(_pastPath, transform.position);
            }
        }
    }

    private void AddToPath(LineRenderer path, Vector3 position) {
        if (path == null) return;
        path.positionCount++;
        path.SetPosition(path.positionCount - 1, position);
    }

    public void StartTimeControl() {
        _controlling = true;
        _lookSum = 0f;
        if (_rb != null) {
            _rb.useGravity = false;
            _rb.isKinematic = true;
        }
        _tempLayerMask = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("TimeControlled");
    }

    public float TimeControl(float lookX) {
        float seekSpeed = 1f;
        _lookSum += lookX * seekSpeed;
        _lookSum = Mathf.Min(_lookSum, 0f);
        _lookSum = Mathf.Max(_lookSum, -_history.Count + 1);

        // Update object pose
        int curIndex = _history.Count - 1 + Mathf.FloorToInt(_lookSum);
        if (_controller != null) {
            // Player must be moved using the controller
            _controller.MoveToPosition(_history[curIndex].position);
            _controller.RotateCamera(_history[curIndex].rotation); // TODO: slerp this?
        } else {
            transform.position = _history[curIndex].position;
            transform.rotation = _history[curIndex].rotation;
        }

        // Update past path
        int i = _pastPath.positionCount;
        _pastPath.positionCount = curIndex + 1;
        for (; i < _pastPath.positionCount; i++)
            _pastPath.SetPosition(i, _history[i].position);
            
        // Update future path
        int j = _futurePath.positionCount;
        _futurePath.positionCount = _history.Count - curIndex;
        for (; j < _futurePath.positionCount; j++)
            _futurePath.SetPosition(j, _history[_history.Count - 1 - j].position);

        return (float)curIndex / (float)_history.Count;
    }

    public bool ReleaseTimeControl() {
        if (_colliding) return false;
        int curIndex = _history.Count - 1 + Mathf.FloorToInt(_lookSum);
        _history.RemoveRange(curIndex + 1, _history.Count - curIndex - 1);
        _controlling = false;
        _lookSum = 0f;
        if (_rb != null) {
            _rb.useGravity = true;
            _rb.isKinematic = false;
        }
        _futurePath.positionCount = 0;
        gameObject.layer = _tempLayerMask;
        return true;
    }

    public bool IsBeingControlled() {
        return _controlling;
    }

    public float GetCurrentTimelineScale() {
        if (_history.Count == 1) return 1;
        int curIndex = _history.Count - 1 + Mathf.FloorToInt(_lookSum);
        return (float)curIndex / (float)(_history.Count - 1);
    }

    public void SetOutline(Color color) {
        if (_outline == null) return;
        _outline.OutlineColor = color;
        _outline.enabled = true;
    }

    public void DisableOutline() {
        if (_outline == null) return;
        _outline.enabled = false;
    }

    public void EnablePaths() {
        if (_pastPath != null) _pastPath.enabled = true;
        if (_futurePath != null) _futurePath.enabled = true;
    }

    public void DisablePaths() {
        if (_pastPath != null) _pastPath.enabled = false;
        if (_futurePath != null) _futurePath.enabled = false;
    }

    // /// <summary>
    // /// OnCollisionEnter is called when this collider/rigidbody has begun
    // /// touching another rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnCollisionEnter(Collision other)
    // {
    //     if (GetComponentInChildren<Collider>().bounds.Intersects(other.collider.bounds)) {
    //         Debug.Log("intersecting");
    //         _colliding = true;
    //     }
    // }

    // /// <summary>
    // /// OnCollisionStay is called once per frame for every collider/rigidbody
    // /// that is touching rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnCollisionStay(Collision other)
    // {
    //     if (GetComponentInChildren<Collider>().bounds.Intersects(other.collider.bounds)) {
    //         if (!_colliding) Debug.Log("intersecting");
    //         _colliding = true;
    //     } else {
    //         if (_colliding) Debug.Log("exit collider");
    //         _colliding = false;
    //     }
    // }

    // /// <summary>
    // /// OnCollisionExit is called when this collider/rigidbody has
    // /// stopped touching another rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnCollisionExit(Collision other)
    // {
    //     // if (!GetComponentInChildren<Collider>().bounds.Intersects(other.collider.bounds)) {
    //         Debug.Log("exit collider");
    //         _colliding = false;
    //     // }
    // }
}
