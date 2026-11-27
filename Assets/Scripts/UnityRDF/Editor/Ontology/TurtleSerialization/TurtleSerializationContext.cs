using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TurtleSerializationContext
{
    public const string Xsd =
        "http://www.w3.org/2001/XMLSchema#";

    public const string OntologyUri =
        "http://unityRDF.org/ontology#";

    public const string StripsUri =
        "http://unityRDF.org/strips#";

    public StringBuilder Builder { get; } = new StringBuilder();

    private int blankNodeCounter;

    public string NewBlankNode(string prefix)
    {
        return $"_:{prefix}{blankNodeCounter++}";
    }

    public static string ToName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Unnamed";

        return new string(
            name.Where(c =>
                char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    public static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    public static string GetOntologyTypeName(Type type)
    {
        if (type == null)
            return null;

        var classAttribute = type
            .GetCustomAttributes(
                typeof(OntologyClassAttribute), false)
            .Cast<OntologyClassAttribute>()
            .FirstOrDefault();

        if (classAttribute != null)
            return ToName(classAttribute.Name);

        var capabilityAttribute = type
            .GetCustomAttributes(
                typeof(OntologyCapabilityAttribute), false)
            .Cast<OntologyCapabilityAttribute>()
            .FirstOrDefault();

        if (capabilityAttribute != null)
            return ToName(capabilityAttribute.Name);

        return ToName(type.Name);
    }

    public static string GetXsdType(Type type)
    {
        if (type == null)
            return null;

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsEnum ||
            type == typeof(string) ||
            type == typeof(char) ||
            type == typeof(Guid))
            return "string";

        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int)) return "integer";
        if (type == typeof(long)) return "long";
        if (type == typeof(short)) return "short";
        if (type == typeof(byte)) return "unsignedByte";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(DateTime)) return "dateTime";

        return null;
    }
}