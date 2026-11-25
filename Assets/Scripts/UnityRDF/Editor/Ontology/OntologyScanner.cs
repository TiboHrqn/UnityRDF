using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Assemblies;

public class OntologyScanner
{
    public class ClassInfo
    {
        public Type Type;
        public OntologyClassAttribute Attribute;
        public Type ParentType;
    }

    public class PropertyInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type RangeType;
    }

    public class RelationInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type TargetType;
    }

    // Un paramètre de l'action : actor, item, door, key...
    public class ParameterInfo
    {
        public string Role;
        public Type ParameterType;
    }

    // Une précondition : predicate(arguments), positive ou négative.
    public class ConditionInfo
    {
        public string Predicate;
        public string[] Arguments;
        public bool IsPositive;
    }

    // Un effet ajouté ou supprimé par l'action.
    public class EffectInfo
    {
        public string Predicate;
        public string[] Arguments;
    }

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

    public List<ClassInfo> Classes { get; } = new();
    public List<PropertyInfo> Properties { get; private set; } = new();
    public List<RelationInfo> Relations { get; private set; } = new();
    public List<ActionInfo> Actions { get; private set; } = new();

    public void Scan()
    {
        Classes.Clear();
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
                types = exception.Types.Where(t => t != null).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (Type type in types)
            {
                var attribute =
                    type.GetCustomAttribute<OntologyClassAttribute>(false);

                if (attribute == null)
                    continue;

                Type parent = type.BaseType;

                while (parent != null &&
                       parent.GetCustomAttribute<OntologyClassAttribute>(false) == null)
                {
                    parent = parent.BaseType;
                }

                Classes.Add(new ClassInfo
                {
                    Type = type,
                    Attribute = attribute,
                    ParentType = parent
                });
            }
        }

        ScanMembers();
        ScanActions();
    }

    private void ScanMembers()
    {
        foreach (ClassInfo classInfo in Classes)
        {
            foreach (var property in classInfo.Type.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                var ontologyProperty =
                    property.GetCustomAttribute<OntologyPropertyAttribute>(true);

                var ontologyRelation =
                    property.GetCustomAttribute<OntologyRelationAttribute>(true);

                Type domain = property.DeclaringType ?? classInfo.Type;

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

        Properties = Properties
            .GroupBy(p => $"{p.Name}|{p.DomainType}|{p.RangeType}")
            .Select(g => g.First())
            .ToList();

        Relations = Relations
            .GroupBy(r => $"{r.Name}|{r.DomainType}|{r.TargetType}")
            .Select(g => g.First())
            .ToList();
    }

    private void ScanActions()
    {
        // Une même interface peut être implémentée par plusieurs classes.
        // La définition de l'action ne doit être enregistrée qu'une fois.
        var scannedMethods = new HashSet<string>();

        foreach (ClassInfo classInfo in Classes)
        {
            Type[] interfaces = classInfo.Type.GetInterfaces();

            foreach (Type interfaceType in interfaces)
            {
                foreach (MethodInfo method in interfaceType.GetMethods())
                {
                    var actionAttribute =
                        method.GetCustomAttribute<OntologyActionAttribute>(false);

                    if (actionAttribute == null)
                        continue;

                    string methodKey =
                        $"{interfaceType.AssemblyQualifiedName}|{method.Name}";

                    if (!scannedMethods.Add(methodKey))
                        continue;

                    var action = new ActionInfo
                    {
                        Name = actionAttribute.Name,
                        DeclaringInterface = interfaceType,
                        Method = method
                    };

                    // Paramètres de l'action
                    foreach (var parameterAttribute in
                             method.GetCustomAttributes<OntologyParameterAttribute>(false))
                    {
                        action.Parameters.Add(new ParameterInfo
                        {
                            Role = parameterAttribute.Role,
                            ParameterType = parameterAttribute.ParameterType
                        });
                    }

                    // Préconditions
                    foreach (var conditionAttribute in
                             method.GetCustomAttributes<OntologyPreConditionAttribute>(false))
                    {
                        action.Preconditions.Add(new ConditionInfo
                        {
                            Predicate = conditionAttribute.Predicate,
                            Arguments = conditionAttribute.Arguments ?? Array.Empty<string>(),
                            IsPositive = conditionAttribute.ConditionType
                        });
                    }

                    // Effets ajoutés
                    foreach (var effectAttribute in
                             method.GetCustomAttributes<OntologyAddEffectAttribute>(false))
                    {
                        action.AddEffects.Add(new EffectInfo
                        {
                            Predicate = effectAttribute.Predicate,
                            Arguments = effectAttribute.Arguments ?? Array.Empty<string>()
                        });
                    }

                    // Effets supprimés
                    foreach (var effectAttribute in
                             method.GetCustomAttributes<OntologyDeleteEffectAttribute>(false))
                    {
                        action.DeleteEffects.Add(new EffectInfo
                        {
                            Predicate = effectAttribute.Predicate,
                            Arguments = effectAttribute.Arguments ?? Array.Empty<string>()
                        });
                    }

                    Actions.Add(action);
                }
            }
        }
    }
}