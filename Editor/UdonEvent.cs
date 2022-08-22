using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;

namespace K13A.USharpAction.Editor
{
    public class UdonEvent
    {
        public Enum executables = UdonEventUtils.CreateEnum(new List<string>() {"None"});
        
        public int ActionType = 0;
        public UdonSharpBehaviour Behaviour = null;
        public string ObjectName = "None";
        public object Value = null;
        public bool IsSynced = false;
        public bool TransferOfOwnership = false;

        public void UpdateExecutables()
        {
            if (Behaviour) 
            {
                switch (ActionType)
                {
                    case 0: // Invoke Function
                        executables = UdonEventUtils.CreateEnum(Behaviour.GetType().GetMethods()
                            .Select((x) => x.Name).ToList());
                        break;
                    case 1: // Set Value
                        executables = UdonEventUtils.CreateEnum(Behaviour.GetType().GetFields()
                            .Select((x) => x.Name).ToList());
                        break; 
                }
                
                if(ObjectName == "None" || String.IsNullOrEmpty(ObjectName) || String.IsNullOrWhiteSpace(ObjectName))
                    executables = (Enum)System.Enum.ToObject(executables.GetType(),0);
                else 
                    executables = (Enum)System.Enum.ToObject(executables.GetType(),Enum.GetNames(executables.GetType()).ToList().IndexOf(ObjectName));

            }
                
            else executables = UdonEventUtils.CreateEnum(new List<string>() {"None"});
        }
    }
}