using UnityEngine;
using Valve.VR;

public class actionstest : MonoBehaviour
{
    public SteamVR_ActionSet test_actions;
    public SteamVR_Action_Boolean test_action;

    void Start()
    {
        test_actions.Activate();
        test_action[SteamVR_Input_Sources.Any].onStateDown += Foo;
    }

    private void Foo(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        Debug.Log(action.GetShortName() + " | " + source);

    }
}
