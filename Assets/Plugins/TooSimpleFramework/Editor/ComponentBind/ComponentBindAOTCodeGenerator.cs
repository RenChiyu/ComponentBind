/*
 * @FileName: ComponentBindAOTCodeGenerator.cs
 * @Author: Chiyu Ren
 * @Date: 2025-01-09 17:25
 * @LastEditors: Chiyu Ren
 * @LastEditTime: 2025-01-19 23:55
 * @Description: Generate wrap code in dll for IL2Cpp AOT
 */
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

using TooSimpleFramework.Components;


namespace TooSimpleFramework.Editor
{
    public class ComponentBindAOTCodeGenerator
    {
        private const string ASMName = "TooSimpleFramework.Components.ComponentBindAOT";
        private const string DLLName = ASMName + ".dll";
        private const string DLLDir = "Assets/Plugins/TooSimpleFramework/Scripts/ComponentBind/AOT/";
        private const string DLLPath = DLLDir + DLLName;

        private static readonly HashSet<Type> IgnoreTypeSet = new()
        {
            typeof(Component),
            typeof(Behaviour),
            typeof(MonoBehaviour),
            typeof(ComponentPropertyBinder),
            typeof(ComponentEventBinder),
        };

        private static readonly HashSet<string> IgnoreNamespaceSet = new()
        {
            "UnityEngine.TestTools",
            "UnityEngine.UIElements",
        };


        [MenuItem("TooSimpleFramework/ComponentBind/Generate AOT Code")]
        public static void Execute()
        {
            if (File.Exists(DLLPath))
            {
                File.Delete(DLLPath);
            }

            var typeList = CollectComponentTypes();
            var propertyMap = CollectProperties(typeList);
            // Will generate:
            // namespace TooSimpleFramework.Components.ComponentBindAOT
            // {
            //     [Preserve]
            //     internal class WrapTypes
            //     {
            //         private class GetterSetterWrapTypes
            //         {
            //             private GetterSetter<GameObject, Transform> __GS_0__;
            //             private GetterSetter<GameObject, int> __GS_1__;
            //             ...
            //         }
            //
            //         private class AddRemoveWrapTypes
            //         {
            //             private AddRemove<Button.ButtonClickedEvent> __AR_0__;
            //             private AddRemove<Dropdown.DropdownEvent, int> __AR_1__;
            //             ...
            //         }
            //     }
            // }
            var asmName = new AssemblyName(ASMName);
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save);
            var moduleBuilder = asm.DefineDynamicModule(ASMName, DLLName);
            var topClassBuilder = moduleBuilder.DefineType(ASMName + ".WrapTypes", TypeAttributes.NotPublic);
            var attrBuilder = new CustomAttributeBuilder(typeof(PreserveAttribute).GetConstructor(new Type[0]), new object[0]);
            topClassBuilder.SetCustomAttribute(attrBuilder);
            // Property bind codes:
            var propertyClassBuilder = topClassBuilder.DefineNestedType("GetterSetterWrapTypes", TypeAttributes.NestedPrivate);
            GenPropertyBindWrapCodes(propertyClassBuilder, propertyMap);
            // Event bind codes:
            var eventClassBuilder = topClassBuilder.DefineNestedType("AddRemoveWrapTypes", TypeAttributes.NestedPrivate);
            GenEventBindWrapCodes(eventClassBuilder, propertyMap);
            topClassBuilder.CreateType();
            asm.Save(DLLName);
            
            if (File.Exists(DLLName))
            {
                File.Move(DLLName, DLLPath);
            }
            WriteLinkXML(); 
            AssetDatabase.Refresh();
        }


        private static List<Type> CollectComponentTypes()
        {
            var ret = new List<Type>() { typeof(GameObject) };
            var baseType = typeof(Component);
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                foreach (var type in asm.GetTypes())
                {
                    if (!type.IsAbstract
                        && type.GetCustomAttribute<ObsoleteAttribute>() == null
                        && baseType.IsAssignableFrom(type)
                        && !IgnoreTypeSet.Contains(type))
                    {
                        var attr = type.Attributes & TypeAttributes.VisibilityMask;
                        if (attr != TypeAttributes.Public && attr != TypeAttributes.NestedPublic)
                        {
                            continue;
                        }
                        var valid = true;
                        if (!string.IsNullOrEmpty(type.Namespace))
                        {
                            foreach (var name in IgnoreNamespaceSet)
                            {
                                if (type.Namespace.Contains(name))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }
                        if (valid)
                        {
                            ret.Add(type);
                        }
                    }
                }
            }
            return ret;
        }


        private static Dictionary<Type, HashSet<Type>> CollectProperties(List<Type> pComponentTypeList)
        {
            var ret = new Dictionary<Type, HashSet<Type>>();
            foreach (var type in pComponentTypeList)
            {
                foreach (var item in type.GetProperties())
                {
                    if (!item.IsIndexer() && !item.IsObsoleted())
                    {
                        var decType = item.DeclaringType;
                        if (!ret.TryGetValue(decType, out var set))
                        {
                            set = new HashSet<Type>();
                            ret.Add(decType, set);
                        }
                        set.Add(item.PropertyType);
                    }
                }
            }
            return ret;
        }


        private static void GenPropertyBindWrapCodes(TypeBuilder pClassBuilder, Dictionary<Type, HashSet<Type>> pPropertyMap)
        {
            var idx = 0;
            var baseGSType = typeof(GetterSetter<,>);
            foreach (var pair in pPropertyMap)
            {
                var componentType = pair.Key;
                foreach (var propertyType in pair.Value)
                {
                    if (!propertyType.IsEvent())
                    {
                        var fieldType = baseGSType.MakeGenericType(componentType, propertyType);
                        pClassBuilder.DefineField($"__GS_{idx++}__", fieldType, FieldAttributes.Private);
                    }
                }
            }
            pClassBuilder.CreateType();
        }


        private static void GenEventBindWrapCodes(TypeBuilder pClassBuilder, Dictionary<Type, HashSet<Type>> pPropertyMap)
        {
            var idx = 0;
            var baseARType = typeof(AddRemove<>);
            var baseAROneParamType = typeof(AddRemove<,>);
            var voidType = typeof(void);
            foreach (var pair in pPropertyMap)
            {
                var componentType = pair.Key;
                foreach (var propertyType in pair.Value)
                {
                    Type eventParamType = null;
                    if (propertyType.IsEvent() && ((eventParamType = propertyType.GetEventParamType()) != null))
                    {
                        var fieldType = eventParamType == voidType
                               ? baseARType.MakeGenericType(propertyType)
                               : baseAROneParamType.MakeGenericType(propertyType, eventParamType);
                        pClassBuilder.DefineField($"__AR_{idx++}__", fieldType, FieldAttributes.Private);
                    }
                }
            }
            pClassBuilder.CreateType();
        }


        private static void WriteLinkXML()
        {
            var xmlPath = DLLDir + "link.xml";
            if (!File.Exists(xmlPath))
            {
                var sb = new StringBuilder();
                sb.AppendLine("<linker>");
                sb.AppendLine($"    <assembly fullname=\"{ASMName}\" preserve=\"all\"/>");
                sb.Append("</linker>");
                File.WriteAllText(xmlPath, sb.ToString());
            }
        }
    }
}