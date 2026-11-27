using System;
using UnityEngine;

public static class TurtleSerializer
{
    public static string Serialize(OntologyScanner scanner)
    {
        if (scanner == null)
            throw new ArgumentNullException(nameof(scanner));

        if (scanner.Errors.Count > 0)
        {
            Debug.LogWarning("OntologyScanner contient " + scanner.Errors.Count + " erreur(s). Certaines actions peuvent être absentes.");
        }

        var context = new TurtleSerializationContext();

        // 1. En-tête et vocabulaire
        TurtleHeaderSerializer.Serialize(context);

        // 2. Classes et capacités
        TurtleClassSerializer.Serialize(context, scanner);

        // 3. Propriétés et relations
        TurtlePropertySerializer.Serialize(context, scanner);

        // 4. Actions STRIPS
        TurtleActionSerializer.Serialize(context, scanner.Actions);

        return context.Builder.ToString();
    }
}