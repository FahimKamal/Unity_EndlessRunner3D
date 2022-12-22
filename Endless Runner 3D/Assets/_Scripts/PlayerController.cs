using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace TempleRun.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float initialPlayerSpeed = 4.0f;
        [SerializeField] private float maximumPlayerSpeed = 30.0f;
        [SerializeField] private float playerSpeedIncreaseRate = 0.1f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float initialGravityValue = -9.81f;

        [SerializeField] private float scoreMultiplier = 10f;
        
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip slidingAnimationClip;


        [SerializeField] private float playerSpeed;
        private float _gravity;
        private Vector3 _movementDirection = Vector3.forward;
        private Vector3 _playerVelocity;

        private PlayerInput _playerInput;
        private InputAction _turnAction;
        private InputAction _jumpAction;
        private InputAction _slideAction;

        private CharacterController _controller;
        
        private int _slidingAnimationId;
        
        private bool _sliding = false;
        private float _score = 0;

        [SerializeField] private UnityEvent<Vector3> turnEvent;
        [SerializeField] private UnityEvent<int> gameOverEvent;
        [SerializeField] private UnityEvent<int> scoreUpdateEvent;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();            
            
            _turnAction = _playerInput.actions["Turn"];
            _jumpAction = _playerInput.actions["Jump"];
            _slideAction = _playerInput.actions["Slide"];
        }

        private void Start()
        {
            playerSpeed = initialPlayerSpeed;
            _gravity = initialGravityValue;
            _slidingAnimationId = Animator.StringToHash("SlidingAnimation");
        }
        
        private void Update()
        {
            if (!IsGrounded(20f))
            {
                GameOver();
                return;
            }
            
            // Score Functionality
            _score += scoreMultiplier * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)_score);
            
            _controller.Move(transform.forward * (playerSpeed * Time.deltaTime));

            if (IsGrounded() && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -0.2f;
            }

            _playerVelocity.y += _gravity * Time.deltaTime;
            _controller.Move(_playerVelocity * (playerSpeed * Time.deltaTime));
        }

        private void OnEnable()
        {
            _turnAction.performed += OnTurnActionPerformed;
            _jumpAction.performed += OnJumpActionPerformed;
            _slideAction.performed += OnSlideActionPerformed;
        }

        private void OnDisable()
        {
            _turnAction.performed -= OnTurnActionPerformed;
            _jumpAction.performed -= OnJumpActionPerformed;
            _slideAction.performed -= OnSlideActionPerformed;
        }

        private void OnTurnActionPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Player Turning.");
            var turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                GameOver();
                return;
            }

            var targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * _movementDirection;
            turnEvent.Invoke(targetDirection);

            Turn(context.ReadValue<float>(), turnPosition.Value);
        }

        private void Turn(float turnValue, Vector3 turnPosition)
        {
            var tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);

            _controller.enabled = false;
            transform.position = tempPlayerPosition;
            _controller.enabled = true;

            var targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            _movementDirection = transform.forward.normalized;
        }

        private Vector3? CheckTurn(float turnValue)
        {
            var hitColliders = Physics.OverlapSphere(transform.position, 0.1f, turnLayer);

            if (hitColliders.Length > 0)
            {
                var tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                var type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1) ||
                    (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        private void OnJumpActionPerformed(InputAction.CallbackContext context)
        {
            if (IsGrounded())
            {
                Debug.Log("Jumping");
                _playerVelocity.y += Mathf.Sqrt(jumpHeight * _gravity * -3f);
                _controller.Move(_playerVelocity * Time.deltaTime);
            }
            else
            {
                Debug.Log("Jump button pressed but not grounded.");
            }
        }

        private void OnSlideActionPerformed(InputAction.CallbackContext context)
        {
            if (!_sliding && IsGrounded())
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            _sliding = true;
            
            // Shrink the collider
            var originalControllerCenter = _controller.center;
            var newControllerCenter = originalControllerCenter;
            _controller.height /= 2;
            newControllerCenter.y -= _controller.height / 2;
            _controller.center = newControllerCenter;

            // Play sliding animation.
            animator.Play(_slidingAnimationId);
            yield return new WaitForSeconds(slidingAnimationClip.length);
            
            // Change controller center and height back to normal.
            _controller.height *= 2;
            _controller.center = originalControllerCenter;
            _sliding = false;
        }

        private bool IsGrounded(float length = 0.5f)
        {
            var rayCastOriginFirst = transform.position;
            rayCastOriginFirst.y -= _controller.height / 2f;
            // rayCastOriginFirst.y += 0.1f;

            var rayCastOriginSecond = rayCastOriginFirst;
            rayCastOriginFirst -= transform.forward * 0.2f;
            rayCastOriginSecond += transform.forward * 0.2f;

            var ray = new Ray(rayCastOriginFirst, Vector3.down);
            var ray2 = new Ray(rayCastOriginSecond, Vector3.down);

            Debug.DrawLine(rayCastOriginFirst, rayCastOriginFirst + Vector3.down * length, Color.green, .1f);
            Debug.DrawLine(rayCastOriginSecond, rayCastOriginSecond + Vector3.down * length, Color.blue, .1f);


            if (Physics.Raycast(ray, length, groundLayer) || Physics.Raycast(ray2, length, groundLayer))
            {
                // Debug.Log("Player is grounded.");
                return true;
            }

            // Debug.Log("Player is not grounded.");
            return false;
        }

        private void GameOver()
        {
            Debug.Log("Game Over");
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                GameOver();
                gameOverEvent.Invoke((int)_score);
                gameObject.SetActive(false);
            }
        }
    }
}