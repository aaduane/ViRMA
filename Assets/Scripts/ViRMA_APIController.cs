using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.Threading;


namespace APIController
{

}


public class ViRMA_APIController : MonoBehaviour
{
    // public
    public static string serverAddress = "https://localhost:44317/api/";
    //public static string imagesDirectory = "C:/Users/aaron/Documents/Unity Projects/ViRMA/LaugavegurDataDDS/"; // for test build
    public static string imagesDirectory = System.IO.Directory.GetCurrentDirectory().ToString() + "/LaugavegurDataDDS/";
    public static bool debugging = true;

    // general API methods
    public static IEnumerator GetRequest(string paramsURL, Action<JSONNode> returnResponse)
    {
        string getRequest = serverAddress + paramsURL;
        float beforeWebRequest = 0, afterWebRequest = 0, beforeJsonParse = 0, afterJsonParse = 0;

        // for debugging
        if (debugging)
        {
            Debug.Log("URL: " + getRequest);
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
            Debug.Log("Server Time: " + (afterWebRequest - beforeWebRequest).ToString("n3") + " seconds");
            beforeJsonParse = Time.realtimeSinceStartup;
        }

        // parse JSON string off the main thread to prevent frame skips in Unity
        Thread thread = new Thread(() => {
            JSONNode response = JSON.Parse(json);
            if (debugging)
            {
                Debug.Log(response.Count + " results!");
            }
            returnResponse(response);
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
            Debug.Log("Process Time: " + (afterJsonParse - beforeJsonParse).ToString("n3") + " seconds");
        }
    }
    public static class Tagsets
    {
        public static JSONNode Data { get; set; }
        public static int Id { get; set; }
        public static string Name { get; set; }

        public class Tag
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public static IEnumerator GetData()
        {
            yield return GetRequest("tagset", (response) =>
            {
                Data = response;         
            });
        }

        public static List<Tag> GetTagsets()
        {
            List<Tag> tagsetNames = new List<Tag>();
            foreach (var obj in Data)
            {
                Tag newTag = new Tag { 
                    Id = obj.Value["Id"],
                    Name = obj.Value["Name"]
                };
                tagsetNames.Add(newTag);
            }
            return tagsetNames;
        }
    }

   

    // cell API methods
    public class Cell
    {
        public Vector3 Coordinates { get; set; }
        public string ImageName { get; set; }
    }
    public class CellParamHandler
    {
        public string Call { get; set; }
        public int Id { get; set; }
        public string Axis { get; set; }
        public string Type { get; set; }
    }  
    private static string CellCallHandler(List<CellParamHandler> cellParamHandlers)
    {
        string url = "cell?";
        foreach (CellParamHandler paramHandler in cellParamHandlers)
        {
            string typeId = paramHandler.Type == "Tagset" ? paramHandler.Type + "Id" : paramHandler.Type + "NodeId";
            url += paramHandler.Axis + "Axis={'AxisType': '" + paramHandler.Type + "', '" + typeId + "': " + paramHandler.Id + "}&";
        }
        url = url.Substring(0, url.Length - 1);
        return url;
    }
    private static List<Cell> CellResponseHandler(JSONNode response)
    {
        List<Cell> cells = new List<Cell>();
        foreach (var obj in response)
        {
            Cell newCell = new Cell();
            newCell.Coordinates = new Vector3(obj.Value["x"], obj.Value["y"], obj.Value["z"]);
            newCell.ImageName = obj.Value["CubeObjects"][0]["FileName"];
            cells.Add(newCell);
        }
        return cells;
    }
    public static IEnumerator GetCells(List<CellParamHandler> paramHandlers, Action<List<Cell>> returnResponse)
    {
        string paramsURL = CellCallHandler(paramHandlers);
        string getRequest = serverAddress + paramsURL;

        Debug.Log(getRequest); // testing

        UnityWebRequest request = UnityWebRequest.Get(getRequest);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.LogError(request.error);
            yield break;
        }
        JSONNode response = JSON.Parse(request.downloadHandler.text);
        List<Cell> cells = CellResponseHandler(response);
        yield return null;
        returnResponse(cells);
    }

    // test methods
    public static IEnumerator TestAPI(int Id)
    {
        string baseURL = "https://localhost:5001/api/";
        string tagURL = baseURL + "tag/" + Id.ToString();
        print("Getting tag...");

        UnityWebRequest GetTagRequest = UnityWebRequest.Get(tagURL);
        yield return GetTagRequest.SendWebRequest();

        if (GetTagRequest.isNetworkError || GetTagRequest.isHttpError)
        {
            Debug.LogError(GetTagRequest.error);
            yield break;
        }

        JSONNode tag = JSON.Parse(GetTagRequest.downloadHandler.text);

        int tagId = tag["Id"];
        string tagName = tag["Name"];
        int tagsetId = tag["TagsetId"];
        print("Id: " + tagId.ToString() + " Name: " + tagName.ToString() + " tagsetId: " + tagsetId);
    }
    static void TestGenericPOST()
    {
        var url = "https://reqbin.com/echo/post/json";

        var httpRequest = (HttpWebRequest)WebRequest.Create(url);
        httpRequest.Method = "POST";

        httpRequest.Accept = "application/json";
        httpRequest.ContentType = "application/json";

        var data = @"{
          ""Id"": 78912,
          ""Customer"": ""Jason Sweet"",
          ""Quantity"": 1,
          ""Price"": 18.00
        }";

        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
        {
            streamWriter.Write(data);
        }

        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }

        Debug.Log(httpResponse.StatusCode);
    }
}