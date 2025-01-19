/*
 * @FileName: ComponentEventBinderEditor.cs
 * @Author: Chiyu Ren
 * @Date: 2024-12-15 17:17
 * @LastEditTime: 2025-01-19 23:59
 * @LastEditors: Chiyu Ren
 * @Description: Editor of ComponentEventBinder
 */
using System;
using System.Collections.Generic;
using UnityEditor;

using TooSimpleFramework.Components;


namespace TooSimpleFramework.Editor
{
    [CustomEditor(typeof(ComponentEventBinder))]
    public class ComponentEventBinderEditor : BaseComponentBinderEditor<ComponentEventBinder>
    {
        protected override EditorTargetType TypeFlag => EditorTargetType.Event;


        protected override bool CheckTargetValid(Type pViewFieldDataType, Type pComponentType, string pTargetName)
        {
            var info = pComponentType.GetProperty(pTargetName);
            return info != null && info.PropertyType.GetEventParamType() == pViewFieldDataType;
        }


        protected override void FillTargetDropdownItemList(List<string> pResultList, Type pComponentType, Type pFieldDataType)
        {
            foreach (var pair in ComponentBindUtils.GetPropertyInfoMap(pComponentType))
            {
                var item = pair.Value;
                if (item.IsEvent() && (pFieldDataType == null || item.PropertyType.GetEventParamType() == pFieldDataType))
                {
                    pResultList.Add(item.Name);
                }
            }
        }
    }
}