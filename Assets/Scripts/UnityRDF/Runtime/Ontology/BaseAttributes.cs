using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class OntologyClassAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public OntologyClassAttribute(string name, string description = "")
    {
        Name = name;
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class OntologyPropertyAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public OntologyPropertyAttribute(string name, string description = "")
    {
        Name = name;
        Description = description;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class OntologyRelationAttribute : Attribute
{
    public string Name { get; }
    public Type TargetType { get; }
    public string Description { get; }

    public OntologyRelationAttribute(string name, Type targetType, string description = "")
    {
        Name = name;
        TargetType = targetType;
        Description = description;
    }
}