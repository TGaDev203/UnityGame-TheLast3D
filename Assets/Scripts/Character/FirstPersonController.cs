using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Button lockButton_Closed;
        [SerializeField] private Button lockButton_Opened;
        [SerializeField] private float idleBobAmount;
        [SerializeField] private float idleBobSpeed;
        [SerializeField] private float moveInputDeadZone;
        [SerializeField] private float runBobAmount;
        [SerializeField] private float runBobSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float walkBobAmount;
        [SerializeField] private float walkBobSpeed;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float animationSmoothTime;
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private float doorCheckDistance;
        [SerializeField] private float smoothTime;
        [SerializeField] private Transform cameraTransform;
        private bool isPlayerNearby = false;
        private bool isMoving = false;
        private bool isOpen = false;
        private CharacterController characterController;
        private CharacterAnimation characterAnimation;
        private float bobTimer = 0f;
        private float halfScreenWidth;
        private Vector2 currentRotation;
        private Vector2 input;
        private Vector2 lookInput;
        private Vector2 moveTouchStartPosition;
        private Vector2 rotationVelocity;
        private Vector2 targetRotation;
        private Vector3 originalCameraLocalPos;
        private Animator animator;
        private int leftFingerId, rightFingerId;
        private Transform detectedDoorLeaf = null;

        private void Awake()
        {
            characterAnimation = GetComponent<CharacterAnimation>();
            animator = GetComponent<Animator>();
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
                    cameraTransform.transform.localPosition = new Vector3(0f, 1.3f, 0.1f);
                    cameraTransform.transform.localRotation = Quaternion.identity;
                }
            }
        }

        private void Update()
        {
            GetTouchInput();

            if (rightFingerId != -1)
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
            CheckForDoor();
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

            transform.rotation = Quaternion.Euler(0, currentRotation.x, 0);
            cameraTransform.localRotation = Quaternion.Euler(currentRotation.y, 0, 0);
        }

        private void Move()
        {
            if (input.sqrMagnitude <= moveInputDeadZone)
            {
                isMoving = false;
                characterAnimation.SetDirection(Vector2.zero);
                return;
            }

            isMoving = true;

            float inputMagnitude = input.magnitude;
            bool isRunning = inputMagnitude > 400f;

            float moveSpeed = isRunning ? runSpeed : walkSpeed;

            Vector2 movementDirection = input.normalized * moveSpeed * Time.deltaTime;
            characterController.Move(
                transform.right * movementDirection.x + transform.forward * movementDirection.y
            );

            if (isRunning)
            {
                characterAnimation.PlayRunAnimation();
            }
            else
            {
                characterAnimation.StopRunAnimation();
            }

            Vector2 movementInput = input.normalized * (isRunning ? 1f : 0.5f);
            characterAnimation.SetDirection(movementInput);

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
            if (isPlayerNearby && detectedDoorLeaf != null)
            {
                Debug.Log("Trying to open door...");
                isOpen = !isOpen;

                float targetAngle = isOpen ? -90f : 0f;
                StartCoroutine(RotateDoor(targetAngle, 0.5f));

                if (isOpen)
                    SoundManager.Instance.PlayOpenDoorSound();
                else
                    SoundManager.Instance.PlayCloseDoorSound();
            }
        }

        private IEnumerator RotateDoor(float targetAngle, float duration)
        {
            Quaternion startRotation = detectedDoorLeaf.rotation;
            Quaternion endRotation = Quaternion.Euler(0f, targetAngle, 0f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                detectedDoorLeaf.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                yield return null;
            }

            detectedDoorLeaf.rotation = endRotation;
        }

        private void CheckForDoor()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, doorCheckDistance))
            {
                if (hit.collider.CompareTag("DoorWay"))
                {
                    Debug.Log("Looking at DoorWay");

                    detectedDoorLeaf = null;
                    foreach (Transform child in hit.collider.transform)
                    {
                        if (child.CompareTag("DoorLeaf"))
                        {
                            detectedDoorLeaf = child;
                            SwitchPadlock();
                            break;
                        }
                    }

                    isPlayerNearby = true;
                    return;
                }
            }

            isPlayerNearby = false;
            detectedDoorLeaf = null;
            lockButton_Closed.gameObject.SetActive(false);
            lockButton_Opened.gameObject.SetActive(false);
        }

        private void SwitchPadlock()
        {
            if (isOpen)
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
    }
}
