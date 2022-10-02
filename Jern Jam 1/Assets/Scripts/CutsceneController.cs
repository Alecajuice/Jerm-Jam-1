using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using EZCameraShake;

public class CutsceneController : MonoBehaviour
{
    [SerializeField] private TimeControlWatch watch;
    [SerializeField] private float cutsceneStartTime;
    [SerializeField] private Transform playerStartPoint;
    [SerializeField] private Transform playerEndPoint;
    [SerializeField] private Animator watchBoulderAnimator;
    [SerializeField] private AudioClip landslideSound;
    [SerializeField] private AudioClip pickUpDropSound;
    [SerializeField] private AudioClip boulderFallSound;
    [SerializeField] private GameObject deathUI;

    private AudioSource _audio;

    private GameObject _player;
    private Animator _playerAnimator;
    private FirstPersonController _playerController;
    private Vector3 _playerVelocity = Vector3.zero;

    private float _timer = 0f;
    private bool _inTrigger = false;
    private bool _starting = false;
    private bool _started = false;
    private bool _ended = false;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (_starting && !_started) {
            _timer += Time.deltaTime;

            // Translate
            Vector3 nextPosition = Vector3.SmoothDamp(_player.transform.position, playerStartPoint.position, ref _playerVelocity, cutsceneStartTime);

            // Rotate view
            Quaternion currentRotation = _playerController.GetCameraRotation();
            Quaternion slerpedRotation = Quaternion.Slerp(currentRotation, playerStartPoint.rotation, _timer / cutsceneStartTime);

            _playerController.MoveToPosition(nextPosition);
            _playerController.RotateCamera(slerpedRotation);

            if (Vector3.Distance(_player.transform.position, playerStartPoint.position) < 0.01) {
                _playerAnimator.enabled = true;
                _playerAnimator.SetTrigger("CutsceneStart");
                Destroy(watch.gameObject);
                watchBoulderAnimator.SetTrigger("Fall");
                _audio.PlayOneShot(pickUpDropSound);
                CameraShaker.Instance.ShakeOnce(4f, 4f, 3f, 3f);
                _started = true;
            }
        }
        if (_started && _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !_ended) {
            _playerAnimator.enabled = false;
            _audio.PlayOneShot(boulderFallSound);
            _playerController.MoveToPosition(playerEndPoint.position);
            _playerController.RotateCamera(playerEndPoint.rotation);
            PlayerTimeControl timeControl = _player.GetComponent<PlayerTimeControl>();
            timeControl.SetEnabled(true);
            deathUI.SetActive(true);
            _ended = true;
        }
        if (_ended) {
            if (_inTrigger) {
                _playerController.EnableMove(false);
                _playerController.EnableLook(false);
            }
        }
    }

    public void StartCutscene(GameObject player) {
        _starting = true;
        _player = player;
        _playerAnimator = _player.GetComponent<Animator>();
        _playerController = _player.GetComponent<FirstPersonController>();
        _playerController.EnableMove(false);
        _playerController.EnableLook(false);
        _audio.PlayOneShot(landslideSound);
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() == _playerController) {
            _inTrigger = true;
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() == _playerController) {
            _inTrigger = false;
            deathUI.SetActive(false);
        }
    }
}
