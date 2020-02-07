using System;
using System.Reflection;

namespace grasmanek94.FindCloserBed
{
    public static class ClassUtility
    {
        public static void SetProperty(object instance, string propertyName, object newValue)
        {
            Type type = instance.GetType();

            PropertyInfo prop = type.BaseType.GetProperty(propertyName);

            prop.SetValue(instance, newValue, null);
        }

        public static void Call(object instance, string function, object[] parameters)
        {
            instance.GetType().GetMethod(function, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, parameters);
        }
    }
}
