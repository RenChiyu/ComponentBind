/*
 * @FileName: ComponentBindUtils.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-08 18:27
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:03
 * @Description: Utils for component bind
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Events;


namespace TooSimpleFramework.Components
{
    public class ComponentBindUtils
    {
        // Key - View name, Value - { Key - Field name }
        private static readonly Dictionary<string, Dictionary<string, FieldInfo>> m_DataFieldCache = new();
        // Key - typeof(Component), Value - { Key - Property name, Value - Properties contains event }
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> m_ComponentPropertyCache = new();
        // Key - DeclaringType of property in component, Value - { Key - Property name, Value - GetterSetter<,> }
        private static readonly Dictionary<Type, Dictionary<string, object>> m_GetterSetterCache = new();
        // Key - typeof(Event), Value - IAddRemove or IAddRemove<T>
        private static readonly Dictionary<Type, object> m_AddRemoveCache = new();

        private static readonly Type IBaseComponentPropertyType = typeof(IComponentProperty);
        private static readonly Type IBaseComponentEventType = typeof(IComponentEvent);
        private static readonly Type GetterSetterBaseType = typeof(GetterSetter<,>);
        private static readonly Type AddRemoveVoidBaseType = typeof(AddRemove<>);
        private static readonly Type AddRemoveOneParamBaseType = typeof(AddRemove<,>);


        public static Dictionary<string, FieldInfo> GetFieldInfoMap(Type pViewType)
        {
            var ret = new Dictionary<string, FieldInfo>();
            var bindingFlag = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var item in pViewType.GetFields(bindingFlag))
            {
                if (IBaseComponentPropertyType.IsAssignableFrom(item.FieldType)
                    || IBaseComponentEventType.IsAssignableFrom(item.FieldType))
                {
                    ret.Add(item.Name, item);
                }
            }
            return ret;
        }


        public static Dictionary<string, PropertyInfo> GetPropertyInfoMap(Type pComponentType)
        {
            var ret = new Dictionary<string, PropertyInfo>();
            foreach (var item in pComponentType.GetProperties())
            {
                // Ignore indexer or obsolete properties
                if (!item.IsIndexer() && !item.IsObsoleted())
                {
                    ret.Add(item.Name, item);
                }
            }
            return ret;
        }


        internal static Dictionary<string, FieldInfo> GetFieldInfoMap(Type pViewType, string pViewName)
        {
            if (!m_DataFieldCache.TryGetValue(pViewName, out var ret))
            {
                ret = GetFieldInfoMap(pViewType);
                m_DataFieldCache.Add(pViewName, ret);
            }
            return ret;
        }


        internal static PropertyInfo GetPropertyInfo(UnityEngine.Object pObject, string pPropertyName)
        {
            var type = pObject.GetType();
            if (!m_ComponentPropertyCache.TryGetValue(type, out var map))
            {
                map = GetPropertyInfoMap(type);
                m_ComponentPropertyCache.Add(type, map);
            }
            map.TryGetValue(pPropertyName, out var ret);
            return ret;
        }


        internal static object GetGetterSetter(PropertyInfo pInfo)
        {
            // Property declared in base class use same GetterSetter to save memory.
            // Example: Image.enabled and Camera.enabled => GetterSetter<Behaviour, bool>
            var decType = pInfo.DeclaringType;
            if (!m_GetterSetterCache.TryGetValue(decType, out var map))
            {
                map = new Dictionary<string, object>();
                m_GetterSetterCache.Add(decType, map);
            }
            if (!map.TryGetValue(pInfo.Name, out var ret))
            {
                var getsetType = GetterSetterBaseType.MakeGenericType(decType, pInfo.PropertyType);
                ret = Activator.CreateInstance(getsetType, pInfo);
                map.Add(pInfo.Name, ret);
            }
            return ret;
        }


        internal static object GetAddRemove(Type pEventType)
        {
            if (!m_AddRemoveCache.TryGetValue(pEventType, out var ret))
            {
                var gtas = pEventType.BaseType?.GenericTypeArguments;
                var addremoveType = pEventType.GetEventParamCount() switch
                {
                    0 => AddRemoveVoidBaseType.MakeGenericType(pEventType),
                    1 => AddRemoveOneParamBaseType.MakeGenericType(pEventType, gtas[0]),
                    _ => null,
                };
                if (addremoveType != null)
                {
                    ret = Activator.CreateInstance(addremoveType);
                    m_AddRemoveCache.Add(pEventType, ret);
                }
            }
            return ret;
        }
    }



    public static class ComponentBindExtensions
    {
        private static readonly Type UnityEventBaseType = typeof(UnityEventBase);


        public static bool IsIndexer(this PropertyInfo pInfo)
        {
            if (pInfo == null)
            {
                return false;
            }
            var getter = pInfo.GetGetMethod();
            if (getter != null && getter.GetParameters().Length > 0)
            {
                return true;
            }
            var setter = pInfo.GetSetMethod();
            if (setter != null && setter.GetParameters().Length > 1)
            {
                return true;
            }
            return false;
        }


        public static bool IsEvent(this PropertyInfo pInfo)
        {
            if (pInfo == null)
            {
                return false;
            }
            return pInfo.PropertyType.IsEvent();
        }


        public static bool IsEvent(this Type pType)
        {
            return UnityEventBaseType.IsAssignableFrom(pType);
        }


        public static bool IsObsoleted(this MemberInfo pItem)
        {
            return pItem.GetCustomAttribute<ObsoleteAttribute>() != null;
        }


        public static int GetEventParamCount(this Type pEventType)
        {
            if (pEventType == null || !UnityEventBaseType.IsAssignableFrom(pEventType))
            {
                return -1;
            }
            var gtas = pEventType.BaseType?.GenericTypeArguments;
            return gtas == null ? -1 : gtas.Length;
        }


        public static Type GetEventParamType(this Type pEventType)
        {
            var count = pEventType.GetEventParamCount();
            if (count == -1)
            { 
                return null;
            }
            var gtas = pEventType.BaseType.GenericTypeArguments;
            return count switch
            {
                0 => typeof(void),
                1 => gtas[0],
                _ => null,
            };
        }
    }
}