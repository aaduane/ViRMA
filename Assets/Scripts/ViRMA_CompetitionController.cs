using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Threading;
using System;

public class ViRMA_CompetitionController : MonoBehaviour
{
    public static IEnumerator SubmitToLSC(string submissionId, Action<bool> onSuccess)
    {
        // DRES LSC TEMPLATE: https://vbs.itec.aau.at:9443/api/v1/submit?session=node017erq93n1e6lt1wyparyixy7bp78&item=b00003532_21i6bq_20150320_054740e

        string serverAddress = "https://vbs.itec.aau.at:9443/api/v1/";
        string sessionId = "node012yg7so0b123qxh488ftze7xp9";

        submissionId = submissionId.Substring(11); // remove folder name
        submissionId = submissionId.Substring(0, submissionId.Length - 4); // remove file extension

        string submissionRequest = serverAddress + "submit?session=" + sessionId + "&item=" + submissionId;

        Debug.Log("SUBMISSION: " + submissionRequest);

        UnityWebRequest request = UnityWebRequest.Get(submissionRequest);

        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        string json = request.downloadHandler.text;

        JSONNode response = JSON.Parse("");
        Thread thread = new Thread(() => {
            response = JSON.Parse(json);
        });
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }

        if (response["status"])
        {
            if (response["submission"] == "WRONG")
            {    
                Debug.Log(response["submission"] + " | " + response["description"]);
                onSuccess(false);
            }
            else
            {
                Debug.Log(response["submission"] + " | " + response["description"]);
                onSuccess(true);  
            }        
        }
        else
        {
            Debug.Log(response["description"]);
            onSuccess(false);    
        }
    }

    public static IEnumerator SubmitToVBS(string submissionId, Action<bool> onSuccess)
    {
        /*
         * http(s)://{server}/api/v1/submit?item={item} where {item} is the identifier for the retrieved media item
         * http(s)://{server}/api/v1/submit?item={item}&shot={shot} where {shot} is the identifier for a pre-defined temporal segment within the {item}
         * http(s)://{server}/api/v1/submit?item={item}&frame={frame} where {frame} is the frame number within the {item}, in case it is a video
         * http(s)://{server}/api/v1/submit?item={item}&timecode={timecode} where {timecode} is a temporal position within the {item} in the form HH:MM:SS:FF. In case just a plain number is passed, the behavior is equivalent to passing the same value as {frame}
        */

        string serverAddress = "https://vbs.videobrowsing.org/api/v1/";
        string sessionId = "node012yg7so0b123qxh488ftze7xp9";

        submissionId = submissionId.Substring(11); // remove folder name
        submissionId = submissionId.Substring(0, submissionId.Length - 4); // remove file extension

        string submissionRequest = serverAddress + "submit?session=" + sessionId + "&item=" + submissionId;

        Debug.Log("SUBMISSION: " + submissionRequest);

        UnityWebRequest request = UnityWebRequest.Get(submissionRequest);

        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        string json = request.downloadHandler.text;

        JSONNode response = JSON.Parse("");
        Thread thread = new Thread(() => {
            response = JSON.Parse(json);
        });
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }

        if (response["status"])
        {
            if (response["submission"] == "WRONG")
            {
                Debug.Log(response["submission"] + " | " + response["description"]);
                onSuccess(false);
            }
            else
            {
                Debug.Log(response["submission"] + " | " + response["description"]);
                onSuccess(true);
            }
        }
        else
        {
            Debug.Log(response["description"]);
            onSuccess(false);
        }
    }
}
