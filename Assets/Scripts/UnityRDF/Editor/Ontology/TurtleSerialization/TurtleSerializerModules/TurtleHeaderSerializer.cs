public static class TurtleHeaderSerializer
{
    public static void Serialize(TurtleSerializationContext context)
    {
        SerializePrefixes(context);
        SerializeOntology(context);
        SerializeStripsVocabulary(context);
    }

    private static void SerializePrefixes(TurtleSerializationContext context)
    {
        var sb = context.Builder;

        sb.AppendLine("@prefix ww: <" + TurtleSerializationContext.OntologyUri + "> .");
        sb.AppendLine("@prefix strips: <" + TurtleSerializationContext.StripsUri + "> .");
        sb.AppendLine("@prefix owl: <http://www.w3.org/2002/07/owl#> .");
        sb.AppendLine("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .");
        sb.AppendLine("@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .");
        sb.AppendLine("@prefix xsd: <" + TurtleSerializationContext.Xsd + "> .");
        sb.AppendLine();
    }

    private static void SerializeOntology(TurtleSerializationContext context)
    {
        var sb = context.Builder;

        sb.AppendLine("ww:unityRDFOntology a owl:Ontology ;");
        sb.AppendLine("    rdfs:label \"Unity RDF Ontology\" .");
        sb.AppendLine();
    }

    private static void SerializeStripsVocabulary(TurtleSerializationContext context)
    {
        var sb = context.Builder;

        sb.AppendLine("strips:Action a owl:Class .");
        sb.AppendLine("strips:Parameter a owl:Class .");
        sb.AppendLine("strips:Fact a owl:Class .");
        sb.AppendLine("strips:Argument a owl:Class .");
        sb.AppendLine();

        // Properties
        SerializeProperty(sb, "name", "DatatypeProperty", range: "xsd:string");
        SerializeProperty(sb, "declaredOn", "ObjectProperty", domain: "strips:Action", range: "owl:Class");
        SerializeProperty(sb, "parameter", "ObjectProperty", domain: "strips:Action", range: "strips:Parameter");
        SerializeProperty(sb, "role", "DatatypeProperty", range: "xsd:string");
        SerializeProperty(sb, "parameterType", "ObjectProperty", domain: "strips:Parameter", range: "owl:Class");
        SerializeProperty(sb, "position", "DatatypeProperty", range: "xsd:integer");
        SerializeProperty(sb, "methodPosition", "DatatypeProperty", range: "xsd:integer");
        SerializeProperty(sb, "isTarget", "DatatypeProperty", range: "xsd:boolean");
        SerializeProperty(sb, "precondition", "ObjectProperty", domain: "strips:Action", range: "strips:Fact");
        SerializeProperty(sb, "addEffect", "ObjectProperty", domain: "strips:Action", range: "strips:Fact");
        SerializeProperty(sb, "deleteEffect", "ObjectProperty", domain: "strips:Action", range: "strips:Fact");
        SerializeProperty(sb, "predicate", "ObjectProperty", domain: "strips:Fact", range: "rdf:Property");
        SerializeProperty(sb, "argument", "ObjectProperty", domain: "strips:Fact", range: "strips:Argument");
        SerializeProperty(sb, "positive", "DatatypeProperty", range: "xsd:boolean");

        sb.AppendLine();
    }

    private static void SerializeProperty(System.Text.StringBuilder sb, string name, string type, string domain = null, string range = null)
    {
        sb.Append($"strips:{name} a owl:{type}");

        if (domain != null || range != null)
        {
            sb.AppendLine(" ;");

            if (domain != null)
                sb.AppendLine($"    rdfs:domain {domain}" + (range != null ? " ;" : " ."));

            if (range != null)
                sb.AppendLine($"    rdfs:range {range} .");
        }
        else
        {
            sb.AppendLine(" .");
        }
    }
}