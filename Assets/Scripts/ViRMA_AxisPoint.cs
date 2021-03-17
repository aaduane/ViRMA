using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViRMA_AxisPoint : MonoBehaviour
{
    public string axisPointLabel;
    public bool x;
    public bool y;
    public bool z;

    public void AxisPointSetup()
    {
        // use prefab to prevent additional draw calls with colour change?

        //MaterialPropertyBlock materialSettings = new MaterialPropertyBlock();
        //materialSettings.SetColor("_Color", Color.red);
        //GetComponent<MeshRenderer>().SetPropertyBlock(materialSettings);

    }

}
