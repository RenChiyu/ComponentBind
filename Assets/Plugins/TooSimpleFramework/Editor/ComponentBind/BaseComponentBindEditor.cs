/*
 * @FileName: BaseComponentBindEditor.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-10 14:28
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:02
 * @Description: Base editor for component binder component
 */
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using TooSimpleFramework.Common;
using TooSimpleFramework.Components;

using Object = UnityEngine.Object;
using RuntimeBindInfo = TooSimpleFramework.Components.BaseComponentBinder.BindInfo;


namespace TooSimpleFramework.Editor
{
    public abstract class BaseComponentBinderEditor<T> : UnityEditor.Editor where T : BaseComponentBinder
    {
        private ReorderableList m_List;
        private List<EditorBindInfo> m_BindInfoList = null;
        private T m_TargetRef;
        private List<ViewFieldInfo> m_ViewFieldInfoList = null;
        private BaseView m_ParentView = null;
        private int m_nSelectedIndex;

        protected abstract EditorTargetType TypeFlag { get; }


        private void OnEnable()
        {
            this.m_TargetRef = base.target as T;
            this.CollectFieldNames();

            if (this.m_TargetRef.EditorInfos == null)
            {
                this.m_TargetRef.EditorInfos = new RuntimeBindInfo[0];
            }
            this.m_BindInfoList = new List<EditorBindInfo>(this.m_TargetRef.EditorInfos.Length);
            for (int i = 0, count = this.m_TargetRef.EditorInfos.Length; i < count; i++)
            {
                var item = EditorBindInfo.FromRuntimeData(this.m_TargetRef.EditorInfos[i]);
                this.m_BindInfoList.Add(item);
            }
            this.CheckInfosValid();

            this.m_List = new ReorderableList(this.m_BindInfoList, typeof(EditorBindInfo), false, false, true, true)
            {
                elementHeight = 64,
                drawElementCallback = (rect, index, isActive, isFocused) => this.DrawInfoItem(rect, index),
                onSelectCallback = list => this.m_nSelectedIndex = list.selectedIndices.Count > 0 ? list.selectedIndices[0] : -1,
                onAddCallback = _ =>
                {
                    this.m_BindInfoList.Add(new EditorBindInfo());
                    this.m_TargetRef.EditorInfos = this.ConvertEditorDataToRuntime();
                },
                onRemoveCallback = _ =>
                {
                    this.m_BindInfoList.RemoveAt(this.m_nSelectedIndex);
                    this.m_TargetRef.EditorInfos = this.ConvertEditorDataToRuntime();
                },
            };
        }


        public override void OnInspectorGUI()
        {
            if (this.m_ParentView == null)
            {
                EditorGUILayout.HelpBox($"Parent view not found. \r\n{this.m_TargetRef.GetType().Name} must work with component that inherit from BaseView.", MessageType.Warning);
                return;
            }

            this.m_List.DoLayoutList();

            var shouldSetDirty = false;
            for (int i = 0, count = this.m_BindInfoList.Count; i < count; i++)
            {
                var item = this.m_BindInfoList[i];
                if (item.IsDirty)
                {
                    shouldSetDirty = true;
                    item.IsDirty = false;
                    var info = this.m_TargetRef.EditorInfos[i];
                    info.ViewFieldName = item.ViewFieldName;
                    info.ComponentRef = item.Component;
                    info.TargetName = item.TargetName;
                }
            }
            if (shouldSetDirty)
            {
                EditorUtility.SetDirty(base.target);
            }
        }


        private void CollectFieldNames()
        {
            // Find all field in BaseView that inherit from IBaseComponentProperty / IBaseComponentEvent
            this.m_ParentView = this.m_TargetRef.gameObject.GetComponentInParent<BaseView>();
            this.m_ViewFieldInfoList = new List<ViewFieldInfo>();
            var baseFieldType = this.TypeFlag == EditorTargetType.Property ? typeof(IComponentProperty) : typeof(IComponentEvent);
            if (this.m_ParentView != null)
            {
                foreach (var pair in ComponentBindUtils.GetFieldInfoMap(this.m_ParentView.GetType()))
                {
                    var item = pair.Value;
                    if (baseFieldType.IsAssignableFrom(item.FieldType))
                    {
                        this.m_ViewFieldInfoList.Add(new ViewFieldInfo(item, this.TypeFlag));
                    }
                }
            }
        }


