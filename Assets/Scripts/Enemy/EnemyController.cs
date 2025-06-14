using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    enum EnemyState
    {
        Patrol = 0,
        Investigate = 1
    }

    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _threshold = 0.5f;
    [SerializeField] private float _waitTime = 2f;
    [SerializeField] private PatrolRoute _patrolRoute;
    [SerializeField] private FieldOfView _fov;
    [SerializeField] private EnemyState _state = EnemyState.Patrol;

    public UnityEvent<Transform> onPlayerFound;
    public UnityEvent onInvestigate;
    public UnityEvent onReturnToPatrol;

    private bool _moving = false;
    private Transform _currentPoint;
    private int _routeIndex = 0;
    private bool _forwardsAlongPath = true;
    private Vector3 _investigationPoint;
    private float _waitTimer = 0f;
    private bool _playerFound = false;

    // Stun variables
    private bool _isStunned = false;
    private float _stunDuration = 2f;
    private float _stunTimer = 0f;

    void Start()
    {
        _currentPoint = _patrolRoute.route[_routeIndex];
    }

    void Update()
    {
        // Handle stun logic
        if (_isStunned)
        {
            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                _isStunned = false;
                _agent.isStopped = false;
            }
            _animator.SetFloat("Speed", 0);
            return; // Skip other behavior while stunned
        }

        _animator.SetFloat("Speed", _agent.velocity.magnitude);

        if (_fov.visibleObjects.Count > 0)
        {
            PlayerFound(_fov.visibleObjects[0].position);
        }

        if (_state == EnemyState.Patrol)
        {
            UpdatePatrol();
        }
        else if (_state == EnemyState.Investigate)
        {
            UpdateInvestigate();
        }
    }

    public void SetStunned()
    {
        if (!_isStunned)
        {
            _isStunned = true;
            _stunTimer = _stunDuration;
            _agent.isStopped = true;
            _animator.SetFloat("Speed", 0);
        }
    }

    public void InvestigatePoint(Vector3 investigatePoint)
    {
        SetInvestigationPoint(investigatePoint);
        onInvestigate.Invoke();
    }

    private void SetInvestigationPoint(Vector3 investigatePoint)
    {
        _state = EnemyState.Investigate;
        _investigationPoint = investigatePoint;
        _agent.SetDestination(_investigationPoint);
    }

    private void PlayerFound(Vector3 investigatePoint)
    {
        if (_playerFound) return;

        SetInvestigationPoint(investigatePoint);
        onPlayerFound.Invoke(_fov.creature.head);
        _playerFound = true;
    }

    private void UpdateInvestigate()
    {
        if (Vector3.Distance(transform.position, _investigationPoint) < _threshold)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer > _waitTime)
            {
                ReturnToPatrol();
            }
        }
    }

    private void ReturnToPatrol()
    {
        Debug.Log("Enemy returning to patrol");
        _state = EnemyState.Patrol;
        _waitTimer = 0;
        _moving = false;
        onReturnToPatrol.Invoke();
    }

    private void UpdatePatrol()
    {
        if (!_moving)
        {
            NextPatrolPoint();
            _agent.SetDestination(_currentPoint.position);
            _moving = true;
        }

        if (_moving && Vector3.Distance(transform.position, _currentPoint.position) < _threshold)
        {
            _moving = false;
        }
    }

    private void NextPatrolPoint()
    {
        if (_forwardsAlongPath)
        {
            _routeIndex++;
        }
        else
        {
            _routeIndex--;
        }

        if (_routeIndex == _patrolRoute.route.Count)
        {
            if (_patrolRoute.patrolType == PatrolRoute.PatrolType.Loop)
            {
                _routeIndex = 0;
            }
            else
            {
                _forwardsAlongPath = false;
                _routeIndex -= 2;
            }
        }

        if (_routeIndex == 0)
        {
            _forwardsAlongPath = true;
        }

        _currentPoint = _patrolRoute.route[_routeIndex];
    }
}
