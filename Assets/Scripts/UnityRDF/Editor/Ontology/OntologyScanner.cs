using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Assemblies;

public class OntologyScanner
{
    public class ClassInfo
    {
        public Type Type;
        public OntologyClassAttribute Attribute;
        public Type ParentType;
    }

    public class PropertyInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type RangeType;
    }

    public class RelationInfo
    {
        public string Name;
        public string Description;
        public Type DomainType;
        public Type TargetType;
    }

    public List<ClassInfo> Classes { get; } = new();
    public List<PropertyInfo> Properties { get; private set; } = new();
    public List<RelationInfo> Relations { get; private set; } = new();

    public void Scan()
    {
        Classes.Clear();
        Properties.Clear();
        Relations.Clear();

        foreach (Assembly assembly in CurrentAssemblies.GetLoadedAssemblies())
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(t => t != null).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (Type type in types)
            {
                var attribute = type.GetCustomAttribute<OntologyClassAttribute>(false);

                if (attribute == null)
                    continue;

                Type parent = type.BaseType;

                while (parent != null && parent.GetCustomAttribute<OntologyClassAttribute>(false) == null)
                {
                    parent = parent.BaseType;
                }

                Classes.Add(new ClassInfo
                {
                    Type = type,
                    Attribute = attribute,
                    ParentType = parent
                });
            }
        }

        ScanMembers();
    }

    private void ScanMembers()
    {
        foreach (ClassInfo classInfo in Classes)
        {
            foreach (var property in classInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var ontologyProperty = property.GetCustomAttribute<OntologyPropertyAttribute>(true);

                var ontologyRelation = property.GetCustomAttribute<OntologyRelationAttribute>(true);

                Type domain = property.DeclaringType ?? classInfo.Type;

                if (ontologyProperty != null)
                {
                    Properties.Add(new PropertyInfo
                    {
                        Name = ontologyProperty.Name,
                        Description = ontologyProperty.Description,
                        DomainType = domain,
                        RangeType = property.PropertyType
                    });
                }

                if (ontologyRelation != null)
                {
                    Relations.Add(new RelationInfo
                    {
                        Name = ontologyRelation.Name,
                        Description = ontologyRelation.Description,
                        DomainType = domain,
                        TargetType = ontologyRelation.TargetType
                    });
                }
            }
        }

        Properties = Properties
            .GroupBy(p => $"{p.Name}|{p.DomainType}|{p.RangeType}")
            .Select(g => g.First())
            .ToList();

        Relations = Relations
            .GroupBy(r => $"{r.Name}|{r.DomainType}|{r.TargetType}")
            .Select(g => g.First())
            .ToList();
    }
}