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
    public int Id { get; set; }
    public string Name { get; set; }
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
        public int Id { get; set; }
        public string Type { get; set; }

        public Filter(int id, string type)
        {
            Id = id;
            Type = type;
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
    public void AddFilter(int id, string type)
    {
        Filter addFilter = new Filter(id, type);
        if (!Filters.Contains(addFilter))
        {
            Filters.Add(addFilter);
        }       
    }
    public void RemoveFilter(int id, string type)
    {
        Filter removeFilter = new Filter(id, type);
        Filters.Remove(removeFilter);
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
        public string Name { get; set; }
        public string Type { get; set; }    
        public List<KeyValuePair<string, int>> Labels { get; set; }
        public AxisLabel(int id, string type, string name, List<KeyValuePair<string, int>> labels)
        {
            Id = id;
            Name = name;
            Type = type;
            Labels = labels;
        }
    }
    public void SetAxisLabsls(string axis, int id, string type, string name, List<KeyValuePair<string, int>> labels)
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
    public static string serverAddress = "https://localhost:44317/api/";

    //public static string imagesDirectory = System.IO.Directory.GetCurrentDirectory().ToString() + "/LaugavegurDataDDS/"; 
    //public static string imagesDirectory = "C:/Users/aaron/Documents/Unity Projects/ViRMA/LaugavegurDataDDS/"; 

    public static string imagesDirectory = System.IO.Directory.GetCurrentDirectory().ToString() + "/../lsc-2021-dds/"; 
    //public static string imagesDirectory = "C:/Users/aaron/Documents/Unity Projects/lsc-2021-dds/"; 

    // private
    private static JSONNode jsonData;

    // general API methods
    public static IEnumerator GetRequest(string paramsURL, Action<JSONNode> onSuccess)
    {
        Debug.Log(paramsURL); // testing

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

    // viz
    public static IEnumerator GetCells(Query query, Action<List<Cell>> onSuccess)
    {
        // OLD: https://localhost:44317/api/cell?xAxis={'AxisType': 'Tagset', 'TagsetId': 3}&yAxis={'AxisType': 'Tagset', 'TagsetId': 7}&zAxis={'AxisType': 'Hierarchy', 'HierarchyNodeId': 77}&filters=[{'type': 'Tagset', 'tagId': 7},{'type': 'Hierarchy', 'nodeId': 5}]
        // NEW: https://localhost:5001/api/cell/?yAxis={"AxisType":"Hierarchy","Id":691}&zAxis={"AxisType":"Tagset","Id":13}&filters=[{"Id":132,"type":"day of week","name":"7"},{"Id":147,"type":"day of week","name":"6"}]

        string url = "cell?";
        if (query.X != null)
        {
            //string typeId = query.X.Type == "Tagset" ? query.X.Type + "Id" : query.X.Type + "NodeId";
            //url += "xAxis={'AxisType': '" + query.X.Type + "', '" + typeId + "': " + query.X.Id + "}&";

            url += "xAxis={'AxisType': '" + query.X.Type + "', 'Id': " + query.X.Id + "}&";
        }
        if (query.Y != null)
        {
            //string typeId = query.Y.Type == "Tagset" ? query.Y.Type + "Id" : query.Y.Type + "NodeId";
            //url += "yAxis={'AxisType': '" + query.Y.Type + "', '" + typeId + "': " + query.Y.Id + "}&";

            url += "yAxis={'AxisType': '" + query.Y.Type + "', 'Id': " + query.Y.Id + "}&";
        }
        if (query.Z != null)
        {
            //string typeId = query.Z.Type == "Tagset" ? query.Z.Type + "Id" : query.Z.Type + "NodeId";
            //url += "zAxis={'AxisType': '" + query.Z.Type + "', '" + typeId + "': " + query.Z.Id + "}&";

            url += "zAxis={'AxisType': '" + query.Z.Type + "', 'Id': " + query.Z.Id + "}&";
        }
        url = url.Substring(0, url.Length - 1);

        if (query.Filters.Count > 0)
        {
            url += "&filters=[";
            foreach (Query.Filter filter in query.Filters)
            {
                string typeId = filter.Type == "Tagset" ? "tagId" : "nodeId";
                url += "{'type': '" + filter.Type.ToLower() + "', '" + typeId + "': " + filter.Id + "},";
            }
            url = url.Substring(0, url.Length - 1) + "]";
            url = url.Replace("\'", "\"");
        }

        //Debug.Log(url); // testing

        yield return GetRequest(url, (response) =>
        {
            jsonData = response;
        });

        List<Cell> cells = new List<Cell>();
        foreach (var obj in jsonData)
        {
            Cell newCell = new Cell();
            newCell.Coordinates = new Vector3(obj.Value["x"], obj.Value["y"], obj.Value["z"]);
            if (obj.Value["CubeObjects"].Count > 0)
            {
                //newCell.ImageName = obj.Value["CubeObjects"][0]["FileName"];

                newCell.ImageName = obj.Value["CubeObjects"][0]["FileURI"];
                string imageNameDDS = newCell.ImageName.Substring(0, newCell.ImageName.Length - 4) + ".dds";
                newCell.ImageName = imageNameDDS;

                //newCell.ImageName = obj.Value["CubeObjects"][0]["FileURI"];
            }
            else
            {
                newCell.Filtered = true;
            }
            cells.Add(newCell);
        }
        onSuccess(cells);
    }
    public static IEnumerator GetAxesLabels(Query query, Action<AxesLabels> onSuccess)
    {
        AxesLabels axisLabels = new AxesLabels();

        (string name, List<KeyValuePair<string, int>> labels) processLabelData(JSONNode response)
        {
            string name;
            List<KeyValuePair<string, int>> labels = new List<KeyValuePair<string, int>>();
            if (response["Tags"] == null)
            {
                // hierarchy
                name = response["Tag"]["Name"];
                foreach (JSONObject child in response["Children"])
                {
                    int labelId = child["Id"];
                    string labelName = child["Tag"]["Name"];
                    int bracketIndex = labelName.IndexOf("(");
                    if (bracketIndex > -1)
                    {
                        labelName = labelName.Substring(0, bracketIndex);
                    }
                    KeyValuePair<string, int> labelIdPair = new KeyValuePair<string, int>(labelName, labelId);
                    labels.Add(labelIdPair);
                }
            }
            else
            {
                // tagset
                name = response["Name"];
                foreach (JSONObject child in response["Tags"])
                {
                    int labelId = child["Id"];
                    string labelName = child["Name"];
                    int bracketIndex = labelName.IndexOf("(");
                    if (bracketIndex > -1)
                    {
                        labelName = labelName.Substring(0, bracketIndex);
                    }
                    KeyValuePair<string, int> labelIdPair = new KeyValuePair<string, int>(labelName, labelId);
                    labels.Add(labelIdPair);
                }
            }
            labels = labels.OrderBy(kvp => kvp.Key).ToList();
            return (name, labels);
        }

        if (query.X != null)
        {
            string type = query.X.Type;
            if (type == "Hierarchy")
            {
                type = "Node";
            }
            yield return GetRequest(type + "/" + query.X.Id, (response) =>
            {
                (string name, List<KeyValuePair<string, int>> labels) = processLabelData(response);
                axisLabels.SetAxisLabsls("X", query.X.Id, type, name, labels);
            });
        }

        if (query.Y != null)
        {
            string type = query.Y.Type;
            if (type == "Hierarchy")
            {
                type = "Node";
            }
            yield return GetRequest(type + "/" + query.Y.Id, (response) =>
            {
                (string name, List<KeyValuePair<string, int>> labels) = processLabelData(response);
                axisLabels.SetAxisLabsls("Y", query.Y.Id, type, name, labels);
            });
        }

        if (query.Z != null)
        {
            string type = query.Z.Type;
            if (type == "Hierarchy")
            {
                type = "Node";
            }
            yield return GetRequest(type + "/" + query.Z.Id, (response) =>
            {
                (string name, List<KeyValuePair<string, int>> labels) = processLabelData(response);
                axisLabels.SetAxisLabsls("Z", query.Z.Id, type, name, labels);
            });
        }

        onSuccess(axisLabels);
    }

    // all tagsets and hierarchies
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
        yield return GetRequest("hierarchy/6", (response) =>
        {
            jsonData = response;
        });

        /*
        Debug.Log(jsonData["Id"]);
        Debug.Log(jsonData["Name"]);
        Debug.Log(jsonData["Nodes"]);
        */

        List<Tag> hierarchies = new List<Tag>();
        foreach (var obj in jsonData)
        {

            ///Debug.Log(obj);

            Tag newTag = new Tag
            {
                Id = obj.Value["Id"],
                Name = obj.Value["Name"]
            };
            hierarchies.Add(newTag);
        }
        onSuccess(hierarchies);
    }


    // dimension explorer
    public static IEnumerator SearchHierachies(string searchParam, Action<List<Tag>> onSuccess)
    {
        //Debug.Log("Submitting... node/name=" + searchParam);

        yield return GetRequest("node/name=" + searchParam, (response) =>
        {
            jsonData = response;
        });

        int limitCounter = 0;
        List<Tag> nodes = new List<Tag>();
        foreach (var obj in jsonData)
        {
            if (limitCounter > 30)
            {
                Debug.Log("Too many results. Limiting to top 30.");
                break;
            }

            Tag newTag = new Tag();

            // tag id
            newTag.Id = obj.Value["Id"];

            // tag name
            string tagName = obj.Value["Name"];
            int bracketIndex = tagName.IndexOf("(");
            if (bracketIndex > -1) {
                tagName = tagName.Substring(0, bracketIndex);
            }
            newTag.Name = tagName;

            // if tag has a parent
            if (obj.Value["ParentNode"] != null)
            {
                Tag parentNode = new Tag();

                // parent tag id
                parentNode.Id = obj.Value["ParentNode"]["Id"];

                // parent tag name
                string parentTagName = obj.Value["ParentNode"]["Name"];
                int parentBracketIndex = parentTagName.IndexOf("(");
                if (parentBracketIndex > -1)
                {
                    parentTagName = parentTagName.Substring(0, parentBracketIndex);
                }
                parentNode.Name = parentTagName;

                // attch parent tag info to new tag
                newTag.Parent = parentNode;
            }
            else
            {
                // when there is no parent, it means we're at the top of the hierarchy and use a tag id of zero to indicate that
                Tag parentNode = new Tag();
                parentNode.Id = 0;
                parentNode.Name = ".";
                newTag.Parent = parentNode;
            }

            nodes.Add(newTag);

            limitCounter++;
        }

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
                    newTag.Id = obj.Value["Id"];

                    // tag name
                    string tagName = obj.Value["Name"];
                    int bracketIndex = tagName.IndexOf("(");
                    if (bracketIndex > -1)
                    {
                        tagName = tagName.Substring(0, bracketIndex);
                    }
                    newTag.Name = tagName;
                    node.Children.Add(newTag);
                }

                List<Tag> orderedList = node.Children.OrderBy(s => s.Name).ToList();
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
                        newTag.Id = obj.Value["Id"];

                        // tag name
                        string tagName = obj.Value["Name"];
                        int bracketIndex = tagName.IndexOf("(");
                        if (bracketIndex > -1)
                        {
                            tagName = tagName.Substring(0, bracketIndex);
                        }
                        newTag.Name = tagName;
                        node.Siblings.Add(newTag);
                    }

                    List<Tag> orderedList = node.Siblings.OrderBy(s => s.Name).ToList();
                    node.Siblings = orderedList;
                });
            }
        }

        // Debug.Log(nodes.Count + " dimension results found!"); // testing

        List<Tag> orderedNodes = nodes.OrderBy(s => s.Id).ToList();
        onSuccess(orderedNodes);
    }
    public static IEnumerator GetHierarchyChildren(int targetId, Action<List<Tag>> onSuccess)
    {
        yield return GetRequest("node/" + targetId.ToString() + "/children", (response) =>
        {
            jsonData = response;
            List<Tag> children = new List<Tag>();
            foreach (var obj in jsonData)
            {
                Tag newTag = new Tag();

                // tag id
                newTag.Id = obj.Value["Id"];

                // tag name
                string tagName = obj.Value["Name"];
                int bracketIndex = tagName.IndexOf("(");
                if (bracketIndex > -1)
                {
                    tagName = tagName.Substring(0, bracketIndex);
                }
                newTag.Name = tagName;
                children.Add(newTag);
            }

            List<Tag> orderedList = children.OrderBy(s => s.Name).ToList();
            onSuccess(orderedList);
        });
    }
    public static IEnumerator GetHierarchyParent(int targetId, Action<Tag> onSuccess)
    {
        yield return GetRequest("node/" + targetId.ToString() + "/parent", (response) =>
        {
            jsonData = response;

            Tag parentTag = new Tag();

            if (jsonData != null)
            {
                // tag id
                parentTag.Id = jsonData["Id"];

                // tag name
                string tagName = jsonData["Name"];
                int bracketIndex = tagName.IndexOf("(");
                if (bracketIndex > -1)
                {
                    tagName = tagName.Substring(0, bracketIndex);
                }
                parentTag.Name = tagName;
            }
            onSuccess(parentTag);
        });
    }
     


} 