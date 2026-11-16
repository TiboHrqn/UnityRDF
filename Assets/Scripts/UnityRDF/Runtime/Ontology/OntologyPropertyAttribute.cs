using System;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
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