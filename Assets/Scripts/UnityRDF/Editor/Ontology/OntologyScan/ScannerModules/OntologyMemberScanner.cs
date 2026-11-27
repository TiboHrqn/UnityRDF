using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class OntologyMemberScanner
{
    public void Scan(
        List<OntologyScanner.ClassInfo> classes,
        List<OntologyScanner.CapabilityInfo> capabilities,
        List<OntologyScanner.PropertyInfo> properties,
        List<OntologyScanner.RelationInfo> relations,
        List<string> errors)
    {
        foreach (var classInfo in classes)
        {
            ScanTypeMembers(classInfo.Type, properties, relations, errors);
        }

        foreach (var capability in capabilities)
        {
            ScanTypeMembers(capability.Type, properties, relations, errors);
        }
        RemoveDuplicates(properties, relations);
    }

    private void ScanTypeMembers(Type type, List<OntologyScanner.PropertyInfo> properties, List<OntologyScanner.RelationInfo> relations, List<string> errors)
    {
        System.Reflection.PropertyInfo[] members;

        try
        {
            members = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );
        }
        catch (Exception exception)
        {
            errors.Add($"Erreur lors du scan des propriétés de {type.Name}: " + exception.Message);
            return;
        }

        foreach (var property in members)
        {
            var ontologyProperty = property.GetCustomAttribute<OntologyPropertyAttribute>(false);

            var ontologyRelation = property.GetCustomAttribute<OntologyRelationAttribute>(false);

            if (ontologyProperty != null)
            {
                properties.Add(new OntologyScanner.PropertyInfo
                {
                    Name = ontologyProperty.Name,
                    Description = ontologyProperty.Description,
                    DomainType = type,
                    RangeType = property.PropertyType
                });
            }

            if (ontologyRelation != null)
            {
                relations.Add(new OntologyScanner.RelationInfo
                {
                    Name = ontologyRelation.Name,
                    Description = ontologyRelation.Description,
                    DomainType = type,
                    TargetType = ontologyRelation.TargetType
                });
            }
        }
    }

    private void RemoveDuplicates(List<OntologyScanner.PropertyInfo> properties, List<OntologyScanner.RelationInfo> relations)
    {
        var uniqueProperties = properties
            .GroupBy(p =>
                $"{p.Name}|{p.DomainType.AssemblyQualifiedName}|" +
                $"{p.RangeType.AssemblyQualifiedName}")
            .Select(g => g.First())
            .ToList();

        properties.Clear();
        properties.AddRange(uniqueProperties);

        var uniqueRelations = relations
            .GroupBy(r =>
                $"{r.Name}|{r.DomainType.AssemblyQualifiedName}|" +
                $"{r.TargetType.AssemblyQualifiedName}")
            .Select(g => g.First())
            .ToList();

        relations.Clear();
        relations.AddRange(uniqueRelations);
    }
}