using System.IO;
using UnityEditor;
using UnityEngine;

public static class OntologyGenerator
{
    [MenuItem("UnityRDF/Generate Ontology (Turtle)")]
    public static void Generate()
    {
        var scanner = new OntologyScanner();

        scanner.Scan();

        if (scanner.Classes.Count == 0)
        {
            Debug.LogWarning("OntologyGenerator: aucune classe avec [OntologyClass] n'a été trouvée.");
            return;
        }

        string turtle = TurtleSerializer.Serialize(scanner);

        string path = EditorUtility.SaveFilePanel("Generate UnityRDF Ontology", Application.dataPath, "ontology", "ttl");

        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllText(path, turtle);

        if (path.StartsWith(Application.dataPath))
            AssetDatabase.Refresh();

        Debug.Log(
            $"OntologyGenerator: ontologie générée avec succès. " +
            $"{scanner.Classes.Count} classes, " +
            $"{scanner.Capabilities.Count} capacités, " +
            $"{scanner.Properties.Count} propriétés, " +
            $"{scanner.Relations.Count} relations, " + 
            $"{scanner.Actions.Count} actions.\n{path}"
        );
    }
}