using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;

public class ViRMA_APIController : MonoBehaviour
{
    // public
    public static string serverAddress = "https://localhost:44317/api/";

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
    private static string CallHandler(List<CellParamHandler> paramHandlers)
    {
        string url = "cell?";
        foreach (CellParamHandler paramHandler in paramHandlers)
        {
            string typeId = paramHandler.Type == "Tagset" ? paramHandler.Type + "Id" : paramHandler.Type + "NodeId";
            url += paramHandler.Axis + "Axis={'AxisType': '" + paramHandler.Type + "', '" + typeId + "': " + paramHandler.Id + "}&";
        }
        url = url.Substring(0, url.Length - 1);
        return url;
    }
    private static List<Cell> ResponseHandler(JSONNode response)
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
        string paramsURL = CallHandler(paramHandlers);
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
        List<Cell> cells = ResponseHandler(response);
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