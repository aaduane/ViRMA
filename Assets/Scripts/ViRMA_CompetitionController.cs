using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class ViRMA_CompetitionController : MonoBehaviour
{
    Dictionary<string, string> convertVBS = new Dictionary<string, string>();
    bool vbsConversionReady = false;
    private void Start()
    {
        StartCoroutine(LoadConversionTable());
    }
    private IEnumerator LoadConversionTable()
    {
        Thread thread = new Thread(() => {
            try
            {
                // C:/Users/aaron/OneDrive - ITU/ViRMA/PhotoCube SQL Resources/

                convertVBS = File.ReadLines("C:/Users/aaron/OneDrive - ITU/ViRMA/PhotoCube SQL Resources/VBS-id-converter.csv").Select(line => line.Split(',')).ToDictionary(line => line[0], line => line[1]);
                vbsConversionReady = true;

                Debug.LogWarning("VBS conversion CSV loaded successfully!");
            }
            catch
            {
                Debug.LogWarning("VBS conversion CSV not availabke!");
                vbsConversionReady = false;
            }            
        });
        thread.Start();
        while (thread.IsAlive)
        {
            yield return null;
        }
    }
    public IEnumerator SubmitToVBS(string fileName, Action<bool> onSuccess)
    {
        if (vbsConversionReady)
        {

            Debug.Log("CONVERTING ---> " + fileName);

            int firstSlash = fileName.IndexOf("/");
            string remainingSlash = fileName.Substring(firstSlash + 1);
            int secondSlash = remainingSlash.IndexOf("/");
            string videoId = fileName.Substring(firstSlash + 1, secondSlash);

            int underScore = fileName.IndexOf("_");
            string keyframeCount = fileName.Substring(underScore + 1);
            keyframeCount = keyframeCount.Substring(0, keyframeCount.Length - 4);

            KeyValuePair<string, string> convertedID = convertVBS.FirstOrDefault(t => t.Key == videoId + "_" + keyframeCount);

            Debug.Log("CONVERTED: " + convertedID.Value);

            //Debug.Log("FILENAME: " + fileName);
            //Debug.Log("CONVERT: " + videoId + "_" + keyframeCount);
            //Debug.Log("SUBMIT: " + convertedID.Value);

            /* TEMPLATES: 
             * http(s)://{server}/api/v1/submit?item={item} where {item} is the identifier for the retrieved media item
             * http(s)://{server}/api/v1/submit?item={item}&shot={shot} where {shot} is the identifier for a pre-defined temporal segment within the {item}
             * http(s)://{server}/api/v1/submit?item={item}&frame={frame} where {frame} is the frame number within the {item}, in case it is a video
             * http(s)://{server}/api/v1/submit?item={item}&timecode={timecode} where {timecode} is a temporal position within the {item} in the form HH:MM:SS:FF. In case just a plain number is passed, the behavior is equivalent to passing the same value as {frame}
            */

            // E.G. https://vbs.videobrowsing.org/api/v1/submit?session=node012yg7so0b123qxh488ftze7xp9&item=09220&frame=320

            string serverAddress = "https://vbs.videobrowsing.org:443/api/v1/";
            string sessionId = "node0c3uge4lsv9ep1wlm5v62717hh1627";

            int underscore = convertedID.Value.IndexOf("_");
            keyframeCount = convertedID.Value.Substring(underscore + 1);

            string submissionRequest = serverAddress + "submit?session=" + sessionId + "&item=" + videoId + "&frame=" + keyframeCount;

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
        else
        {
            Debug.LogError("VBS frame converter did not load!");
        }
        
    }
    public IEnumerator SubmitToLSC(string submissionId, Action<bool> onSuccess)
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
}
