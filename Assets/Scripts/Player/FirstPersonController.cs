using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Button lockButton_Closed;
        [SerializeField] private Button lockButton_Opened;
        [SerializeField] private float animationSmoothTime;
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private float chestCheckDistance;
        [SerializeField] private float doorCheckDistance;
        [SerializeField] private float fallMultiplier;
        [SerializeField] private float groundCheckRadius;
        [SerializeField] private float gravity;
        [SerializeField] private float idleBobAmount;
        [SerializeField] private float idleBobSpeed;
        [SerializeField] private float jumpForce;
        [SerializeField] private float moveInputDeadZone;
        [SerializeField] private float runBobAmount;
        [SerializeField] private float runBobSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float smoothTime;
        [SerializeField] private float swipeSpeedThreshold;
        [SerializeField] private float rotationThreshold;
        [SerializeField] private float walkBobAmount;
        [SerializeField] private float walkBobSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform groundCheck;
        private bool canLookAround = true;
        private bool canToggleDoor = true;
        private bool canToggleChest = true;
        private bool hasStarted = false;
        private bool isPlayerNearby = false;
        private bool isTurning = false;
        private bool isMoving = false;
        private bool isJumping = false;
        private bool isRunning = false;
        private bool wasGrounded = false;
        private bool wasGroundedLastFrame = false;
        private CharacterController characterController;
        private PlayerAnimationController playerAnim;
        private float bodyRotationY;
        private float bodyTurnSpeed = 150f;
        private float bobTimer = 0f;
        private float bodyYaw;
        private float doorToggleCooldown = 1f;
        private const float doubleTapThreshold = 0.3f;
        private float halfScreenWidth;
        private float lastTapTime = 0f;
        private Vector3 verticalVelocity;
        private Vector2 currentRotation;
        private Vector2 input;
        private Vector2 lookInput;
        private Vector2 moveTouchStartPosition;
        private Vector2 rotationVelocity;
        private Vector2 targetRotation;
        private Vector3 originalCameraLocalPos;
        private Animator animator;
        private DoorController detectedDoor;
        private ChestController detectedChest;
        private int leftFingerId, rightFingerId;
        private int turnLayerIndex;
        private PlayerHealth playerHealth;

        public void DisableLookAround() => canLookAround = false;
        private void TriggerTurnRight() => StartCoroutine(PerformTurn("turnRight"));
        private void TriggerTurnLeft() => StartCoroutine(PerformTurn("turnLeft"));
        private bool IsGrounded() => Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerAnim = GetComponent<PlayerAnimationController>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            // Id = -1 means the finger is not being tracked
            leftFingerId = -1;
            rightFingerId = -1;

            // Only calculate once
            halfScreenWidth = Screen.width / 2;

            characterController = GetComponent<CharacterController>();

            // Calculate the movement input dead zone
            moveInputDeadZone = Mathf.Pow(Screen.height / moveInputDeadZone, 2);

            originalCameraLocalPos = cameraTransform.localPosition;

            if (animator != null && animator.isHuman)
            {
                Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (hips != null)
                {
                    cameraTransform.transform.SetParent(hips);
                    cameraTransform.transform.localRotation = Quaternion.identity;
                }
            }

            bodyRotationY = transform.rotation.eulerAngles.y;
            turnLayerIndex = animator.GetLayerIndex("Upper Body Layer");

            float yaw = transform.rotation.eulerAngles.y;

            currentRotation = new Vector2(yaw, 0);
            targetRotation = currentRotation;

            bodyRotationY = bodyYaw;
            bodyYaw = yaw;
        }

        private void Update()
        {
            GetTouchInput();

            bool isGrounded = IsGrounded();
            if (hasStarted && !wasGrounded && isGrounded)
            {
                SoundManager.Instance.PlayLandSound();
            }

            wasGrounded = isGrounded;
            if (rightFingerId != -1 && canLookAround)
            {
                LookAround();
            }

            if (leftFingerId == -1)
            {
                isMoving = false;
                playerAnim.SetDirection(Vector2.zero);
                playerAnim.SetIsRunning(false);
            }

            HandleHeadBob();
            CheckForInteractables();

            bodyYaw = Mathf.MoveTowardsAngle(bodyYaw, currentRotation.x, bodyTurnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, bodyYaw, 0);

            if (characterController.isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }
            else if (verticalVelocity.y < 0)
            {
                verticalVelocity.y += gravity * fallMultiplier * Time.deltaTime;
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
            }

            Vector3 horizontalMovement = GetMovementVector();
            Vector3 totalMovement = horizontalMovement + verticalVelocity * Time.deltaTime;

            characterController.Move(totalMovement.sqrMagnitude > 0.0001f ? totalMovement : Vector3.zero);

            CheckGroundTag();
            HandleLandingState();

            hasStarted = true;
        }

        private void GetTouchInput()
        {
            // Iterate through all the detected touches
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // Check each touch's phase
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (touch.position.x < halfScreenWidth && leftFingerId == -1)
                        {
                            leftFingerId = touch.fingerId;
                            // #if UNITY_EDITOR
                            // Debug.Log("Tracking left finger");
                            // #endif
                            moveTouchStartPosition = touch.position;
                        }
                        else if (touch.position.x > halfScreenWidth && rightFingerId == -1)
                        {
                            rightFingerId = touch.fingerId;

                            float yaw = transform.rotation.eulerAngles.y;
                            currentRotation.x = yaw;
                            targetRotation.x = yaw;

                            if (Time.time - lastTapTime <= doubleTapThreshold && lastTapTime != 0f)
                            {
                                Jump();
                                lastTapTime = 0f;
                            }
                            else
                            {
                                lastTapTime = Time.time;
                            }

                            // Debug.Log("Synced yaw rotation: " + yaw);
                        }

                        break;

                    case TouchPhase.Ended:

                    case TouchPhase.Canceled:
                        if (touch.fingerId == leftFingerId)
                        {
                            // Stop tracking the left finger
                            leftFingerId = -1;
                            // Debug.Log("Stopped tracking left finger");
                            input = Vector2.zero;
                            playerAnim.SetDirection(Vector2.zero);
                            playerAnim.SetIsRunning(false);
                        }
                        else if (touch.fingerId == rightFingerId)
                        {
                            // Stop tracking the right finger
                            rightFingerId = -1;
                            // Debug.Log("Stop tracking right finger");
                        }

                        break;

                    case TouchPhase.Moved:
                        // Get input for looking around
                        if (touch.fingerId == leftFingerId)
                        {
                            // Caculating the position delta from the start position
                            input = touch.position - moveTouchStartPosition;
                        }
                        else if (touch.fingerId == rightFingerId)
                        {
                            lookInput = touch.deltaPosition * cameraSensitivity * Time.deltaTime;
                        }

                        break;

                    case TouchPhase.Stationary:
                        // Set the look input to zero if the finger is still
                        if (touch.fingerId == rightFingerId)
                        {
                            lookInput = Vector2.zero;
                        }
                        break;
                }
            }
        }

        private Vector3 GetMovementVector()
        {
            if (playerHealth.IsDead || input.sqrMagnitude <= moveInputDeadZone)
            {
                playerAnim.SetIsRunning(false);
                return Vector3.zero;
            }

            isMoving = true;

            Vector2 movementInput = input.normalized;
            isRunning = input.magnitude > 400f;

            playerAnim.SetDirection(movementInput);

            bool isMovingStraightForward = isRunning &&
                                            movementInput.y > 0.7f &&
                                            Mathf.Abs(movementInput.x) < 0.3f;

            float moveSpeed = isRunning ? runSpeed : walkSpeed;

            if (isJumping)
            {
                moveSpeed *= 0.9f;
                playerAnim.SetIsRunning(false);
            }
            else
            {
                bool shouldRun = isMovingStraightForward;
                playerAnim.SetIsRunning(shouldRun);

                float speedMultiplier = 1f;
                if (!shouldRun && isRunning)
                    speedMultiplier = 1.5f;

                playerAnim.SetSpeedMultiplier(speedMultiplier);
            }

            Vector2 movementDirection = movementInput * moveSpeed * Time.deltaTime;
            SoundManager.Instance.PlayFootStepSounds(isRunning && !isJumping);

            return transform.right * movementDirection.x + transform.forward * movementDirection.y;
        }

        private IEnumerator PerformTurn(string triggerName)
        {
            isTurning = true;
            animator.SetLayerWeight(turnLayerIndex, 1);
            animator.SetTrigger(triggerName);

            yield return new WaitForSeconds(0.5f);

            animator.SetLayerWeight(turnLayerIndex, 0);

            bodyRotationY = currentRotation.x;

            isTurning = false;
        }

        private void Jump()
        {
            if (IsGrounded() && !playerHealth.IsDead)
            {
                isJumping = true;
                verticalVelocity.y = jumpForce;

                Vector2 movementInput = input.normalized;

                int jumpType = 1;

                if (movementInput.y > 0.5f && Mathf.Abs(movementInput.x) < 0.3f)
                {
                    jumpType = 2;
                }
                else if (movementInput.y < -0.3f)
                {
                    jumpType = 3;
                }

                playerAnim.SetJumpType(jumpType);
                SoundManager.Instance.PlayJumpSound();

                // Debug.Log("Jump triggered! Type: " + jumpType);
            }
        }

        private string CheckGroundTag()
        {
            BoxCollider box = groundCheck.GetComponent<BoxCollider>();
            if (box == null)
            {
                // Debug.LogWarning("GroundCheck doesn't have a BoxCollider!");
                return null;
            }

            Vector3 boxCenter = groundCheck.position + box.center;
            Vector3 boxSize = Vector3.Scale(box.size, groundCheck.lossyScale);

            Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, groundCheck.rotation);

            bool foundStandable = false;

            foreach (var hit in hits)
            {
                if (hit.gameObject != gameObject)
                {
                    // Debug.Log($"Ground tag: {hit.tag}");

                    if (hit.CompareTag("Standable"))
                    {
                        foundStandable = true;
                        playerAnim.SetJumpType(0);
                        break;
                    }
                }
            }

            return foundStandable ? "Standable" : null;
        }

        private void HandleLandingState()
        {
            bool isGroundedNow = IsGrounded();

            if (isJumping && isGroundedNow && !wasGroundedLastFrame)
            {
                isJumping = false;
                playerAnim.SetJumpType(0);
                // Debug.Log("Landed!");
            }

            if (!isGroundedNow)
            {
                playerAnim.SetIsRunning(false);
            }

            wasGroundedLastFrame = isGroundedNow;
        }

        private void LookAround()
        {
            targetRotation.x += lookInput.x * cameraSensitivity;
            targetRotation.y -= lookInput.y * cameraSensitivity;
            targetRotation.y = Mathf.Clamp(targetRotation.y, -90f, 90f);

            currentRotation.x = Mathf.SmoothDamp(
                currentRotation.x,
                targetRotation.x,
                ref rotationVelocity.x,
                smoothTime
            );
            currentRotation.y = Mathf.SmoothDamp(
                currentRotation.y,
                targetRotation.y,
                ref rotationVelocity.y,
                smoothTime
            );

            cameraTransform.localRotation = Quaternion.Euler(currentRotation.y, 0, 0);

            float angleDifference = Mathf.DeltaAngle(bodyRotationY, currentRotation.x);
            float swipeSpeed = Mathf.Abs(lookInput.x);
            bool isPlayerIdle = input.sqrMagnitude <= moveInputDeadZone;

            if (!isTurning && isPlayerIdle)
            {
                if (Mathf.Abs(angleDifference) > rotationThreshold || swipeSpeed > swipeSpeedThreshold)
                {
                    if (lookInput.x > 0)
                        TriggerTurnRight();
                    else if (lookInput.x < 0)
                        TriggerTurnLeft();
                }
            }
        }

        private void HandleHeadBob()
        {
            float bobAmount = 0f;
            float bobSpeed = 0f;

            if (isMoving)
            {
                bobAmount = isRunning ? runBobAmount : walkBobAmount;
                bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
            }
            else
            {
                bobAmount = idleBobAmount;
                bobSpeed = idleBobSpeed;
            }

            bobTimer += Time.deltaTime * bobSpeed;

            float yOffset = Mathf.Sin(bobTimer) * bobAmount;
            float xOffset = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;

            cameraTransform.localPosition =
                originalCameraLocalPos + new Vector3(xOffset, yOffset, 0);
        }

        private void CheckForInteractables()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Max(doorCheckDistance, chestCheckDistance)))
            {
                DoorController door = hit.collider.GetComponentInParent<DoorController>();
                if (door != null && Vector3.Distance(cameraTransform.position, hit.point) <= doorCheckDistance)
                {
                    detectedDoor = door;
                    detectedChest = null;
                    isPlayerNearby = true;
                    SwitchPadlock();
                    return;
                }

                ChestController chest = hit.collider.GetComponentInParent<ChestController>();
                if (chest != null && Vector3.Distance(cameraTransform.position, hit.point) <= chestCheckDistance)
                {
                    detectedChest = chest;
                    detectedDoor = null;
                    isPlayerNearby = true;
                    SwitchPadlock();
                    return;
                }
            }

            isPlayerNearby = false;
            detectedDoor = null;
            detectedChest = null;
            lockButton_Closed?.gameObject.SetActive(false);
            lockButton_Opened?.gameObject.SetActive(false);
        }

        public void OpenDoor()
        {
            if (isPlayerNearby && detectedDoor != null && canToggleDoor)
            {
                detectedDoor.ToggleDoor();
                SwitchPadlock();
                StartCoroutine(DoorToggleCooldown());
            }
        }

        private IEnumerator DoorToggleCooldown()
        {
            canToggleDoor = false;
            yield return new WaitForSeconds(doorToggleCooldown);
            canToggleDoor = true;
        }

        private void SwitchPadlock()
        {
            if (detectedDoor != null && detectedDoor.IsOpen)
            {
                lockButton_Opened.gameObject.SetActive(true);
                lockButton_Closed.gameObject.SetActive(false);
            }
            else
            {
                lockButton_Opened.gameObject.SetActive(false);
                lockButton_Closed.gameObject.SetActive(true);
            }
        }

        public void OpenChest()
        {
            if (isPlayerNearby && detectedChest != null && canToggleChest)
            {
                detectedChest.ToggleChest();
                StartCoroutine(ChestToggleCooldown());
            }
        }

        private IEnumerator ChestToggleCooldown()
        {
            canToggleChest = false;
            yield return new WaitForSeconds(doorToggleCooldown);
            canToggleChest = true;
        }

        public void SetBobAmountValue(float value)
        {
            idleBobAmount = value;
            walkBobAmount = value;
        }
    }
}