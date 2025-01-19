/*
 * @FileName: TestPanel.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-08 11:34
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-20 00:04
 * @Description: Test panel
 */
using UnityEngine;
using UnityEngine.Events;
using System.Diagnostics;
using System.Text;

using TooSimpleFramework.Common;
using TooSimpleFramework.Components;


public class TestPanel : BaseView
{
    #region PropertyBindTest
    private ComponentPropertyString P_ResultText = new();
    private ComponentEventVoid E_BtnGetSet = new();
    private ComponentEventVoid E_BtnUpdate = new();

    // Original Test
    [SerializeField]
    private Transform ImageTransform;

    // ComponentPropertyBind Test
    private ComponentPropetyVector3 P_ImageRotation = new();

    // Update Test
    private ComponentPropertyString P_BtnUpdateText = new();
    private bool m_bRun = false;

    private const int TestTimes = 1000000;
    #endregion


    #region EventBindTest
    private ComponentEventVector2 E_ScrollView = new();
    private ComponentEventFloat E_HorizontalBar = new();
    private ComponentEventFloat E_VerticalBar = new();
    private ComponentPropertyString P_ScrollViewText = new();
    private ComponentPropertyString P_HorizontalText = new();
    private ComponentPropertyString P_VerticalText = new();
    #endregion


    private void Awake()
    {
        this.E_BtnGetSet.AddListener(this.E_GetSetTest_OnInvoke);
        this.E_BtnUpdate.AddListener(this.E_UpdateTest_OnInvoke);

        this.E_ScrollView.AddListener(this.E_ScrollView_OnInvoke);
        this.E_HorizontalBar.AddListener(this.E_HorizontalBar_OnInvoke);
        this.E_VerticalBar.AddListener(this.E_VerticalBar_OnInvoke);
    }


    private void Update()
    {
        if (this.m_bRun)
        {
            var rot = this.P_ImageRotation.Get();
            rot.z += 32.0f * Time.deltaTime % 360.0f;
            this.P_ImageRotation.Set(rot);
        }
    }


    public void E_GetSetTest_OnInvoke()
    {
        if (this.m_bRun)
        {
            this.E_UpdateTest_OnInvoke();
        }

        long doTest(UnityAction pFunc)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < TestTimes; i++)
            {
                pFunc.Invoke();
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        };

        var originGetCost = doTest(() => _ = this.ImageTransform.localEulerAngles);
        var bindGetCost = doTest(() => _ = this.P_ImageRotation.Get());
        var originSetCost = doTest(() => this.ImageTransform.localEulerAngles = new Vector3(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
        var bindSetCost = doTest(() => this.P_ImageRotation.Set(new Vector3(0.0f, 0.0f, Random.Range(0.0f, 360.0f))));
        var originGetSetCost = doTest(() =>
        {
            var rot = this.ImageTransform.localEulerAngles;
            rot.z = (rot.z + 32.0f) % 360.0f;
            this.ImageTransform.localEulerAngles = rot;
        });
        var bindGetSetCost = doTest(() =>
        {
            var rot = this.P_ImageRotation.Get();
            rot.z = (rot.z + 32.0f) % 360.0f;
            this.P_ImageRotation.Set(rot);
        });

        var sb = new StringBuilder();
        sb.Append("Origin / PropertyBind\r\n");
        sb.Append($"Get: {originGetCost}ms / {bindGetCost}ms\r\n");
        sb.Append($"Set: {originSetCost}ms / {bindSetCost}ms\r\n");
        sb.Append($"GetSet: {originGetSetCost}ms / {bindGetSetCost}ms\r\n");
        this.P_ResultText.Set(sb.ToString());
    }


    public void E_UpdateTest_OnInvoke()
    {
        this.m_bRun = !this.m_bRun;
        this.P_BtnUpdateText.Set(this.m_bRun ? "Stop" : "Update Test");
    }


    private void E_ScrollView_OnInvoke(Vector2 pParam)
    {
        this.P_ScrollViewText.Set(pParam.ToString());
    }


    private void E_HorizontalBar_OnInvoke(float pParam)
    {
        this.P_HorizontalText.Set(pParam.ToString());
    }


    private void E_VerticalBar_OnInvoke(float pParam)
    {
        this.P_VerticalText.Set(pParam.ToString());
    }
}