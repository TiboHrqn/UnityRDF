using System;
using System.Collections.Generic;
using System.Linq;

public class OntologyActionValidator
{
    public bool ValidateAction(OntologyScanner.ActionInfo action, List<string> errors)
    {
        bool valid = true;

        var methodParameters = action.Method.GetParameters();

        var usedRoles = new HashSet<string>(StringComparer.Ordinal);
        usedRoles.Add("target");

        var usedPositions = new HashSet<int>();

        foreach (var parameter in action.Parameters)
        {
            if (string.IsNullOrWhiteSpace(parameter.Role))
            {
                errors.Add($"Action {action.Name}: rôle vide.");
                valid = false;
                continue;
            }

            if (!parameter.IsTarget && !usedRoles.Add(parameter.Role))
            {
                errors.Add($"Action {action.Name}: rôle dupliqué ou réservé : " + parameter.Role);
                valid = false;
            }

            if (parameter.IsTarget)
            {
                if (parameter.Position != -1 || parameter.ParameterType != action.DeclaringInterface)
                {
                    errors.Add($"Action {action.Name}: définition invalide de target.");
                    valid = false;
                }

                continue;
            }

            if (parameter.Position < 0 || parameter.Position >= methodParameters.Length)
            {
                errors.Add($"Action {action.Name}: position invalide pour " + parameter.Role);
                valid = false;
                continue;
            }

            if (!usedPositions.Add(parameter.Position))
            {
                errors.Add($"Action {action.Name}: position " + parameter.Position + " utilisée plusieurs fois.");
                valid = false;
                continue;
            }

            Type actualType = methodParameters[parameter.Position].ParameterType;

            if (parameter.ParameterType != actualType)
            {
                errors.Add($"Action {action.Name}: type incompatible pour " + parameter.Role + ". Type déclaré : " + parameter.ParameterType.Name + ", type réel : " + actualType.Name);
                valid = false;
            }
        }

        for (int i = 0; i < methodParameters.Length; i++)
        {
            if (!usedPositions.Contains(i))
            {
                errors.Add($"Action {action.Name}: le paramètre C# à la position " + i + " n'est pas déclaré.");
                valid = false;
            }
        }

        foreach (var condition in action.Preconditions)
        {
            if (string.IsNullOrWhiteSpace(condition.Predicate))
            {
                errors.Add($"Action {action.Name}: prédicat vide dans une précondition.");
                valid = false;
            }

            if (condition.Arguments == null)
                continue;

            foreach (string argument in condition.Arguments)
            {
                if (!usedRoles.Contains(argument))
                {
                    errors.Add($"Action {action.Name}: rôle inconnu dans la " + $"précondition {condition.Predicate}: {argument}");
                    valid = false;
                }
            }
        }


        ValidateEffects(action, action.AddEffects, usedRoles, errors, ref valid);

        ValidateEffects(action, action.DeleteEffects, usedRoles, errors, ref valid);

        var conditionGroups = action.Preconditions.GroupBy(c => $"{c.Predicate}|{string.Join("|", c.Arguments)}");

        foreach (var group in conditionGroups)
        {
            if (group.Select(c => c.IsPositive).Distinct().Count() > 1)
            {
                errors.Add($"Action {action.Name}: préconditions contradictoires " + $"pour {group.Key}.");
                valid = false;
            }
        }

        foreach (var add in action.AddEffects)
        {
            bool conflict = action.DeleteEffects.Any(del => del.Predicate == add.Predicate && del.Arguments.SequenceEqual(add.Arguments));

            if (conflict)
            {
                errors.Add($"Action {action.Name}: {add.Predicate} est ajouté " + "et supprimé par la même action.");

                valid = false;
            }
        }

        return valid;
    }

    private void ValidateEffects(OntologyScanner.ActionInfo action, List<OntologyScanner.EffectInfo> effects, HashSet<string> usedRoles, List<string> errors, ref bool valid)
    {
        foreach (var effect in effects)
        {
            if (string.IsNullOrWhiteSpace(effect.Predicate))
            {
                errors.Add($"Action {action.Name}: prédicat vide dans un effet.");
                valid = false;
            }

            if (effect.Arguments == null)
                continue;

            foreach (string argument in effect.Arguments)
            {
                if (!usedRoles.Contains(argument))
                {
                    errors.Add($"Action {action.Name}: rôle inconnu dans l'effet " + $"{effect.Predicate}: {argument}");
                    valid = false;
                }
            }
        }
    }
}
