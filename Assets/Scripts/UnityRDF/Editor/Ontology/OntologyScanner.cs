
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Assemblies;

public class OntologyScanner
{
    // --------------------------------------------------
    // INFORMATIONS SUR LES CLASSES
    // --------------------------------------------------

    public class ClassInfo
    {
        public Type Type;
        public OntologyClassAttribute Attribute;
        public Type ParentType;
    }

    // --------------------------------------------------
    // INFORMATIONS SUR LES CAPACITÉS
    // --------------------------------------------------

    public class CapabilityInfo
    {
        public Type Type;
        public OntologyCapabilityAttribute Attribute;
        public List<Type> ParentCapabilities = new();
    }

    // --------------------------------------------------
    // PROPRIÉTÉS
    // --------------------------------------------------

    public class PropertyInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type RangeType;
    }

    // --------------------------------------------------
    // RELATIONS
    // --------------------------------------------------

    public class RelationInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type TargetType;
    }

    // --------------------------------------------------
    // PARAMÈTRES DES ACTIONS
    // --------------------------------------------------

    public class ParameterInfo
    {
        public string Role;
        public Type ParameterType;
    }

    // --------------------------------------------------
    // PRÉCONDITIONS
    // --------------------------------------------------

    public class ConditionInfo
    {
        public string Predicate;
        public string[] Arguments;
        public bool IsPositive;
    }

    // --------------------------------------------------
    // EFFETS STRIPS
    // --------------------------------------------------

    public class EffectInfo
    {
        public string Predicate;
        public string[] Arguments;
    }

    // --------------------------------------------------
    // ACTIONS
    // --------------------------------------------------

    public class ActionInfo
    {
        public string Name;
        public Type DeclaringInterface;
        public MethodInfo Method;

        public List<ParameterInfo> Parameters = new();
        public List<ConditionInfo> Preconditions = new();
        public List<EffectInfo> AddEffects = new();
        public List<EffectInfo> DeleteEffects = new();
    }

    // --------------------------------------------------
    // RÉSULTATS DU SCAN
    // --------------------------------------------------

    public List<ClassInfo> Classes { get; } = new();

    public List<CapabilityInfo> Capabilities { get; } = new();

    public List<PropertyInfo> Properties { get; private set; } = new();

    public List<RelationInfo> Relations { get; private set; } = new();

    public List<ActionInfo> Actions { get; private set; } = new();

    // --------------------------------------------------
    // SCAN PRINCIPAL
    // --------------------------------------------------

    public void Scan()
    {
        Classes.Clear();
        Capabilities.Clear();
        Properties.Clear();
        Relations.Clear();
        Actions.Clear();

        foreach (Assembly assembly in CurrentAssemblies.GetLoadedAssemblies())
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
            }
            catch
            {
                continue;
            }

            foreach (Type type in types)
            {
                if (type == null)
                    continue;

                // Les classes métier
                var classAttribute =
                    type.GetCustomAttribute<OntologyClassAttribute>(false);

                if (classAttribute != null && type.IsClass)
                {
                    Type parent = type.BaseType;

                    while (parent != null &&
                           parent.GetCustomAttribute<OntologyClassAttribute>(false) == null)
                    {
                        parent = parent.BaseType;
                    }

                    Classes.Add(new ClassInfo
                    {
                        Type = type,
                        Attribute = classAttribute,
                        ParentType = parent
                    });
                }

                // Les interfaces de capacités
                var capabilityAttribute =
                    type.GetCustomAttribute<OntologyCapabilityAttribute>(false);

                if (capabilityAttribute != null && type.IsInterface)
                {
                    Capabilities.Add(new CapabilityInfo
                    {
                        Type = type,
                        Attribute = capabilityAttribute
                    });
                }
            }
        }

        ScanCapabilityInheritance();
        ScanMembers();
        ScanActions();

        SortResults();
    }

    // --------------------------------------------------
    // HÉRITAGE DES CAPACITÉS
    // --------------------------------------------------

    private void ScanCapabilityInheritance()
    {
        foreach (CapabilityInfo capability in Capabilities)
        {
            capability.ParentCapabilities.Clear();

            foreach (Type parentInterface in capability.Type.GetInterfaces())
            {
                var parentAttribute =
                    parentInterface.GetCustomAttribute<OntologyCapabilityAttribute>(false);

                if (parentAttribute != null)
                {
                    capability.ParentCapabilities.Add(parentInterface);
                }
            }
        }
    }

    // --------------------------------------------------
    // SCAN DES PROPRIÉTÉS ET RELATIONS
    // --------------------------------------------------

    private void ScanMembers()
    {
        // Classes métier
        foreach (ClassInfo classInfo in Classes)
        {
            ScanTypeMembers(classInfo.Type);
        }

        // Interfaces de capacités
        foreach (CapabilityInfo capability in Capabilities)
        {
            ScanTypeMembers(capability.Type);
        }

        // Élimination des doublons
        Properties = Properties
            .GroupBy(p =>
                $"{p.Name}|{p.DomainType.AssemblyQualifiedName}|{p.RangeType.AssemblyQualifiedName}")
            .Select(g => g.First())
            .ToList();

        Relations = Relations
            .GroupBy(r =>
                $"{r.Name}|{r.DomainType.AssemblyQualifiedName}|{r.TargetType.AssemblyQualifiedName}")
            .Select(g => g.First())
            .ToList();
    }

    private void ScanTypeMembers(Type type)
    {
        foreach (var property in type.GetProperties(
                     BindingFlags.Public |
                     BindingFlags.Instance |
                     BindingFlags.DeclaredOnly))
        {
            var ontologyProperty =
                property.GetCustomAttribute<OntologyPropertyAttribute>(false);

            var ontologyRelation =
                property.GetCustomAttribute<OntologyRelationAttribute>(false);

            // Le domaine est le type directement déclaré.
            // Pour une interface, il s'agit donc de la capacité.
            Type domain = type;

            if (ontologyProperty != null)
            {
                Properties.Add(new PropertyInfo
                {
                    Name = ontologyProperty.Name,
                    Description = ontologyProperty.Description,
                    DomainType = domain,
                    RangeType = property.PropertyType
                });
            }

            if (ontologyRelation != null)
            {
                Relations.Add(new RelationInfo
                {
                    Name = ontologyRelation.Name,
                    Description = ontologyRelation.Description,
                    DomainType = domain,
                    TargetType = ontologyRelation.TargetType
                });
            }
        }
    }

    // --------------------------------------------------
    // SCAN DES ACTIONS
    // --------------------------------------------------

    private void ScanActions()
    {
        var scannedMethods = new HashSet<string>();

        // Les actions sont découvertes directement sur les capacités.
        foreach (CapabilityInfo capability in Capabilities)
        {
            Type interfaceType = capability.Type;

            foreach (MethodInfo method in interfaceType.GetMethods(
                         BindingFlags.Public |
                         BindingFlags.Instance |
                         BindingFlags.DeclaredOnly))
            {
                var actionAttribute =
                    method.GetCustomAttribute<OntologyActionAttribute>(false);

                if (actionAttribute == null)
                    continue;

                string methodKey =
                    $"{method.DeclaringType?.AssemblyQualifiedName}|{method.Name}";

                if (!scannedMethods.Add(methodKey))
                    continue;

                var action = new ActionInfo
                {
                    Name = actionAttribute.Name,
                    DeclaringInterface = interfaceType,
                    Method = method
                };

                ScanActionParameters(method, action);
                ScanActionPreconditions(method, action);
                ScanActionEffects(method, action);

                Actions.Add(action);
            }
        }
    }

    private void ScanActionParameters(
        MethodInfo method,
        ActionInfo action)
    {
        foreach (var attribute in
                 method.GetCustomAttributes<OntologyParameterAttribute>(false))
        {
            action.Parameters.Add(new ParameterInfo
            {
                Role = attribute.Role,
                ParameterType = attribute.ParameterType
            });
        }
    }

    private void ScanActionPreconditions(
        MethodInfo method,
        ActionInfo action)
    {
        foreach (var attribute in
                 method.GetCustomAttributes<OntologyPreConditionAttribute>(false))
        {
            action.Preconditions.Add(new ConditionInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>(),
                IsPositive = attribute.ConditionType
            });
        }
    }

    private void ScanActionEffects(
        MethodInfo method,
        ActionInfo action)
    {
        foreach (var attribute in
                 method.GetCustomAttributes<OntologyAddEffectAttribute>(false))
        {
            action.AddEffects.Add(new EffectInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>()
            });
        }

        foreach (var attribute in
                 method.GetCustomAttributes<OntologyDeleteEffectAttribute>(false))
        {
            action.DeleteEffects.Add(new EffectInfo
            {
                Predicate = attribute.Predicate,
                Arguments = attribute.Arguments ?? Array.Empty<string>()
            });
        }
    }

    // --------------------------------------------------
    // TRI DES RÉSULTATS
    // --------------------------------------------------

    private void SortResults()
    {
        Classes.Sort((a, b) =>
            string.Compare(a.Attribute.Name, b.Attribute.Name,
                StringComparison.Ordinal));

        Capabilities.Sort((a, b) =>
            string.Compare(a.Attribute.Name, b.Attribute.Name,
                StringComparison.Ordinal));

        Properties.Sort((a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        Relations.Sort((a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        Actions.Sort((a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }
}
