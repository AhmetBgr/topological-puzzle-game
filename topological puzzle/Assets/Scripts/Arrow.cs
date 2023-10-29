using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class Arrow : MonoBehaviour{

    public GameObject startingNode;
    public GameObject destinationNode;
    //public Material defultMaterial;
    public ArrowCC arrowColorController;

    public bool isPermanent = false;

    private GameManager gameManager;
    private LineRenderer lr;
    private Transform head;
    private EdgeCollider2D col;

    private List<ChangeArrowDir> changeDirCommands = new List<ChangeArrowDir>();
    //private Material material;

    private IEnumerator widthAnim;
    private IEnumerator RemoveCor;
    private Vector3[] linePoints;
    private int pointsCount;
    private float defWidth = 0.05f;
    
    //private float glowIntensity1 = 2f;
    //private float glowIntensity2 = 7f;
    //private bool animatingWidth = false;

    // Start is called before the first frame update
    void Awake(){
        head = transform.GetChild(0);
        lr = gameObject.GetComponent<LineRenderer>();
        col = GetComponent<EdgeCollider2D>();
        lr.startWidth = defWidth;
        SavePoints();
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnEnable(){
        Node.OnNodeRemove += ChangeDirIfLinkedToStar;
        Node.OnNodeAdd += UndoChangeDirIfLinkedToStar;

        GameManager.OnCurCommandChange += CheckIfSuitable;
        LevelManager.OnLevelLoad += GetOnTheLevel;
    }

    void OnDisable(){
        Node.OnNodeRemove -= ChangeDirIfLinkedToStar;
        Node.OnNodeAdd -= UndoChangeDirIfLinkedToStar;

        GameManager.OnCurCommandChange -= CheckIfSuitable;
        LevelManager.OnLevelLoad -= GetOnTheLevel;
    }
    void OnMouseEnter(){
        if(widthAnim != null){
            StopCoroutine(widthAnim);
        }

        head.DOScale(Vector3.one * 1.6f, 0.2f);
        widthAnim = ChangeWidth(defWidth + 0.1f, 0.2f);
        
        StartCoroutine(widthAnim);
    }

    void OnMouseExit(){
        if(widthAnim != null){
            StopCoroutine(widthAnim);
        }
        head.DOScale(Vector3.one, 0.2f);
        widthAnim = ChangeWidth(defWidth, 0.2f);
        StartCoroutine(ChangeWidth(defWidth, 0.2f));
    }

    // Changes direction on any other node removed if the starting node of this arrow is a star node
    private void ChangeDirIfLinkedToStar(GameObject removedNode)
    {
        if (removedNode == startingNode) return;
        if(startingNode.CompareTag("HexagonNode") || destinationNode.CompareTag("HexagonNode")){
            if (!(startingNode.CompareTag("HexagonNode") && destinationNode.CompareTag("HexagonNode")))
            {
                ChangeArrowDir changeDirCommand = new ChangeArrowDir(gameManager, Commands.RemoveNode, LayerMask.GetMask("Node"), false);
                changeDirCommand.Execute(new List<GameObject>(){gameObject});
                changeDirCommands.Add(changeDirCommand);
            }
                
        }
    }
    
    private void UndoChangeDirIfLinkedToStar(GameObject removedNode, bool skipPermanent)
    {
        if (changeDirCommands.Count == 0) return;
        if (removedNode == startingNode)
        {
            return;
        }
        
        if(startingNode.CompareTag("HexagonNode") || destinationNode.CompareTag("HexagonNode")){
            if (!(startingNode.CompareTag("HexagonNode") && destinationNode.CompareTag("HexagonNode")))
            {
                ChangeArrowDir lastChangeDirCommand = changeDirCommands[changeDirCommands.Count - 1];
                lastChangeDirCommand.Undo(skipPermanent);
                changeDirCommands.Remove(lastChangeDirCommand);
            }
                
        }
    }
    
    public void Remove(){ //GameObject node
        LevelManager.ChangeArrowCount(-1);
        destinationNode.GetComponent<Node>().RemoveFromArrowsToThisNodeList(gameObject);
        RemoveCor = DisappearAnim(0.6f, onCompleteCallBack: () =>
        {
            DisableObject();
        });
        StartCoroutine(RemoveCor);
        /*if(node == startingNode){
            //Debug.Log("starting node equls to selected node");
            LevelManager.ChangeArrowCount(-1);
            destinationNode.GetComponent<Node>().RemoveFromArrowsToThisNodeList(gameObject);
            RemoveCor = DisappearAnim(0.6f, onCompleteCallBack: () =>
            {
                DisableObject();
            });
            StartCoroutine(RemoveCor);

        }
        else if(startingNode.CompareTag("HexagonNode") || destinationNode.CompareTag("HexagonNode")){
            if(!( startingNode.CompareTag("HexagonNode") && destinationNode.CompareTag("HexagonNode") ))
                ChangeDir(); //gameObject
        }*/
    }

    public void Add(){//GameObject node
        
        /*if(node == startingNode && isPermanent){ 
            gameObject.SetActive(false);
            return; 
        }
        */
        if(RemoveCor != null)
            StopCoroutine(RemoveCor);
        destinationNode.GetComponent<Node>().AddToArrowsToThisNodeList(gameObject);
        StartCoroutine(AppearAnim(0.4f, 0.4f, () => {
            LevelManager.ChangeArrowCount(+1);
        }));
        
        /*if(node == startingNode){
            if(RemoveCor != null)
                StopCoroutine(RemoveCor);
            destinationNode.GetComponent<Node>().AddToArrowsToThisNodeList(gameObject);
            StartCoroutine(AppearAnim(0.4f, 0.4f, () => {
                LevelManager.ChangeArrowCount(+1);
            }));
        }
        else if(!isPermanent && (startingNode.CompareTag("HexagonNode") || destinationNode.CompareTag("HexagonNode"))){
            if (!(startingNode.CompareTag("HexagonNode") && destinationNode.CompareTag("HexagonNode")))
            {
                if(RemoveCor != null)
                    StopCoroutine(RemoveCor);
                ChangeDir(); //gameObject
            }

        }*/
        
        /*if(isMagical){
            ChangeDir(gameObject, 0.8f);
        }   */     

        
    }

    private void GetOnTheLevel(){
        //transform.localScale = Vector3.zero;
        //head.localScale = Vector3.zero;
        //head.DOScale(Vector3.one, 1f);
        float duration = UnityEngine.Random.Range(0.2f, 0.6f);
        StartCoroutine(AppearAnim(duration, 0f));
    }

    public void ChangeDirOnUndo(GameObject arrow){
        if(isPermanent) return;
        ChangeDir(); //arrow
    }

    public void ChangeDirSubscriber(GameObject arrow){
        //if(isPermanent){return;}

        ChangeDir(); //arrow

       /* if(isMagical){
            ChangeDir(gameObject, 0.8f);
        }   */
    }

    private void CheckIfSuitable(LayerMask targetLM, int targetIndegree, bool bypass){
        if ( (((1<<gameObject.layer) & targetLM) != 0) || bypass){
            // Highlight
            arrowColorController.Highlight(arrowColorController.glowIntensityHigh, 1f);
            col.enabled = true;
        }
        else{
            // Not selectable
            arrowColorController.Highlight(arrowColorController.glowIntensityMedium, 1f);
            col.enabled = false;
        }   
    }

    public void ChangeDir(float delay = 0f){ //GameObject arrow, 
        //if(arrow != gameObject) return;

        col.enabled = false;

        /*head.DOScale(Vector3.zero, 0.5f);
        StartCoroutine(ChangeWidth(0.08f, 0.5f, 0f, () => {
            Debug.Log("on complete working");
            InvertPoints();
            StartCoroutine(ChangeWidth(defWidth, 0.5f));
            head.position = linePoints[linePoints.Length - 1];
            head.DOScale(Vector3.one, 0.5f);
            col.enabled = true;
        }));*/

        Node node1 = startingNode.GetComponent<Node>();
        Node node2 = destinationNode.GetComponent<Node>();

        node1.RemoveFromArrowsFromThisNodeList(gameObject);
        node1.AddToArrowsToThisNodeList(gameObject);

        node2.RemoveFromArrowsToThisNodeList(gameObject);
        node2.AddToArrowsFromThisNodeList(gameObject);

        GameObject temp = startingNode;
        startingNode = destinationNode;
        destinationNode = temp;
        
        
        StartCoroutine(DisappearAnim(0.4f, delay, onCompleteCallBack : () => {
            InvertPoints();
            StartCoroutine(AppearAnim(0.4f));
            //col.enabled = true;
        }));
    }


    private IEnumerator DisappearAnim(float duration, float delay = 0f, Action onCompleteCallBack = null){
        yield return new WaitForSeconds(delay);

        int piece = (1*pointsCount - 1 );
        //piece =  (2*pointsCount - 3 );
        float segmentDuration = duration / piece ;
        //Debug.Log("here1");

        for (int i = pointsCount - 1; i >= 1 ; i--) {
            //Debug.Log("here2");
            float startTime = Time.time;

            Vector3 startPosition = linePoints [ i - 1 ];
            Vector3 endPosition = linePoints [ i ];

            Vector3 pos = endPosition ;


            while (pos != startPosition) {
                float t = (Time.time - startTime) / segmentDuration ;
                //Debug.Log("here3");
                pos = Vector3.Lerp (endPosition, startPosition, t) ;
                Vector3 pos2 = Vector3.zero;
                if(i>=2)
                    pos2 = Vector3.Lerp(linePoints[i-1], linePoints[i-2], t);


                // animate all other points except point at index i
                for (int j = i; j < pointsCount; j++){
                    //Debug.Log("here4");
                    lr.SetPosition (j, pos);

                    if(i>= 2){
                        lr.SetPosition(j-1, pos2);
                        
                        Vector3 lookAt = lr.GetPosition(j-2); // world pos
                        float AngleRad = Mathf.Atan2( head.position.y - lookAt.y,  head.position.x - lookAt.x);
                        float AngleDeg = (180 / Mathf.PI) * AngleRad;
                        head.rotation = Quaternion.Euler(0, 0, AngleDeg);
                    }
                    head.position = pos;
                }
                yield return null ;
            }
        }
        if(onCompleteCallBack != null)
            onCompleteCallBack();
    }

    private IEnumerator AppearAnim(float duration, float delay = 0f, Action OnComplete = null){
        head.transform.localScale = Vector3.zero;
        head.transform.position = linePoints[0];
        //transform.position = new Vector3(0f, -2000f, 0f);
        float dur = delay == 0f ?  0.3f : delay;
        //transform.DOMove(Vector3.zero, 0f).SetDelay(delay);
        head.transform.DOScale(Vector3.one, dur).SetDelay(0.1f);
        
        lr.enabled = false;

        yield return new WaitForSeconds(delay);

        lr.enabled = true;

        int piece = (1*pointsCount - 1 );
        //piece =  (2*pointsCount - 3 );
        float segmentDuration = duration / piece ;

        for (int i = 0; i < pointsCount - 1; i++) {
            float startTime = Time.time;

            Vector3 startPosition = linePoints [ i ];
            Vector3 endPosition = linePoints [ i + 1 ];

            Vector3 pos = startPosition ;

            Vector3 lookAt = startPosition; // world pos
            float AngleRad = Mathf.Atan2( endPosition.y - lookAt.y,  endPosition.x - lookAt.x);
            float AngleDeg = (180 / Mathf.PI) * AngleRad;
            //head.rotation = Quaternion.Euler(0, 0, AngleDeg);

            bool headRotationChanged = head.rotation == Quaternion.Euler(0, 0, AngleDeg) ? false : true;
            if(headRotationChanged){
                head.DORotate(Quaternion.Euler(0, 0, AngleDeg).eulerAngles, 0.15f);
            }

            while (pos != endPosition) {

                float t = (Time.time - startTime) / segmentDuration ;
                pos = Vector3.Lerp (startPosition, endPosition, t) ;

                // animate all other points except point at index i
                for (int j = i + 1; j < pointsCount; j++){
                    lr.SetPosition (j, pos) ;
                    
                }
                head.position = pos; //lr.GetPosition(pointsCount -1)
                

                yield return null ;
            }
        }
        if(OnComplete != null)
            OnComplete();
    }

    private IEnumerator ChangeWidth(float targetWidth, float duration, float delay = 0f, Action OnComplete = null){
        //animatingWidth = true;
        yield return new WaitForSeconds(delay);

        float initialTime = Time.time;
        float initialWidth = lr.startWidth;

        while(lr.startWidth != targetWidth){
            float t = (Time.time - initialTime) / duration;
            float width = Mathf.Lerp(initialWidth, targetWidth, t);
            lr.startWidth = width;

            yield return null;
        }
        //animatingWidth = false;
        widthAnim = null;
        if(OnComplete != null) 
            OnComplete();
    }

    public void SavePoints(){
        // Store a copy of lineRenderer's points in linePoints array
        pointsCount = lr.positionCount;
        linePoints = new Vector3[pointsCount] ;
        for (int i = 0; i < pointsCount; i++) {
            linePoints [ i ] = lr.GetPosition (i) ;
        }
    }

    private void InvertPoints(){

        for (int i = 0; i < linePoints.Length/2; i++){
            if(i == 0){

            }

            Vector3 temp;
            temp = linePoints[i];
            linePoints[i] = linePoints[linePoints.Length - 1 - i];
            linePoints[linePoints.Length - 1 - i] = temp;     
        }

        // Carries the gap for the arrow's head 
        for (int i = 0; i < linePoints.Length; i += linePoints.Length - 1){
            
            Vector3 from = linePoints[ Mathf.Abs(1 - i) ]; // next point pos after the first one or previous point pos before the last one, depends on i
            Vector3 to = linePoints[i]; // first or last point pos depends on i
            Vector3 dir = (from - to);

            float segmentLength = dir.magnitude;
            float gap = i == 0 ? 0.16f : -0.16f;
            segmentLength += gap; 

            linePoints[i] = from - (dir.normalized*segmentLength);
        }

        for (int i = 0; i < linePoints.Length; i++){
            lr.SetPosition(i, linePoints[0]);
        }
        //transform.position = linePoints[0];
        
    }

    private void DisableObject(){
        gameObject.SetActive(false);
    }

    private IEnumerator DisappearAnim2(float duration){

        int piece = (1*pointsCount - 1 );
        //piece =  (2*pointsCount - 3 );
        
        //Debug.Log("here1");
        float totalLenght = 0f;
        for (int i = pointsCount - 1; i > 0; i--){
            totalLenght += (linePoints[i] - linePoints[i - 1]).magnitude;
        }
        
        for (int i = pointsCount - 1; i >= 1 ; i--) {
            //Debug.Log("here2");
            float startTime = Time.time;

            Vector3 startPosition = linePoints [ i - 1 ];
            Vector3 endPosition = linePoints [ i ];
            Vector3 pos = endPosition;

            float pieceLength = (endPosition - startPosition).magnitude;
            float segmentDuration = duration * (pieceLength / totalLenght) ;

            Vector3 lookAt = startPosition; // world pos
            float AngleRad = Mathf.Atan2( endPosition.y - lookAt.y,  endPosition.x - lookAt.x);
            float AngleDeg = (180 / Mathf.PI) * AngleRad;
            //head.rotation = Quaternion.Euler(0, 0, AngleDeg);

            bool headRotationChanged = head.rotation == Quaternion.Euler(0, 0, AngleDeg) ? false : true;
            if(headRotationChanged){
                head.DORotate(Quaternion.Euler(0, 0, AngleDeg).eulerAngles, 0.15f);
            }

            while (pos != startPosition) {
                float t = (Time.time - startTime) / segmentDuration ;
                //Debug.Log("here3");
                pos = Vector3.Lerp (endPosition, startPosition, t) ;

                // animate all other points except point at index i
                for (int j = i; j < pointsCount; j++){
                    //Debug.Log("here4");
                    lr.SetPosition (j, pos);
                        
                    head.position = pos;


                }
                yield return null ;
            }
        }
       
    }

    public void FixHeadPos(){
        head.localPosition = transform.InverseTransformPoint( lr.GetPosition(lr.positionCount - 1));
        Vector3 lookAt = lr.GetPosition(lr.positionCount - 2); // world pos
        float AngleRad = Mathf.Atan2( head.position.y - lookAt.y,  head.position.x - lookAt.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        head.rotation = Quaternion.Euler(0, 0, AngleDeg);
    }

    public void FixCollider(){
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < lr.positionCount - 1; i++){
            points.Add((Vector2)lr.GetPosition(i));
        }
        points.Add((Vector2)lr.GetPosition(lr.positionCount - 1));
        col.points = points.ToArray();
    }

    // Moves single line point in word pos
    public void MoveLinePoint(int linePointIndex, Vector3 targetPos)
    {
        lr.SetPosition(linePointIndex, targetPos);

        // Also moves the head if moving last line point
        if ( linePointIndex == lr.positionCount - 1)
        {
            FixHeadPos();
        }
        
        
    }

}
