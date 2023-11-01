using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Transporter : MonoBehaviour
{
    public Arrow arrow;
    public TextMeshProUGUI priorityText;
    public RectTransform priorityObj;

    public float speed = 0.25f;

    private GameManager gameManager;

    private static int priorityNext = 1;
    public int priority;
  /*private int _priority = 0;
    public int priority
    {
        get { return _priority; }
        set { 
            _priority = value;
            priorityText.text = value.ToString();
            priorityNext = value + 1;
        }
    }*/

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        /*if (priority == 0)
            GiveNextPriorityValue();
        */
        priorityText.text = priority.ToString();
    }

    private void OnEnable()
    {
        RemoveNode.OnExecute += InstantiateTransportCommand;
        arrow.OnChanged += FixPriorityTextPos;
        LevelManager.OnLevelLoad += GetOnTheLevel;


    }

    private void OnDisable()
    {
        RemoveNode.OnExecute -= InstantiateTransportCommand;
        arrow.OnChanged -= FixPriorityTextPos;
        LevelManager.OnLevelLoad -= GetOnTheLevel;
        //Node.OnNodeRemove -= InstantiateTransportCommand;
    }

#if UNITY_EDITOR
    private void OnMouseOver()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Debug.Log("mouse scroll delta: " + Input.mouseScrollDelta.y);
            SetPriority(priority + (int)Input.mouseScrollDelta.y);
        }
    }
#endif

    private void InstantiateTransportCommand(GameObject removedNode, RemoveNode command)
    {
        Node node = arrow.startingNode.GetComponent<Node>();

        foreach (var item in node.arrowsFromThisNode)
        {
            Transporter otherTransporter;
            item.TryGetComponent(out otherTransporter);
            if (item != arrow.gameObject && otherTransporter != null && otherTransporter.priority <= priority)
            {
                return;
            }
        }

        LockController startingLockCont= node.lockController;

        if (!startingLockCont.hasKey) return;

        Transform key = startingLockCont.key;
        Node destNode = arrow.destinationNode.GetComponent<Node>();

        LockController destLockCont = destNode.lockController;

        if (destLockCont.hasKey)
        {
            return;
        }

        List<GameObject> affectedObjects = new List<GameObject>();
        affectedObjects.Add(key.gameObject);
        TransportCommand transportCommand = new TransportCommand(gameManager, this, startingLockCont, destLockCont, arrow);

        StartCoroutine(TransportWithDelay(transportCommand, affectedObjects, command, 0.02f));
    }

    private IEnumerator TransportWithDelay(TransportCommand transportCommand, List<GameObject> affectedObjects, RemoveNode command, float delay)
    {
        yield return new WaitForSeconds(delay);

        transportCommand.Execute(affectedObjects);
        command.affectedCommands.Add(transportCommand);
    }

    public void Transport(Transform key, LockController startingLockCont, LockController destLockCont, Vector3[] lrPoints)
    {
        startingLockCont.key = null;
        startingLockCont.keyImage = null;
        startingLockCont.hasKey = false;


        destLockCont.key = key;
        key.SetParent(destLockCont.transform);

        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(key.position);
        pathlist.AddRange(lrPoints);
        pathlist.Add(destLockCont.transform.position + new Vector3(-0.24f, -0.20f, 0));
        Vector3[] path = pathlist.ToArray();

        key.DOPath(path, speed);
        Transform keyImageObj = key.Find("Image");
        if (keyImageObj)
        {
            destLockCont.keyImage = keyImageObj.GetComponent<SpriteRenderer>();
        }
        destLockCont.hasKey = true;
    }

    private void GetOnTheLevel()
    {
        Vector3 initialScale = priorityObj.localScale;
        priorityObj.localScale = Vector3.zero;
        
        priorityObj.DOScale(initialScale, 0.5f).SetDelay(0.5f);
    }

    private void FixPriorityTextPos()
    {
        priorityObj.position = (arrow.lr.GetPosition(0) + arrow.lr.GetPosition(1))/ 2;
    }

    public void SetPriority(int value)
    {
        priority = value;
        priorityText.text = value.ToString();
        priorityNext = value + 1;
    }

    public void GiveNextPriorityValue()
    {
        priority = priorityNext;
    }
}
