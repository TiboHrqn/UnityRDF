
using System;

[AttributeUsage(AttributeTargets.Method)]
public class OntologyActionAttribute : Attribute
{
    public string Name { get; }

    public OntologyActionAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyParameterAttribute : Attribute
{
    public string Role { get; }
    public Type ParameterType { get; }

    public OntologyParameterAttribute(string role, Type parameterType)
    {
        Role = role;
        ParameterType = parameterType;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyPreConditionAttribute : Attribute
{
    public string Predicate { get; }

    public string[] Arguments { get; }

    public bool ConditionType { get; }

    public OntologyPreConditionAttribute(bool conditionType , string predicate , params string[] arguments)
    {
        Predicate = predicate;
        ConditionType = conditionType;
        Arguments = arguments;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyAddEffectAttribute : Attribute
{
    public string Predicate { get; }
    public string[] Arguments { get; }

    public OntologyAddEffectAttribute(string predicate , params string[] arguments)
    {
        Predicate = predicate;
        Arguments = arguments;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyDeleteEffectAttribute : Attribute
{
    public string Predicate { get; }
    public string[] Arguments { get; }

    public OntologyDeleteEffectAttribute(string predicate , params string[] arguments)
    {
        Predicate = predicate;
        Arguments = arguments;
    }
}
