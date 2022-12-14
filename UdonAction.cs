
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace K13A.USharpAction
{
    public class UdonAction : UdonSharpBehaviour
    {
        public string name = "UdonSharp Event Action";

        public int[] ActionType = {0};
        public UdonSharpBehaviour[] Behaviours = {null};
        public string[] ObjectNames = {""};
        public object[] Value = {null};
        public bool[] IsSynced = {false};
        public bool[] TransferOfOwnership = {false};

        public void Invoke()
        {
            for (var i = 0; i < Behaviours.Length; i++)
            {
                var behaviour = Behaviours[i];
                if (ActionType == null || behaviour == null || ObjectNames == null || Value == null ||
                    IsSynced == null ||
                    TransferOfOwnership == null || ObjectNames[i] == "None")
                {
                    Debug.LogError($"[{name}] Event is NULL!");
                    continue;
                }

                switch (ActionType[i])
                {
                    case 0: // Invoke Function
                        if (IsSynced[i])
                        {
                            if (TransferOfOwnership[i])
                                Networking.SetOwner(Networking.LocalPlayer, behaviour.gameObject);
                            behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, ObjectNames[i]);
                        }
                        else behaviour.SendCustomEvent(ObjectNames[i]);

                        break;
                    case 1: // Set Value
                        behaviour.SetProgramVariable(ObjectNames[i], Value[i]);
                        break;
                }
            }
        }

        public int[] AddToActionType(int[] array, int newObject)
        {
            int[] newArray = new int[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[array.Length] = newObject;

            return newArray;
        }

        public UdonSharpBehaviour[] AddToActionType(UdonSharpBehaviour[] array, UdonSharpBehaviour newObject)
        {
            UdonSharpBehaviour[] newArray = new UdonSharpBehaviour[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[array.Length] = newObject;

            return newArray;
        }

        public string[] AddToObjectNames(string[] array, string newObject)
        {
            string[] newArray = new string[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[array.Length] = newObject;

            return newArray;
        }

        public object[] AddToValue(object[] array, object newObject)
        {
            object[] newArray = new object[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[array.Length] = newObject;

            return newArray;
        }

        public bool[] AddToBool(bool[] array, bool newObject)
        {
            bool[] newArray = new bool[array.Length + 1];
            for (var i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[array.Length] = newObject;

            return newArray;
        }


        public void AddFunction(UdonSharpBehaviour behaviour, string functionName)
        {
            ActionType = AddToActionType(ActionType, 0);
            Behaviours = AddToActionType(Behaviours, behaviour);
            ObjectNames = AddToObjectNames(ObjectNames, functionName);
            Value = AddToValue(Value, .0f);
            IsSynced = AddToBool(IsSynced, false);
            TransferOfOwnership = AddToBool(TransferOfOwnership, false);
        }

        public void AddValue(UdonSharpBehaviour behaviour, string VariableName, object value)
        {
            ActionType = AddToActionType(ActionType, 1);
            Behaviours = AddToActionType(Behaviours, behaviour);
            ObjectNames = AddToObjectNames(ObjectNames, VariableName);
            Value = AddToValue(Value, value);
            IsSynced = AddToBool(IsSynced, false);
            TransferOfOwnership = AddToBool(TransferOfOwnership, false);
        }

        public void IgnoreEvent(int index)
        {
            Behaviours[index] = null;
        }
    }
}
