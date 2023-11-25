using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public enum LevelPool
{
    Original, Player
}

public class LevelManager : MonoBehaviour{

    public DropdownHandler levelsDropdownHandler;
    public GameObject arrow;
    //public GameObject permanentArrow;
    public GameObject transporterArrow;
    public GameObject basicNode;
    public GameObject squareNode;
    public GameObject lockedNode;
    public GameObject hexagonNode;
    public GameObject padLockPrefab;
    public GameObject keyPrefab;
    public GameObject nodeSwapperPrefab;

    public GameObject[] levelPrefabs;
    public TextAsset[] levelTxts;
    public List<LevelProperty> originalLevels = new List<LevelProperty>();
    public List<LevelProperty> playerLevels = new List<LevelProperty>();
    //public LevelProperty[] curLevelPool;
    public List<LevelProperty> curLevelPool = new List<LevelProperty>();
    //public PlayerLevelsMetaData playerLevelsMetaData;
    public static GameObject curLevel;
    public LevelPool curPool;
    //public GameObject levelContainer;

    public static int curLevelIndex = 0;
    public int levelProgressIndex;

    public static int nodecount = 0;
    public static int arrowCount = 0;

    private string saveName = "save01.save";
    private string path = "Assets/Resources/Levels_txt/";
    private string backupPath = "Assets/Resources/Levels_txt Backup/";
    private string myLevelsPath = "/My Levels";

    private IEnumerator loadLevelCor = null;
    public List<GameObject> nodesPool = new List<GameObject>();
    public List<GameObject> arrowsPool = new List<GameObject>();
    private List<string> playerLevelsNames = new List<string>();
    private List<string> originalLevelsNames = new List<string>();

    private bool startIncreasingLevelIndex;
    private bool startDecreasingLevelIndex;
    private float time = 0;
    public float changeLevelIndexDur = 1f;
    private float defChangeLevelIndexDur;

    public delegate void OnCurLevelIndexChangeDelegate(int curIndex);
    public static OnCurLevelIndexChangeDelegate OnCurLevelIndexChange;

    public delegate void OnLevelLoadDelegate();
    public static OnLevelLoadDelegate OnLevelLoad;

    public delegate void OnNodeCountChangeDelegate(Transform curLevel);
    public static OnNodeCountChangeDelegate OnNodeCountChange;

    public delegate void OnLevelPoolChangedDelegate(List<string> levelNames);
    public static OnLevelPoolChangedDelegate OnLevelPoolChanged;

    private void Awake()
    {
        LoadPlayerLevels();

        originalLevelsNames.Clear();
        for (int i = 0; i < levelTxts.Length; i++)
        {
            originalLevels.Add(JsonUtility.FromJson<LevelProperty>(levelTxts[i].text));
            originalLevelsNames.Add(originalLevels[i].levelName);
        }

        SetCurLevelPool(LevelPool.Original);
    }

    void Start(){
        // Get progression data
        GetAndSetProgressionData();


        //LoadLevel(curLevelIndex);
        LoadLevelWithIndex(curLevelIndex); // "multiple square test"

        defChangeLevelIndexDur = changeLevelIndexDur;

    }

    void OnEnable(){
        GameManager.OnLevelComplete += LoadNextLevel;
        LevelEditor.OnEnter += LoadPlayerLevels;
        LevelEditor.OnExit += UpdateObjectCount;
        levelsDropdownHandler.OnValueChanged += SetCurLevelIndex;
        levelsDropdownHandler.OnValueChanged += LoadLevelWithIndex;
    }
    void OnDisable(){
        GameManager.OnLevelComplete -= LoadNextLevel;
        LevelEditor.OnExit -= UpdateObjectCount;
        LevelEditor.OnEnter -= LoadPlayerLevels;
        levelsDropdownHandler.OnValueChanged -= SetCurLevelIndex;
        levelsDropdownHandler.OnValueChanged -= LoadLevelWithIndex;
    }

    private void Update()
    {

        int amount;
        if (startDecreasingLevelIndex)
        {
            amount = -1;
        }
        else if (startIncreasingLevelIndex)
        {
            amount = 1;
        }
        else
        {
            return;
        }

        time += Time.deltaTime;
        if (time >= changeLevelIndexDur)
        {
            ChangeCurLevelIndex(amount);

            changeLevelIndexDur = changeLevelIndexDur >= 0.05f ? changeLevelIndexDur / 1.5f : 0.05f;
            time = 0;
        }
    }

