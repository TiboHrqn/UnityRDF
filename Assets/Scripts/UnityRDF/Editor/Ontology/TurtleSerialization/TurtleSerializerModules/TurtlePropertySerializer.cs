using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TurtlePropertySerializer
{
    public static void Serialize(TurtleSerializationContext context, OntologyScanner scanner)
    {
        SerializeProperties(context, scanner.Properties, scanner);
        SerializeRelations(context, scanner.Relations, scanner);
    }

    private static void SerializeProperties(TurtleSerializationContext context, List<OntologyScanner.PropertyInfo> properties, OntologyScanner scanner)
    {
        var sb = context.Builder;

        var groups = properties
            .GroupBy(p => TurtleSerializationContext.ToName(p.Name))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var first = group.First();

            var ranges = group
                .Select(p => TurtleSerializationContext.GetXsdType(p.RangeType))
                .Distinct()
                .ToList();

            if (ranges.Count != 1 || ranges[0] == null)
            {
                Debug.LogWarning($"TurtleSerializer : range incohérent pour la propriété {first.Name}.");
                continue;
            }

            string range = ranges[0];

            var domains = GetNormalizedDomains(group.Select(p => p.DomainType), scanner);

            if (domains.Count == 0)
                continue;

            sb.AppendLine($"ww:{TurtleSerializationContext.ToName(first.Name)} a owl:DatatypeProperty ;");

            SerializeDomain(sb, domains);

            sb.AppendLine($"    rdfs:range xsd:{range} ;");

            if (!string.IsNullOrWhiteSpace(first.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{TurtleSerializationContext.Escape(first.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{TurtleSerializationContext.Escape(first.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static void SerializeRelations(TurtleSerializationContext context, List<OntologyScanner.RelationInfo> relations, OntologyScanner scanner)
    {
        var sb = context.Builder;

        var groups = relations
            .GroupBy(r => TurtleSerializationContext.ToName(r.Name))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var first = group.First();

            var ranges = group
                .Select(r => TurtleSerializationContext.GetOntologyTypeName(r.TargetType))
                .Distinct()
                .ToList();

            if (ranges.Count != 1 ||
                string.IsNullOrEmpty(ranges[0]))
            {
                Debug.LogWarning($"TurtleSerializer : range incohérent pour la relation {first.Name}.");
                continue;
            }

            var domains = GetNormalizedDomains(
                group.Select(r => r.DomainType),
                scanner);

            if (domains.Count == 0)
                continue;

            sb.AppendLine($"ww:{TurtleSerializationContext.ToName(first.Name)} a owl:ObjectProperty ;");

            SerializeDomain(sb, domains);

            sb.AppendLine($"    rdfs:range ww:{ranges[0]} ;");

            if (!string.IsNullOrWhiteSpace(first.Description))
            {
                sb.AppendLine($"    rdfs:comment \"{TurtleSerializationContext.Escape(first.Description)}\" ;");
            }

            sb.AppendLine($"    rdfs:label \"{TurtleSerializationContext.Escape(first.Name)}\" .");
            sb.AppendLine();
        }
    }

    private static List<string> GetNormalizedDomains(IEnumerable<Type> domainTypes, OntologyScanner scanner)
    {
        var types = domainTypes
            .Where(t => t != null)
            .Distinct()
            .ToList();

        var capabilityTypes = new HashSet<Type>(
            scanner.Capabilities.Select(c => c.Type));

        var domainsToRemove = new HashSet<Type>();

        foreach (Type domain in types)
        {
            if (!domain.IsClass)
                continue;

            foreach (Type implementedInterface in domain.GetInterfaces())
            {
                if (capabilityTypes.Contains(implementedInterface)
                    && types.Contains(implementedInterface))
                {
                    domainsToRemove.Add(domain);
                    break;
                }
            }
        }

        return types
            .Where(t => !domainsToRemove.Contains(t))
            .Select(TurtleSerializationContext.GetOntologyTypeName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    private static void SerializeDomain(StringBuilder sb, List<string> domains)
    {
        if (domains.Count == 1)
        {
            sb.AppendLine($"    rdfs:domain ww:{domains[0]} ;");
            return;
        }

        string unionMembers = string.Join(" ", domains.Select(d => $"ww:{d}"));
        sb.AppendLine($"    rdfs:domain [ a owl:Class ; owl:unionOf ( {unionMembers} ) ] ;");
    }
}