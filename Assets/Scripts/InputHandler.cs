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

    private Vector3 m_camera_input = new Vector3();
    private SelectionState m_selection_state = SelectionState.None;
    private LinkedList<SelectionState> m_selection_queue = new LinkedList<SelectionState>();
    public bool m_waiting_for_mouse_to_move = false;

    private GameObject m_tmp;
    private GameObject m_tmp2;

    public Vector3 CameraInput
    {
        get { return m_camera_input; }
    }

    void Awake()
    {
        m_tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_tmp2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_tmp2.transform.localScale = new Vector3(1.0f, 1.0f, .25f);
        m_tmp2.GetComponent<Renderer>().material.color = Color.blue;
    }

    void Update()
    {
        UpdateCameraInputs();
        HandleSelectionStates();
        if (m_selection_state == SelectionState.Movemement)
            HandleMovementState();
        else
        {
            m_tmp2.SetActive(false);
            m_tmp.SetActive(false);
        }
    }

    private void UpdateCameraInputs()
    {
        m_camera_input[1] += Input.GetKeyDown(KeyCode.W) ? 1.0f : .0f;
        m_camera_input[1] += Input.GetKeyDown(KeyCode.S) ? -1.0f : .0f;
        m_camera_input[1] += Input.GetKeyUp(KeyCode.W) ? -1.0f : .0f;
        m_camera_input[1] += Input.GetKeyUp(KeyCode.S) ? 1.0f : .0f;

        m_camera_input[0] += Input.GetKeyDown(KeyCode.D) ? 1.0f : .0f;
        m_camera_input[0] += Input.GetKeyDown(KeyCode.A) ? -1.0f : .0f;
        m_camera_input[0] += Input.GetKeyUp(KeyCode.D) ? -1.0f : .0f;
        m_camera_input[0] += Input.GetKeyUp(KeyCode.A) ? 1.0f : .0f;
    }

    private void HandleSelectionStates()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            m_selection_queue.AddLast(SelectionState.Movemement);
        if (Input.GetKeyUp(KeyCode.Space))
        {
            m_waiting_for_mouse_to_move = false;
            m_selection_queue.Remove(SelectionState.Movemement);
        }

        if (m_selection_queue.Count == 0)
            m_selection_state = SelectionState.None;
        else
            m_selection_state = m_selection_queue.First.Value;
    }

    private void HandleMovementState()
    {
        bool move = false;
        if (!Input.GetMouseButton(0))
        {
            if (!m_waiting_for_mouse_to_move)
                return;
            else
            {
                move = true;
                m_waiting_for_mouse_to_move = false;
            }
        }

        var t = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        Ray r = Camera.main.ScreenPointToRay(t);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(r, out hit, 1000.0f, NavGridScript.Instance.invertAccesbileEnviromentMask))
        { 
            if (Input.GetMouseButtonDown(0))
            {
                m_tmp.SetActive(true);
                m_tmp2.SetActive(true);
                m_waiting_for_mouse_to_move = true;
            }
            float x = hit.point[0];
            float y = hit.point[1];
            bool accessable = NavGridScript.Instance.IsWorldPositionAccessable(ref x, ref y);
            if (move && accessable)
            {
                var mover = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleCharacterMovement>();
                mover.MoveTo(new Vector3(x, y, 1.0f));
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
