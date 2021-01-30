using UnityEngine;

public class MoveActionCell : MonoBehaviour
{
    public bool isFarCell;

    public delegate void MoveActionCellEventHandlerPosBool(Vector3 _vect, bool _isFar);
    public event MoveActionCellEventHandlerPosBool OnCellClicked;
    public delegate void MoveActionCellEventHandlerPos(Vector3 _vect);
    public event MoveActionCellEventHandlerPos OnCellEnter;
    public delegate void MoveActionCellEventHandler();
    public event MoveActionCellEventHandler OnCellExit;

    void OnMouseDown()
    {
        OnCellClicked?.Invoke(transform.position, isFarCell);
    }

    private void OnMouseEnter()
    {
        // Draw line
        OnCellEnter?.Invoke(transform.position);
    }

    private void OnMouseExit()
    {
        //Remove line
        OnCellExit?.Invoke();
    }

    private void OnDestroy()
    {
        OnCellClicked = null;
        OnCellEnter = null;
        OnCellExit = null;
    }
}
