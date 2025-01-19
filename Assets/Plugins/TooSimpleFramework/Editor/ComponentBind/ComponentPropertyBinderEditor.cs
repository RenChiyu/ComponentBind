/*
 * @FileName: ComponentPropertyBinderEditor.cs
 * @Author: Chiyu Ren
 * @Date: 2024-12-15 17:17
 * @LastEditTime: 2025-01-18 17:42
 * @LastEditors: Chiyu Ren
 * @Description: Editor of ComponentPropertyBinder
 */
using System;
using System.Collections.Generic;
using UnityEditor;

using TooSimpleFramework.Components;


namespace TooSimpleFramework.Editor
{
    [CustomEditor(typeof(ComponentPropertyBinder))]
    public class ComponentPropertyBinderEditor : BaseComponentBinderEditor<ComponentPropertyBinder>
    {
        protected override EditorTargetType TypeFlag => EditorTargetType.Property;


        protected override bool CheckTargetValid(Type pViewFieldDataType, Type pComponentType, string pTargetName)
        {
            var info = pComponentType.GetProperty(pTargetName);
            return info != null && info.PropertyType == pViewFieldDataType;
        }


        protected override void FillTargetDropdownItemList(List<string> pResultList, Type pComponentType, Type pFieldDataType)
        {
            foreach (var pair in ComponentBindUtils.GetPropertyInfoMap(pComponentType))
            {
                var item = pair.Value;
                if (!item.IsEvent() && (pFieldDataType == null || item.PropertyType == pFieldDataType))
                {
                    pResultList.Add(item.Name);
                }
            }
        }
    }
}