using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
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
        [SerializeField] private float idleBobAmount;
        [SerializeField] private float idleBobSpeed;
        [SerializeField] private float moveInputDeadZone;
        [SerializeField] private float runBobAmount;
        [SerializeField] private float runBobSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float smoothTime;
        [SerializeField] private float swipeSpeedThreshold = 0.5f;
        [SerializeField] private float rotationThreshold = 20f;
        [SerializeField] private float walkBobAmount;
        [SerializeField] private float walkBobSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private Transform cameraTransform;
        private bool canLookAround = true;
        private bool canToggleDoor = true;
        private bool canToggleChest = true;
        private bool isPlayerNearby = false;
        private bool isMoving = false;
        private bool isTurning = false;
        private CharacterController characterController;
        private PlayerAnimation characterAnimation;
        private float bodyRotationY;
        private float bodyTurnSpeed = 150f;
        private float bobTimer = 0f;
        private float bodyYaw;
        private float doorToggleCooldown = 1f;
        private float halfScreenWidth;
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

        private void Awake()
        {
            animator = GetComponent<Animator>();
            characterAnimation = GetComponent<PlayerAnimation>();
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

            if (rightFingerId != -1 && canLookAround)
            {
                // Only look around if the right finger is being tracked;
                LookAround();
                Debug.Log("Rotating");
            }

            if (leftFingerId != -1)
            {
                Move();
                Debug.Log("Moving");
            }

            HandleHeadBob();
            CheckForInteractables();

            bodyYaw = Mathf.MoveTowardsAngle(bodyYaw, currentRotation.x, bodyTurnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, bodyYaw, 0);

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
                            Debug.Log("Tracking left finger");
                            moveTouchStartPosition = touch.position;
                        }
                        else if (touch.position.x > halfScreenWidth && rightFingerId == -1)
                        {
                            rightFingerId = touch.fingerId;

                            float yaw = transform.rotation.eulerAngles.y;
                            currentRotation.x = yaw;
                            targetRotation.x = yaw;

                            Debug.Log("Synced yaw rotation: " + yaw);
                        }


                        break;

                    case TouchPhase.Ended:

                    case TouchPhase.Canceled:
                        if (touch.fingerId == leftFingerId)
                        {
                            // Stop tracking the left finger
                            leftFingerId = -1;
                            Debug.Log("Stopped tracking left finger");
                            input = Vector2.zero;
                            characterAnimation.SetDirection(Vector2.zero);
                            characterAnimation.StopRunAnimation();
                        }
                        else if (touch.fingerId == rightFingerId)
                        {
                            // Stop tracking the right finger
                            rightFingerId = -1;
                            Debug.Log("Stop tracking right finger");
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

        private void TriggerTurnLeft()
        {
            StartCoroutine(PerformTurn("turnLeft"));
        }

        private void TriggerTurnRight()
        {
            StartCoroutine(PerformTurn("turnRight"));
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

        private void Move()
        {
            if (playerHealth.IsDead) return;

            if (input.sqrMagnitude <= moveInputDeadZone)
            {
                isMoving = false;
                characterAnimation.SetDirection(Vector2.zero);
                characterAnimation.SetSpeedMultiplier(1f);
                characterAnimation.SetIsRunning(false);  // Dừng state Run
                return;
            }

            isMoving = true;

            Vector2 movementInput = input.normalized;
            characterAnimation.SetDirection(movementInput);

            float inputMagnitude = input.magnitude;
            bool isRunning = inputMagnitude > 400f;

            bool isMovingStraightForward = isRunning &&
                                            movementInput.y > 0.7f &&
                                            Mathf.Abs(movementInput.x) < 0.3f;

            characterAnimation.SetIsRunning(isMovingStraightForward);

            if (!isMovingStraightForward)
            {
                float speedMultiplier = isRunning ? 1.5f : 1f;
                characterAnimation.SetSpeedMultiplier(speedMultiplier);
            }
            else
            {
                characterAnimation.SetSpeedMultiplier(1f);
            }

            float moveSpeed = isRunning ? runSpeed : walkSpeed;
            Vector2 movementDirection = movementInput * moveSpeed * Time.deltaTime;

            characterController.Move(
                transform.right * movementDirection.x + transform.forward * movementDirection.y
            );

            SoundManager.Instance.PlayFootStepSounds(isRunning);
        }


        private void HandleHeadBob()
        {
            float bobAmount = 0f;
            float bobSpeed = 0f;

            if (isMoving)
            {
                bool isRunning = false;
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

        public void DisableLookAround()
        {
            canLookAround = false;
        }
    }
}