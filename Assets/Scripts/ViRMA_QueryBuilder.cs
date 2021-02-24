using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViRMA_QueryBuilder : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(ViRMA_APIController.GetTagsets("tagset", (response) => {

            foreach (var obj in response)
            {
                Debug.Log(obj.Value["Name"]);
            }

            Debug.Log("Finished!");

        }));

    }



}
