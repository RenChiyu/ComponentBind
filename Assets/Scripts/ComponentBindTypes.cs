/*
 * @FileName: ComponentBindTypes.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-13 00:15
 * @LastEditTime: 2025-01-20 00:04
 * @LastEditors: Chiyu Ren
 * @Description: Custom component bind classes
 */
using UnityEngine;

using TooSimpleFramework.Components;


#region Property
public sealed class ComponentPropertyString : BaseComponentProperty<string> { }
public sealed class ComponentPropetyVector3 : BaseComponentProperty<Vector3> { }
#endregion


#region Event
public sealed class ComponentEventFloat : BaseComponentEvent<float> { }
public sealed class ComponentEventVector2 : BaseComponentEvent<Vector2> { }
#endregion