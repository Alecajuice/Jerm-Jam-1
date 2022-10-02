using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;
		public float MaxStepHeight = 0.35f;
		public float LookAheadRange = 0.85f;
		public float SlopeLimit = 60f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// enable controls
		private bool enableLook = true;
		private bool enableMove = true;
	
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		public void EnableLook(bool enableLook) {
			this.enableLook = enableLook;
		}

		public void EnableMove(bool enableMove) {
			this.enableMove = enableMove;
		}

		public Quaternion GetCameraRotation() {
			return Quaternion.Euler(_cinemachineTargetPitch, transform.rotation.eulerAngles.y, 0f);
		}

		public void RotateCamera(Quaternion rotation) {
            transform.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
			_cinemachineTargetPitch = rotation.eulerAngles.x;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Update Cinemachine camera target pitch
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
		}

		public void MoveToPosition(Vector3 target) {
			_controller.Move(target - transform.position);
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold && enableLook)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// can't move
			if (!enableMove) return;

			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			Vector3 movementVector = inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;
			if (Grounded) {
				// CheckForStepUp(ref movementVector);
				CheckForSlope(ref movementVector);
			}

			// move the player
			_controller.Move(movementVector);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private void CheckForSlope(ref Vector3 movementVector)
		{
			Vector3 lookAheadStartPoint = transform.position + 0.1f * Vector3.up;
			Vector3 lookAheadDirection = movementVector;
			lookAheadDirection.y = 0;
			lookAheadDirection = lookAheadDirection.normalized;
			float lookAheadDistance = LookAheadRange;

			// check if there is a potential step ahead
			RaycastHit hit;
			if (Physics.Raycast(lookAheadStartPoint, lookAheadDirection, out hit, lookAheadDistance, 
								GroundLayers, QueryTriggerInteraction.Ignore))
			{
				lookAheadStartPoint = transform.position + Vector3.up * MaxStepHeight;
				float angle = Vector3.Angle(hit.normal, Vector2.up);
				if (angle <= SlopeLimit) {
					Vector3 moveDirection = (hit.point - transform.position).normalized;
					Vector3 horizontalMoveDirection = moveDirection;
					horizontalMoveDirection.y = 0f;
					movementVector = moveDirection * (movementVector.magnitude / horizontalMoveDirection.magnitude);
				}
				// // check if there is clear space above the step
				// if (!Physics.Raycast(lookAheadStartPoint, lookAheadDirection, lookAheadDistance,
				// 					GroundLayers, QueryTriggerInteraction.Ignore))
				// {
				// 	Vector3 candidatePoint = lookAheadStartPoint + lookAheadDirection * lookAheadDistance;

				// 	// check the surface of the step
				// 	RaycastHit hitResult;
				// 	if (Physics.Raycast(candidatePoint, Vector3.down, out hitResult, MaxStepHeight * 2f,
				// 						GroundLayers, QueryTriggerInteraction.Ignore))
				// 	{
				// 		// is the step shallow enough in slope
				// 		if (Vector3.Angle(Vector3.up, hitResult.normal) <= SlopeLimit)
				// 		{
				// 			
				// 		}
				// 	}
				// }
			}
		}

		// private void CheckForStepUp(ref Vector3 movementVector)
		// {
		// 	Vector3 lookAheadStartPoint = transform.position + 0.01f * Vector3.up;
		// 	Vector3 lookAheadDirection = movementVector;
		// 	lookAheadDirection.y = 0;
		// 	lookAheadDirection = lookAheadDirection.normalized;
		// 	float lookAheadDistance = LookAheadRange;

		// 	// check if there is a potential step ahead
		// 	if (Physics.Raycast(lookAheadStartPoint, lookAheadDirection, lookAheadDistance, 
		// 						GroundLayers, QueryTriggerInteraction.Ignore))
		// 	{
		// 		lookAheadStartPoint = transform.position + Vector3.up * MaxStepHeight;

		// 		// check if there is clear space above the step
		// 		if (!Physics.Raycast(lookAheadStartPoint, lookAheadDirection, lookAheadDistance,
		// 							GroundLayers, QueryTriggerInteraction.Ignore))
		// 		{
		// 			Vector3 candidatePoint = lookAheadStartPoint + lookAheadDirection * lookAheadDistance;

		// 			// check the surface of the step
		// 			RaycastHit hitResult;
		// 			if (Physics.Raycast(candidatePoint, Vector3.down, out hitResult, MaxStepHeight * 2f,
		// 								GroundLayers, QueryTriggerInteraction.Ignore))
		// 			{
		// 				// is the step shallow enough in slope
		// 				if (Vector3.Angle(Vector3.up, hitResult.normal) <= SlopeLimit)
		// 				{
		// 					movementVector = (hitResult.point - transform.position).normalized * movementVector.magnitude;
		// 				}
		// 			}
		// 		}
		// 	}
		// }

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			// if (lfAngle < -360f) lfAngle += 360f;
			// if (lfAngle > 360f) lfAngle -= 360f;
			if (lfAngle < -180f) lfAngle += 360f;
			if (lfAngle > 180f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}