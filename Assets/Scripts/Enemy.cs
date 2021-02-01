using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Grid grid;
    public float movementSpeed = 1f;
    public float rotationSpeed = 90f;

    public List<Transform> patrolPoints = new List<Transform>();
    private int patrolIndex;

    private bool alert;
    private Vector3 alertPosition;
    private bool hasLastPosition;
    private Vector3 lastPosition;
    private Vector3 lastRotation;
    private bool stopMovement;

    private Animator animator;
    private Rigidbody rb;
    private FiledOfView fov;
    private AudioSource audiosource;

    private Coroutine moveToCoroutine;

    private Footstep[] feet;

    public bool IsStopped
    {
        get{ return stopMovement; }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        fov = GetComponentInChildren<FiledOfView>();
        fov.OnPlayerEnter += CheckIfPlayerVisible;

        audiosource = GetComponent<AudioSource>();

        transform.position = grid.CellToLocal(grid.LocalToCell(transform.position));
    }

    private void Start()
    {
        feet = GetComponentsInChildren<Footstep>();
        SetFootstepVolume(1);
    }

    private void OnDestroy()
    {
        fov.OnPlayerEnter -= CheckIfPlayerVisible;
    }

    private void CheckIfPlayerVisible(Vector3 _playerPosition)
    {
        // Change car layer to detect raycast
        foreach (Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Default");
            CarDoor[] _carDoors = _car.GetComponentsInChildren<CarDoor>();
            foreach (CarDoor _door in _carDoors)
            {
                _door.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        Vector3 direction = _playerPosition - transform.position;
        Ray _ray = new Ray(transform.position + Vector3.up, direction);
        if (Physics.Raycast(_ray, out RaycastHit _hit, 10f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                StopAllCoroutines();
                moveToCoroutine = null;
                audiosource.Play();
                GameController.Loose();
                StartCoroutine(PlayerFound(_playerPosition));
                fov.OnPlayerEnter -= CheckIfPlayerVisible;
            }
        }

        // Change car layer to not detect it with OnMouse events
        foreach (Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    IEnumerator PlayerFound(Vector3 _position)
    {
        float _sign = Mathf.Sign(Vector3.SignedAngle(transform.forward, (_position - transform.position), Vector3.up));

        while (Vector3.Angle(transform.forward, (_position - transform.position)) > 10f)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, _sign * rotationSpeed * Time.deltaTime, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);
            yield return null;
        }

        animator.SetBool("Walking", true);
        animator.SetBool("Forward", true);
        animator.SetBool("Running", true);
        SetFootstepVolume(3);

        rb.velocity = 3 * movementSpeed * transform.forward;

        while ((transform.position - _position).magnitude >= 1f)
        {
            yield return null;
        }

        animator.SetBool("Walking", false);
        animator.SetBool("Forward", false);
        animator.SetBool("Running", false);

        rb.velocity = Vector3.zero;
        SetFootstepVolume(1);
    }

    private void MoveTo(Vector3 _position)
    {
        Debug.Log(gameObject.name + " starts MoveTo");

        Vector3Int _destCell = grid.LocalToCell(_position);
        Vector3Int _selfCell = grid.LocalToCell(transform.position);

        Vector3Int xMovementCell = Vector3Int.zero;
        Vector3Int zMovementCell = Vector3Int.zero;

        int xDiff = _destCell.x - _selfCell.x;
        int zDiff = _destCell.z - _selfCell.z;

        // Test movement on x axis
        Vector3 testedPosition = grid.LocalToCell(_selfCell) + grid.LocalToCell(new Vector3(Mathf.Sign(xDiff), 0, 0));
        Ray _ray = new Ray(testedPosition + new Vector3(0f, 5f, 0f), -Vector3.up);
        if (Physics.Raycast(_ray, out RaycastHit _hit, 6f))
        {
            if (_hit.collider.CompareTag("Ground"))
            {
                xMovementCell = new Vector3Int((int) Mathf.Sign(xDiff), 0, 0);
            }
        }
        // Test movement on z axis
        testedPosition = grid.LocalToCell(_selfCell) + grid.LocalToCell(new Vector3(0, 0, Mathf.Sign(zDiff)));
        _ray = new Ray(testedPosition + new Vector3(0f, 5f, 0f), -Vector3.up);
        if (Physics.Raycast(_ray, out RaycastHit _hit2, 6f))
        {
            if (_hit2.collider.CompareTag("Ground"))
            {
                zMovementCell = new Vector3Int(0, 0, (int)Mathf.Sign(zDiff));
            }
        }

        if (Mathf.Abs(xDiff) >= Mathf.Abs(zDiff) && xMovementCell != Vector3Int.zero)
        {
            moveToCoroutine = StartCoroutine(MovingToNextCell(grid.CellToLocal(_selfCell + xMovementCell), alert));
        }
        else
        {
            if (zMovementCell != Vector3Int.zero)
            {
                moveToCoroutine = StartCoroutine(MovingToNextCell(grid.CellToLocal(_selfCell + zMovementCell), alert));
            }
            else
            {
                CheckIfMovementEnds();
            }
        }
    }

    IEnumerator MovingToNextCell(Vector3 _position, bool _running)
    {
        Debug.Log(gameObject.name + " starts MovingToNextCell");

        float _sign = Mathf.Sign(Vector3.SignedAngle(transform.forward, (_position - transform.position), Vector3.up));

        while (Vector3.Angle(transform.forward, (_position - transform.position)) > 10f)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, _sign * rotationSpeed * Time.deltaTime, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);
            yield return null;
        }

        transform.LookAt(_position);

        animator.SetBool("Walking", true);
        animator.SetBool("Forward", true);
        animator.SetBool("Running", _running);

        rb.velocity = (_running ? (2 * movementSpeed) : movementSpeed) * transform.forward;

        while ((transform.position - _position).magnitude >= 0.05f)
        {
            yield return null;
        }

        rb.velocity = Vector3.zero;
        transform.position = _position;

        animator.SetBool("Walking", false);
        animator.SetBool("Forward", false);

        moveToCoroutine = null;
        CheckIfMovementEnds();
    }

    private void CheckIfMovementEnds()
    {
        if(alert)
        {
            if ((alertPosition - transform.position).magnitude < 3f)
            {
                stopMovement = true;
                alert = false;
                SetFootstepVolume(1);
            }
        }
        else if (hasLastPosition)
        {            
                if(grid.LocalToCell(lastPosition) == grid.LocalToCell(transform.position))
                {
                    stopMovement = true;
                    hasLastPosition = false;
                    StartCoroutine(RotateToLastRotation());
                    SetFootstepVolume(1);
            }
        }
        else if(patrolPoints.Count > 0)
        {
            if (grid.LocalToCell(patrolPoints[patrolIndex].position) == grid.LocalToCell(transform.position))
            {
                stopMovement = true;
                patrolIndex++;
                if (patrolIndex >= patrolPoints.Count) patrolIndex = 0;
                SetFootstepVolume(1);
            }
        }
    }

    private IEnumerator RotateToLastRotation()
    {
        Debug.Log(gameObject.name + " starts RotateToLastRotation");

        float _sign = Mathf.Sign(Vector3.SignedAngle(transform.forward, (lastRotation - transform.position), Vector3.up));

        while (Vector3.Angle(transform.forward, (lastRotation - transform.position)) > 10f)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, _sign * rotationSpeed * Time.deltaTime, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);
            yield return null;
        }
        transform.LookAt(lastRotation);
    }

    public void StartTurn()
    {
        stopMovement = false;
        StopAllCoroutines();
        moveToCoroutine = null;

        // if is in alert
        if (alert)
        {
            SetFootstepVolume(3);
            StartCoroutine(MovementSequence(alertPosition, true));
        }
        // if returns from an alert
        else if(hasLastPosition)
        {
            SetFootstepVolume(2);
            StartCoroutine(MovementSequence(lastPosition, false));
        }
        // if has patrol points
        else if(patrolPoints.Count > 0)
        {
            SetFootstepVolume(2);
            StartCoroutine(MovementSequence(patrolPoints[patrolIndex].position, false));
        }
        else
        {
            stopMovement = true;
        }
    }

    IEnumerator MovementSequence(Vector3 _position, bool _running)
    {
        Debug.Log(gameObject.name + " starts MovementSequence");

        int _turns = _running ? 7 : 5;

        for(int i=0; i<_turns;i++)
        {
            if (stopMovement) break;
            while(moveToCoroutine != null)
            {
                yield return null;
            }
            MoveTo(_position);
        }
        SetFootstepVolume(1);
        stopMovement = true;
    }

    public void Alert(Vector3 _position)
    {
        alert = true;
        alertPosition = _position;
        lastPosition = transform.position;
        lastRotation = transform.position + transform.forward;
        hasLastPosition = true;

        StartCoroutine(RotateToward(alertPosition));
    }

    IEnumerator RotateToward(Vector3 _position)
    {
        Debug.Log(gameObject.name + " starts RotateToward");

        float _sign = Mathf.Sign(Vector3.SignedAngle(transform.forward, (_position - transform.position), Vector3.up));

        while (Vector3.Angle(transform.forward, (_position - transform.position)) > 10f)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, _sign * rotationSpeed * Time.deltaTime, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);
            yield return null;
        }
        transform.LookAt(_position);
    }

    public static void StartEnemyTurn()
    {
        // Change car layer to detect raycast
        foreach (Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Default");
            CarDoor[] _carDoors = _car.GetComponentsInChildren<CarDoor>();
            foreach (CarDoor _door in _carDoors)
            {
                _door.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }

    public static void EndEnemyTurn()
    {
        // Change car layer to not detect it with OnMouse events
        foreach (Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    private void SetFootstepVolume(int _phase)
    {
        foreach(Footstep _foot in feet)
        {
            _foot.SetVolume(_phase);
        }
    }

}
