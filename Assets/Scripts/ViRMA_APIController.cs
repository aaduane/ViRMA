using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;
using System.Threading;
using System.Linq;

public class Tag
{
    private string _label; // needed to check for brackets/slashes when Label is get
    public string Label
    {
        get
        {
            // remove slashes
            int slashIndex = _label.IndexOf("/");

            // check if Label has a / in it
            if (slashIndex > -1)
            {
                // if it does, check if there is a second / in it (this is a date, and should be ignored)
                string restOfLabel = _label.Substring(slashIndex + 1);
                if (restOfLabel.IndexOf("/") == -1)
                {
                    // if there is only a single /, then cut off everything after
                    _label = _label.Substring(0, slashIndex);
                }
            }

            /*
            // remove brackets
            int bracketIndex = _label.IndexOf("(");
            if (bracketIndex > -1)
            {
                _label = _label.Substring(0, bracketIndex);
            }
            */

            return _label;
        }
        set { _label = value; }
    }
    public int Id { get; set; } 
    public Tag Parent { get; set; }
    public List<Tag> Siblings { get; set; }
    public List<Tag> Children { get; set; }
}
public class Cell
{
    public bool Filtered;
    public Vector3 Coordinates { get; set; }
    public string ImageName { get; set; }
    public Texture2D ImageTexture { get; set; }
    public Texture2D TextureArray { get; set; }
    public Material TextureArrayMaterial { get; set; }
    public int TextureArrayId { get; set; }
    public int TextureArraySize { get; set; }
    public int imageCount { get; set; }
}
public class Query
{
    public Axis X;
    public Axis Y;
    public Axis Z;
    public List<Filter> Filters = new List<Filter>();

    public class Axis
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Axis(int id, string type) {
            Id = id;
            Type = type;
        }
    }
    public class Filter
    {
        public string Type { get; set; }
        public List<int> Ids { get; set; }
        public string FilterId { get; set; }
        public Filter(string type, List<int> ids, int parentId = -1)
        {
            Type = type;
            Ids = ids;

            if (parentId != -1)
            {
                FilterId = type + "_" + parentId;
            }
            else
            {
                FilterId = "null_0";
            }
        }
    }
    public void SetAxis(string axis, int id, string type)
    {
        if (axis.ToUpper() == "X")
        {
            X = new Axis(id, type);
        }
        else if (axis.ToUpper() == "Y")
        {
            Y = new Axis(id, type);
        }
        else if (axis.ToUpper() == "Z")
        {
            Z = new Axis(id, type);
        }
        else
        {
            Debug.LogError("Invalid axis selected.");
        }
    }
    public void ClearAxis(string axis, bool hardCLear = false)
    {
        if (axis.ToUpper() == "X")
        {
            if (X != null)
            {
                if (hardCLear == false)
                {
                    AddFilter(X.Id, X.Type);
                }
                X.Id = -1;
                X.Type = null;
            }
        }
        if (axis.ToUpper() == "Y")
        {
            if (Y != null)
            {
                if (hardCLear == false)
                {
                    AddFilter(Y.Id, Y.Type);
                }
                Y.Id = -1;
                Y.Type = null;
            }
        }
        if (axis.ToUpper() == "Z")
        {
            if (Z != null)
            {
                if (hardCLear == false)
                {
                    AddFilter(Z.Id, Z.Type);
                }
                Z.Id = -1;
                Z.Type = null;
            }
        }
    }
    public void AddFilter(int id, string type, int parentId = -1)
    {
        if (Filters.Count == 0)
        {
            // if no filters exist yet, just add the first filter
            List<int> newIdList = new List<int>() { id };
            Filter newFilter = new Filter(type, newIdList, parentId);
            Filters.Add(newFilter);
        }
        else
        {
            // if filter to be added has no parent id (e.g. nodes from a hierarchy)
            if (parentId == -1)
            {
                // check if an identical filter with no parent id exists already
                bool alreadyAdded = false;
                for (int i = 0; i < Filters.Count; i++)
                {
                    if (Filters[i].Type == type)
                    {
                        if (Filters[i].Ids.Contains(id))
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }              
                }
                // if an identical filter does not exist, make and add it
                if (alreadyAdded == false)
                {
                    List<int> newIdList = new List<int>() { id };
                    Filter newFilter = new Filter(type, newIdList);
                    Filters.Add(newFilter);
                }
            }
            else
            {
                // if filter to be added has a parent id (e.g. tags from tagsets)
                bool alreadyAdded = false;
                string newFilterId = type + "_" + parentId;
                for (int i = 0; i < Filters.Count; i++)
                {
                    // check if a filter with the same parent id already exists and add it if it does
                    if (Filters[i].FilterId == newFilterId)
                    {
                        if (!Filters[i].Ids.Contains(id))
                        {
                            Filters[i].Ids.Add(id);
                            alreadyAdded = true;
                            break;
                        }
                    }
                }
                // if a filter with the same parent id does not exist, create a new one and add it
                if (alreadyAdded == false)
                {
                    List<int> newIdList = new List<int>() { id };
                    Filter newFilter = new Filter(type, newIdList, parentId);
                    Filters.Add(newFilter);
                }
            }
        }
    }
    public void RemoveFilter(int id, string type, int parentId = -1)
    {
        string targetFilterId = "null_0";
        if (parentId != -1)
        {
            targetFilterId = type + "_" + parentId;
        }

        for (int i = 0; i < Filters.Count; i++)
        {
            if (Filters[i].FilterId == targetFilterId)
            {
                if (Filters[i].Type == type)
                {
                    if (Filters[i].Ids.Contains(id))
                    {
                        Filters[i].Ids.Remove(id);
                        if (Filters[i].Ids.Count == 0)
                        {
                            Filters.Remove(Filters[i]);
                            break;
                        }
                    }
                }
            }
        }
    }
    public void ClearFilters()
    {
        Filters.Clear();
    }

}
public class AxesLabels {

