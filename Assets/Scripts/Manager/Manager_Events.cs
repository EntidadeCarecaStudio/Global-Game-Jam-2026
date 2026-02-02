using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static Observer;
using Event = Observer.Event;

public static class Manager_Events
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Setup()
    {
        foreach (var item in Extensions.Fields)
        {
            var obj = Activator.CreateInstance(item.Value.FieldType);

            item.Value.SetValue(null, obj);

            ((IEvent)obj).SetEventName(item.Key);
        }
    }

    public static class GameManager
    {
        public static Event OnPause;
    }

    public static class Input
    {
        public static Event<Vector2> OnMove;
        public static Event OnAttack;
        public static Event OnDodge;
        public static Event OnInteract;
        public static Event OnSubmit;
        public static Event OnHit;
        public static Event OnRotateClockwise;
        public static Event OnRotateCounterclockwise;
    }

    public static class Settings
    {
        public static Event<float> OnSoundMaster;
        public static Event<float> OnSoundSfx;
    }

    public static bool TryGetEvent(InspectorEvent inspectorEvent, out IEvent ev)
    {
        ev = null;

        if (!Extensions.TryGetField(inspectorEvent, out FieldInfo field))
            return false;

        ev = (IEvent)field.GetValue(null);

        return ev != null;
    }

    public static void Add(InspectorEvent inspectorEvent, Action callback)
    {
        IEvent ev;

        if (!Extensions.TryGetField(inspectorEvent, out FieldInfo field))
            return;

        ev = (IEvent)field.GetValue(inspectorEvent);

        ev.Add(callback, inspectorEvent.Order);
    }

    public static void Remove(InspectorEvent inspectorEvent, Action callback)
    {
        IEvent ev;

        if (!Extensions.TryGetField(inspectorEvent, out FieldInfo field))
            return;

        ev = (IEvent)field.GetValue(inspectorEvent);

        ev.Remove(callback, inspectorEvent.Order);
    }

    public static class Extensions
    {
        private static readonly Type MainType = typeof(Manager_Events);

        private static readonly Dictionary<string, FieldInfo> m_fields = new();

        public static Dictionary<string, FieldInfo> Fields
        {
            get
            {
                if (m_fields == null || m_fields.Count <= 0)
                    Setup();

                return m_fields;
            }
        }

        private static void Setup()
        {
            GetFieldsRecursively(MainType);
        }

        private static void GetFieldsRecursively(Type type)
        {
            foreach (var item in type.GetFields())
                m_fields.Add(GetBasePath(type, item), item);

            foreach (var item in type.GetNestedTypes())
                if (IsValidEventType(item))
                    GetFieldsRecursively(item);
        }

        private static bool IsValidEventType(Type type)
        {
            if (typeof(IEvent).IsAssignableFrom(type)) return false;

            if (type == typeof(Extensions)) return false;

            return true;
        }

        private static string GetBasePath(Type type, FieldInfo field)
        {
            string result = $"{type}/{field.Name}";

            result = result.Remove(0, typeof(Manager_Events).ToString().Length + 1);

            result = result.Replace("+", "/");

            return result;
        }

        public static bool TryGetField(InspectorEvent eventInspector, out FieldInfo field)
        {
            field = null;

            if (!m_fields.ContainsKey(eventInspector.Event))
                return false;

            field = m_fields[eventInspector.Event];

            return true;
        }

    }

}
