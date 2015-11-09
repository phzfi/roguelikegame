using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputHandler : Singleton<InputHandler> {

    protected InputHandler() { }

    public enum SelectionState
    {
        None,
        Movemement,
        Count
    }

    private NavGridScript m_navGrid;
    private Vector3 m_cameraInput = new Vector3();
    private SelectionState m_selectionState = SelectionState.None;
    private LinkedList<SelectionState> m_selectionQueue = new LinkedList<SelectionState>();
    public bool m_waitingForMouseToMove = false;

    private GameObject m_tmp;
    private GameObject m_tmp2;

    public Vector3 CameraInput
    {
        get { return m_cameraInput; }
    }

    void Awake()
    {
        m_tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_tmp2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_tmp2.transform.localScale = new Vector3(1.0f, 1.0f, .25f);
        m_tmp2.GetComponent<Renderer>().material.color = Color.blue;
        m_navGrid = FindObjectOfType<NavGridScript>();
    }

    void Update()
    {
        UpdateCameraInputs();
        HandleSelectionStates();
        if (m_selectionState == SelectionState.Movemement)
            HandleMovementState();
        else
        {
            m_tmp2.SetActive(false);
            m_tmp.SetActive(false);
        }
    }

    private void UpdateCameraInputs()
    {
        m_cameraInput[1] = Input.GetKey(KeyCode.W) ? 1.0f : .0f;
        m_cameraInput[1] += Input.GetKey(KeyCode.S) ? -1.0f : .0f;

        m_cameraInput[0] = Input.GetKey(KeyCode.D) ? 1.0f : .0f;
        m_cameraInput[0] += Input.GetKey(KeyCode.A) ? -1.0f : .0f;
    }

    private void HandleSelectionStates()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            m_selectionQueue.AddLast(SelectionState.Movemement);
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_waitingForMouseToMove = false;
            m_selectionQueue.Remove(SelectionState.Movemement);
        }

        if (m_selectionQueue.Count == 0)
            m_selectionState = SelectionState.None;
        else
            m_selectionState = m_selectionQueue.First.Value;
    }

    private void HandleMovementState()
    {
        bool move = false;
        if (!Input.GetMouseButton(0))
        {
            if (!m_waitingForMouseToMove)
                return;
            else
            {
                move = true;
                m_waitingForMouseToMove = false;
            }
        }

        var t = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        Ray r = Camera.main.ScreenPointToRay(t);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(r, out hit, 1000.0f, m_navGrid.invertAccesbileEnviromentMask))
        { 
            if (Input.GetMouseButtonDown(0))
            {
                m_tmp.SetActive(true);
                m_tmp2.SetActive(true);
                m_waitingForMouseToMove = true;
            }
            float x = hit.point[0];
            float y = hit.point[1];
            bool accessable = m_navGrid.IsWorldPositionAccessable(ref x, ref y);
            if (move && accessable)
            {
                var players = GameObject.FindGameObjectsWithTag("Player");
                foreach (var p in players)
                {
                    var mover = p.GetComponent<SimpleCharacterMovement>();
                    if(mover.enabled)
                        mover.MoveTo(new Vector3(x, y, 1.0f));
                }
            }
            else if (accessable)
                m_tmp.GetComponent<Renderer>().material.color = Color.green;
            else
                m_tmp.GetComponent<Renderer>().material.color = Color.red;

            m_tmp.transform.position = new Vector3(hit.point[0], hit.point[1], .5f);
            m_tmp2.transform.position = new Vector3(x, y, 1.0f);
        }
    }
}
