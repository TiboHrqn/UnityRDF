using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
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