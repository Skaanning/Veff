using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Veff.Snapshot;

namespace Veff;

public interface IFeatureFlagContainer
{
}

public static class FeatureFlagContainerExtensions
{
    public static T ToSnapshot<T>(this IFeatureFlagContainer container) where T : IFeatureFlagSnapshotContainer
    {
        var containerType = container.GetType();
        var snapshotType = typeof(T);
        
        var containerProperties = containerType.GetProperties();
        var snapshotProperties = snapshotType.GetProperties();
        
        RefreshValues(container, containerProperties);

        var snapshotInstance = Activator.CreateInstance<T>();
        
        foreach (var containerProp in containerProperties)
        {
            var matchingSnapshotProp = snapshotProperties.FirstOrDefault(sp => sp.Name == containerProp.Name);
            if (matchingSnapshotProp == null)
                continue;
            
            var containerValue = containerProp.GetValue(container);
            if (containerValue == null)
                continue;
            
            var snapshotValue = MapToSnapshot(containerValue, matchingSnapshotProp.PropertyType);
            if (snapshotValue != null)
            {
                matchingSnapshotProp.SetValue(snapshotInstance, snapshotValue);
            }
        }
        
        return snapshotInstance;
    }

    private static void RefreshValues(IFeatureFlagContainer container, PropertyInfo[] containerProperties)
    {
        // Refresh cached values by calling IsEnabled/IsEnabledNow/EnabledFor methods on each flag
        foreach (var prop in containerProperties)
        {
            var flagValue = prop.GetValue(container);
            if (flagValue == null)
                continue;
            
            var flagType = flagValue.GetType();
            
            // Try to call various IsEnabled methods to refresh cache
            var isEnabledMethod = flagType.GetMethod("IsEnabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, [], null);
            if (isEnabledMethod != null)
            {
                _ = isEnabledMethod.Invoke(flagValue, null);
                continue;
            }
            
            var isEnabledNowMethod = flagType.GetMethod("IsEnabledNow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, [], null);
            if (isEnabledNowMethod != null)
            {
                _ = isEnabledNowMethod.Invoke(flagValue, null);
                continue;
            }
            
            var enabledForMethod = flagType.GetMethod("EnabledFor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (enabledForMethod != null)
            {
                // For string flags, call with a sample string; for percentage, call with a sample int
                if (enabledForMethod.GetParameters().FirstOrDefault()?.ParameterType == typeof(string))
                {
                    _ = enabledForMethod.Invoke(flagValue, ["sample"]);
                }
                else if (enabledForMethod.GetParameters().FirstOrDefault()?.ParameterType == typeof(int))
                {
                    _ = enabledForMethod.Invoke(flagValue, [0]);
                }
                else if (enabledForMethod.GetParameters().FirstOrDefault()?.ParameterType == typeof(Guid))
                {
                    _ = enabledForMethod.Invoke(flagValue, [Guid.Empty]);
                }
            }
        }
    }

    private static object? MapToSnapshot(object value, Type targetType)
    {
        var valueType = value.GetType();
        
        var idProp = valueType.GetProperty("Id");
        var nameProp = valueType.GetProperty("Name");
        var descriptionProp = valueType.GetProperty("Description");
        
        var id = (int)idProp!.GetValue(value)!;
        var name = (string)nameProp!.GetValue(value)!;
        var description = (string)descriptionProp!.GetValue(value)!;
        
        if (valueType.Name == "BooleanFlag" && targetType.Name == "BooleanFlagSnapshot")
        {
            var isEnabledProp = valueType.GetProperty("IsEnabled");
            var isEnabled = (bool)isEnabledProp!.GetValue(value)!;
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(bool)],
                null);
            
            return ctor?.Invoke([id, name, description, isEnabled]);
        }
        
        if (valueType.Name == "DateFlag" && targetType.Name == "DateFlagSnapshot")
        {
            // Extract cached dates from private fields via reflection
            var cachedFromField = GetFieldFromTypeOrBase(valueType, "_cachedFromValue");
            var cachedToField = GetFieldFromTypeOrBase(valueType, "_cachedToValue");
            
            var fromDate = (DateTime?)cachedFromField?.GetValue(value);
            var toDate = (DateTime?)cachedToField?.GetValue(value);
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(DateTime?), typeof(DateTime?)],
                null);
            
            return ctor?.Invoke([id, name, description, fromDate, toDate]);
        }
        
        if (valueType.Name == "PercentageFlag" && targetType.Name == "PercentageFlagSnapshot")
        {
            var percentageEnabledProp = valueType.GetProperty("PercentageEnabled");
            var randomSeedProp = valueType.GetProperty("RandomSeed");
            
            var percentageEnabled = (int)percentageEnabledProp!.GetValue(value)!;
            var randomSeed = (string)randomSeedProp!.GetValue(value)!;
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(int), typeof(string)],
                null);
            
            return ctor?.Invoke([id, name, description, percentageEnabled, randomSeed]);
        }
        
        if (valueType.Name.StartsWith("StringEqualsFlag") && targetType.Name == "StringEqualsFlagSnapshot")
        {
            var valuesField = GetFieldFromTypeOrBase(valueType, "_cachedValue");
            
            var cachedHashSet = (HashSet<string>?)valuesField?.GetValue(value);
            var values = cachedHashSet?.ToArray() ?? [];
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(string[])],
                null);
            
            return ctor?.Invoke([id, name, description, values]);
        }
        
        if (valueType.Name == "StringContainsFlag" && targetType.Name == "StringContainsFlagSnapshot")
        {
            var valuesField = GetFieldFromTypeOrBase(valueType, "_cachedValue");
            
            var cachedHashSet = (HashSet<string>?)valuesField?.GetValue(value);
            var values = cachedHashSet?.ToArray() ?? [];
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(string[])],
                null);
            
            return ctor?.Invoke([id, name, description, values]);
        }
        
        if (valueType.Name == "StringEndsWithFlag" && targetType.Name == "StringEndsWithFlagSnapshot")
        {
            var valuesField = GetFieldFromTypeOrBase(valueType, "_cachedValue");
            
            var cachedHashSet = (HashSet<string>?)valuesField?.GetValue(value);
            var values = cachedHashSet?.ToArray() ?? [];
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(string[])],
                null);
            
            return ctor?.Invoke([id, name, description, values]);
        }
        
        if (valueType.Name == "StringStartsWithFlag" && targetType.Name == "StringStartsWithFlagSnapshot")
        {
            var valuesField = GetFieldFromTypeOrBase(valueType, "_cachedValue");
            
            var cachedHashSet = (HashSet<string>?)valuesField?.GetValue(value);
            var values = cachedHashSet?.ToArray() ?? [];
            
            var ctor = targetType.GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                [typeof(int), typeof(string), typeof(string), typeof(string[])],
                null);
            
            return ctor?.Invoke([id, name, description, values]);
        }
        
        return null;
    }
    
    private static System.Reflection.FieldInfo? GetFieldFromTypeOrBase(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return field;
        
        var baseType = type.BaseType;
        while (baseType != null)
        {
            field = baseType.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                return field;
            
            baseType = baseType.BaseType;
        }
        
        return null;
    }
}