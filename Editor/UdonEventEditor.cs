using System;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = UnityEngine.Object;

namespace K13A.USharpAction.Editor
{
    public class UdonEventEditor : UnityEditor.Editor
    {
        
        public static void DrawEvent(Rect rect, UdonEvent element)
        {
            
            EditorGUI.BeginChangeCheck();

            element.ActionType = (int)(ActionType)DrawActionPopup(rect, (ActionType)element.ActionType);

            element.Behaviour = DrawBehaviourField(rect, element.Behaviour);

                
            if (EditorGUI.EndChangeCheck())
            {
                element.UpdateExecutables();
            }

            if (element.ActionType == 0)
                SyncedOptions(rect, element);
            
            if (element.Behaviour == null) return;

            element.executables = DrawExcutePopup(rect, element.executables);
            element.ObjectName = element.executables.ToString();
            
            if (element.ActionType == 1) element.Value = DrawFieldForType(
                new Rect(rect.x + 180, rect.y + 50, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                element.Value, 
                element.Behaviour.GetType().GetField(element.ObjectName)) ;
        }

        public static Enum DrawActionPopup(Rect rect, Enum type)
        {
            return EditorGUI.EnumPopup(
                new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight)
                , type);
        }

        public static UdonSharpBehaviour DrawBehaviourField(Rect rect, UdonSharpBehaviour Behaviour)
        {
            return EditorGUI.ObjectField(
                new Rect(rect.x + 180, rect.y, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                Behaviour, typeof(UdonSharpBehaviour)) as UdonSharpBehaviour;
        }

        public static void SyncedOptions(Rect rect, UdonEvent element)
        {
            element.IsSynced = GUI.Toggle(
                new Rect(rect.x, rect.y + 25, 200, EditorGUIUtility.singleLineHeight),
                element.IsSynced, "  Is Synced");

            element.TransferOfOwnership = GUI.Toggle(
                new Rect(rect.x, rect.y + 50, 200, EditorGUIUtility.singleLineHeight),
                element.TransferOfOwnership, "  Transfer of ownership");
        }

        public static Enum DrawExcutePopup(Rect rect, Enum excuteable)
        {
            return EditorGUI.EnumPopup(new Rect(rect.x + 180, rect.y + 25, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                excuteable);
        }

        public static object DrawValueField(Rect rect, Object value)
        {
            return EditorGUI.PropertyField(
                new Rect(rect.x + 180, rect.y + 50, rect.width - 130 - 50, EditorGUIUtility.singleLineHeight),
                new SerializedObject(value).GetIterator());
            
            
        }
        
        internal static object DrawFieldForType(Rect rect, object value, FieldInfo symbolField)
        {

            TooltipAttribute tooltip = symbolField.GetAttribute<TooltipAttribute>();

            GUIContent fieldLabel = GUIContent.none;

            if (symbolField == null) return null;
            
            var type = symbolField.FieldType;
            
            //Not Support Array
            /*
            if (type.IsArray)
            {
                bool foldoutEnabled = GetFoldoutState(sourceBehaviour, symbol);

                // Event tempEvent = new Event(Event.current);

                Rect foldoutRect = EditorGUI.GetControlRect();
                foldoutEnabled = EditorGUI.Foldout(foldoutRect, foldoutEnabled, fieldLabel, true);

                SetFoldoutState(sourceBehaviour, symbol, foldoutEnabled);

                if (foldoutEnabled)
                {
                    Type elementType = type.GetElementType();

                    if (value == null)
                    {
                        GUI.changed = true;
                        return Activator.CreateInstance(type, 0);
                    }

                    EditorGUI.indentLevel++;

                    Array valueArray = value as Array;

                    using (new EditorGUI.VerticalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        int newLength = EditorGUI.DelayedIntField("Size", valueArray.Length);
                        if (newLength < 0)
                        {
                            Debug.LogError("Array size must be non-negative.");
                            newLength = valueArray.Length;
                        }

                        // We need to resize the array
                        if (EditorGUI.EndChangeCheck())
                        {
                            Array newArray = Activator.CreateInstance(type, newLength) as Array;

                            for (int i = 0; i < newLength && i < valueArray.Length; ++i)
                            {
                                newArray.SetValue(valueArray.GetValue(i), i);
                            }

                            // Fill the empty elements with the last element's value when expanding the array
                            if (valueArray.Length > 0 && newLength > valueArray.Length)
                            {
                                object lastElementVal = valueArray.GetValue(valueArray.Length - 1);
                                if (!(lastElementVal is Array)) // We do not want copies of the reference to a jagged array element to be copied
                                {
                                    for (int i = valueArray.Length; i < newLength; ++i)
                                    {
                                        newArray.SetValue(lastElementVal, i);
                                    }
                                }
                            }

                            EditorGUI.indentLevel--;
                            return newArray;
                        }

                        for (int i = 0; i < valueArray.Length; ++i)
                        {
                            EditorGUI.BeginChangeCheck();
                            object newArrayVal = DrawFieldForType(sourceBehaviour, $"Element {i}", $"{symbol}_element{i}", valueArray.GetValue(i), elementType, symbolField);

                            if (EditorGUI.EndChangeCheck())
                            {
                                valueArray = (Array)valueArray.Clone();
                                valueArray.SetValue(newArrayVal, i);
                            }
                        }

                        EditorGUI.indentLevel--;

                        return valueArray;
                    }
                }
            } 
            */

            if (value != null && value.GetType() != type) value = null;
            if (type == typeof(string))
            {
                TextAreaAttribute textArea = symbolField?.GetAttribute<TextAreaAttribute>();

                if (textArea != null)
                {
                    string textAreaText = EditorGUI.TextArea(rect, (string)value);

                    return textAreaText;
                }
                else
                {
                    return EditorGUI.TextField(rect, fieldLabel, (string)value);
                }
            }
            else if (type == typeof(float))
            {
                RangeAttribute range = symbolField?.GetAttribute<RangeAttribute>();

                if (range != null)
                    return EditorGUI.Slider(rect, fieldLabel, (float?)value ?? default, range.min, range.max);
                
                return EditorGUI.FloatField(rect, fieldLabel, (float?)value ?? default);
            }
            else if (type == typeof(double))
            {
                RangeAttribute range = symbolField?.GetAttribute<RangeAttribute>();

                if (range != null)
                    return EditorGUI.Slider(rect, fieldLabel, (float)((double?)value ?? default), range.min, range.max);
                    
                return EditorGUI.DoubleField(rect, fieldLabel, (double?)value ?? default);
            }
            else if (type == typeof(int))
            {
                RangeAttribute range = symbolField?.GetAttribute<RangeAttribute>();

                if (range != null)
                    return EditorGUI.IntSlider(rect, fieldLabel, (int?)value ?? default, (int)range.min, (int)range.max);
                    
                return EditorGUI.IntField(rect, fieldLabel, (int?)value ?? default);
            }
            else if (type == typeof(bool))
            {
                return EditorGUI.Toggle(rect, fieldLabel, (bool?)value ?? default);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUI.Vector2Field(rect, fieldLabel, (Vector2?)value ?? default);
            }
            else if (type == typeof(Vector2Int))
            {
                return EditorGUI.Vector2IntField(rect, fieldLabel, (Vector2Int?)value ?? default);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUI.Vector3Field(rect, fieldLabel, (Vector3?)value ?? default);
            }
            else if (type == typeof(Vector3Int))
            {
                return EditorGUI.Vector3IntField(rect, fieldLabel, (Vector3Int?)value ?? default);
            }
            else if (type == typeof(Vector4))
            {
                return EditorGUI.Vector4Field(rect, fieldLabel, (Vector4?)value ?? default);
            }
            else if (type == typeof(Color))
            {
                ColorUsageAttribute colorUsage = symbolField?.GetAttribute<ColorUsageAttribute>();

                if (colorUsage != null)
                    return EditorGUI.ColorField(rect, fieldLabel, (Color?)value ?? default, false, colorUsage.showAlpha, colorUsage.hdr);
                
                return EditorGUI.ColorField(rect, fieldLabel, (Color?)value ?? default);
            }
            else if (type == typeof(Color32))
            {
                ColorUsageAttribute colorUsage = symbolField?.GetAttribute<ColorUsageAttribute>();

                if (colorUsage != null)
                    return (Color32)EditorGUI.ColorField(rect, fieldLabel, (Color32?)value ?? default, false, colorUsage.showAlpha, false);
                
                return (Color32)EditorGUI.ColorField(rect, fieldLabel, (Color32?)value ?? default);
            }
            else if (type == typeof(Quaternion))
            {
                Quaternion quatVal = (Quaternion?)value ?? default;
                Vector4 newQuat = EditorGUI.Vector4Field(rect, fieldLabel, new Vector4(quatVal.x, quatVal.y, quatVal.z, quatVal.w));
                return new Quaternion(newQuat.x, newQuat.y, newQuat.z, newQuat.w);
            }
            else if (type == typeof(Bounds))
            {
                return EditorGUI.BoundsField(rect, fieldLabel, (Bounds?)value ?? default);
            }
            else if (type == typeof(BoundsInt))
            {
                return EditorGUI.BoundsIntField(rect, fieldLabel, (BoundsInt?)value ?? default);
            }
            else if (type == typeof(ParticleSystem.MinMaxCurve))
            {
                // This is just matching the standard Udon editor's capability at the moment, I want to eventually switch it to use the proper curve editor, but that will take a chunk of work
                ParticleSystem.MinMaxCurve minMaxCurve = (ParticleSystem.MinMaxCurve?)value ?? default;

                EditorGUI.LabelField(rect, fieldLabel);
                EditorGUI.indentLevel++;
                minMaxCurve.curveMultiplier = EditorGUI.FloatField(rect, "Multiplier", minMaxCurve.curveMultiplier);
                minMaxCurve.curveMin = EditorGUI.CurveField(rect, "Min Curve", minMaxCurve.curveMin);
                minMaxCurve.curveMax = EditorGUI.CurveField(rect, "Max Curve", minMaxCurve.curveMax);

                EditorGUI.indentLevel--;


                return minMaxCurve;
            }
            else if (type == typeof(LayerMask))
            {
                return (LayerMask)EditorGUI.MaskField(rect, fieldLabel, (LayerMask?)value ?? default, Enumerable.Range(0, 32).Select(e => LayerMask.LayerToName(e).Length > 0 ? e + ": " + LayerMask.LayerToName(e) : "").ToArray());
            }
            else if (type.IsEnum)
            {
                return EditorGUI.EnumPopup(rect, fieldLabel, (Enum)(value ?? Activator.CreateInstance(type)));
            }
            else if (type == typeof(System.Type))
            {
                string typeName = value != null ? ((Type)value).FullName : "null";
                EditorGUI.LabelField(rect, fieldLabel, typeName);
            }
            else if (type == typeof(Gradient))
            {
                GradientUsageAttribute gradientUsage = symbolField?.GetAttribute<GradientUsageAttribute>();

                if (value == null)
                {
                    value = new Gradient();
                    GUI.changed = true;
                }

                if (gradientUsage != null)
                    return EditorGUI.GradientField(rect, fieldLabel, (Gradient)value, gradientUsage.hdr);
                
                return EditorGUI.GradientField(rect, fieldLabel, (Gradient)value);
            }
            else if (type == typeof(AnimationCurve))
            {
                return EditorGUI.CurveField(rect, fieldLabel, (AnimationCurve)value ?? new AnimationCurve());
            }
            else if (type == typeof(char))
            {
                string stringVal = EditorGUI.TextField(rect, fieldLabel, (((char?)value) ?? default).ToString());
                if (stringVal.Length > 0)
                    return stringVal[0];
                
                return (char?)value ?? default;
            }
            else if (type == typeof(uint))
            {
                return (uint)Math.Min(Math.Max(EditorGUI.LongField(rect, fieldLabel, (uint?)value ?? default), uint.MinValue), uint.MaxValue);
            }
            else if (type == typeof(long))
            {
                return EditorGUI.LongField(rect, fieldLabel, (long?)value ?? default);
            }
            else if (type == typeof(byte))
            {
                return (byte)Mathf.Clamp(EditorGUI.IntField(rect, fieldLabel, (byte?)value ?? default), byte.MinValue, byte.MaxValue);
            }
            else if (type == typeof(sbyte))
            {
                return (sbyte)Mathf.Clamp(EditorGUI.IntField(rect, fieldLabel, (sbyte?)value ?? default), sbyte.MinValue, sbyte.MaxValue);
            }
            else if (type == typeof(short))
            {
                return (short)Mathf.Clamp(EditorGUI.IntField(rect, fieldLabel, (short?)value ?? default), short.MinValue, short.MaxValue);
            }
            else if (type == typeof(ushort))
            {
                return (ushort)Mathf.Clamp(EditorGUI.IntField(rect, fieldLabel, (ushort?)value ?? default), ushort.MinValue, ushort.MaxValue);
            }
            else if (type == typeof(Rect))
            {
                return EditorGUI.RectField(rect, fieldLabel, (Rect?)value ?? default);
            }
            else if (type == typeof(RectInt))
            {
                return EditorGUI.RectIntField(rect, fieldLabel, (RectInt?)value ?? default);
            }
            else if (type == typeof(VRC.SDKBase.VRCUrl))
            {
                VRC.SDKBase.VRCUrl url = (VRC.SDKBase.VRCUrl)value ?? new VRC.SDKBase.VRCUrl("");
                url = new VRC.SDKBase.VRCUrl(EditorGUI.TextField(rect, fieldLabel, url.Get()));
                return url;
            }
            else
            {
                EditorGUI.LabelField(rect, $"{fieldLabel}: no drawer for type {type}");

                return value;
            }

            return value;
        }
    }
}