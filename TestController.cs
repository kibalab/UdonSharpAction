
using K13A.USharpAction;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TestController : UdonSharpBehaviour
{
    public UdonAction OnAction;

    public override void Interact()
    {
        OnAction.Invoke();
    }
}
