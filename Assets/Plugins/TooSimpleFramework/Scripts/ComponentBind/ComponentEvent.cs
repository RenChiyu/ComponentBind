/*
 * @FileName: ComponentEvent.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-12 10:19
 * @LastEditTime: 2025-01-20 00:00
 * @LastEditors: Chiyu Ren
 * @Description: Base middleware for event of component
 */
using UnityEngine.Events;


namespace TooSimpleFramework.Components
{
    public interface IComponentEvent
    {
        void Bind(UnityEventBase pTargetEvent);
    }


    public abstract class BaseComponentEvent
    {
        protected UnityEventBase m_Target = null;
    }


    public abstract class BaseComponentEvent<T> : BaseComponentEvent, IComponentEvent
    {
        private IAddRemove<T> m_AddRemove = null;


        public void AddListener(UnityAction<T> pCallback)
        {
            if (pCallback != null && base.m_Target != null)
            {
                this.m_AddRemove?.AddListener(base.m_Target, pCallback);
            }
        }


        public void RemoveListener(UnityAction<T> pCallback)
        {
            if (pCallback != null && base.m_Target != null)
            {
                this.m_AddRemove?.RemoveListener(base.m_Target, pCallback);
            }
        }


        public void Bind(UnityEventBase pTargetEvent)
        {
            this.m_Target = pTargetEvent;
            this.m_AddRemove = ComponentBindUtils.GetAddRemove(pTargetEvent.GetType()) as IAddRemove<T>;
        }
    }


    public class ComponentEventVoid : BaseComponentEvent, IComponentEvent
    {
        private IAddRemove m_AddRemove = null;


        public ComponentEventVoid() { }


        public void AddListener(UnityAction pCallback)
        {
            if (pCallback != null && base.m_Target != null)
            {
                this.m_AddRemove?.AddListener(base.m_Target, pCallback);
            }
        }


        public void RemoveListener(UnityAction pCallback)
        {
            if (pCallback != null && base.m_Target != null)
            {
                this.m_AddRemove?.RemoveListener(base.m_Target, pCallback);
            }
        }


        public void Bind(UnityEventBase pTargetEvent)
        {
            this.m_Target = pTargetEvent;
            this.m_AddRemove = ComponentBindUtils.GetAddRemove(pTargetEvent.GetType()) as IAddRemove;
        }
    }
}