using UnityEngine;
using System.Collections;


public class SimpleCharacterMovement : MonoBehaviour {

    public float speed = 1.0f;
    public float rotationSpeed = 1.0f;

    private NavPath pathScript;

    public NavPath.GridPosition pos;
    private NavGridScript navGrid;
    private MovementEvalFuncDelegate m_del;
    private AudioSource m_audioSource;

    void Start ()
    {
        navGrid = NavGridScript.Instance;

        pathScript = new NavPath();
        pathScript.Initialize(navGrid);
        pathScript._characterSize = .5f;
        m_audioSource = GetComponent<AudioSource>();

        m_del = new MovementEvalFuncDelegate(EvalPathMovementFunc);

        float x = transform.position.x;
        float y = transform.position.y;
        if (!NavGridScript.Instance.IsWorldPositionAccessable(ref x, ref y))
            Debug.LogError("Character " + gameObject.name + "is in unaccessable location");

        transform.position = new Vector3(x, y, transform.position.z);
    }
    float EvalPathMovementFunc(NavMovementEvalData d) { return d.f; }

    public void Update()
    {
        transform.position = pathScript.GetWorldPos(pos);
    }

    public void MoveTo(Vector3 to)
    {
        pathScript.SeekPath(m_del, transform.position, to - new Vector3(.5f, .5f, .5f));
    }

    public void TakeStep()
    {
        if (pathScript._path.Count == 0)
            return;

        var nextPos = pathScript._path[0];

        bool movementBlocked = false;

        var objs = GameObject.FindGameObjectsWithTag("Player"); // get list of dynamic objects (stupid version)
        foreach(var obj in objs) // loop over objects to check next path step is not blocked
        {
            var mover = obj.GetComponent<SimpleCharacterMovement>();
            if (mover == null || mover == this)
                continue;

            if(mover.pos == nextPos)
            {
                movementBlocked = true;
                break;
            }
        }

        if (!movementBlocked) // if nextPos was not blocked, move there and remove one segment from path
        {
            pos = nextPos;
            pathScript._path.RemoveAt(0);
            m_audioSource.Play();
        }
    }

 }
