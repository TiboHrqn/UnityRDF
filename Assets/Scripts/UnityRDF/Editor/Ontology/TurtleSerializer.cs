using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class TurtleSerializer
{
    private const string Xsd = "http://www.w3.org/2001/XMLSchema#";

    private static string OntologyUri = "http://unityRDF.org/ontology#";

    public static string Serialize(OntologyScanner scanner)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@prefix ww: <" + OntologyUri + "> .");
        sb.AppendLine("@prefix owl: <http://www.w3.org/2002/07/owl#> .");
        sb.AppendLine("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .");
        sb.AppendLine("@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .");
        sb.AppendLine("@prefix xsd: <" + Xsd + "> .");
        sb.AppendLine();

        sb.AppendLine("ww:unityRDFOntology a owl:Ontology .");
        sb.AppendLine();

        SerializeClasses(sb, scanner.Classes);
        SerializeProperties(sb, scanner.Properties);
        SerializeRelations(sb, scanner.Relations);

        return sb.ToString();
    }

    private static void SerializeClasses(StringBuilder sb, List<OntologyScanner.ClassInfo> Classes)
    {
        foreach (var info in Classes)
        {
            sb.AppendLine($"ww:{ToName(info.Attribute.Name)} a owl:Class ;");

            if (info.ParentType != null)
            {
                var parentAttribute =
                    info.ParentType.GetCustomAttributes(
                        typeof(OntologyClassAttribute), false)
                        .Cast<OntologyClassAttribute>()
                        .FirstOrDefault();

                if (parentAttribute != null)
                {
                    sb.AppendLine($"    rdfs:subClassOf ww:{ToName(parentAttribute.Name)} ;");
                }
            }

            if (!string.IsNullOrWhiteSpace(info.Attribute.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{Escape(info.Attribute.Description)}\" ;");
            }

            sb.AppendLine("    rdfs:label \"" + Escape(info.Attribute.Name) + "\" .");
            sb.AppendLine();
        }
    }

    private static void SerializeProperties(StringBuilder sb, List<OntologyScanner.PropertyInfo> Properties)
    {
        foreach (var property in Properties)
        {
            string range = GetXsdType(property.RangeType);

            if (range == null)
            {
                UnityEngine.Debug.LogWarning($"TurtleSerializer : Type non pris en charge pour la propriété {property.Name}: {property.RangeType}");
                continue;
            }

            sb.AppendLine($"ww:{ToName(property.Name)} a owl:DatatypeProperty ;");
            sb.AppendLine($"    rdfs:domain ww:{GetClassName(property.DomainType)} ;");
            sb.AppendLine($"    rdfs:range xsd:{range} ;");

            if (!string.IsNullOrWhiteSpace(property.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{Escape(property.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{Escape(property.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static void SerializeRelations(StringBuilder sb, List<OntologyScanner.RelationInfo> Relations)
    {
        foreach (var relation in Relations)
        {
            sb.AppendLine($"ww:{ToName(relation.Name)} a owl:ObjectProperty ;");
            sb.AppendLine($"    rdfs:domain ww:{GetClassName(relation.DomainType)} ;");
            sb.AppendLine($"    rdfs:range ww:{GetClassName(relation.TargetType)} ;");

            if (!string.IsNullOrWhiteSpace(relation.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{Escape(relation.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{Escape(relation.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static string GetClassName(Type type)
    {
        var attribute =
            type.GetCustomAttributes(
                typeof(OntologyClassAttribute), false)
                .Cast<OntologyClassAttribute>()
                .FirstOrDefault();

        return ToName(attribute?.Name ?? type.Name);
    }

    private static string GetXsdType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsEnum || type == typeof(string) || type == typeof(char) || type == typeof(Guid)) return "string";
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

    private static string ToName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}