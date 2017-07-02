using UnityEngine;
using System;
using System.Reflection;

namespace RSTUtils
{
    public static class GameObjectExtension
    {
        public static T AddComponentWithInit<T>(this GameObject obj, System.Action<T> onInit) where T : Component
        {
            bool oldState = obj.activeSelf;
            obj.SetActive(false);
            T comp = obj.AddComponent<T>();
            if (onInit != null)
                onInit(comp);
            obj.SetActive(oldState);
            return comp;
        }

        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }
    }

    public static class TransformExtension
    {
        public static Transform Clear(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            return transform;
        }

        public static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent.gameObject.name == childName)
            {
                return parent;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform transform = TransformExtension.FindChildRecursive(parent.GetChild(i), childName);
                if (transform != null)
                {
                    return transform;
                }
            }
            return null;
        }

    }

    public static class ComponentExtension
    {
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                 BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch
                    {
                        RSTLogWriter.Log("Internal failure in GetCopyOf ComponentExtension");
                    }
                        // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }
    }
}
