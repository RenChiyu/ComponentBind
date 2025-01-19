/*
 * @FileName: DelegateEncapsulation.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-09 15:35
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:03
 * @Description: Encapsulation of delegate for getter/setter of property, or AddListener/RemoveListener of UnityEvent
 */
using System;
using System.Reflection;
using UnityEngine.Events;


namespace TooSimpleFramework.Components
{
    #region Property Bind
    internal interface IGetterSetter<T>
    {
        T Get(object pInvoker);
        void Set(object pInvoker, T pValue);
    }


    // Public for code gen
    public class GetterSetter<TComponent, TValue> : IGetterSetter<TValue> where TComponent : UnityEngine.Object
    {
        private Func<TComponent, TValue> m_GetterDelegate = null;
        private Action<TComponent, TValue> m_SetterDelegate = null;


        // Use for Activator.CreateInstance
        public GetterSetter(PropertyInfo pInfo)
        {
            if (pInfo == null)
            {
                return;
            }

            // Use Delegate.CreateDelegate instead of Property.GetValue/SetValue to avoid gc alloc in boxing/unboxing
            var getMethod = pInfo.GetGetMethod();
            if (getMethod != null)
            {
                this.m_GetterDelegate = (Func<TComponent, TValue>)Delegate.CreateDelegate(typeof(Func<TComponent, TValue>), getMethod);
            }
            var setMethod = pInfo.GetSetMethod();
            if (setMethod != null)
            {
                this.m_SetterDelegate = (Action<TComponent, TValue>)Delegate.CreateDelegate(typeof(Action<TComponent, TValue>), setMethod);
            }
        }


        public TValue Get(object pInvoker)
        {
            return this.m_GetterDelegate == null
                ? default
                : this.m_GetterDelegate.Invoke(pInvoker as TComponent);
        }


        public void Set(object pInvoker, TValue pValue)
        {
            this.m_SetterDelegate?.Invoke(pInvoker as TComponent, pValue);
        }
    }
    #endregion


    #region Event Bind(void)
    internal interface IAddRemove
    {
        void AddListener(object pInvoker, UnityAction pCallback);
        void RemoveListener(object pInvoker, UnityAction pValue);
    }


    public class AddRemove<TEvent> : IAddRemove where TEvent : UnityEventBase
    {
        private Action<TEvent, UnityAction> m_AddDelegate = null;
        private Action<TEvent, UnityAction> m_RemoveDelegate = null;


        // Use for Activator.CreateInstance
        public AddRemove()
        {
            var eventType = typeof(TEvent);
            var addMethod = eventType.GetMethod("AddListener");
            if (addMethod != null)
            {
                this.m_AddDelegate = (Action<TEvent, UnityAction>)Delegate.CreateDelegate(typeof(Action<TEvent, UnityAction>), addMethod);
            }
            var removeMethod = eventType.GetMethod("RemoveListener");
            if (removeMethod != null)
            {
                this.m_RemoveDelegate = (Action<TEvent, UnityAction>)Delegate.CreateDelegate(typeof(Action<TEvent, UnityAction>), addMethod);
            }
        }


        public void AddListener(object pInvoker, UnityAction pCallback)
        {
            this.m_AddDelegate?.Invoke(pInvoker as TEvent, pCallback);
        }


        public void RemoveListener(object pInvoker, UnityAction pCallback)
        {
            this.m_RemoveDelegate?.Invoke(pInvoker as TEvent, pCallback);
        }
    }
    #endregion


    #region Event Bind(T)
    internal interface IAddRemove<T>
    {
        void AddListener(object pInvoker, UnityAction<T> pCallback);
        void RemoveListener(object pInvoker, UnityAction<T> pValue);
    }


    public class AddRemove<TEvent, TValue> : IAddRemove<TValue> where TEvent : UnityEventBase
    {
        private Action<TEvent, UnityAction<TValue>> m_AddDelegate = null;
        private Action<TEvent, UnityAction<TValue>> m_RemoveDelegate = null;


        // Use for Activator.CreateInstance
        public AddRemove()
        {
            var eventType = typeof(TEvent);
            var addMethod = eventType.GetMethod("AddListener");
            if (addMethod != null)
            {
                this.m_AddDelegate = (Action<TEvent, UnityAction<TValue>>)Delegate.CreateDelegate(typeof(Action<TEvent, UnityAction<TValue>>), addMethod);
            }
            var removeMethod = eventType.GetMethod("RemoveListener");
            if (removeMethod != null)
            {
                this.m_RemoveDelegate = (Action<TEvent, UnityAction<TValue>>)Delegate.CreateDelegate(typeof(Action<TEvent, UnityAction<TValue>>), addMethod);
            }
        }


        public void AddListener(object pInvoker, UnityAction<TValue> pCallback)
        {
            this.m_AddDelegate?.Invoke(pInvoker as TEvent, pCallback);
        }


        public void RemoveListener(object pInvoker, UnityAction<TValue> pCallback)
        {
            this.m_RemoveDelegate?.Invoke(pInvoker as TEvent, pCallback);
        }
    }
    #endregion
}