    public void SwitchLevelPool()
    {
        if(curPool == LevelPool.Original)
        {
            OpenPlayerLevels();
        }
        else
        {
            OpenOriginalLevels();
        }
    }

    public void OpenPlayerLevels()
    {
        if (playerLevels.Count == 0) return;

        SetCurLevelPool(LevelPool.Player);
        SetCurLevelIndex(0);
        LoadLevelWithIndex(0);

        Debug.Log("cur level index: " + curLevelIndex);
    }

    public void OpenOriginalLevels()
    {
        if (originalLevels.Count == 0 | curPool == LevelPool.Original) return;

        SetCurLevelPool(LevelPool.Original);
        GetAndSetProgressionData();
        LoadLevelWithIndex(curLevelIndex);

        Debug.Log("cur level index: " + curLevelIndex);
    }

    private void LoadProgressionData()
    {
        SaveData saveData = (SaveData)Utility.BinaryDeserialization("/", saveName);
        levelProgressIndex = saveData.levelProgressIndex;
        SetCurLevelIndex(levelProgressIndex);
    }

    private void SaveProgressionData()
    {
        SaveData saveData = new SaveData();
        saveData.levelProgressIndex = curLevelIndex;
        levelProgressIndex = curLevelIndex;
        Utility.BinarySerialization("", saveName, saveData);
    }
    private void GetAndSetProgressionData()
    {
        if (File.Exists(Application.persistentDataPath + "/" + saveName))
        {
            LoadProgressionData();
            Debug.Log("save data loaded");
        }
        else
        {
            // Create default level data
            curLevelIndex = 0;
            levelProgressIndex = 0;
            SaveProgressionData();
            SetCurLevelIndex(curLevelIndex);
            Debug.Log("Save Created");
        }
    }

    public void LoadNextLevel(float delay){
        if(GameState.gameState == GameState_EN.testingLevel)    return;
        if(curLevelIndex >= curLevelPool.Count -1)              return;

        if( loadLevelCor != null )
            StopCoroutine(loadLevelCor);
            
        SetCurLevelIndex(curLevelIndex + 1);
        loadLevelCor = LoadLevelWithDelay(curLevelIndex, delay);
        StartCoroutine(loadLevelCor);

        if (curLevelIndex > levelProgressIndex && curPool == LevelPool.Original)
            SaveProgressionData();
    }

    public void LoadPreviousLevel(){
        if(curLevelIndex <= 0)  return;

        if( loadLevelCor != null )
            StopCoroutine(loadLevelCor);
            
        SetCurLevelIndex(curLevelIndex - 1);
        loadLevelCor = LoadLevelWithDelay(curLevelIndex, 0.2f);
        StartCoroutine(loadLevelCor);
    }

    private IEnumerator LoadLevelWithDelay(int curIndex, float delay = 0){
        yield return new WaitForSeconds(delay);

        //LoadLevel(curLevelIndex);
        LoadLevelWithIndex(curIndex);

        loadLevelCor = null;
    }

    private void LoadLevelWithPrefab(string name){

        int index = -1;
        for (int i = 0; i < curLevelPool.Count; i++)
        {
            LevelProperty level = curLevelPool[i];
            if (level.levelName == name)
            {
                index = i;
                break;
            }
        }
        if(index == -1)
        {
            Debug.Log("Level couldn't found, name: " + name);
            return;
        }

        DestroyCurLevel();
        SetCurLevelIndex(index);
        curLevel = null;
        curLevel = Instantiate(levelPrefabs[index], Vector3.zero, Quaternion.identity);
        curLevel.gameObject.name = curLevel.name; //.Replace("(Clone)", "");
        Debug.Log("cur level name: " + curLevel.name);
        UpdateObjectCount();

        if(OnLevelLoad != null){
            
            OnLevelLoad();
        }
        
    }

    public void RestartCurLevel(){
        LoadLevelWithIndex(curLevelIndex);
        
        //LoadLevel(curLevelIndex);
    }

    public void LoadLevelWithIndex(int index = 0)
    {
        DestroyCurLevel();
        string levelName = curLevelPool[index].levelName;
        Transform levelHolder = GenerateNewLevelHolder(levelName);

        try
        {
            LoadLevelProperty(levelName, levelHolder);
            curLevel = levelHolder.gameObject;
            Debug.Log("level loaded with Deserialization");
        }
        catch (System.Exception)
        {
            LoadLevelWithPrefab(levelName);
            //SaveLevelProperty(curLevel.transform);
            //Destroy(curLevel);
            //levelContainer = new GameObject("level").transform;
            //LoadLevelProperty(levelName, level);
            Debug.Log("level loaded with prefab");
            throw;
        }

        if (OnLevelLoad != null)
        {
            OnLevelLoad();
        }
    }

