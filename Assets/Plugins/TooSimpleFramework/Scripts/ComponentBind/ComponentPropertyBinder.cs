/*
 * @FileName: ComponentPropertyBinder.cs
 * @Author: Chiyu Ren
 * @Date: 2024-12-15 17:15
 * @LastEditTime: 2025-01-20 00:01
 * @LastEditors: Chiyu Ren
 * @Description: Bind property of component and field in view
 */
using UnityEngine;

using TooSimpleFramework.Common;


namespace TooSimpleFramework.Components
{
    public class ComponentPropertyBinder : BaseComponentBinder
    {
        protected override void OnBind(BindInfo[] pInfos)
        {
            var view = base.GetComponentInParent<BaseView>();
            var viewName = view.Name;
            var fieldMap = ComponentBindUtils.GetFieldInfoMap(view.GetType(), viewName);
            foreach (var item in pInfos)
            {
                if (string.IsNullOrEmpty(item.ViewFieldName) || item.ComponentRef == null || string.IsNullOrEmpty(item.TargetName))
                {
                    continue;
                }
                if (!fieldMap.TryGetValue(item.ViewFieldName, out var fieldInfo))
                {
                    Debug.LogError($"ComponentPropertyBind.OnBind >>Field {fieldInfo.Name} not found in {viewName}.");
                    continue;
                }
                if (fieldInfo.GetValue(view) is not IComponentProperty field)
                {
                    Debug.LogError($"ComponentPropertyBind.OnBind >>Field {fieldInfo.Name} is not IBaseComponentProperty type.");
                    continue;
                }
                if (field == null)
                {
                    Debug.LogError($"ComponentPropertyBind.OnBind >>Field {fieldInfo.Name} is null in {viewName}.");
                    continue;
                }
                var propertyInfo = ComponentBindUtils.GetPropertyInfo(item.ComponentRef, item.TargetName);
                if (propertyInfo == null)
                {
                    Debug.LogError($"ComponentPropertyBind.OnBind >>Property {item.TargetName} not found in {item.ComponentRef.name}.");
                    continue;
                }
                field.Bind(item.ComponentRef, propertyInfo);
            }
        }
    }
}