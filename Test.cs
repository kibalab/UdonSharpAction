
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Test : UdonSharpBehaviour
{
    public string Variable2 = "Hello, World!";
    public Vector4 Variavble1;
    public Material mat;

    public void Red()
    {
        mat.color = Color.red;
    }
}