    public void LoadCurLevelWithDeserialization()
    {
        if (loadLevelCor != null)
            StopCoroutine(loadLevelCor);

        loadLevelCor = LoadLevelWithDelay(curLevelIndex, 0.2f);
        StartCoroutine(loadLevelCor);
    }

    public static void ChangeNodeCount(int amount){
        nodecount += amount;

        if(OnNodeCountChange != null){
            OnNodeCountChange(curLevel.transform);
        }
        //Debug.Log("node count:" + nodecount);
        
    }

    public static void ChangeArrowCount(int amount){
        arrowCount += amount;
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
        Debug.Log("cur level pool count : " + curLevelPool.Count);
        Debug.Log("level index : " + levelIndex);
        if (levelIndex < 0 || levelIndex > curLevelPool.Count - 1) return;

        curLevelIndex = levelIndex;
        if(OnCurLevelIndexChange != null){
            OnCurLevelIndexChange(curLevelIndex);
        }
    }

    public void ChangeCurLevelIndex(int amount)
    {
        SetCurLevelIndex(curLevelIndex + amount);
    }
    public static int GetCurLevelIndex()
    {
        return curLevelIndex;
    }

    public void DestroyCurLevel(){
        if(curLevel != null)
        {
            Destroy(curLevel);
        }
    }

    public Transform GenerateNewLevelHolder(string levelName)
    {
        Transform levelHolder = new GameObject(levelName).transform; //Instantiate(levelContainer, Vector3.zero, Quaternion.identity).transform;

        levelHolder.gameObject.AddComponent<Level>();

        levelHolder.transform.position = Vector3.zero;
        curLevel = levelHolder.gameObject;
        return levelHolder;
    }

    private void UpdateObjectCount(){
        Level level = curLevel.GetComponent<Level>();  
        level.UpdateObjectCount();
        nodecount = level.nodeCount;
        arrowCount = level.arrowCount;
        Debug.Log("node count:" + nodecount);
    }


    public void SaveLevelProperty(Transform level, bool saveAsBackup = false){

        LevelProperty levelProperty = CreateLevelProperty(level);

        // Serialize level property

        string fullPath;
        fullPath = Application.persistentDataPath + myLevelsPath + levelProperty.levelName + ".txt";
        #if UNITY_EDITOR
            if (saveAsBackup)
            {
                fullPath = this.backupPath + levelProperty.levelName + ".txt";
            }
            else
            {
                fullPath = this.path + levelProperty.levelName + ".txt";
            }
        #endif

        Utility.SaveAsJson(fullPath, levelProperty);
        /*BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + "/Basic Levels";
        
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        FileStream file = File.Create(path + "/" + levelProperty.levelName + ".save");
        bf.Serialize(file, levelProperty);
        file.Close();
        */
        //string json = JsonUtility.ToJson(levelProperty);

    }

    public LevelProperty CreateLevelProperty(Transform level)
    {
        level.name = level.name.Replace("(Clone)", "");

        LevelProperty levelProperty = new LevelProperty();
        levelProperty.levelName = level.name;
        levelProperty.nodeCount = 0;
        levelProperty.arrowCount = 0;

        int objCount = level.childCount;

        for (int i = 0; i < objCount; i++)
        {
            Transform obj = level.GetChild(i);
            if (((1 << obj.gameObject.layer) & LayerMask.GetMask("Node")) != 0 && obj.gameObject.activeSelf)
            {
                Node node = obj.GetComponent<Node>();
                NodeProperty nodeP = new NodeProperty();

                nodeP.tag = node.isPermanent ? "p," + obj.tag : obj.tag;
                nodeP.posX = obj.position.x;
                nodeP.posY = obj.position.y;
                nodeP.id = obj.gameObject.GetInstanceID();

                foreach (var item in node.itemController.itemContainer.items)
                {
                    if (item.CompareTag("AddNewItem")) continue;

                    string tag = item.isPermanent ? "p," + item.tag : item.tag;

                    nodeP.itemTags.Add(tag);
                }

                levelProperty.nodes.Add(nodeP);

                levelProperty.nodeCount++;
            }
            else if (((1 << obj.gameObject.layer) & LayerMask.GetMask("Arrow")) != 0 && obj.gameObject.activeSelf)
            {
                Arrow arrow = obj.GetComponent<Arrow>();
                LineRenderer lr = obj.GetComponent<LineRenderer>();
                ArrowProperty arrowP = new ArrowProperty();

                arrowP.tag = arrow.isPermanent ? "p," + obj.tag : obj.tag;
                //arrowP.position = obj.position;

                arrowP.id = obj.gameObject.GetInstanceID();
                arrowP.startingNodeID = arrow.startingNode.GetInstanceID();
                arrowP.destinationNodeID = arrow.destinationNode.GetInstanceID();

                arrowP.pointsX = new float[lr.positionCount];
                arrowP.pointsY = new float[lr.positionCount];
                Vector3[] positions = new Vector3[lr.positionCount];

                lr.GetPositions(positions);

                for (int j = 0; j < positions.Length; j++)
                {
                    arrowP.pointsX[j] = positions[j].x;
                    arrowP.pointsY[j] = positions[j].y;
                }

                levelProperty.arrows.Add(arrowP);
                levelProperty.arrowCount++;

                if (arrow.CompareTag("TransporterArrow"))
                {
                    arrowP.priority = arrow.GetComponent<Transporter>().priority;
                }
            }
        }

        return levelProperty;
    }

