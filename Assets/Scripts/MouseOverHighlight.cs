using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOverHighlight : MonoBehaviour
{
    public Grid grid;

    private float offset;

    private void Start()
    {
        offset = transform.position.y;
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = grid.LocalToCell(hit.point + new Vector3(0.5f, 0f, 0.5f));
            transform.position = new Vector3(transform.position.x, offset, transform.position.z);
        }
    }
}
