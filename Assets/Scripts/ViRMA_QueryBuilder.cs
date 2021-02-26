using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class ViRMA_QueryBuilder : MonoBehaviour
{

    private IEnumerator Start()
    {
        /*
        StartCoroutine(ViRMA_APIController.GetTagsets("tagset", (response) => {

            Debug.Log(ViRMA_APIController.TagsetsHolder.Data.Count);

            List<string> tagsetNames = ViRMA_APIController.TagsetsHolder.GetTagsets();

            foreach (var item in tagsetNames)
            {
                Debug.Log(item);
            }

        }));
        */

        //yield return StartCoroutine(ViRMA_APIController.Test("cell/?xAxis={\"AxisDirection\":\"X\",\"AxisType\":\"Tagset\",\"TagsetId\":7,\"HierarchyNodeId\":0}&yAxis={\"AxisDirection\":\"Y\",\"AxisType\":\"Tagset\",\"TagsetId\":7,\"HierarchyNodeId\":0}&zAxis={\"AxisDirection\":\"Z\",\"AxisType\":\"Tagset\",\"TagsetId\":7,\"HierarchyNodeId\":0}"));
        //List<string> tagsetNames = ViRMA_APIController.TagsetsHolder.GetTagsets();
        //Debug.Log("Done!");

        //yield return StartCoroutine(ViRMA_APIController.Test("Tagset"));
        //List<string> tagsetNames = ViRMA_APIController.TagsetsHolder.GetTagsets();
        //Debug.Log(tagsetNames.Count);



        yield return StartCoroutine(ViRMA_APIController.Tagsets.GetData());
        List<ViRMA_APIController.Tagsets.Tag> tags = ViRMA_APIController.Tagsets.GetTagsets();
        foreach (var tag in tags)
        {
            Debug.Log(tag.Id + " | " + tag.Name);
        }

    }




}