    public string SerializeLevelAsJson(Transform level)
    {
        LevelProperty levelProperty = CreateLevelProperty(level);

        return Utility.JsonSerialization(levelProperty);
    }

    public void LoadLevelWithName(string levelName)
    {
        LoadLevelProperty(levelName, transform);
    }

    public void LoadLevelProperty(string levelName, Transform levelParent){
        // Find objects in level
        Transform objects = levelParent;
        int childCount = objects.childCount;
        List<GameObject> childrenToDestroy = new List<GameObject>();


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

        LevelProperty levelProperty = null;
        foreach (var item in curLevelPool)
        {
            if(item.levelName == levelName)
            {
                levelProperty = item;
                break;
            }
        }

        if (levelProperty != null)
        {
            LoadLevelWithLevelProperty(levelProperty, levelName, levelParent);
        }
        else
        {
            Debug.Log("No level found with given name: " + levelName);
            throw new System.Exception();
        }
        ///string filePath = this.path + levelName + ".txt";
        //if (File.Exists(filePath)){

        // Deserialize level property
        /*BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);
        LevelProperty levelProperty = (LevelProperty)bf.Deserialize(file);
        file.Close();*/


        //string fullPath = this.path + levelName + ".txt";
        //LevelProperty levelProperty = Utility.LoadLevePropertyFromJson(fullPath);

        //FileStream file = File.Open(filePath, FileMode.Open);

        //LevelProperty level = JsonUtility.FromJson<LevelProperty>()

        //LoadLevelWithLevelProperty(levelProperty, levelName, levelParent);
        //}
    }

