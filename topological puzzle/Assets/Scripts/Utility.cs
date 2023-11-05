using System.Collections;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
            Directory.CreateDirectory(path);

        FileStream file = File.Create(path + "/" + fileName + ".save");
        bf.Serialize(file, saveData);
        file.Close();
    }

    public static object BinaryDeserialization(string folderName, string fileName)
    {
        string filePath = Application.persistentDataPath + folderName + fileName + ".save";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.Open);

        var saveData = bf.Deserialize(file);
        file.Close();

        return saveData;
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
