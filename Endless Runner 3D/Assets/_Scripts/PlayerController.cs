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
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;


        private float _playerSpeed;
        private float _gravity;
        private Vector3 _movementDirection = Vector3.forward;
        private Vector3 _playerVelocity;

        private PlayerInput _playerInput;
        private InputAction _turnAction;
        private InputAction _jumpAction;
        private InputAction _slideAction;

        private CharacterController _controller;

        [SerializeField] private UnityEvent<Vector3> turnEvent;

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
            _playerSpeed = initialPlayerSpeed;
            _gravity = initialGravityValue;
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
            var turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                return;
            }

            var targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * _movementDirection;
            turnEvent.Invoke(targetDirection);

            Turn(context.ReadValue<float>(), turnPosition);
        }

        private void Turn(float turnValue, Vector3? turnPosition)
        {
            Vector3? tempPlayerPosition;
            if (turnPosition != null)
            {
                tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            }
            
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
                    (type == TileType.STRAIGHT))
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
        }

        private void Update()
        {
            _controller.Move(transform.forward * (_playerSpeed * Time.deltaTime));

            if (IsGrounded() && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -0.2f;
            }

            _playerVelocity.y += _gravity * Time.deltaTime;
            _controller.Move(_playerVelocity * (_playerSpeed * Time.deltaTime));
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
    }
}