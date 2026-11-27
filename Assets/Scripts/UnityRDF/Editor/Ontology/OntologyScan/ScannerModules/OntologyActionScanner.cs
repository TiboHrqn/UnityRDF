using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class OntologyActionScanner
{
    private readonly OntologyActionValidator validator;

    public OntologyActionScanner(OntologyActionValidator validator)
    {
        this.validator = validator;
    }

    public void Scan(List<OntologyScanner.CapabilityInfo> capabilities, List<OntologyScanner.ActionInfo> actions, List<string> errors)
    {
        var scannedMethods = new HashSet<string>();

        foreach (var capability in capabilities)
        {
            foreach (Type interfaceType in GetCapabilityHierarchy(capability.Type))
            {
                MethodInfo[] methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (MethodInfo method in methods)
                {
                    ScanMethod(method, interfaceType, scannedMethods, actions, errors);
                }
            }
        }
    }

    private IEnumerable<Type> GetCapabilityHierarchy(Type root)
    {
        var visited = new HashSet<Type>();
        var pending = new Queue<Type>();

        pending.Enqueue(root);

        while (pending.Count > 0)
        {
            Type current = pending.Dequeue();

            if (!visited.Add(current))
                continue;

            var attribute = current.GetCustomAttribute<OntologyCapabilityAttribute>(false);

            if (attribute == null)
                continue;

            yield return current;

            foreach (Type parent in current.GetInterfaces())
            {
                if (parent.GetCustomAttribute<OntologyCapabilityAttribute>(false) != null)
                    pending.Enqueue(parent);
            }
        }
    }

    private void ScanMethod(MethodInfo method, Type interfaceType, HashSet<string> scannedMethods, List<OntologyScanner.ActionInfo> actions, List<string> errors)
    {
        var actionAttribute = method.GetCustomAttribute<OntologyActionAttribute>(false);

        if (actionAttribute == null)
            return;

        string methodKey = $"{method.Module.ModuleVersionId}|{method.MetadataToken}";

        if (!scannedMethods.Add(methodKey))
            return;

        var action = new OntologyScanner.ActionInfo
        {
            Name = actionAttribute.Name,
            DeclaringInterface = interfaceType,
            Method = method
        };

        ScanTarget(interfaceType, action);
        ScanParameters(method, action);
        ScanPreconditions(method, action);
        ScanEffects(method, action);

        if (validator.ValidateAction(action, errors))
            actions.Add(action);
    }

    private void ScanTarget(Type interfaceType, OntologyScanner.ActionInfo action)
    {
        var target = new OntologyScanner.ParameterInfo
        {
            Role = "target",
            ParameterType = interfaceType,
            Position = -1,
            IsTarget = true
        };

        action.Target = target;
        action.Parameters.Add(target);
    }

    private void ScanParameters( MethodInfo method, OntologyScanner.ActionInfo action)
    {
        var attributes = method.GetCustomAttributes<OntologyActionParameterAttribute>(false);

        foreach (var attribute in attributes)
        {
            action.Parameters.Add(new OntologyScanner.ParameterInfo
            {
                Role = attribute.Role,
                ParameterType = attribute.ParameterType,
                Position = attribute.Position,
                IsTarget = false
            });
        }

        action.Parameters = action.Parameters.OrderBy(p => p.IsTarget ? -1 : p.Position).ToList();
    }

    private void ScanPreconditions(MethodInfo method, OntologyScanner.ActionInfo action)
    {
        foreach (var attribute in method.GetCustomAttributes<OntologyPreConditionAttribute>(false))
        {
            action.Preconditions.Add(new OntologyScanner.ConditionInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>(),
                IsPositive = attribute.IsPositive
            });
        }
    }

    private void ScanEffects(MethodInfo method, OntologyScanner.ActionInfo action)
    {
        foreach (var attribute in method.GetCustomAttributes<OntologyAddEffectAttribute>(false))
        {
            action.AddEffects.Add(new OntologyScanner.EffectInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>()
            });
        }

        foreach (var attribute in method.GetCustomAttributes<OntologyDeleteEffectAttribute>(false))
        {
            action.DeleteEffects.Add(new OntologyScanner.EffectInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>()
            });
        }
    }
}