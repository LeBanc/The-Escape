using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Grid grid;
    public GameObject nearCellPrefab;
    public GameObject farCellPrefab;
    public float cellHeightOffset = 0.01f;

    public ParticleSystem particles;

    private List<GameObject> cellsList = new List<GameObject>();
    private Coroutine moveToCoroutine;

    private List<Cell> allCells = new List<Cell>();
    private List<Cell> lastCells = new List<Cell>();
    private Cell origin;


    private Animator animator;
    private Rigidbody rb;
    private LineRenderer lineRenderer;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();

        GameController.OnLoose += StopMovement;
    }

    private void OnDestroy()
    {
        GameController.OnLoose -= StopMovement;
    }

    private void Start()
    {
        ClearLine();
    }

    public void StartTurn()
    {
        SetMovementCells();
    }

    public void EndTurn()
    {
        ClearLine();
        ClearMovementCells();
    }

    private void SetMovementCells()
    {
        allCells.Clear();
        lastCells.Clear();

        origin = new Cell(grid.LocalToCell(transform.position + new Vector3(0.5f, 0f, 0.5f)), null, null);
        allCells.Add(origin);
        lastCells.Add(origin);

        // Change car layer to detect raycast
        foreach(Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Default");
            CarDoor[] _carDoors = _car.GetComponentsInChildren<CarDoor>();
            foreach(CarDoor _door in _carDoors)
            {
                _door.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        for (int n = 0; n < 6; n++)
        {
            List<Cell> _tempCells = new List<Cell>(lastCells);
            lastCells.Clear();
            foreach(Cell _c in _tempCells)
            {
                for(int i=-1;i<2;i++)
                {
                    for(int j=-1;j<2;j++)
                    {
                        if ((Mathf.Abs(i) + Mathf.Abs(j)) != 1) continue;

                        //Else
                        Vector3 testedPosition = _c.position + grid.LocalToCell(new Vector3(i, 0, j));
                        if (allCells.Find(cell => cell.position == testedPosition) == null)
                        {
                            Ray _ray = new Ray(testedPosition + new Vector3(0f, 5f, 0f), -Vector3.up);
                            if (Physics.Raycast(_ray, out RaycastHit _hit, 6f))
                            {
                                if (_hit.collider.CompareTag("Ground"))
                                {
                                    GameObject _go = Instantiate((n > 2) ? farCellPrefab : nearCellPrefab);
                                    Cell newCell = new Cell(testedPosition, _c, _go);
                                    allCells.Add(newCell);
                                    lastCells.Add(newCell);
                                    _go.transform.position = newCell.position + new Vector3(0f, cellHeightOffset, 0f);
                                    _go.GetComponent<MoveActionCell>().OnCellClicked += MoveTo;
                                    _go.GetComponent<MoveActionCell>().OnCellExit += ClearLine;
                                    _go.GetComponent<MoveActionCell>().OnCellEnter += DrawLine;

                                }
                            }
                        }
                    }
                }
            }
        }

        // Change car layer to not detect it with OnMouse events
        foreach (Car _car in GameController.Cars)
        {
            _car.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    private void ClearMovementCells()
    {
        foreach(GameObject _go in cellsList)
        {
            Destroy(_go);
        }
        cellsList.Clear();

        foreach(Cell _c in allCells)
        {
            if(_c.prefab != null)
            {
                Destroy(_c.prefab);
            }
        }
        allCells.Clear();
    }

    private void MoveTo(Vector3 _position, bool _running)
    {
        StartCoroutine(MovingSequence(_position, _running));
        UIManager.HideUIWhenMoving();
    }

    private void StopMovement()
    {
        StopAllCoroutines();
        rb.velocity = Vector3.zero;
        animator.SetBool("Walking", false);
        animator.SetBool("Forward", false);
        animator.SetBool("Running", false);

        if (moveToCoroutine != null)
        {
            moveToCoroutine = null;
        }
    }

    IEnumerator MovingSequence(Vector3 _position, bool _running)
    {
        // Get all cells
        List<Cell> movementCells = new List<Cell>();
        Cell _tempCell = allCells.Find(cell => cell.position == grid.LocalToCell(_position));
        if(_tempCell != null)
        {
            movementCells.Add(_tempCell);
            while (_tempCell != null)
            {
                _tempCell = _tempCell.origin;
                if (_tempCell != null)  movementCells.Add(_tempCell);
            }
        }
        ClearMovementCells();
        ClearLine();

        for(int i = movementCells.Count-2; i >= 0; i--)
        {
            moveToCoroutine = StartCoroutine(MovingToNextCell(movementCells[i].position, _running));
            while (moveToCoroutine != null)
            {
                yield return null;
            }
        }

        GameController.EndTurn();
    }

    IEnumerator MovingToNextCell(Vector3 _position, bool _running)
    {
        float _sign = Mathf.Sign(Vector3.SignedAngle(transform.forward, (_position - transform.position), Vector3.up));

        while(Vector3.Angle(transform.forward,(_position - transform.position)) > 8f)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0f, _sign, 0f));
            rb.MoveRotation(rb.rotation * deltaRotation);
            yield return null;
        }

        transform.LookAt(_position);

        animator.SetBool("Walking", true);
        animator.SetBool("Forward", true);
        animator.SetBool("Running", _running);
        
        rb.velocity = (_running ? 3f : 1f) * transform.forward;

        while ( (transform.position - _position).magnitude >= 0.05f)
        {
            yield return null;
        }

        // Alert penemy if running
        if(_running)
        {
            particles.Play();

            Collider[] _colls = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider _c in _colls)
            {
                if (_c.CompareTag("Enemy"))
                {
                    _c.GetComponent<Enemy>().Alert(transform.position);
                }
            }
        }

        rb.velocity = Vector3.zero;
        transform.position = _position;

        animator.SetBool("Walking", false);
        animator.SetBool("Forward", false);

        moveToCoroutine = null;
    }

    void DrawLine(Vector3 _position)
    {
        // Get all cells
        List<Vector3> linePositions = new List<Vector3>();
        Cell _tempCell = allCells.Find(cell => cell.position == grid.LocalToCell(_position));
        if (_tempCell != null)
        {
            linePositions.Add(_tempCell.position + 4f*cellHeightOffset*Vector3.up);
            while (_tempCell != null)
            {
                _tempCell = _tempCell.origin;
                if (_tempCell != null) linePositions.Add(_tempCell.position + 4f * cellHeightOffset * Vector3.up);
            }
        }
        lineRenderer.positionCount = linePositions.Count;
        lineRenderer.SetPositions(linePositions.ToArray());
        lineRenderer.enabled = true;
    }

    void ClearLine()
    {
        lineRenderer.enabled = false;
    }

}

public class Cell
{
    public Vector3 position;
    public Cell origin;
    public GameObject prefab;

    public Cell()
    {
        position = Vector3.zero;
        origin = null;
        prefab = null;
    }

    public Cell(Vector3 _p, Cell _c, GameObject _go)
    {
        position = _p;
        origin = _c;
        prefab = _go;
    }
}
