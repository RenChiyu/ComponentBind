/*
 * @FileName: BaseView.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-06 09:45
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:03
 * @Description: Simple view base class
 */
using UnityEngine;


namespace TooSimpleFramework.Common
{
    public abstract class BaseView : MonoBehaviour
    {
        private System.Type m_ViewType = null;
        /// <summary>
        /// Override this for hotupdate view like lua
        /// </summary>
        public virtual string Name => (this.m_ViewType ??= this.GetType()).Name;
    }
}