    public AxisLabel X;
    public AxisLabel Y;
    public AxisLabel Z;

    public class AxisLabel
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }    
        public List<Tag> Labels { get; set; }
        public AxisLabel(int id, string type, string label, List<Tag> labels)
        {
            Id = id;
            Label = label;
            Type = type;
            Labels = labels;
        }
    }
    public void SetAxisLabsls(string axis, int id, string type, string name, List<Tag> labels)
    {
        if (axis.ToUpper() == "X")
        {
            X = new AxisLabel(id, type, name, labels);
        }
        else if (axis.ToUpper() == "Y")
        {
            Y = new AxisLabel(id, type, name, labels);
        }
        else if (axis.ToUpper() == "Z")
        {
            Z = new AxisLabel(id, type, name, labels);
        }
        else
        {
            Debug.LogError("Invalid axis selected.");
        }
    }
}

public class ViRMA_APIController : MonoBehaviour
{
    // public
    public static bool debugging = false;
    public static string restAPI = "https://localhost:44317/api/";

    // medta database variables
    public static bool useLocalMedia;
    public static string localMediaType;
    public static string localMediaDirectory;
    public static string remoteMediaDirectory;
    public static string remoteThumbnailMediaDirectory;
    public static string database;
    private static void SetLSC2021(bool isLocal = false, string fileType = "JPG")
    {
        useLocalMedia = isLocal;
        if (!isLocal)
        {
            fileType = "JPG";
        }
        localMediaType = fileType;
        localMediaDirectory = "D:/Datasets/LSC2021/";
        remoteMediaDirectory = "http://bjth.itu.dk:5005/LSC21/";
        remoteThumbnailMediaDirectory = "http://bjth.itu.dk:5005/LSC21/";
        database = "LSC";
    }
    private static void SetLSC2022(bool isLocal = false, string fileType = "JPG")
    {
        useLocalMedia = isLocal;
        if (!isLocal)
        {
            fileType = "JPG";
        }
        localMediaType = fileType;
        localMediaDirectory = "C:/Users/aaron/Documents/LSC2022/";
        remoteMediaDirectory = "http://bjth.itu.dk:5007/";
        remoteThumbnailMediaDirectory = "http://bjth.itu.dk:5007/";
        database = "LSC";
    }
    private static void SetVBS2022(bool isLocal = false, string fileType = "JPG")
    {
        useLocalMedia = isLocal;
        if (!isLocal)
        {
            fileType = "JPG";
        }
        localMediaType = fileType;
        localMediaDirectory = "D:/Datasets/VBS2022/";
        remoteMediaDirectory = "http://bjth.itu.dk:5008/";
        remoteThumbnailMediaDirectory = "http://bjth.itu.dk:5008/";
        database = "VBS";
    }

