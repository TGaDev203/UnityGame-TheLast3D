using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraSensitivity;
        [SerializeField] private float moveInputDeadZone;
        [SerializeField] private float runSpeed;
        [SerializeField] private float walkSpeed;
        // [SerializeField] private AudioClip[] m_FootstepSounds;
        // [SerializeField] private AudioClip m_JumpSound;
        // [SerializeField] private AudioClip m_LandSound;
        private CharacterController characterController;
        private CharacterAnimation characterAnimation;
        private Vector2 moveTouchStartPosition;
        private Vector2 input;

        // Touch detection
        private float halfScreenWidth;
        private int leftFingerId, rightFingerId;

        // Camera controller
        private float cameraPitch;
        private Vector2 lookInput;

        private void Awake()
        {
            characterAnimation = GetComponent<CharacterAnimation>();
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
                            characterAnimation.StopWalkingAmimation();

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
                        if (touch.fingerId == rightFingerId)
                        {
                            lookInput = touch.deltaPosition * cameraSensitivity * Time.deltaTime;
                        }
                        else if (touch.fingerId == leftFingerId)
                        {
                            // Caculating the position delta from the start position
                            input = touch.position - moveTouchStartPosition;
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
            // Vertial (pitch) rotation
            cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -60f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

            // Hoizontal (yaw) rotation
            transform.Rotate(transform.up, lookInput.x);
        }

        // private void PlayLandingSound()
        // {
        //     m_AudioSource.clip = m_LandSound;
        //     m_AudioSource.Play();
        // }

        // private void PlayJumpSound()
        // {
        //     m_AudioSource.clip = m_JumpSound;
        //     m_AudioSource.Play();
        // }

        // private void PlayFootStepAudio()
        // {
        //     if (!m_CharacterController.isGrounded)
        //     {
        //         return;
        //     }
        //     // pick & play a random footstep sound from the array,
        //     // excluding sound at index 0
        //     int n = Random.Range(1, m_FootstepSounds.Length);
        //     m_AudioSource.clip = m_FootstepSounds[n];
        //     m_AudioSource.PlayOneShot(m_AudioSource.clip);
        //     // move picked sound to index 0 so it's not picked next time
        //     m_FootstepSounds[n] = m_FootstepSounds[0];
        //     m_FootstepSounds[0] = m_AudioSource.clip;
        // }

        private void Move()
        {
            // Do not move if the touch delta is shorter than the designated dead zone
            if (input.sqrMagnitude <= moveInputDeadZone)
            {
                // characterAnimation.StopWalkingAmimation();
                return;
            }

            // Multiply the normalized direction by the speed
            Vector2 movementDirection = input.normalized * walkSpeed * Time.deltaTime;

            // Move relatively to the local transform's direction
            characterController.Move(transform.right * movementDirection.x + transform.forward * movementDirection.y);
            characterAnimation.PlayWalkingAnimation();
        }
    }
}
