using System;
using System.Linq;
using System.Collections.Generic;

public static class TurtleClassSerializer
{
    public static void Serialize(TurtleSerializationContext context, OntologyScanner scanner)
    {
        SerializeClasses(context, scanner);
        SerializeCapabilities(context, scanner);
    }

    private static void SerializeClasses(TurtleSerializationContext context, OntologyScanner scanner)
    {
        var sb = context.Builder;

        foreach (var info in scanner.Classes.OrderBy(c => c.Attribute.Name))
        {
            string className =
                TurtleSerializationContext.GetOntologyTypeName(info.Type);

            var superClasses = new List<string>();

            if (info.ParentType != null)
            {
                string parentName =TurtleSerializationContext.GetOntologyTypeName(info.ParentType);

                if (!string.IsNullOrEmpty(parentName))
                    superClasses.Add($"ww:{parentName}");
            }

            foreach (Type capability in GetOntologyInterfaces(info.Type, scanner))
            {
                string capabilityName = TurtleSerializationContext.GetOntologyTypeName(capability);

                if (!string.IsNullOrEmpty(capabilityName))
                    superClasses.Add($"ww:{capabilityName}");
            }

            superClasses = superClasses.Distinct().ToList();

            sb.AppendLine($"ww:{className} a owl:Class ;");

            foreach (string superClass in superClasses)
            {
                sb.AppendLine($"    rdfs:subClassOf {superClass} ;");
            }

            if (!string.IsNullOrWhiteSpace(info.Attribute.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{TurtleSerializationContext.Escape(info.Attribute.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{TurtleSerializationContext.Escape(info.Attribute.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static void SerializeCapabilities(TurtleSerializationContext context,OntologyScanner scanner)
    {
        var sb = context.Builder;

        foreach (var capability in scanner.Capabilities.OrderBy(c => c.Attribute.Name))
        {
            string capabilityName = TurtleSerializationContext.GetOntologyTypeName(capability.Type);

            var parents = capability.ParentCapabilities
                .Select(TurtleSerializationContext.GetOntologyTypeName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            sb.AppendLine($"ww:{capabilityName} a owl:Class ;");

            foreach (string parentName in parents)
            {
                sb.AppendLine($"    rdfs:subClassOf ww:{parentName} ;");
            }

            if (!string.IsNullOrWhiteSpace(capability.Attribute.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{TurtleSerializationContext.Escape(capability.Attribute.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{TurtleSerializationContext.Escape(capability.Attribute.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static IEnumerable<Type> GetOntologyInterfaces(Type type, OntologyScanner scanner)
    {
        if (type == null)
            return Enumerable.Empty<Type>();

        var knownCapabilities = new HashSet<Type>(scanner.Capabilities.Select(c => c.Type));

        return type.GetInterfaces()
            .Where(i => knownCapabilities.Contains(i))
            .Distinct()
            .OrderBy(TurtleSerializationContext.GetOntologyTypeName);
    }
}