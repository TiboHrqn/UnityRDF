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
public class OntologyActionParameterAttribute : Attribute
{
    public string Role { get; }
    public Type ParameterType { get; }
    public int Position { get; }

    public OntologyActionParameterAttribute(string role, Type parameterType, int position)
    {
        Role = role; ParameterType = parameterType; Position = position;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyPreConditionAttribute : Attribute
{
    public string Predicate { get; }
    public string[] Arguments { get; }
    public bool IsPositive { get; }

    public OntologyPreConditionAttribute(bool isPositive, string predicate, params string[] arguments)
    {
        Predicate = predicate;
        IsPositive = isPositive;
        Arguments = arguments;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OntologyAddEffectAttribute : Attribute
{
    public string Predicate { get; }
    public string[] Arguments { get; }

    public OntologyAddEffectAttribute(string predicate, params string[] arguments)
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

    public OntologyDeleteEffectAttribute(string predicate, params string[] arguments)
    {
        Predicate = predicate;
        Arguments = arguments;
    }
}