        private void CheckInfosValid()
        {
            foreach (var item in this.m_BindInfoList)
            {
                this.CheckInfoValid(item);
            }
        }


        private void CheckInfoValid(EditorBindInfo pItem)
        {
            pItem.ViewFieldNameValid = false;
            pItem.ComponentValid = false;
            pItem.TargetNameValid = false;
            // Check ViewFieldName
            //
            ViewFieldInfo viewFieldInfo = null;
            if (!string.IsNullOrEmpty(pItem.ViewFieldName))
            {
                viewFieldInfo = this.m_ViewFieldInfoList.Find(v => v.Name == pItem.ViewFieldName);
                pItem.ViewFieldNameValid = viewFieldInfo != null;
            }
            // Check TargetRef
            //
            Type targetType = null;
            if (pItem.Component != null)
            {
                if (pItem.Component is GameObject)
                {
                    targetType = typeof(GameObject);
                }
                else
                {
                    var t = pItem.Component.GetType();
                    if (this.m_TargetRef.gameObject.TryGetComponent(t, out var component))
                    {
                        targetType = component == pItem.Component ? t : null;
                    }
                }
            }
            pItem.ComponentValid = targetType != null;
            // Check TargetName
            //
            if (!string.IsNullOrEmpty(pItem.TargetName) && pItem.ComponentValid)
            {
                pItem.TargetNameValid = viewFieldInfo == null || this.CheckTargetValid(viewFieldInfo.TargetType, targetType, pItem.TargetName);
            }
        }


        protected abstract bool CheckTargetValid(Type pViewFieldDataType, Type pComponentType, string pTargetName);


        private void DrawInfoItem(Rect pRect, int pIndex)
        {
            GUILayout.BeginVertical();
            var item = this.m_BindInfoList[pIndex];
            this.DrawItemDataField(pRect, item);
            this.DrawItemComponent(pRect, item);
            this.DrawItemTarget(pRect, item);
            if (pIndex != this.m_BindInfoList.Count - 1)
            {
                ComponentBindEditorUtils.DrawSeparator(pRect);
            }
            GUILayout.EndVertical();
        }


