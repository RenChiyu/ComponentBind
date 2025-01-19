/*
 * @FileName: ComponentProperty.cs
 * @Author: Chiyu Ren
 * @Date: 2024-12-15 17:23
 * @LastEditTime: 2025-01-20 00:01
 * @LastEditors: Chiyu Ren
 * @Description: Base middleware for property of component
 */
using UnityEngine;
using System.Reflection;


namespace TooSimpleFramework.Components
{
    public interface IComponentProperty
    {
        void Bind(Object pTargetObj, PropertyInfo pPropertyInfo);
    }


    public abstract class BaseComponentProperty<T> : IComponentProperty
    {
        private Object m_Target = null;
        protected Object Target => this.m_Target; 
        private IGetterSetter<T> m_GetterSetter = null;
        private T m_Value; // Cached value


        public T Get()
        {
            return this.m_Value;
        }


        public void Set(T pValue)
        {
            this.m_Value = pValue;
            this.OnSet(pValue);
        }


        /// <summary>
        /// Call getter of property to refresh value
        /// </summary>
        public void Refresh()
        {
            if (this.m_GetterSetter != null)
            {
                this.m_Value = this.m_GetterSetter.Get(this.m_Target);
            }
        }


        public void Bind(Object pTargetObj, PropertyInfo pPropertyInfo)
        {
            if (pPropertyInfo.PropertyType == typeof(T))
            {
                this.m_Target = pTargetObj;
                this.m_GetterSetter = ComponentBindUtils.GetGetterSetter(pPropertyInfo) as IGetterSetter<T>;
                this.Refresh();
            }
        }


        // Derived class can do something like Image.SetNativeSize()
        // before/after get or set through override those methods
        protected virtual T OnGet()
        {
            return this.m_Value;
        }


        protected virtual void OnSet(T pValue)
        {
            this.m_GetterSetter?.Set(this.m_Target, pValue);
        }
    }
}