
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class TurtleSerializer
{
    private const string Xsd =
        "http://www.w3.org/2001/XMLSchema#";

    private const string OntologyUri =
        "http://unityRDF.org/ontology#";

    private const string StripsUri =
        "http://unityRDF.org/strips#";

    private static int blankNodeCounter;

    // ============================================================
    // SERIALIZE
    // ============================================================

    public static string Serialize(OntologyScanner scanner)
    {
        if (scanner == null)
            throw new ArgumentNullException(nameof(scanner));

        var sb = new StringBuilder();
        blankNodeCounter = 0;

        SerializePrefixes(sb);
        SerializeOntology(sb);
        SerializeStripsVocabulary(sb);

        SerializeClasses(sb, scanner);
        SerializeCapabilities(sb, scanner);

        SerializeProperties(sb, scanner.Properties, scanner);
        SerializeRelations(sb, scanner.Relations, scanner);

        SerializeActions(sb, scanner.Actions);

        return sb.ToString();
    }

    // ============================================================
    // PREFIXES
    // ============================================================

    private static void SerializePrefixes(StringBuilder sb)
    {
        sb.AppendLine("@prefix ww: <" + OntologyUri + "> .");
        sb.AppendLine("@prefix strips: <" + StripsUri + "> .");
        sb.AppendLine("@prefix owl: <http://www.w3.org/2002/07/owl#> .");
        sb.AppendLine("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .");
        sb.AppendLine("@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .");
        sb.AppendLine("@prefix xsd: <" + Xsd + "> .");
        sb.AppendLine();
    }

    private static void SerializeOntology(StringBuilder sb)
    {
        sb.AppendLine("ww:unityRDFOntology a owl:Ontology ;");
        sb.AppendLine("    rdfs:label \"Unity RDF Ontology\" .");
        sb.AppendLine();
    }

    // ============================================================
    // STRIPS VOCABULARY
    // ============================================================

    private static void SerializeStripsVocabulary(StringBuilder sb)
    {
        sb.AppendLine("strips:Action a owl:Class .");
        sb.AppendLine("strips:Parameter a owl:Class .");
        sb.AppendLine("strips:Fact a owl:Class .");
        sb.AppendLine("strips:Argument a owl:Class .");
        sb.AppendLine();

        SerializeStripsProperty(sb, "name", "DatatypeProperty");
        SerializeStripsProperty(sb, "declaredOn", "ObjectProperty");
        SerializeStripsProperty(sb, "parameter", "ObjectProperty");
        SerializeStripsProperty(sb, "role", "DatatypeProperty");
        SerializeStripsProperty(sb, "parameterType", "ObjectProperty");
        SerializeStripsProperty(sb, "precondition", "ObjectProperty");
        SerializeStripsProperty(sb, "addEffect", "ObjectProperty");
        SerializeStripsProperty(sb, "deleteEffect", "ObjectProperty");
        SerializeStripsProperty(sb, "predicate", "ObjectProperty");
        SerializeStripsProperty(sb, "argument", "ObjectProperty");
        SerializeStripsProperty(sb, "position", "DatatypeProperty");
        SerializeStripsProperty(sb, "positive", "DatatypeProperty");

        sb.AppendLine();
    }

    private static void SerializeStripsProperty(
        StringBuilder sb,
        string name,
        string type)
    {
        sb.AppendLine($"strips:{name} a owl:{type} .");
    }

    // ============================================================
    // CLASSES
    // ============================================================

    private static void SerializeClasses(
        StringBuilder sb,
        OntologyScanner scanner)
    {
        foreach (var info in scanner.Classes.OrderBy(c => c.Attribute.Name))
        {
            string className = GetOntologyTypeName(info.Type);

            var superClasses = new List<string>();

            // Héritage entre classes métier
            if (info.ParentType != null)
            {
                string parentName = GetOntologyTypeName(info.ParentType);

                if (!string.IsNullOrEmpty(parentName))
                    superClasses.Add($"ww:{parentName}");
            }

            // Capacités implémentées
            foreach (Type capability in GetOntologyInterfaces(
                         info.Type, scanner))
            {
                string capabilityName =
                    GetOntologyTypeName(capability);

                if (!string.IsNullOrEmpty(capabilityName))
                    superClasses.Add($"ww:{capabilityName}");
            }

            superClasses = superClasses.Distinct().ToList();

            sb.AppendLine($"ww:{className} a owl:Class ;");

            foreach (string superClass in superClasses)
            {
                sb.AppendLine(
                    $"    rdfs:subClassOf {superClass} ;");
            }

            if (!string.IsNullOrWhiteSpace(info.Attribute.Description))
            {
                sb.AppendLine(
                    $"    rdfs:comment \"{Escape(info.Attribute.Description)}\" ;");
            }

            sb.AppendLine(
                $"    rdfs:label \"{Escape(info.Attribute.Name)}\" .");

            sb.AppendLine();
        }
    }

    // ============================================================
    // CAPABILITIES
    // ============================================================

    private static void SerializeCapabilities(
        StringBuilder sb,
        OntologyScanner scanner)
    {
        foreach (var capability in scanner.Capabilities
                     .OrderBy(c => c.Attribute.Name))
        {
            string capabilityName =
                GetOntologyTypeName(capability.Type);

            var parents = capability.ParentCapabilities
                .Select(GetOntologyTypeName)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

            sb.AppendLine(
                $"ww:{capabilityName} a owl:Class ;");

            foreach (string parentName in parents)
            {
                sb.AppendLine(
                    $"    rdfs:subClassOf ww:{parentName} ;");
            }

            if (!string.IsNullOrWhiteSpace(
                    capability.Attribute.Description))
            {
                sb.AppendLine(
                    $"    rdfs:comment \"{Escape(capability.Attribute.Description)}\" ;");
            }

            sb.AppendLine(
                $"    rdfs:label \"{Escape(capability.Attribute.Name)}\" .");

            sb.AppendLine();
        }
    }

    // ============================================================
    // DATATYPE PROPERTIES
    // ============================================================

    private static void SerializeProperties(
        StringBuilder sb,
        List<OntologyScanner.PropertyInfo> properties,
        OntologyScanner scanner)
    {
        var groups = properties
            .GroupBy(p => ToName(p.Name))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var first = group.First();

            var ranges = group
                .Select(p => GetXsdType(p.RangeType))
                .Distinct()
                .ToList();

            // Une propriété OWL ne doit pas recevoir des ranges
            // incompatibles sans décision explicite.
            if (ranges.Count != 1 || ranges[0] == null)
            {
                Debug.LogWarning(
                    $"TurtleSerializer : range incohérent pour la propriété {first.Name}.");

                continue;
            }

            string range = ranges[0];

            var domains = GetNormalizedDomains(
                group.Select(p => p.DomainType),
                scanner);

            if (domains.Count == 0)
                continue;

            sb.AppendLine(
                $"ww:{ToName(first.Name)} a owl:DatatypeProperty ;");

            SerializeDomain(sb, domains);

            sb.AppendLine($"    rdfs:range xsd:{range} ;");

            if (!string.IsNullOrWhiteSpace(first.Description))
            {
                sb.AppendLine(
                    $"    rdfs:comment \"{Escape(first.Description)}\" ;");
            }

            sb.AppendLine(
                $"    rdfs:label \"{Escape(first.Name)}\" .");

            sb.AppendLine();
        }
    }

    // ============================================================
    // OBJECT PROPERTIES / RELATIONS
    // ============================================================

    private static void SerializeRelations(
        StringBuilder sb,
        List<OntologyScanner.RelationInfo> relations,
        OntologyScanner scanner)
    {
        var groups = relations
            .GroupBy(r => ToName(r.Name))
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var first = group.First();

            var ranges = group
                .Select(r => GetOntologyTypeName(r.TargetType))
                .Distinct()
                .ToList();

            if (ranges.Count != 1 || string.IsNullOrEmpty(ranges[0]))
            {
                Debug.LogWarning(
                    $"TurtleSerializer : range incohérent pour la relation {first.Name}.");

                continue;
            }

            var domains = GetNormalizedDomains(
                group.Select(r => r.DomainType),
                scanner);

            if (domains.Count == 0)
                continue;

            sb.AppendLine(
                $"ww:{ToName(first.Name)} a owl:ObjectProperty ;");

            SerializeDomain(sb, domains);

            sb.AppendLine(
                $"    rdfs:range ww:{ranges[0]} ;");

            if (!string.IsNullOrWhiteSpace(first.Description))
            {
                sb.AppendLine(
                    $"    rdfs:comment \"{Escape(first.Description)}\" ;");
            }

            sb.AppendLine(
                $"    rdfs:label \"{Escape(first.Name)}\" .");

            sb.AppendLine();
        }
    }

    // ============================================================
    // DOMAIN NORMALIZATION
    // ============================================================

    private static List<string> GetNormalizedDomains(
        IEnumerable<Type> domainTypes,
        OntologyScanner scanner)
    {
        var types = domainTypes
            .Where(t => t != null)
            .Distinct()
            .ToList();

        var capabilityTypes = new HashSet<Type>(
            scanner.Capabilities.Select(c => c.Type));

        // Si une classe implémente déjà une capacité présente
        // dans les domaines, son domaine est redondant.
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
            .Select(GetOntologyTypeName)
            .Where(n => !string.IsNullOrEmpty(n))
            .Distinct()
            .OrderBy(n => n)
            .ToList();
    }

    // ============================================================
    // DOMAIN OWL
    // ============================================================

    private static void SerializeDomain(
        StringBuilder sb,
        List<string> domains)
    {
        if (domains.Count == 1)
        {
            sb.AppendLine(
                $"    rdfs:domain ww:{domains[0]} ;");

            return;
        }

        string unionMembers = string.Join(
            " ",
            domains.Select(d => $"ww:{d}"));

        sb.AppendLine(
            $"    rdfs:domain [ a owl:Class ; owl:unionOf ( {unionMembers} ) ] ;");
    }

    // ============================================================
    // ACTIONS STRIPS
    // ============================================================

    private static void SerializeActions(
        StringBuilder sb,
        List<OntologyScanner.ActionInfo> actions)
    {
        foreach (var action in actions
                     .OrderBy(a => a.Name))
        {
            string actionNode = NewBlankNode("action");

            sb.AppendLine($"{actionNode} a strips:Action .");

            sb.AppendLine(
                $"{actionNode} strips:name \"{Escape(action.Name)}\" .");

            sb.AppendLine(
                $"{actionNode} strips:declaredOn " +
                $"ww:{GetOntologyTypeName(action.DeclaringInterface)} .");

            // Paramètres
            for (int i = 0; i < action.Parameters.Count; i++)
            {
                var parameter = action.Parameters[i];

                string parameterNode = NewBlankNode("parameter");

                sb.AppendLine(
                    $"{actionNode} strips:parameter {parameterNode} .");

                sb.AppendLine(
                    $"{parameterNode} a strips:Parameter ;");

                sb.AppendLine(
                    $"    strips:role \"{Escape(parameter.Role)}\" ;");

                sb.AppendLine(
                    $"    strips:position \"{i}\"^^xsd:integer ;");

                sb.AppendLine(
                    $"    strips:parameterType " +
                    $"ww:{GetOntologyTypeName(parameter.ParameterType)} .");
            }

            // Préconditions
            foreach (var condition in action.Preconditions)
            {
                string factNode = SerializeFact(
                    sb,
                    condition.Predicate,
                    condition.Arguments,
                    condition.IsPositive);

                sb.AppendLine(
                    $"{actionNode} strips:precondition {factNode} .");
            }

            // Effets ajoutés
            foreach (var effect in action.AddEffects)
            {
                string factNode = SerializeFact(
                    sb,
                    effect.Predicate,
                    effect.Arguments,
                    true);

                sb.AppendLine(
                    $"{actionNode} strips:addEffect {factNode} .");
            }

            // Effets supprimés :
            // STRIPS supprime le fait positif de l'état.
            foreach (var effect in action.DeleteEffects)
            {
                string factNode = SerializeFact(
                    sb,
                    effect.Predicate,
                    effect.Arguments,
                    true);

                sb.AppendLine(
                    $"{actionNode} strips:deleteEffect {factNode} .");
            }

            sb.AppendLine();
        }
    }

    // ============================================================
    // FACTS
    // ============================================================

    private static string SerializeFact(
        StringBuilder sb,
        string predicate,
        string[] arguments,
        bool positive)
    {
        string factNode = NewBlankNode("fact");

        sb.AppendLine($"{factNode} a strips:Fact ;");

        sb.AppendLine(
            $"    strips:predicate ww:{ToName(predicate)} ;");

        sb.AppendLine(
            $"    strips:positive \"{positive.ToString().ToLowerInvariant()}\"^^xsd:boolean .");

        if (arguments == null)
            return factNode;

        for (int i = 0; i < arguments.Length; i++)
        {
            string argumentNode = NewBlankNode("argument");

            sb.AppendLine(
                $"{factNode} strips:argument {argumentNode} .");

            sb.AppendLine(
                $"{argumentNode} a strips:Argument ;");

            sb.AppendLine(
                $"    strips:position \"{i}\"^^xsd:integer ;");

            sb.AppendLine(
                $"    strips:role \"{Escape(arguments[i])}\" .");
        }

        return factNode;
    }

    // ============================================================
    // ONTOLOGY TYPE HELPERS
    // ============================================================

    private static string GetOntologyTypeName(Type type)
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

    private static IEnumerable<Type> GetOntologyInterfaces(
        Type type,
        OntologyScanner scanner)
    {
        if (type == null)
            return Enumerable.Empty<Type>();

        var knownCapabilities = new HashSet<Type>(
            scanner.Capabilities.Select(c => c.Type));

        return type.GetInterfaces()
            .Where(i => knownCapabilities.Contains(i))
            .Distinct()
            .OrderBy(GetOntologyTypeName);
    }

    // ============================================================
    // XSD TYPES
    // ============================================================

    private static string GetXsdType(Type type)
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

    // ============================================================
    // STRING HELPERS
    // ============================================================

    private static string ToName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Unnamed";

        return new string(
            name.Where(c =>
                char.IsLetterOrDigit(c) || c == '_')
            .ToArray());
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static string NewBlankNode(string prefix)
    {
        return $"_:{prefix}{blankNodeCounter++}";
    }
}
