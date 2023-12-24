using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum Direction {
    right,
    left,
    up,
    down,
    none
}

public static class Utility 
{
    static readonly Vector3[] vectorDirections = new Vector3[] {
        Vector3.right,
        Vector3.left,
        Vector3.up,
        Vector3.down,
        Vector3.zero
    };
    public static T[] ShuffleArray<T>(T[] array) {
		System.Random prng = new System.Random ();

		for (int i =0; i < array.Length -1; i ++) {
			int randomIndex = prng.Next(i,array.Length);
			T tempItem = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = tempItem;
		}

		return array; 
    }

    public static float EuclidFormula(int a, int b){
        return Mathf.Sqrt(Mathf.Pow(a,2) + Mathf.Pow(b,2));
    }

    public static IEnumerator SetActiveObjWithDelay(GameObject obj, bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(active);
    }

	public static void SetActiveObj(GameObject obj, bool active){
		obj.SetActive(active);
	}
    public static GameObject CheckForObjectAt(Vector3 pos, LayerMask lm)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, distance: 5f, lm);

        if (hit)
        {
            return hit.transform.gameObject;
        }
        return null;

    }

    public static GameObject CheckForObjectFrom(Vector3 pos, Vector3 dir, float distance, LayerMask lm)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, dir, distance: distance, lm);

        if (hit)
        {
            return hit.transform.gameObject;
        }
        return null;

    }

    public static void BinarySerialization(string folderName, string fileName, object saveData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        string path = Application.persistentDataPath + folderName;

        if (!Directory.Exists(path))
        {
            Debug.Log("path created : " + path);
            Directory.CreateDirectory(path);
        }


        FileStream file = File.Create(path + "/" + fileName);
        bf.Serialize(file, saveData);
        file.Close();
    }

    public static object BinaryDeserialization(string folderName, string fileName)
    {
        string filePath = Application.persistentDataPath + folderName + fileName;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);

        var saveData = bf.Deserialize(file);
        file.Close();

        return saveData;
    }

    public static string JsonSerialization(object data){
        string json = JsonUtility.ToJson(data, true);
        return json;
    }

    public static void SaveAsJson(string path, object data){
        string json = JsonSerialization(data);
        FileStream fileStream = new FileStream(path, FileMode.Create);

        using (StreamWriter writer = new StreamWriter(fileStream)){
            writer.Write(json);
        }
        //fileStream.Close();
    }

    public static T LoadDataFromJson<T>(string path) {
        if (File.Exists(path)) {
            using (StreamReader reader = new StreamReader(path)) {
                string json = reader.ReadToEnd();
                T data = JsonUtility.FromJson<T>(json);
                return data;
            }
        }
        else {
            Debug.LogWarning("File could not found");
            return default(T);
        }
    }

    public static LevelProperty LoadLevePropertyFromJson(string path){
        if (File.Exists(path)){
            using (StreamReader reader = new StreamReader(path)){
                string json = reader.ReadToEnd();
                LevelProperty levelProperty = JsonUtility.FromJson<LevelProperty>(json);
                return levelProperty;
            }
        }
        else{
            Debug.LogWarning("File could not found");
            return null;
        }
    }

    public static string EncodeBase64(string inputText){
        byte[] bytesToEncode = Encoding.UTF8.GetBytes(inputText);
        string encodedText = Convert.ToBase64String(bytesToEncode);

        return encodedText;
    }

    public static string EncodeBase64FromBytes(byte[] bytesToEncode){
        return Convert.ToBase64String(bytesToEncode);
    }

    public static string DecodeBase64(string encodedText){
        byte[] decodedBytes = Convert.FromBase64String(encodedText);
        string decodedText = Encoding.UTF8.GetString(decodedBytes);

        return decodedText;
    }

    public static byte[] DecodeBase64ToBytes(string encodedText){
        return Convert.FromBase64String(encodedText);
    }

    public static void CopyTo(Stream src, Stream dest){
        byte[] bytes = new byte[4096];

        int cnt;
        while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0){
            dest.Write(bytes, 0, cnt);
        }
    }

    public static byte[] Zip(string str){
        var bytes = Encoding.UTF8.GetBytes(str);

        using (var msi = new MemoryStream(bytes))

        using (var mso = new MemoryStream()){
            using (var gs = new GZipStream(mso, CompressionMode.Compress)){
                
                CopyTo(msi, gs);
            }

            return mso.ToArray();
        }
    }

    public static string Unzip(byte[] bytes){
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream()){
            using (var gs = new GZipStream(msi, CompressionMode.Decompress)){
                CopyTo(gs, mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }

    public static Vector3 DirToVectorDir(Direction dir)
    {
        return vectorDirections[(int)dir];
    }

    public static Direction VectorDirToDir(Vector3 vectorDir)
    {
        if (vectorDir == Vector3.right)
            return Direction.right;
        else if (vectorDir == Vector3.left)
            return Direction.left;
        else if (vectorDir == Vector3.up)
            return Direction.up;
        else if (vectorDir == Vector3.down)
            return Direction.down;
        else
            return Direction.none;
    }

    public static IEnumerator MakeButtonNoninteractive(Button button, float duration)
    {
        button.interactable = false;
        yield return new WaitForSeconds(duration);
        button.interactable = true;
    }
}
