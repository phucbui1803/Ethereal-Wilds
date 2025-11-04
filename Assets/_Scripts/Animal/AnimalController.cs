using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class AnimalController : MonoBehaviour
{
    [Header("Movement")]
    public float WalkSpeed = 0.5f;
    public float RunSpeed = 1f;
    [Range(0f, 0.3f)] public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10f;
    public float Gravity = -15f;

    [Header("Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Wandering")]
    public float IdleTimeMin = 2f;
    public float IdleTimeMax = 5f;
    public float WalkTimeMin = 3f;
    public float WalkTimeMax = 6f;
    public float RunTimeMin = 2f;
    public float RunTimeMax = 4f;
    public float WalkRadius = 5f;
    public Transform Origin;
    public int MaxPickAttempts = 12;
    public float MaxSlopeAngle = 45f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2f;
    public float avoidStrength = 1f;
    public LayerMask obstacleLayers;

    [Header("Status")]
    public bool canMove = true;

    // internal
    private CharacterController _controller;
    private Animator _animator;
    private float _speed;
    private float _animationBlend;
    private float _verticalVelocity;
    private float _targetRotation;
    private float _rotationVelocity;

    private Vector3 _targetPosition;
    private int _currentState = 0; // 0 = Idle, 1 = Walk, 2 = Run
    private float _stateTimer = 0f;
    private float _stateDuration = 0f;

    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private const float _threshold = 0.01f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        AssignAnimationIDs();

        if (Origin == null) Origin = transform;

        ChooseNewState(0); // Bắt đầu Idle
    }

    void Update()
    {
        if (!_controller.enabled || !gameObject.activeInHierarchy) return;

        GroundedCheck();
        ApplyGravity();
        UpdateState();
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void ApplyGravity()
    {
        if (Grounded)
        {
            if (_verticalVelocity < 0f) _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void UpdateState()
    {
        if (!canMove) return;

        _stateTimer += Time.deltaTime;
        if (_stateTimer >= _stateDuration)
        {
            int nextState = Random.Range(0, 3); // 0 = Idle, 1 = Walk, 2 = Run
            ChooseNewState(nextState);
        }
    }

    private void ChooseNewState(int state)
    {
        _currentState = state;
        _stateTimer = 0f;

        switch (_currentState)
        {
            case 0: // Idle
                _stateDuration = Random.Range(IdleTimeMin, IdleTimeMax);
                break;
            case 1: // Walk
                _stateDuration = Random.Range(WalkTimeMin, WalkTimeMax);
                PickNewTarget();
                break;
            case 2: // Run
                _stateDuration = Random.Range(RunTimeMin, RunTimeMax);
                PickNewTarget();
                break;
        }
    }

    private bool PickNewTarget()
    {
        Vector3 center = Origin.position;

        for (int i = 0; i < MaxPickAttempts; i++)
        {
            Vector2 circle = Random.insideUnitCircle * WalkRadius;
            Vector3 candidate = center + new Vector3(circle.x, 0, circle.y);

            if (Physics.Raycast(candidate + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 50f, GroundLayers))
            {
                float slope = Vector3.Angle(hit.normal, Vector3.up);
                if (slope > MaxSlopeAngle) continue;

                candidate.y = hit.point.y + _controller.center.y;
                _targetPosition = candidate;
                return true;
            }
        }

        return false;
    }

    private void Move()
    {
        if (!canMove || _currentState == 0)
        {
            _speed = 0f;
            _animationBlend = 0f;
            UpdateAnimator();
            return;
        }

        float targetSpeed = (_currentState == 1) ? WalkSpeed : RunSpeed;
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > _threshold)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else _speed = targetSpeed;

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

        Vector3 direction = (_targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            direction = AvoidObstacles(direction);

            _targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;
            _controller.Move(moveDir.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }
        else
        {
            _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (_animator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
        }
    }

    private Vector3 AvoidObstacles(Vector3 direction)
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, out RaycastHit hit, avoidDistance, obstacleLayers))
        {
            Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up).normalized;
            if (Vector3.Dot(avoidDir, direction) < 0f) avoidDir = -avoidDir;
            direction = Vector3.Lerp(direction, avoidDir, avoidStrength).normalized;
        }
        return direction;
    }
}
