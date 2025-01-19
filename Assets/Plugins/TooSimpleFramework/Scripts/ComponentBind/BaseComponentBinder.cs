/*
 * @FileName: BaseComponentBinder.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-10 15:39
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-19 23:59
 * @Description: Base class for ComponentPropertyBinder and ComponentEventBinder
 */
using UnityEngine;


namespace TooSimpleFramework.Components
{
    public abstract class BaseComponentBinder : MonoBehaviour
    {
        [SerializeField]
        private BindInfo[] m_Infos;


#if UNITY_EDITOR
        public BindInfo[] EditorInfos { get => this.m_Infos; set => this.m_Infos = value; }
#endif


        private void Awake()
        {
            this.OnBind(this.m_Infos);
        }


        protected abstract void OnBind(BindInfo[] pInfos);


        [System.Serializable]
        public class BindInfo
        {
            public string ViewFieldName;
            public Object ComponentRef; // GameObject or Component
            public string TargetName; // PropertyName or EventName
        }
    }
}