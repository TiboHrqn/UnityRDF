using System;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
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