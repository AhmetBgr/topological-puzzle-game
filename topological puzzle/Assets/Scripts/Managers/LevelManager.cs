using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SerializableTypes;

public class LevelManager : MonoBehaviour{
    //public static Material arrowGlow;
    //public static Material nodeGlow;

    public GameObject arrow;
    public GameObject permanentArrow;
    public GameObject basicNode;
    public GameObject squareNode;
    public GameObject lockedNode;
    public GameObject hexagonNode;
    public GameObject padLockPrefab;
    public GameObject permanentPadLockPrefab;
    public GameObject keyPrefab;
    public GameObject permanentKeyPrefab;

    public GameObject[] levels;
    public static GameObject curLevel;

    public Button nextLevelButton;
    public Button previousLevelButton;

    public static int curLevelIndex = 1;


    public static int nodecount = 0;
    public static int arrowCount = 0;

    private IEnumerator loadLevelCor = null;
    public List<GameObject> nodesPool = new List<GameObject>();
    public List<GameObject> arrowsPool = new List<GameObject>();
    /*List<GameObject> squareNodesPool = new List<GameObject>();
    List<GameObject> hexagonNodesPool = new List<GameObject>();
    List<GameObject> lockedNodesPool = new List<GameObject>();*/

    public delegate void OnLevelCompleteDelegate(int curIndex);
    public static OnLevelCompleteDelegate OnLevelComplete;

    public delegate void OnCurLevelIndexChangeDelegate(int curIndex);
    public static OnCurLevelIndexChangeDelegate OnCurLevelIndexChange;

    public delegate void OnLevelLoadDelegate();
    public static OnLevelLoadDelegate OnLevelLoad;

    public delegate void OnNodeCountChangeDelegate(Transform curLevel);
    public static OnNodeCountChangeDelegate OnNodeCountChange;

    // Start is called before the first frame update
    void Start(){
        //arrowGlow = Resources.Load<Material>("Glow Materials/Arrow Glow");
        //nodeGlow = Resources.Load<Material>("Glow Materials/Node Glow");
        SetCurLevelIndex(1);
        LoadLevel(curLevelIndex);

        // Example for loading a level file from resources folder
        /*TextAsset textAsset =  Resources.Load<TextAsset>("level.save");
        BinaryFormatter bf = new BinaryFormatter();
        var stream = new MemoryStream(textAsset.bytes);
        LevelProperty levelProperty = (LevelProperty)bf.Deserialize(stream);*/
    }

    void OnEnable(){
        LevelManager.OnLevelComplete += LoadNextLevel;
        LevelEditor.OnExit += UpdateObjectCount;
        LevelEditor.OnEnter += ToggleLevelChangeButtons;
        LevelEditor.OnExit += ToggleLevelChangeButtons;
    }
    void OnDisable(){
        LevelManager.OnLevelComplete -= LoadNextLevel;
        LevelEditor.OnExit -= UpdateObjectCount;
        LevelEditor.OnEnter -= ToggleLevelChangeButtons;
        LevelEditor.OnExit -= ToggleLevelChangeButtons;
    }

    public void LoadNextLevel(int curIndex){
        if(curLevelIndex >= levels.Length)  return;

        if( loadLevelCor != null )
            StopCoroutine(loadLevelCor);
            
        SetCurLevelIndex(curLevelIndex + 1);
        loadLevelCor = LoadLevelWithDelay(curLevelIndex, 0.2f);
        StartCoroutine(loadLevelCor);
    }

    public void LoadPreviousLevel(){
        if(curLevelIndex <= 1)  return;

        if( loadLevelCor != null )
            StopCoroutine(loadLevelCor);
            
        SetCurLevelIndex(curLevelIndex - 1);
        loadLevelCor = LoadLevelWithDelay(curLevelIndex, 0.2f);
        StartCoroutine(loadLevelCor);
    }

    private IEnumerator LoadLevelWithDelay(int curIndex, float delay = 0){
        yield return new WaitForSeconds(delay);    

        LoadLevel(curLevelIndex);

        loadLevelCor = null;
    }

