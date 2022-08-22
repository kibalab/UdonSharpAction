using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using K13A.USharpAction.Editor;
using UdonSharp;
using UdonSharpEditor;
using VRC.SDKBase;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.Udon;
using Object = UnityEngine.Object;

namespace K13A.USharpAction.Editor
{
    [CustomEditor(typeof(UdonAction))]
    public class UdonActionEditor : UnityEditor.Editor
    {
        private ReorderableList list;

        public UdonAction Target;

        public List<UdonEvent> UdonEvents = new List<UdonEvent>();

        private void OnEnable()
        {
            Target = (UdonAction) target;

            var j = 0;
            foreach (var objectName in Target.Behaviours)
            {
                var udonEvent = new UdonEvent();
                udonEvent.ActionType = Target.ActionType[j];
                udonEvent.Behaviour = Target.Behaviours[j];
                udonEvent.ObjectName = Target.ObjectNames[j];
                udonEvent.Value = Target.Value[j];
                udonEvent.IsSynced = Target.IsSynced[j];
                udonEvent.TransferOfOwnership = Target.TransferOfOwnership[j];

                udonEvent.UpdateExecutables();
                UdonEvents.Add(udonEvent);
                j++;
            }

            list = new ReorderableList(UdonEvents, typeof(UdonEvent),
                true, true, true, true);

            list.drawHeaderCallback = (rect) =>
                EditorGUI.LabelField(rect, Target.name);

            list.elementHeight = 80;

            list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    UdonEventEditor.DrawEvent(rect, UdonEvents[index]);
                };
        }

        private void Reset()
        {
            UdonEvents.Clear();
        }

        public void SaveToAction()
        {
            List<int> ActionType = new List<int>();
            List<UdonSharpBehaviour> Behaviours = new List<UdonSharpBehaviour>();
            List<string> ObjectNames = new List<string>();
            List<object> Value = new List<object>();
            List<bool> IsSynced = new List<bool>();
            List<bool> TransferOfOwnership = new List<bool>();

            foreach (var udonEvent in UdonEvents)
            {
                ActionType.Add(udonEvent.ActionType);
                Behaviours.Add(udonEvent.Behaviour);
                ObjectNames.Add(udonEvent.ObjectName);
                Value.Add(udonEvent.Value);
                IsSynced.Add(udonEvent.IsSynced);
                TransferOfOwnership.Add(udonEvent.TransferOfOwnership);
            }

            Target.ActionType = ActionType.ToArray();
            Target.Behaviours = Behaviours.ToArray();
            Target.ObjectNames = ObjectNames.ToArray();
            Target.Value = Value.ToArray();
            Target.IsSynced = IsSynced.ToArray();
            Target.TransferOfOwnership = TransferOfOwnership.ToArray();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(10);
            list.DoLayoutList();
            SaveToAction();
        }
    }

    public static class SyncedModeEvent
    {
        public static Networking.SyncType syncType;
    }

    [CustomPropertyDrawer(typeof(UdonAction))]
    public class UdonActionPropertyEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!property.objectReferenceValue)
            {
                var action = ((UdonSharpBehaviour) property.serializedObject.targetObject).gameObject
                    .AddComponent<UdonAction>();
                action.name = label.text;
                property.objectReferenceValue = action;
            }
            else
            {

                if (UdonSharpEditorUtility
                    .GetBackingUdonBehaviour(((UdonSharpBehaviour) property.serializedObject.targetObject))
                    .SyncIsContinuous)
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(((UdonSharpBehaviour) property.objectReferenceValue))
                        .SyncMethod = Networking.SyncType.Continuous;
                else if (UdonSharpEditorUtility
                         .GetBackingUdonBehaviour(((UdonSharpBehaviour) property.serializedObject.targetObject))
                         .SyncIsManual)
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(((UdonSharpBehaviour) property.objectReferenceValue))
                        .SyncMethod = Networking.SyncType.Manual;
                else
                    UdonSharpEditorUtility.GetBackingUdonBehaviour(((UdonSharpBehaviour) property.objectReferenceValue))
                        .SyncMethod = Networking.SyncType.None;
                EditorGUI.PropertyField(position, property);
            }
        }
    }
}