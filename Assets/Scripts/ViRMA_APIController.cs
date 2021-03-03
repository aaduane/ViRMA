using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.Threading;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public class Cell
{
    public Vector3 Coordinates { get; set; }
    public string ImageName { get; set; }
}
public class Query
{
    public class Axis
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public Axis(int id, string type) {
            Id = id;
            Type = type;
        }
    }

    public Axis X;
    public Axis Y;
    public Axis Z;
}

public class ViRMA_APIController : MonoBehaviour
{
    // public
    public static string serverAddress = "https://localhost:44317/api/";
    //public static string imagesDirectory = "C:/Users/aaron/Documents/Unity Projects/ViRMA/LaugavegurDataDDS/"; // for testing full build
    public static string imagesDirectory = System.IO.Directory.GetCurrentDirectory().ToString() + "/LaugavegurDataDDS/"; // for testing in Unity Editor
    public static bool debugging = true;

    // private
    private static JSONNode jsonData;

    // general API methods
    public static IEnumerator GetRequest(string paramsURL, Action<JSONNode> onSuccess)
    {
        string getRequest = serverAddress + paramsURL;
        float beforeWebRequest = 0, afterWebRequest = 0, beforeJsonParse = 0, afterJsonParse = 0;

        // for debugging
        if (debugging)
        {
            Debug.Log("URL REQUEST ~ ~ ~ ~ ~ " + getRequest);
            beforeWebRequest = Time.realtimeSinceStartup;
        }

        // unity web request packet
        UnityWebRequest request = UnityWebRequest.Get(getRequest);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
            yield break;
        }
        string json = request.downloadHandler.text;

        // for debugging
        if (debugging)
        {
            afterWebRequest = Time.realtimeSinceStartup;
            Debug.Log("SERVER TIME ~ ~ ~ ~ ~ " + (afterWebRequest - beforeWebRequest).ToString("n3") + " seconds");
            beforeJsonParse = Time.realtimeSinceStartup;
        }

        // parse JSON string off the main thread to prevent frame skips in Unity
        Thread thread = new Thread(() => {
            JSONNode response = JSON.Parse(json);
            if (debugging)
            {
                Debug.Log("RESULTS COUNT ~ ~ ~ ~ ~ " + response.Count + " results!");
            }
            onSuccess(response);
        });
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }

        // for debugging
        if (debugging)
        {
            afterJsonParse = Time.realtimeSinceStartup;
            Debug.Log("PARSE TIME ~ ~ ~ ~ ~ " + (afterJsonParse - beforeJsonParse).ToString("n3") + " seconds");
        }
    }
    public static IEnumerator GetTagsets(Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("tagset", (response) =>
        {
            jsonData = response;
        });

        List<Tag> tagsets = new List<Tag>();
        foreach (var obj in jsonData)
        {
            Tag newTag = new Tag
            {
                Id = obj.Value["Id"],
                Name = obj.Value["Name"]
            };
            tagsets.Add(newTag);
        }
        onSuccess(tagsets);
    }
    public static IEnumerator GetHierarchies(Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("hierarchy", (response) =>
        {
            jsonData = response;
        });

        List<Tag> hierarchies = new List<Tag>();
        foreach (var obj in jsonData)
        {
            Tag newTag = new Tag
            {
                Id = obj.Value["Id"],
                Name = obj.Value["Name"]
            };
            hierarchies.Add(newTag);
        }
        onSuccess(hierarchies);
    }
    public static IEnumerator GetCells(Query query, Action<List<Cell>> onSuccess)
    {
        string url = "cell?";
        if (query.X != null)
        {
            string typeId = query.X.Type == "Tagset" ? query.X.Type + "Id" : query.X.Type + "NodeId";
            url += "xAxis={'AxisType': '" + query.X.Type + "', '" + typeId + "': " + query.X.Id + "}&";
        }
        if (query.Y != null)
        {
            string typeId = query.Y.Type == "Tagset" ? query.Y.Type + "Id" : query.Y.Type + "NodeId";
            url += "yAxis={'AxisType': '" + query.Y.Type + "', '" + typeId + "': " + query.Y.Id + "}&";
        }
        if (query.Z != null)
        {
            string typeId = query.Z.Type == "Tagset" ? query.Z.Type + "Id" : query.Z.Type + "NodeId";
            url += "zAxis={'AxisType': '" + query.Z.Type + "', '" + typeId + "': " + query.Z.Id + "}&";
        }
        url = url.Substring(0, url.Length - 1);

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        List<Cell> cells = new List<Cell>();
        foreach (var obj in jsonData)
        {
            Cell newCell = new Cell();
            newCell.Coordinates = new Vector3(obj.Value["x"], obj.Value["y"], obj.Value["z"]);
            newCell.ImageName = obj.Value["CubeObjects"][0]["FileName"];
            cells.Add(newCell);
        }
        onSuccess(cells);
    }
}