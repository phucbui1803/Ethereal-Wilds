using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 1.0f;
    public float SprintSpeed = 3.0f;
    [Range(0.0f, 0.3f)] public float RotationSmoothTime = 0.12f;
    public float SpeedChangeRate = 10f;
    public float Gravity = -15f;

    [Header("Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;

    [Header("Wandering")]
    public float IdleTimeMin = 7f;
    public float IdleTimeMax = 10f;
    public float WalkTimeMin = 3f;
    public float WalkTimeMax = 5f;
    public float WalkRadius = 8f;
    public Transform Origin;
    public int MaxPickAttempts = 12;
    public float MaxSlopeAngle = 45f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2f;
    public float avoidStrength = 1f;
    public LayerMask obstacleLayers;

    [Header("Chasing Player")]
    public Transform Player;
    public float ChaseDistance = 10f;

    [Header("Status")]
    public bool _isAttacking = false;  // dùng skill từ SkillController
    public bool _isUsingSkill = false; // mới: enemy đang thực hiện skill
    public bool canMove = true;

    // internal
    private CharacterController _controller;
    private Animator _animator;
    private EnemyStats _enemyStats;
    private float _speed;
    private float _animationBlend;
    private float _verticalVelocity;
    private float _targetRotation;
    private float _rotationVelocity;

    private Vector3 _targetPosition;
    private bool _isWalking = false;
    private float _stateDuration;
    private float _stateTimer = 0f;
    private bool _isChasing = false;

    public bool IsChasing
    {
        get => _isChasing;
        set => _isChasing = value;
    }

    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private const float _threshold = 0.01f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _enemyStats = GetComponent<EnemyStats>();

        AssignAnimationIDs();

        if (Origin == null) Origin = transform;
        ChooseNewState(false); // bắt đầu idle
    }

    void Update()
    {
        if (!_controller.enabled || !gameObject.activeInHierarchy)
            return; // ✅ tránh gọi Move nếu controller hoặc object bị disable

        GroundedCheck();
        ApplyGravity();
        Wander();
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
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void Wander()
    {
        if (_isAttacking) return; // ✅ nếu đang tấn công → không wander

        bool chasingHandled = false;

        if (Player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, Player.position);

            if (distanceToPlayer <= ChaseDistance)
            {
                // đánh dấu là đang combat luôn khi thấy player
                _enemyStats.SetInCombat(true);

                if (distanceToPlayer <= 3f)
                {
                    // đứng yên khi gần player
                    _isChasing = false;
                    _isWalking = false;
                    _speed = 0f;
                    _animationBlend = 0f;

                    // xoay về player
                    Vector3 lookDir = (Player.position - transform.position);
                    lookDir.y = 0;
                    if (lookDir.sqrMagnitude > 0.01f)
                        transform.rotation = Quaternion.LookRotation(lookDir);

                    return;
                }
                else
                {
                    _isChasing = true;
                    _isWalking = false;
                    _targetPosition = Player.position;
                    _stateTimer = 0f;
                    _stateDuration = Mathf.Infinity;
                    return;
                }
            }
            else
            {
                if (_enemyStats.InCombat)
                {
                    _isChasing = false;
                    _enemyStats.OnPlayerOutOfRange();
                }
            }
        }

        if (!chasingHandled)
        {
            _stateTimer += Time.deltaTime;
            if (_stateTimer >= _stateDuration)
            {
                bool nextWalk = Random.value > 0.5f;
                ChooseNewState(nextWalk);
            }
        }
    }

    public void ChooseNewState(bool walk)
    {
        if (_isChasing || _isAttacking) return; // không đổi state khi chase hoặc attack

        _isWalking = walk;
        _stateTimer = 0f;
        _stateDuration = walk ? Random.Range(WalkTimeMin, WalkTimeMax) : Random.Range(IdleTimeMin, IdleTimeMax);

        if (walk)
        {
            bool picked = PickNewTarget();
            if (!picked)
            {
                _isWalking = false;
                _stateTimer = 0f;
                _stateDuration = Random.Range(IdleTimeMin, IdleTimeMax);
            }
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
        // ✅ nếu không được phép di chuyển thì dừng hoàn toàn
        if (!canMove)
        {
            _speed = 0f;
            _animationBlend = 0f;
            if (_animator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
            return;
        }

        // ✅ Dừng di chuyển nếu đang tấn công hoặc đang dùng skill
        if (_isAttacking || _isUsingSkill)
        {
            _speed = 0f;
            _animationBlend = 0f;
            if (_animator)
            {
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 0f);
            }
            return;
        }

        float targetSpeed = _isChasing ? SprintSpeed : (_isWalking ? MoveSpeed : 0f);
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        if (currentHorizontalSpeed < targetSpeed - _threshold || currentHorizontalSpeed > targetSpeed + _threshold)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else _speed = targetSpeed;

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 direction = (_targetPosition - transform.position);
        direction.y = 0;

        if (direction.magnitude > 0.1f && (_isWalking || _isChasing))
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
