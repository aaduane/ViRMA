using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Threading;
using System;

public class ViRMA_CompetitionController : MonoBehaviour
{
    // DRES TEMPLATE: https://vbs.itec.aau.at:9443/api/v1/submit?session=node017erq93n1e6lt1wyparyixy7bp78&item=b00003532_21i6bq_20150320_054740e

    private static string serverAddress = "https://vbs.itec.aau.at:9443/api/v1/";

    private static string sessionId = "node017erq93n1e6lt1wyparyixy7bp78";

    public static IEnumerator SubmitToLSC(string submissionId, Action<bool> onSuccess)
    {
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