    public void LoadLevelWithLevelProperty(LevelProperty levelProperty, string levelName, Transform levelParent)
    {
        Debug.Log("should load level named " + levelName);
        Debug.Log("node count in loaded level: " + levelProperty.nodes.Count);
        Debug.Log("arrow count in loaded level: " + levelProperty.arrows.Count);

        nodesPool.Clear();
        arrowsPool.Clear();

        // Create nodes and set properties
        foreach (var nodeProperty in levelProperty.nodes)
        {
            PrefabAndPool prefabAndPool = GetPrefabAndPoolByTag(nodeProperty.tag.Replace("p,", ""));
            Vector3 pos = new Vector3(nodeProperty.posX, nodeProperty.posY, 0);
            Transform obj = Instantiate(prefabAndPool.prefab, pos, Quaternion.identity).transform;
            //obj.tag = nodeProperty.tag;
            obj.name = nodeProperty.id.ToString();
            obj.SetParent(levelParent);
            if (nodeProperty.tag.Contains("p,"))
            {
                Debug.Log("should set permanent item : " + tag);
                Node node = obj.GetComponent<Node>();
                node.ChangePermanent(true);
            }
            GameObject prefab;
            // Generate items that this node have
            for (int i = 0; i < nodeProperty.itemTags.Count; i++)
            {
                string tag = nodeProperty.itemTags[i];
                prefab = GetPrefabAndPoolByTag(tag.Replace("p,", "")).prefab;

                GameObject itemObj = obj.GetComponent<ItemController>().GenerateItem(prefab);
                if (tag.Contains("p,"))
                {
                    Debug.Log("should set permanent item : " + tag);
                    Item item = itemObj.GetComponent<Item>();
                    item.ChangePermanent(true);
                }
                else
                {
                    Debug.Log(" item is not permanent : " + tag);
                }
            }

            nodesPool.Add(obj.gameObject);
        }

        foreach (var arrowProperty in levelProperty.arrows)
        {
            PrefabAndPool prefabAndPool = GetPrefabAndPoolByTag(arrowProperty.tag.Replace("p,", ""));
            Transform obj = Instantiate(prefabAndPool.prefab, Vector3.zero, Quaternion.identity).transform;
            //obj.tag = arrowProperty.tag;    //.Split(",")[1];
            obj.name = arrowProperty.id.ToString();
            obj.SetParent(levelParent);
            LineRenderer lr = obj.GetComponent<LineRenderer>();
            lr.positionCount = arrowProperty.pointsX.Length;
            Vector3[] positions = new Vector3[arrowProperty.pointsX.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = new Vector3(arrowProperty.pointsX[i], arrowProperty.pointsY[i], 0);
            }
            lr.SetPositions(positions);

            Arrow arrow = obj.GetComponent<Arrow>();
            arrow.SavePoints();

            arrow.FixHeadPos();
            arrow.FixCollider();

            if (arrowProperty.tag.Contains("p,"))
            {
                Debug.Log("should set permanent item : " + tag);
                arrow.ChangePermanent(true);
            }

            if (arrow.CompareTag("TransporterArrow"))
            {
                arrow.GetComponent<Transporter>().priority = arrowProperty.priority;
            }

            arrowsPool.Add(obj.gameObject);
        }

        // Set links between objects
        foreach (var arrowProperty in levelProperty.arrows)
        {
            //List<GameObject> pool = GetPrefabAndPoolByTag(arrowProperty.tag).pool;
            GameObject obj = FindObjInPool(arrowProperty.id.ToString(), arrowsPool);

            Arrow arrow = obj.GetComponent<Arrow>();

            Node startingNode = FindObjInPool(arrowProperty.startingNodeID.ToString(), nodesPool).GetComponent<Node>();
            Node destinationNode = FindObjInPool(arrowProperty.destinationNodeID.ToString(), nodesPool).GetComponent<Node>();

            arrow.startingNode = startingNode.gameObject;
            arrow.destinationNode = destinationNode.gameObject;
            startingNode.AddToArrowsFromThisNodeList(obj);
            destinationNode.AddToArrowsToThisNodeList(obj);
        }

        nodecount = levelProperty.nodeCount;
        arrowCount = levelProperty.arrowCount;
    }

    public void LoadPlayerLevels()
    {
        // Loads levels from my levels
        string[] playerLevelsPaths = Directory.GetFiles(Application.persistentDataPath + myLevelsPath + "/", "*.txt");

        //myLevels = new LevelProperty[playerLevelsPaths.Length];
        playerLevelsNames = new List<string>();
        for (int i = 0; i < playerLevelsPaths.Length; i++)
        {
            LevelProperty level = Utility.LoadLevePropertyFromJson(playerLevelsPaths[i]);
            playerLevels.Add(level);
            playerLevelsNames.Add(level.levelName);
        }
    }

    public void SetCurLevelPool(LevelPool value)
    {
        curPool = value;

        curLevelPool = value == LevelPool.Original ? originalLevels : playerLevels;

        if (OnLevelPoolChanged != null)
        {
            OnLevelPoolChanged(GetCurLevelsNameList());
        }
    }

    public List<string> GetCurLevelsNameList()
    {
        List<string> value = curPool == LevelPool.Original ? originalLevelsNames : playerLevelsNames;

        return value;
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
        else if (tag == "TransporterArrow")
        {
            prefabAndPool.prefab = transporterArrow;
        }
        else if (tag == "Key")
        {
            prefabAndPool.prefab = keyPrefab;
        }
        else if (tag == "Padlock")
        {
            prefabAndPool.prefab = padLockPrefab;
        }
        else if (tag == "NodeSwapper")
        {
            prefabAndPool.prefab = nodeSwapperPrefab;
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

    public void StartIncreasingLevelIndex()
    {
        startIncreasingLevelIndex = true;
        changeLevelIndexDur = defChangeLevelIndexDur;
        time = 0;
    }

    public void StopIncreasingLevelIndex()
    {
        startIncreasingLevelIndex = false;
    }

    public void StartDecreasingLevelIndex()
    {
        startDecreasingLevelIndex = true;
        changeLevelIndexDur = defChangeLevelIndexDur;
        time = 0;
    }

    public void StopDecreasingLevelIndex()
    {
        startDecreasingLevelIndex = false;
    }


    struct PrefabAndPool
    {
        public GameObject prefab;
        //public List<GameObject> pool;
    }

}