    private void LoadLevel(int index){
        DestroyCurLevel();

        curLevel = null;
        curLevel = Instantiate(levels[index -1], Vector3.zero, Quaternion.identity);
        curLevel.gameObject.name = curLevel.name; //.Replace("(Clone)", "");
        Debug.Log("cur level name: " + curLevel.name);
        UpdateObjectCount();

        if(OnLevelLoad != null){
            
            OnLevelLoad();
        }

        //Debug.Log("node count after load:" + nodecount);
    }

    public void RestartCurLevel(){
        LoadLevel(curLevelIndex);
    }

    public static void ChangeNodeCount(int amount){
        nodecount += amount;

        if(OnNodeCountChange != null){
            OnNodeCountChange(curLevel.transform);
        }
        //Debug.Log("node count:" + nodecount);
        
        if(nodecount == 0 && OnLevelComplete != null){
            //Debug.Log("should load next level");
            OnLevelComplete(curLevelIndex);
        }
    }

    public static void ChangeArrowCount(int amount){
        arrowCount += amount;
    }

    public static int GetCurLevelIndex(){
        return curLevelIndex;
    }

    public static int GetNodeCount(){
        Transform levelTransform = curLevel.transform;
        int childCount = levelTransform.childCount;

        int nodeCount = 0;
        for (int i = 0; i < childCount; i++){
            GameObject child = levelTransform.GetChild(i).gameObject;
            if(child.activeInHierarchy && ((1<<child.layer) & LayerMask.GetMask("Node")) != 0){
                nodeCount++;
            }
        }

        return nodeCount;
    }

    public void SetCurLevelIndex(int levelIndex){
        if(levelIndex <= 1){
            previousLevelButton.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(true);
        }
        else if(levelIndex >= levels.Length){
            previousLevelButton.gameObject.SetActive(true);
            nextLevelButton.gameObject.SetActive(false);
        }
        else{
            previousLevelButton.gameObject.SetActive(true);
            nextLevelButton.gameObject.SetActive(true);         
        }
        
        if(levelIndex < 1 || levelIndex > levels.Length) return;

        curLevelIndex = levelIndex;
        if(OnCurLevelIndexChange != null){
            OnCurLevelIndexChange(curLevelIndex);
        }

    }

    public void DestroyCurLevel(){
        if(curLevel != null)
            Destroy(curLevel);
    }

    private void UpdateObjectCount(){
        Level level = curLevel.GetComponent<Level>();  
        level.UpdateObjectCount();
        nodecount = level.nodeCount;
        arrowCount = level.arrowCount;
        Debug.Log("node count:" + nodecount);
    }


    public void SaveLevelProperty(Transform level){
        
        LevelProperty levelProperty = new LevelProperty();
        levelProperty.levelName = level.name;
        levelProperty.nodeCount = 0;
        levelProperty.arrowCount = 0;

        int objCount = level.childCount;
        
        for (int i = 0; i < objCount; i++){
            Transform obj = level.GetChild(i);
            if( ((1<<obj.gameObject.layer) & LayerMask.GetMask("Node")) != 0 && obj.gameObject.activeSelf){
                Node node = obj.GetComponent<Node>();
                NodeProperty nodeP = new NodeProperty();
                
                nodeP.tag = obj.tag;
                nodeP.position = obj.position;
                nodeP.id = obj.gameObject.GetInstanceID();
                //nodeP.isLocked = node.isLocked;
                foreach (var arrow in node.arrowsFromThisNode){
                    nodeP.arrowsIDFromThisNode.Add(arrow.GetInstanceID());
                }
                foreach (var arrow in node.arrowsToThisNode){
                    nodeP.arrowsIDToThisNode.Add(arrow.GetInstanceID());
                }

                if (node.lockController && node.lockController.padLock)
                {
                    nodeP.padLockTag = node.lockController.padLock.tag;
                }

                if (node.lockController && node.lockController.key)
                {
                    nodeP.keyTag = node.lockController.key.tag;
                }
                
                levelProperty.nodes.Add(nodeP);

                levelProperty.nodeCount++;
            }
            else if( ((1<<obj.gameObject.layer) & LayerMask.GetMask("Arrow")) != 0 && obj.gameObject.activeSelf)
            {
                Arrow arrow = obj.GetComponent<Arrow>();
                LineRenderer lr = obj.GetComponent<LineRenderer>();
                ArrowProperty arrowP = new ArrowProperty();
                
                arrowP.tag = obj.tag;
                arrowP.position = obj.position;

                arrowP.id = obj.gameObject.GetInstanceID();
                arrowP.startingNodeID = arrow.startingNode.GetInstanceID();
                arrowP.destinationNodeID = arrow.destinationNode.GetInstanceID();

                arrowP.points = new SVector3[lr.positionCount];
                Vector3[] positions = new Vector3[lr.positionCount];

                lr.GetPositions( positions );

                for (int j = 0; j < positions.Length; j++){
                    arrowP.points[j] = positions[j];
                }

                levelProperty.arrows.Add(arrowP);
                levelProperty.arrowCount++;
            }
        }

        // Serialize level property
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + "/Basic Levels";
        
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        FileStream file = File.Create(path + "/" + levelProperty.levelName + ".save");
        bf.Serialize(file, levelProperty);
        file.Close();

        //string json = JsonUtility.ToJson(levelProperty);
        
    }

