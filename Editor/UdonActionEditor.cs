using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


[CustomEditor(typeof(UdonAction))]
public class UdonActionEditor : Editor
{
    private ReorderableList list;

    enum EditorActionType : int
    {
        Function = 0,
        Variable = 1
    }
    
    public List<Enum> enumerators = new List<Enum>();

    public UdonAction Target;
    
    private void OnEnable()
    {
        Target = (UdonAction) target;
        
        list = new ReorderableList(serializedObject,
            serializedObject.FindProperty("Behaviours"),
            true, true, true, true);
        
        list.drawHeaderCallback = (rect) =>
            EditorGUI.LabelField (rect, Target.name);
        
        list.elementHeight = 80; 

        enumerators = new List<Enum>(){};
        var j = 0;
        foreach (var objectName in Target.ObjectNames)
        {
            if (Target.Behaviours[j] != null) 
            {
                switch (Target.ActionType[j])
                {
                    case 0: // Invoke Function
                        enumerators.Add(CreateEnum(Target.Behaviours[j].GetType().GetMethods()
                            .Select((x) => x.Name).ToList()));
                        break;
                    case 1: // Set Value
                        enumerators.Add(CreateEnum(Target.Behaviours[j].GetType().GetFields()
                            .Select((x) => x.Name).ToList()));
                        break; 
                }
                
                if(objectName == "None" || String.IsNullOrEmpty(objectName) || String.IsNullOrWhiteSpace(objectName))
                    enumerators[j] = (Enum)System.Enum.ToObject(enumerators[j].GetType(),0);
                else 
                    enumerators[j] = (Enum)System.Enum.ToObject(enumerators[j].GetType(),Enum.GetNames(enumerators[j].GetType()).ToList().IndexOf(objectName));
            }
            else 
                enumerators.Add(CreateEnum(new List<string>(){"None"}));
            j++;
        }
        
        list.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {
                
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);


                EditorGUI.BeginChangeCheck();
                
                Target.ActionType[index] = (int)(EditorActionType)EditorGUI.EnumPopup(
                    new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight)
                    , (EditorActionType)Target.ActionType[index]);
                
                EditorGUI.PropertyField(
                    new Rect(rect.x + 180, rect.y, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                    element, GUIContent.none);
                
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (Target.Behaviours[index] != null)
                    {
                        switch (Target.ActionType[index])
                        {
                            case 0: // Invoke Function
                                enumerators[index] = CreateEnum(Target.Behaviours[index].GetType().GetMethods()
                                    .Select((x) => x.Name).ToList());
                                break;
                            case 1: // Set Value
                                enumerators[index] = CreateEnum(Target.Behaviours[index].GetType().GetFields()
                                    .Select((x) => x.Name).ToList());
                                break;
                        }
                    }

                    else enumerators[index] = CreateEnum(new List<string>(){"None"});
                }

                if (Target.ActionType[index] == 0)
                {
                    Target.IsSynced[index] = GUI.Toggle(
                        new Rect(rect.x, rect.y + 25, 200, EditorGUIUtility.singleLineHeight),
                        Target.IsSynced[index], "  Is Synced");

                    Target.TransferOfOwnership[index] = GUI.Toggle(
                        new Rect(rect.x, rect.y + 50, 200, EditorGUIUtility.singleLineHeight),
                        Target.TransferOfOwnership[index], "  Transfer of ownership");
                }

                EditorGUI.BeginChangeCheck();
                enumerators[index] = EditorGUI.EnumPopup(
                    new Rect(rect.x + 180, rect.y + 25, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                    enumerators[index]);
                if (EditorGUI.EndChangeCheck())
                {
                    var i = 0;
                    foreach (var enumerator in enumerators)
                    {
                        Target.ObjectNames[i] = enumerator.ToString();
                        i++;
                    }
                }
                
                if(Target.ActionType[index] == 1) Target.Value[index] = EditorGUI.FloatField(
                    new Rect(rect.x + 180, rect.y + 50, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                    Target.Value[index]);
            };

        list.onAddCallback = reorderableList =>
        {
            Target.AddFunction(null, "");
            enumerators.Add(CreateEnum(new List<string>() {"None"}));
        };
    }

    private void Reset()
    {
        enumerators = new List<Enum>() { CreateEnum(new List<string>(){"None"}) };
    }

    public static System.Enum CreateEnum(List<string> list){

        System.AppDomain currentDomain = System.AppDomain.CurrentDomain;
        AssemblyName aName = new AssemblyName("Enum");
        AssemblyBuilder ab = currentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
        ModuleBuilder mb = ab.DefineDynamicModule(aName.Name);
        EnumBuilder enumerator = mb.DefineEnum("Enum", TypeAttributes.Public, typeof(int));

        int i = 0;
        enumerator.DefineLiteral("None", i); //Here = enum{ None }

        foreach(string names in list){
            i++;
            enumerator.DefineLiteral(names, i);
        }

//Here = enum { None, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }

        System.Type finished = enumerator.CreateType();

        return (System.Enum)System.Enum.ToObject(finished,0);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
