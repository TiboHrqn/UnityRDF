using UnityEngine;

[OntologyClass("Object")]
public abstract class Object : Entity
{
    [OntologyRelation("locatedIn", typeof(Room))]
    public Room room { get; set; }
}