    // private
    private static JSONNode jsonData;
    public static IEnumerator GetRequest(string paramsURL, Action<JSONNode> onSuccess)
    {
        // set correct database settings
        SetVBS2022(); 

        string getRequest = restAPI + paramsURL;
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
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            throw new Exception("Cannot connect to database! Is the server running?");
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
        JSONNode response = JSON.Parse("");
        Thread thread = new Thread(() => {
            response = JSON.Parse(json);
        });
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }

        // for debugging
        if (debugging)
        {
            Debug.Log("RESULTS COUNT ~ ~ ~ ~ ~ " + response.Count + " results!");
        }

        // if API returns a status 404, set the response to null
        if (response == "")
        {
            Debug.LogWarning("JSON PARSE FAILED: " + paramsURL);
            response = null;
        }
        else if (response["status"] != null)
        {
            if (response["status"] == 404)
            {
                response = null;
                //Debug.LogWarning("STATUS 404: " + paramsURL);
            }
        }

        // return the parsed JSON response
        onSuccess(response);

        // for debugging
        if (debugging)
        {
            afterJsonParse = Time.realtimeSinceStartup;
            Debug.Log("PARSE TIME ~ ~ ~ ~ ~ " + (afterJsonParse - beforeJsonParse).ToString("n3") + " seconds");
        }
    }

    // viz API
    public static IEnumerator GetCells(Query query, Action<List<Cell>> onSuccess)
    {
        // V1:  /api/cell?xAxis={'AxisType': 'Tagset', 'TagsetId': 3}&yAxis={'AxisType': 'Tagset', 'TagsetId': 7}&zAxis={'AxisType': 'Hierarchy', 'HierarchyNodeId': 77}&filters=[{'type': 'Tagset', 'tagId': 7},{'type': 'Hierarchy', 'nodeId': 5}]
        // V2:  /api/cell/?yAxis={"AxisType":"Hierarchy","Id":691}&zAxis={"AxisType":"Tagset","Id":13}&filters=[{"Id":132,"type":"day of week","name":"7"},{"Id":147,"type":"day of week","name":"6"}]
        // V3:  /api/cell/?yAxis={"type":"node","id":691}&zAxis={"type":"tagset","id":13}&filters=[{"type":"tag","ids":["147","132"]}]       

        string url = "cell?";
        if (query.X != null)
        {
            // OLD:
            // string typeId = query.X.Type == "Tagset" ? query.X.Type + "Id" : query.X.Type + "NodeId";
            // url += "xAxis={'AxisType': '" + query.X.Type + "', '" + typeId + "': " + query.X.Id + "}&";
            // url += "xAxis={'AxisType': '" + query.X.Type + "', 'Id': " + query.X.Id + "}&";

            url += "xAxis={'type': '" + query.X.Type + "', 'id': " + query.X.Id + "}&";
        }
        if (query.Y != null)
        {
            // OLD:
            // string typeId = query.Y.Type == "Tagset" ? query.Y.Type + "Id" : query.Y.Type + "NodeId";
            // url += "yAxis={'AxisType': '" + query.Y.Type + "', '" + typeId + "': " + query.Y.Id + "}&";
            // url += "yAxis={'AxisType': '" + query.Y.Type + "', 'Id': " + query.Y.Id + "}&";

            url += "yAxis={'type': '" + query.Y.Type + "', 'id': " + query.Y.Id + "}&";
        }
        if (query.Z != null)
        {
            // OLD:
            // string typeId = query.Z.Type == "Tagset" ? query.Z.Type + "Id" : query.Z.Type + "NodeId";
            // url += "zAxis={'AxisType': '" + query.Z.Type + "', '" + typeId + "': " + query.Z.Id + "}&";
            // url += "zAxis={'AxisType': '" + query.Z.Type + "', 'Id': " + query.Z.Id + "}&";

            url += "zAxis={'type': '" + query.Z.Type + "', 'id': " + query.Z.Id + "}&";
        }

        if (query.X != null || query.Y != null || query.Y != null)
        {
            url = url.Substring(0, url.Length - 1);
        }

        if (query.Filters.Count > 0)
        {
            url += "&filters=[";
            foreach (Query.Filter filter in query.Filters)
            {
                //string typeId = filter.Type == "Tagset" ? "tagId" : "nodeId";
                //url += "{'type': '" + filter.Type.ToLower() + "', '" + typeId + "': " + filter.Id + "},";

                string idString = string.Join("','", filter.Ids);
                url += "{'type': '" + filter.Type.ToLower() + "', 'ids': ['" + idString + "']},";
            }
            url = url.Substring(0, url.Length - 1) + "]";
            url = url.Replace("\'", "\"");
        }

        Debug.Log("GetCells: " + url); // debugging

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        List<Cell> cells = new List<Cell>();
        foreach (var obj in jsonData)
        {
            Cell newCell = new Cell();
            newCell.Coordinates = new Vector3(obj.Value["x"], obj.Value["y"], obj.Value["z"]);

            if (obj.Value["count"] > 0)
            {
                // OLD: 
                // newCell.ImageName = obj.Value["CubeObjects"][0]["FileName"];
                // newCell.ImageName = obj.Value["CubeObjects"][0]["FileURI"];

                newCell.ImageName = obj.Value["cubeObjects"][0]["fileURI"];
                //string imageNameDDS = newCell.ImageName.Substring(0, newCell.ImageName.Length - 4) + ".dds";
                //newCell.ImageName = imageNameDDS;
                newCell.imageCount = obj.Value["count"];
            }
            else
            {
                newCell.Filtered = true;
            }
            cells.Add(newCell);
        }

        onSuccess(cells);
    }

    // tagset and hierarchy endpoints
    public static IEnumerator GetTagset(string targetId, Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("tagset/" + targetId, (response) =>
        {
            jsonData = response;
        });

        List<Tag> tagsetData = new List<Tag>();

        if (jsonData["tags"] != null)
        {
            foreach (var tag in jsonData["tags"])
            {
                Tag newTag = new Tag
                {
                    Id = tag.Value["id"],
                    Label = tag.Value["name"]
                };

                newTag.Parent = new Tag
                {
                    Id = jsonData["id"],
                    Label = jsonData["name"]
                };

                tagsetData.Add(newTag);
            }
        }
        else
        {
            foreach (var tagset in jsonData)
            {
                Tag newTagset = new Tag
                {
                    Id = tagset.Value["id"],
                    Label = tagset.Value["name"]
                };
                tagsetData.Add(newTagset);
            }
        }

        onSuccess(tagsetData);
    }
    public static IEnumerator SearchHierachies(string searchParam, Action<List<Tag>> onSuccess)
    {
        //Debug.Log("Submitting... node/name=" + searchParam);

        yield return GetRequest("node/name=" + searchParam, (response) =>
        {
            jsonData = response;
        });

        List<Tag> unorderedNodes = new List<Tag>();
        foreach (var obj in jsonData)
        {
            Tag newTag = new Tag();

            // tag id
            newTag.Id = obj.Value["id"];

            // tag name
            newTag.Label = obj.Value["name"];

            // if tag has a parent
            if (obj.Value["parentNode"] != null)
            {
                Tag parentNode = new Tag();

                // parent tag id
                parentNode.Id = obj.Value["parentNode"]["id"];

                // parent tag name
                parentNode.Label = obj.Value["parentNode"]["name"];

                // attch parent tag info to new tag
                newTag.Parent = parentNode;
            }

            unorderedNodes.Add(newTag);
        }

        List<Tag> nodes = unorderedNodes.OrderBy(s => s.Label).ToList();
        if (nodes.Count > 30)
        {
            nodes = nodes.GetRange(0, 30);
        }

        // get rest of node data
        foreach (var node in nodes)
        {
            // get children         
            yield return GetRequest("node/" + node.Id.ToString() + "/children", (response) =>
            {
                jsonData = response;
                node.Children = new List<Tag>();
                foreach (var obj in jsonData)
                {
                    Tag newTag = new Tag();

                    // tag id
                    newTag.Id = obj.Value["id"];

                    // tag name
                    newTag.Label = obj.Value["name"];
                    node.Children.Add(newTag);
                }

                List<Tag> orderedList = node.Children.OrderBy(s => s.Label).ToList();
                node.Children = orderedList;
            });

            // get siblings  
            if (node.Parent != null)
            {
                yield return GetRequest("node/" + node.Parent.Id.ToString() + "/children", (response) =>
                {
                    jsonData = response;
                    node.Siblings = new List<Tag>();
                    foreach (var obj in jsonData)
                    {
                        Tag newTag = new Tag();

                        // tag id
                        newTag.Id = obj.Value["id"];

                        // tag name
                        newTag.Label = obj.Value["name"];
                        node.Siblings.Add(newTag);
                    }

                    List<Tag> orderedList = node.Siblings.OrderBy(s => s.Label).ToList();
                    node.Siblings = orderedList;
                });
            }
            else
            {
                // if there is no parent, set the siblings as just the searched for tag by itself
                node.Siblings = new List<Tag>() { node };

                // NOTE: you need the parent tag to get the siblings of a searched for tag
                // if there is no parent, we are at the top of a hierarchy
                // and the searched for tag should be the only sibling as a result
            }
        }

        // Debug.Log(nodes.Count + " dimension results found!"); // testing

        onSuccess(nodes);
    }
    public static IEnumerator GetHierarchyTag(int targetId, Action<Tag> onSuccess)
    {
        yield return GetRequest("node/" + targetId.ToString(), (response) =>
        {
            jsonData = response;
        });

        Tag nodeTag = null;

        if (jsonData != null)
        {
            nodeTag = new Tag();

            // tag id
            nodeTag.Id = targetId;

            if (jsonData["tagName"] != null)
            {
                // tag name
                nodeTag.Label = jsonData["tagName"];
            }
        }

        onSuccess(nodeTag);
    }
    public static IEnumerator GetHierarchyChildren(int targetId, Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("node/" + targetId.ToString() + "/children", (response) =>
        {
            jsonData = response;
        });

        List<Tag> children = new List<Tag>();

        if (jsonData != null)
        {
            foreach (var obj in jsonData)
            {
                Tag newTag = new Tag();

                // tag id
                newTag.Id = obj.Value["id"];

                // tag name
                newTag.Label = obj.Value["name"];
                children.Add(newTag);
            }
        }

        List<Tag> orderedList = children.OrderBy(s => s.Label).ToList();
        onSuccess(orderedList);
    }
    public static IEnumerator GetHierarchyParent(int targetId, Action<Tag> onSuccess)
    {
        // Debug.Log("GetHierarchyParent | node/ " + targetId.ToString() + "/ parent"); // testing

        yield return GetRequest("node/" + targetId.ToString() + "/parent", (response) =>
        {
            jsonData = response;
        });

        Tag parentTag = null;

        if (jsonData != null)
        {
            parentTag = new Tag();

            // tag id
            parentTag.Id = jsonData["id"];

            // tag name
            parentTag.Label = jsonData["name"];
        }

        onSuccess(parentTag);
    }

    // cell contents and timeline endpoints
    public static IEnumerator GetCellContents(Query cellQueryData, Action<List<KeyValuePair<int, string>>> onSuccess)
    {
        // OLD: cell/?filters=[{"type":"node","ids":[1001]},{"type":"tag","ids":[55]},{"type":"tag","ids":[23]},{"type":"tag","ids":[1872]},{"type":"tag","ids":[1874]}]&all=[]

        // NEW: cell/?xAxis={"type":"node","id":1001}&yAxis={"type":"tag","id":55}&filters=[{"type":"tag","ids":[23]},{"type":"tag","ids":[1872]},{"type":"tag","ids":[1874]}]&all=[]

        List<KeyValuePair<int, string>> results = new List<KeyValuePair<int, string>>();

        string url = "cell?";


        if (cellQueryData.X != null)
        {
            url += "&xAxis={'type':'" + cellQueryData.X.Type + "','id':" + cellQueryData.X.Id + "}";
        }

        if (cellQueryData.Y != null)
        {
            url += "&yAxis={'type':'" + cellQueryData.Y.Type + "','id':" + cellQueryData.Y.Id + "}";
        }

        if (cellQueryData.Z != null)
        {
            url += "&zAxis={'type':'" + cellQueryData.Z.Type + "','id':" + cellQueryData.Z.Id + "}";
        }

        if (cellQueryData.Filters.Count > 0)
        {
            url += "&filters=[";

            foreach (Query.Filter filter in cellQueryData.Filters)
            {
                if (filter.Type.ToLower() == "tagset")
                {
                    filter.Type = "tag";
                }
                string idString = string.Join("','", filter.Ids);
                url += "{'type': '" + filter.Type.ToLower() + "', 'ids': ['" + idString + "']},";
            }

            url = url.Substring(0, url.Length - 1) + "]";

        }

        url += "&all=[]";
        url = url.Replace("\'", "\"");

        Debug.Log("GetCellContents: " + url); // debugging

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        foreach (var obj in jsonData)
        {
            int imageId = obj.Value["id"];
            string imagePath = obj.Value["fileURI"];
            //string imageNameDDS = imagePath.Substring(0, imagePath.Length - 4) + ".dds";
            KeyValuePair<int, string> imageIdPath = new KeyValuePair<int, string>(imageId, imagePath);
            results.Add(imageIdPath);
        }

        onSuccess(results);
    }
    public static IEnumerator GetContextTimeline(DateTime timestamp, int minutes, Action<List<KeyValuePair<int, string>>> onSuccess)
    {
        // OLD: cell?filters=[{'type':'daterange','ids':['2'],'ranges':[['23-08-2016','23-08-2016']]},{'type':'timerange','ids':['3'],'ranges':[['10:00','11:00']]}]&all=[]
        // NEW: cell?filters=[{'type':'timestamprange','ids':['18'],'ranges':[['2016-09-24T19:26:00','2016-09-24T21:26:00']]}]&all=[]

        TimeSpan timeSpan = new TimeSpan(0, minutes, 0);
        DateTime future = timestamp.Add(timeSpan);
        DateTime past = timestamp.Subtract(timeSpan);

        string timestampTagsetIdUTC = "19";
        // confirm tagset ID for UTC timestamp is correct
        yield return GetRequest("tagset/", (tagsets) =>
        {
            if (tagsets["tags"] == null)
            {
                foreach (var tagset in tagsets)
                {
                    if (tagset.Value["name"] == "Timestamp UTC")
                    {
                        timestampTagsetIdUTC = tagset.Value["id"];
                    }
                }
            }
        });

        string url = "cell?filters=[{'type':'timestamprange','ids':['" + timestampTagsetIdUTC + "'],'ranges':[['" + past.ToString("dd/MM/yyyy HH:mm:ss") + "','" + future.ToString("dd/MM/yyyy HH:mm:ss") + "']]}]&all=[]";

        Debug.Log("GetTimeline: " + url); // debugging

        List<KeyValuePair<int, string>> results = new List<KeyValuePair<int, string>>();
        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        foreach (var obj in jsonData)
        {
            int imageId = obj.Value["id"];
            string imagePath = obj.Value["fileURI"];

            if (imagePath.Length > 0)
            {
                KeyValuePair<int, string> imageIdPath = new KeyValuePair<int, string>(imageId, imagePath);
                results.Add(imageIdPath);
            }
            else
            {
                Debug.LogError("Something wrong with this image ---> " + imagePath);
            }
        }

        onSuccess(results);
    }
    public static IEnumerator GetMediaObjectTagData(int targetId, Action<List<Tag>> onSuccess)
    {
        // /api/CubeObject/5/tags

        string url = "CubeObject/" + targetId + "/tags";

        //Debug.Log(url);

        List<Tag> results = new List<Tag>();
        yield return GetRequest(url, (response) =>
        {    
            foreach (var tagData in response)
            {
                Tag checkIfTagsetExists = results.FirstOrDefault(s => s.Label == tagData.Value["tagsetName"]);

                if (checkIfTagsetExists == null)
                {
                    Tag newTagset = new Tag
                    {
                        Id = tagData.Value["tagsetId"],
                        Label = tagData.Value["tagsetName"]
                    };
                    newTagset.Children = new List<Tag>();
                    Tag newTag = new Tag
                    {
                        Id = tagData.Value["id"],
                        Label = tagData.Value["name"]
                    };
                    newTagset.Children.Add(newTag);

                    results.Add(newTagset);
                }
                else
                {
                    if (checkIfTagsetExists.Children == null)
                    {
                        checkIfTagsetExists.Children = new List<Tag>();
                    }
                    Tag newTag = new Tag
                    {
                        Id = tagData.Value["id"],
                        Label = tagData.Value["name"]
                    };
                    checkIfTagsetExists.Children.Add(newTag);
                }
            }
        });

        onSuccess(results);
    }
    public static IEnumerator GetMediaObjectTags(int targetId, Action<List<string>> onSuccess)
    {
        /* This endpoint is basically deprecated in favour of GetMediaObjectTagData() */

        // tag?cubeObjectId=169869

        string url = "tag?cubeObjectId=" + targetId;

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        List<string> results = new List<string>();
        foreach (var obj in jsonData)
        {
            string tag = obj.Value.ToString().Trim('"');
            results.Add(tag);
        }

        onSuccess(results);
    }
    public static IEnumerator GetTagNodes(int targetId, Action<List<int>> onSuccess)
    {
        // /api/tag/37/nodes

        string url = "tag/" + targetId + "/nodes";

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        List<int> nodeIDs = new List<int>();
        foreach (var obj in jsonData)
        {
            int id = obj.Value["id"];
            nodeIDs.Add(id);
        }

        onSuccess(nodeIDs);
    }

    // static helper methods
    public static Texture2D FormatDDSTexture(byte[] ddsBytes)
    {
        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
        {
            throw new Exception("Invalid DDS DXTn texture size! (not 124)");
        }
        int height = ddsBytes[13] * 256 + ddsBytes[12];
        int width = ddsBytes[17] * 256 + ddsBytes[16];

        int ddsHeaderSize = 128;
        byte[] dxtBytes = new byte[ddsBytes.Length - ddsHeaderSize];
        Buffer.BlockCopy(ddsBytes, ddsHeaderSize, dxtBytes, 0, ddsBytes.Length - ddsHeaderSize);
        Texture2D texture = new Texture2D(width, height, TextureFormat.DXT1, false);

        texture.LoadRawTextureData(dxtBytes);
        texture.Apply();
        return (texture);
    }
    public static Texture2D FormatJPGTexture(byte[] jpgBytes)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(jpgBytes);
        TextureScale.Bilinear(tex, 1024, 768);
        return (tex);
    }

    // deprecated
    public static IEnumerator GetAllHierarchies(Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("hierarchy/", (response) =>
        {
            jsonData = response;
        });

        List<Tag> hierarchies = new List<Tag>();
        foreach (var obj in jsonData)
        {

            ///Debug.Log(obj);

            Tag newTag = new Tag
            {
                Id = obj.Value["id"],
                Label = obj.Value["name"]
            };
            hierarchies.Add(newTag);
        }
        onSuccess(hierarchies);
    }
    public static IEnumerator GetTimeline(List<Query.Filter> cellFiltersForTimeline, Action<List<KeyValuePair<int, string>>> onSuccess)
    {
        // cell?filters=[{'type':'node','ids':['699']},{'type':'tag','ids':['17']},{'type':'tag','ids':['147','132']}]&all=[];

        List<KeyValuePair<int, string>> results = new List<KeyValuePair<int, string>>();

        if (cellFiltersForTimeline.Count > 0)
        {
            string url = "cell?filters=[";
            foreach (Query.Filter filter in cellFiltersForTimeline)
            {
                if (filter.Type.ToLower() == "tagset")
                {
                    filter.Type = "tag";
                }
                string idString = string.Join("','", filter.Ids);
                url += "{'type': '" + filter.Type.ToLower() + "', 'ids': ['" + idString + "']},";
            }
            url = url.Substring(0, url.Length - 1) + "]&all=[]";
            url = url.Replace("\'", "\"");

            Debug.Log("GetTimeline: " + url); // debugging

            yield return GetRequest(url, (response) =>
            {
                jsonData = response;
            });

            foreach (var obj in jsonData)
            {
                int imageId = obj.Value["id"];
                string imagePath = obj.Value["fileURI"];
                //string imageNameDDS = imagePath.Substring(0, imagePath.Length - 4) + ".dds";
                KeyValuePair<int, string> imageIdPath = new KeyValuePair<int, string>(imageId, imagePath);
                results.Add(imageIdPath);
            }
        }

        onSuccess(results);
    }
}
public class WaitForFrames : CustomYieldInstruction
{
    private int _targetFrameCount;

    public WaitForFrames(int numberOfFrames)
    {
        _targetFrameCount = Time.frameCount + numberOfFrames;
    }

    public override bool keepWaiting
    {
        get
        {
            return Time.frameCount < _targetFrameCount;
        }
    }
}