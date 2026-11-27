using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class OntologyTypeScanner
{
    public void Scan(IReadOnlyList<Assembly> assemblies, List<OntologyScanner.ClassInfo> classes, List<OntologyScanner.CapabilityInfo> capabilities, List<string> errors)
    {
        foreach (Assembly assembly in assemblies)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types
                    .Where(t => t != null)
                    .ToArray();

                foreach (Exception loaderException in exception.LoaderExceptions)
                {
                    if (loaderException != null)
                        errors.Add(loaderException.Message);
                }
            }
            catch (Exception exception)
            {
                errors.Add($"Impossible de scanner l'assembly {assembly.FullName}: " + exception.Message);
                continue;
            }

            foreach (Type type in types)
            {
                if (type == null)
                    continue;

                ScanClass(type, classes);
                ScanCapability(type, capabilities);
            }
        }
    }

    private void ScanClass(Type type, List<OntologyScanner.ClassInfo> classes)
    {
        if (!type.IsClass)
            return;

        var attribute = type.GetCustomAttribute<OntologyClassAttribute>(false);

        if (attribute == null)
            return;

        Type parent = type.BaseType;

        while (parent != null && parent.GetCustomAttribute<OntologyClassAttribute>(false) == null)
        {
            parent = parent.BaseType;
        }

        classes.Add(new OntologyScanner.ClassInfo
        {
            Type = type,
            Attribute = attribute,
            ParentType = parent
        });
    }

    private void ScanCapability(Type type, List<OntologyScanner.CapabilityInfo> capabilities)
    {
        if (!type.IsInterface)
            return;

        var attribute = type.GetCustomAttribute<OntologyCapabilityAttribute>(false);

        if (attribute == null)
            return;

        capabilities.Add(new OntologyScanner.CapabilityInfo
        {
            Type = type,
            Attribute = attribute
        });
    }

    public void ScanCapabilityInheritance(List<OntologyScanner.CapabilityInfo> capabilities)
    {
        foreach (var capability in capabilities)
        {
            capability.ParentCapabilities.Clear();

            foreach (Type parent in capability.Type.GetInterfaces())
            {
                var attribute = parent.GetCustomAttribute<OntologyCapabilityAttribute>(false);

                if (attribute != null)
                    capability.ParentCapabilities.Add(parent);
            }
        }
    }
}