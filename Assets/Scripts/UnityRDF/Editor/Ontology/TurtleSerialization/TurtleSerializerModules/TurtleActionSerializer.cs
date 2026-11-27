using System;
using System.Linq;

public static class TurtleActionSerializer
{
    public static void Serialize(TurtleSerializationContext context, System.Collections.Generic.List<OntologyScanner.ActionInfo> actions)
    {
        SerializeActions(context, actions);
    }

    private static void SerializeActions(TurtleSerializationContext context, System.Collections.Generic.List<OntologyScanner.ActionInfo> actions)
    {
        var sb = context.Builder;

        foreach (var action in actions.OrderBy(a => a.Name))
        {
            string actionNode = context.NewBlankNode("action");

            sb.AppendLine($"{actionNode} a strips:Action ;");
            sb.AppendLine($"    strips:name \"{TurtleSerializationContext.Escape(action.Name)}\" ;");
            sb.AppendLine($"    strips:declaredOn ww:{TurtleSerializationContext.GetOntologyTypeName(action.DeclaringInterface)} .");

            var parameters = action.Parameters
                .OrderBy(p => p.IsTarget ? 0 : p.Position + 1)
                .ToList();

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                string parameterNode = context.NewBlankNode("parameter");

                sb.AppendLine($"{actionNode} strips:parameter {parameterNode} .");
                sb.AppendLine($"{parameterNode} a strips:Parameter ;");
                sb.AppendLine($"    strips:role \"{TurtleSerializationContext.Escape(parameter.Role)}\" ;");
                sb.AppendLine($"    strips:position \"{i}\"^^xsd:integer ;");
                sb.AppendLine($"    strips:methodPosition \"{parameter.Position}\"^^xsd:integer ;");
                sb.AppendLine($"    strips:isTarget \"{parameter.IsTarget.ToString().ToLowerInvariant()}\"^^xsd:boolean ;");
                sb.AppendLine($"    strips:parameterType ww:{TurtleSerializationContext.GetOntologyTypeName(parameter.ParameterType)} .");
            }

            foreach (var condition in action.Preconditions)
            {
                string factNode = SerializeFact(context, condition.Predicate, condition.Arguments, condition.IsPositive);

                sb.AppendLine($"{actionNode} strips:precondition {factNode} .");
            }

            foreach (var effect in action.AddEffects)
            {
                string factNode = SerializeFact(context, effect.Predicate, effect.Arguments, true);

                sb.AppendLine($"{actionNode} strips:addEffect {factNode} .");
            }

            foreach (var effect in action.DeleteEffects)
            {
                string factNode = SerializeFact(context, effect.Predicate, effect.Arguments, true);

                sb.AppendLine(
                    $"{actionNode} strips:deleteEffect {factNode} .");
            }

            sb.AppendLine();
        }
    }

    private static string SerializeFact(TurtleSerializationContext context, string predicate, string[] arguments, bool positive)
    {
        var sb = context.Builder;

        string factNode = context.NewBlankNode("fact");

        sb.AppendLine($"{factNode} a strips:Fact ;");

        sb.AppendLine($"    strips:predicate ww:{TurtleSerializationContext.ToName(predicate)} ;");

        sb.AppendLine($"    strips:positive \"{positive.ToString().ToLowerInvariant()}\"^^xsd:boolean .");

        if (arguments == null)
            return factNode;

        for (int i = 0; i < arguments.Length; i++)
        {
            string argumentNode = context.NewBlankNode("argument");

            sb.AppendLine($"{factNode} strips:argument {argumentNode} .");

            sb.AppendLine($"{argumentNode} a strips:Argument ;");

            sb.AppendLine($"    strips:position \"{i}\"^^xsd:integer ;");

            sb.AppendLine($"    strips:role \"{TurtleSerializationContext.Escape(arguments[i])}\" .");
        }
        return factNode;
    }
}