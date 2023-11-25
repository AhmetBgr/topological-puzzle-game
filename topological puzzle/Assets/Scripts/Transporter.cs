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

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
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

        //if (node.gameObject != removedNode) return;
        if (command.isRewindCommand) return;

        foreach (var arrow in node.arrowsFromThisNode)
        {
            Transporter otherTransporter;
            arrow.TryGetComponent(out otherTransporter);
            if (arrow != arrow.gameObject && otherTransporter != null && otherTransporter.priority <= priority)
            {
                return;
            }
        }

        ItemController startingItemCont = node.itemController;

        if (startingItemCont.itemContainer.items.Count == 0) return;

        Item item = startingItemCont.FindLastTransportableItem();

        //if (command.isRewindCommand && item.isPermanent) return;

        Node destNode = arrow.destinationNode.GetComponent<Node>();

        ItemController destLockCont = destNode.itemController;

        /*if (destLockCont.hasKey)
        {
            return;
        }*/

        //List<GameObject> affectedObjects = new List<GameObject>();
        //affectedObjects.Add(item.gameObject);
        TransportCommand transportCommand = new TransportCommand(gameManager, this, startingItemCont, 
            destLockCont, arrow, item.gameObject);

        StartCoroutine(TransportWithDelay(transportCommand, command, 0.02f));
    }

    private IEnumerator TransportWithDelay(TransportCommand transportCommand, RemoveNode command, float delay)
    {
        yield return new WaitForSeconds(delay);

        transportCommand.Execute(gameManager.commandDur);
        command.affectedCommands.Add(transportCommand);
    }

    public void Transport(Transform itemT, ItemController startingItemCont, 
        ItemController destItemCont, Vector3[] lrPoints, float dur, int destContainerIndex = 0)
    {

        itemT.SetParent(LevelManager.curLevel.transform);
        Item item = itemT.GetComponent<Item>();
        startingItemCont.RemoveItem(item, dur/2);


        List<Vector3> pathlist = new List<Vector3>();
        pathlist.Add(itemT.position);
        pathlist.AddRange(lrPoints);
        Vector3[] path = pathlist.ToArray();

        itemT.DOPath(path, dur/2).OnComplete(() => { 
            destItemCont.itemContainer.AddItem(item, destContainerIndex, dur/2);
            destItemCont.itemContainer.FixItemPositions(dur/2);
        });
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