        private void DrawItemDataField(Rect pRect, EditorBindInfo pItem)
        {
            var originColor = GUI.backgroundColor;
            if (!pItem.ViewFieldNameValid)
            {
                GUI.backgroundColor = Color.red;
            }

            ComponentBindEditorUtils.CalcDropdownRect(pRect, 0, out var labelRect, out var dropRect);

            GUILayout.BeginHorizontal();
            EditorGUI.LabelField(labelRect, "Field");
            if (EditorGUI.DropdownButton(dropRect, new GUIContent(pItem.ViewFieldName), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                foreach (var item in this.m_ViewFieldInfoList)
                {
                    var name = item.Name;
                    menu.AddItem(new GUIContent(name), name == pItem.ViewFieldName, () =>
                    {
                        pItem.ViewFieldName = name;
                        this.CheckInfoValid(pItem);
                    });
                }
                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = originColor;
        }

        private void DrawItemComponent(Rect pRect, EditorBindInfo pItem)
        {
            var originColor = GUI.backgroundColor;
            if (!pItem.ComponentValid)
            {
                GUI.backgroundColor = Color.red;
            }

            ComponentBindEditorUtils.CalcDropdownRect(pRect, 1, out var labelRect, out var dropRect);

            GUILayout.BeginHorizontal();
            EditorGUI.LabelField(labelRect, "Component");
            if (EditorGUI.DropdownButton(dropRect, new GUIContent(pItem.ComponentShowName), FocusType.Keyboard))
            {
                var componentList = ComponentBindEditorUtils.GetComponentListOnGameObject(this.m_TargetRef.gameObject);
                var menu = new GenericMenu();
                foreach (var item in componentList)
                {
                    var itemName = item.GetType().Name;
                    menu.AddItem(new GUIContent(itemName), itemName == pItem.ComponentShowName, () =>
                    {
                        pItem.Component = item;
                        this.CheckInfoValid(pItem);
                    });
                }
                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = originColor;
        }


        private void DrawItemTarget(Rect pRect, EditorBindInfo pItem)
        {
            var originColor = GUI.backgroundColor;
            if (!pItem.TargetNameValid)
            {
                GUI.backgroundColor = Color.red;
            }

            ComponentBindEditorUtils.CalcDropdownRect(pRect, 2, out var labelRect, out var dropRect);

            GUILayout.BeginHorizontal();
            EditorGUI.LabelField(labelRect, this.TypeFlag == EditorTargetType.Property ? "Property" : "Event");
            if (EditorGUI.DropdownButton(dropRect, new GUIContent(pItem.TargetName), FocusType.Keyboard))
            {
                if (!pItem.ComponentValid)
                {
                    GUILayout.EndHorizontal();
                    return;
                }
                Type dataType = null;
                if (pItem.ViewFieldNameValid)
                {
                    dataType = this.m_ViewFieldInfoList.Find(item => item.Name == pItem.ViewFieldName).TargetType;
                }
                var itemNameList = new List<string>();
                this.FillTargetDropdownItemList(itemNameList, pItem.Component.GetType(), dataType);
                var menu = new GenericMenu();
                foreach (var item in itemNameList)
                {
                    menu.AddItem(new GUIContent(item), item == pItem.TargetName, () =>
                    {
                        pItem.TargetName = item;
                        this.CheckInfoValid(pItem);
                    });
                }
                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = originColor;
        }


        protected abstract void FillTargetDropdownItemList(List<string> pResultList, Type pComponentType, Type pFieldDataType);


        private RuntimeBindInfo[] ConvertEditorDataToRuntime()
        {
            var ret = new RuntimeBindInfo[this.m_BindInfoList.Count];
            for (int i = 0, count = this.m_BindInfoList.Count; i < count; i++)
            {
                ret[i] = this.m_BindInfoList[i].ToRuntimeData();
            }
            return ret;
        }


        protected enum EditorTargetType
        {
            Property,
            Event,
        }


        /// <summary>
        /// Field info that assigned in parent view
        /// </summary>
        private class ViewFieldInfo
        {
            /// <summary>
            /// Field name
            /// </summary>
            public string Name;
            /// <summary>
            /// Generic type，currently support one-param field only
            /// </summary>
            public Type TargetType;


            public ViewFieldInfo(FieldInfo pInfo, EditorTargetType pFlag)
            {
                this.Name = pInfo.Name;
                this.TargetType = null;

                var gtas = pInfo.FieldType.BaseType?.GenericTypeArguments;
                switch (pFlag)
                {
                    case EditorTargetType.Property:
                        if (gtas != null && gtas.Length == 1)
                        {
                            this.TargetType = gtas[0];
                        }
                        break;
                    case EditorTargetType.Event:
                        if (pInfo.FieldType == typeof(ComponentEventVoid))
                        {
                            this.TargetType = typeof(void);
                        }
                        else if (gtas.Length == 1)
                        {
                            this.TargetType = gtas[0];
                        }
                        break;
                }

            }
        }


        private class EditorBindInfo
        {
            public bool IsDirty;
            private string m_ViewFieldName;
            public string ViewFieldName
            {
                get => this.m_ViewFieldName;
                set
                {
                    this.m_ViewFieldName = value;
                    this.IsDirty = true;
                }
            }
            public bool ViewFieldNameValid;

            private Object m_Component;
            public Object Component
            {
                get => this.m_Component;
                set
                {
                    this.m_Component = value;
                    this.ComponentShowName = value == null ? null : value.GetType().Name;
                    this.IsDirty = true;
                }
            }
            public bool ComponentValid;

            public string ComponentShowName { get; private set; }

            private string m_TargetName;
            public string TargetName
            {
                get => this.m_TargetName;
                set
                {
                    this.m_TargetName = value;
                    this.IsDirty = true;
                }
            }
            public bool TargetNameValid;


            public EditorBindInfo() { }


            public static EditorBindInfo FromRuntimeData(RuntimeBindInfo pInfo)
            {
                var ret = new EditorBindInfo()
                {
                    m_ViewFieldName = pInfo.ViewFieldName,
                    Component = pInfo.ComponentRef,
                    m_TargetName = pInfo.TargetName,
                    IsDirty = false,
                };
                return ret;
            }


            public RuntimeBindInfo ToRuntimeData()
            {
                var ret = new RuntimeBindInfo()
                {
                    ViewFieldName = this.ViewFieldName,
                    ComponentRef = this.Component,
                    TargetName = this.TargetName,
                };
                return ret;
            }
        }
    }
}