    public void LoadLevelProperty(string levelName, Transform levelParent){
        // Find objects in level
        Transform objects = levelParent;
        int childCount = objects.childCount;
        List<GameObject> childrenToDestroy = new List<GameObject>();
        nodesPool.Clear();
        arrowsPool.Clear();

        for (int i = 0; i < childCount; i++)
        {
            GameObject obj = objects.GetChild(i).gameObject;
            childrenToDestroy.Add(obj);
        }

        // Destroy inactive objects before saving the level
        foreach (var obj in childrenToDestroy)
        {
            DestroyImmediate(obj);
        }


        string filePath = Application.persistentDataPath + "/Basic Levels/" + levelName + ".save";
        if (File.Exists(filePath)){

            // Deserialize level property
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            
            LevelProperty levelProperty = (LevelProperty)bf.Deserialize(file);
            file.Close();

            //FileStream file = File.Open(filePath, FileMode.Open);
            
            //LevelProperty level = JsonUtility.FromJson<LevelProperty>()

            Debug.Log("should load level named " + levelName);
            Debug.Log("node count in loaded level: " + levelProperty.nodes.Count);
            Debug.Log("arrow count in loaded level: " + levelProperty.arrows.Count);

            // Create nodes and set properties
            foreach (var nodeProperty in levelProperty.nodes){
                PrefabAndPool prefabAndPool = GetPrefabAndPoolByTag(nodeProperty.tag);
                Transform obj = Instantiate(prefabAndPool.prefab, nodeProperty.position, Quaternion.identity).transform;
                obj.tag = nodeProperty.tag;
                obj.name = nodeProperty.id.ToString();
                obj.SetParent(levelParent);
                
                if (nodeProperty.padLockTag != null) // generate padlock if node has one
                {
                    GameObject prefab;
                    if (permanentPadLockPrefab.CompareTag(nodeProperty.padLockTag))
                    {
                        prefab = permanentPadLockPrefab;
                        obj.GetComponent<LockController>().GeneratePadLock(prefab);
                    }
                    else if (padLockPrefab.CompareTag(nodeProperty.padLockTag))
                    {
                        prefab = padLockPrefab;
                        obj.GetComponent<LockController>().GeneratePadLock(prefab);
                    }
                }
                
                if (nodeProperty.keyTag != null) // generate key if the node has one
                {
                    GameObject prefab;
                    if (permanentKeyPrefab.CompareTag(nodeProperty.keyTag))
                    {
                        prefab = permanentKeyPrefab;
                        obj.GetComponent<LockController>().GenerateKey(prefab);
                    }
                    else if (keyPrefab.CompareTag(nodeProperty.keyTag))
                    {
                        prefab = keyPrefab;
                        obj.GetComponent<LockController>().GenerateKey(prefab);
                    }
                }

                nodesPool.Add(obj.gameObject);
                
            }
            foreach (var arrowProperty in levelProperty.arrows){
                PrefabAndPool prefabAndPool = GetPrefabAndPoolByTag(arrowProperty.tag);
                Transform obj = Instantiate(prefabAndPool.prefab, arrowProperty.position, Quaternion.identity).transform;
                obj.tag = arrowProperty.tag;
                obj.name = arrowProperty.id.ToString();
                obj.SetParent(levelParent);
                LineRenderer lr = obj.GetComponent<LineRenderer>();
                lr.positionCount = arrowProperty.points.Length;
                Vector3[] positions = new Vector3[arrowProperty.points.Length];
                for (int i = 0; i < positions.Length; i++){
                    positions[i] = arrowProperty.points[i];
                }
                lr.SetPositions(positions);

                Arrow arrow = obj.GetComponent<Arrow>();
                arrow.SavePoints();

                arrow.FixHeadPos();
                arrow.FixCollider();

                arrowsPool.Add(obj.gameObject);
            }
            

            // Set links between objects
            foreach (var nodeProperty in levelProperty.nodes){
                //List<GameObject> pool = GetPrefabAndPoolByTag(nodeProperty.tag).pool;
                GameObject obj = FindObjInPool(nodeProperty.id.ToString(), nodesPool);
                
                Node node = obj.GetComponent<Node>();
                Debug.Log("to list count : " + nodeProperty.arrowsIDToThisNode.Count);
                for (int i = 0; i < nodeProperty.arrowsIDFromThisNode.Count; i++)
                {
                    node.AddToArrowsFromThisNodeList( FindObjInPool(nodeProperty.arrowsIDFromThisNode[i].ToString(), arrowsPool) );
                }
                
                for (int i = 0; i < nodeProperty.arrowsIDToThisNode.Count; i++)
                {
                    node.AddToArrowsToThisNodeList( FindObjInPool(nodeProperty.arrowsIDToThisNode[i].ToString(), arrowsPool) );
                }
                Debug.Log("here");
            }
            foreach (var arrowProperty in levelProperty.arrows){
                //List<GameObject> pool = GetPrefabAndPoolByTag(arrowProperty.tag).pool;
                GameObject obj = FindObjInPool(arrowProperty.id.ToString(), arrowsPool);
                
                Arrow arrow = obj.GetComponent<Arrow>();

                arrow.startingNode = FindObjInPool(arrowProperty.startingNodeID.ToString(), nodesPool);
                arrow.destinationNode = FindObjInPool(arrowProperty.destinationNodeID.ToString(), nodesPool);
            }

            nodecount = levelProperty.nodeCount;
            arrowCount = levelProperty.arrowCount;
        }
        else
        {
            Debug.Log("No level found with given name: " + levelName);
        }
    }

