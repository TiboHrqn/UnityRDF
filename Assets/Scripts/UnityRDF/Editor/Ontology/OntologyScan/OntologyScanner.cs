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

    public class CapabilityInfo
    {
        public Type Type;
        public OntologyCapabilityAttribute Attribute;
        public List<Type> ParentCapabilities = new List<Type>();
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

    public class ParameterInfo
    {
        public string Role;
        public Type ParameterType;
        public int Position;
        public bool IsTarget;
    }

    public class ConditionInfo
    {
        public string Predicate;
        public string[] Arguments;
        public bool IsPositive;
    }

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

        public ParameterInfo Target;

        public List<ParameterInfo> Parameters = new List<ParameterInfo>();

        public List<ConditionInfo> Preconditions = new List<ConditionInfo>();

        public List<EffectInfo> AddEffects = new List<EffectInfo>();

        public List<EffectInfo> DeleteEffects = new List<EffectInfo>();
    }

    public List<ClassInfo> Classes { get; private set; } = new List<ClassInfo>();
    public List<CapabilityInfo> Capabilities { get; private set; } = new List<CapabilityInfo>();
    public List<PropertyInfo> Properties { get; private set; } = new List<PropertyInfo>();
    public List<RelationInfo> Relations { get; private set; } = new List<RelationInfo>();
    public List<ActionInfo> Actions { get; private set; } = new List<ActionInfo>();
    public List<string> Errors { get; private set; } = new List<string>();


    private OntologyTypeScanner typeScanner;
    private OntologyMemberScanner memberScanner;
    private OntologyActionScanner actionScanner;
    private OntologyActionValidator actionValidator;

    public OntologyScanner()
    {
        typeScanner = new OntologyTypeScanner();
        memberScanner = new OntologyMemberScanner();
        actionValidator = new OntologyActionValidator();
        actionScanner = new OntologyActionScanner(actionValidator);
    }

    public void Scan()
    {
        ClearResults();

        IReadOnlyList<Assembly> assemblies = CurrentAssemblies.GetLoadedAssemblies();

        // 1. Classes et capacités
        typeScanner.Scan(assemblies, Classes, Capabilities, Errors);
        // 2. Héritage des capacités
        typeScanner.ScanCapabilityInheritance(Capabilities);
        // 3. Propriétés et relations
        memberScanner.Scan(Classes, Capabilities, Properties, Relations, Errors);
        // 4. Actions définies dans les capacités
        actionScanner.Scan(Capabilities, Actions, Errors);

        SortResults();
    }

    private void ClearResults()
    {
        Classes.Clear();
        Capabilities.Clear();
        Properties.Clear();
        Relations.Clear();
        Actions.Clear();
        Errors.Clear();
    }

    private void SortResults()
    {
        Classes.Sort((a, b) => string.Compare(a.Attribute.Name, b.Attribute.Name, StringComparison.Ordinal));

        Capabilities.Sort((a, b) => string.Compare(a.Attribute.Name, b.Attribute.Name, StringComparison.Ordinal));

        Properties.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        Relations.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        Actions.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }
}