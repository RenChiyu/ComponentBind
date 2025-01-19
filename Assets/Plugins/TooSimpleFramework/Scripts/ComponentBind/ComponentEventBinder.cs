/*
 * @FileName: ComponentEventBinder.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-16 22:50
 * @LastEditTime: 2025-01-20 00:02
 * @LastEditors: Chiyu Ren
 * @Description: Bind event of component and field in view
 */
using UnityEngine;
using UnityEngine.Events;

using TooSimpleFramework.Common;


namespace TooSimpleFramework.Components
{
    public class ComponentEventBinder : BaseComponentBinder
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
                    Debug.LogError($"ComponentEventBind.OnBind >>Field {fieldInfo.Name} not found in {viewName}.");
                    continue;
                }
                if (fieldInfo.GetValue(view) is not IComponentEvent field)
                {
                    Debug.LogError($"ComponentEventBind.OnBind >>Field {fieldInfo.Name} is not IBaseComponentEvent type.");
                    continue;
                }
                if (field == null)
                {
                    Debug.LogError($"ComponentEventBind.OnBind >>Field {fieldInfo.Name} is null in {viewName}.");
                    continue;
                }
                var propertyInfo = ComponentBindUtils.GetPropertyInfo(item.ComponentRef, item.TargetName);
                if (propertyInfo == null)
                {
                    Debug.LogError($"ComponentEventBind.OnBind >>Event {item.TargetName} not found in {item.ComponentRef.name}.");
                    continue;
                }
                field.Bind(propertyInfo.GetValue(item.ComponentRef) as UnityEventBase);
            }
        }
    }
}