    private PrefabAndPool GetPrefabAndPoolByTag(string tag){
        PrefabAndPool prefabAndPool = new PrefabAndPool();

        if (tag == "BasicNode")
        {
            prefabAndPool.prefab = basicNode;
            //prefabAndPool.pool = nodesPool;
        }
        else if (tag == "Arrow")
        {
            prefabAndPool.prefab = arrow;
            //prefabAndPool.pool = arrowsPool;
        }
        else if (tag == "HexagonNode")
        {
            prefabAndPool.prefab = hexagonNode;
            //prefabAndPool.pool = nodesPool;
        }
        else if (tag == "StarNode")
        {
            prefabAndPool.prefab = lockedNode;
            //prefabAndPool.pool = nodesPool;
        }
        else if (tag == "SquareNode")
        {
            prefabAndPool.prefab = squareNode;
            //prefabAndPool.pool = nodesPool;
        }
        else if (tag == "PermanentArrow")
        {
            prefabAndPool.prefab = permanentArrow;
        }
            
        return prefabAndPool;
    }

    private GameObject FindObjInPool(string id, List<GameObject> pool){
        GameObject obj = null;
        
        foreach (var item in pool){
            //Debug.Log("-----------------------------");
            //Debug.Log("name: " + item.name + " =? " + "id: " + id);
            if(id == item.name){
                obj = item;
                break;
            }
        }
        
        return obj;
    }

    private void ToggleLevelChangeButtons()
    {
        if (previousLevelButton.gameObject.activeInHierarchy ||  nextLevelButton.gameObject.activeInHierarchy)
        {
            previousLevelButton.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            if(curLevelIndex <= 1){
                previousLevelButton.gameObject.SetActive(false);
                nextLevelButton.gameObject.SetActive(true);
            }
            else if(curLevelIndex >= levels.Length){
                previousLevelButton.gameObject.SetActive(true);
                nextLevelButton.gameObject.SetActive(false);
            }
            else{
                previousLevelButton.gameObject.SetActive(true);
                nextLevelButton.gameObject.SetActive(true);         
            }
        }
        

    }


    struct PrefabAndPool
    {
        public GameObject prefab;
        //public List<GameObject> pool;
    }

}
