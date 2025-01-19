/*
 * @FileName: ComponentBindEditorUtils.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-16 14:01
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:02
 * @Description: Utils for component bind editor
 */
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using TooSimpleFramework.Components;


namespace TooSimpleFramework.Editor
{
    public abstract class ComponentBindEditorUtils
    {
        public static void CalcDropdownRect(Rect pRect, int pItemIndex, out Rect pOutLableRect, out Rect pOutDropRect)
        {
            var yOffset = pItemIndex * 20;
            pOutLableRect = new Rect(pRect)
            {
                width = 80,
                height = 16,
                y = pRect.y + 3 + yOffset,
            };
            pOutDropRect = new Rect(pRect)
            {
                width = pRect.width - 90,
                height = 16,
                x = pRect.x + 90,
                y = pRect.y + 3 + yOffset,
            };
        }


        public static List<Object> GetComponentListOnGameObject(GameObject pGameObject)
        {
            // Collect all component on this gameObject
            var ret = new List<Object>()
            {
                pGameObject, // GameObject does not inherit from Component, add manually
            };
            var type = typeof(BaseComponentBinder);
            foreach (var item in pGameObject.GetComponents<Component>())
            {
                if (!type.IsAssignableFrom(item.GetType()))
                {
                    ret.Add(item);
                }
            }
            return ret;
        }


        public static void DrawSeparator(Rect pRect)
        {
            pRect.y += 3 * 20 + 4;
            pRect.height = 1;
            EditorGUI.DrawRect(pRect, new Color(0.157f, 0.157f, 0.157f));
        }
    }
}