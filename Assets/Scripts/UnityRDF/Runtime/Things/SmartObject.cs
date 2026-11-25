using UnityEngine;
using System;

[OntologyClass("SmartObject")]
public abstract class SmartObject : MonoBehaviour 
{
    [OntologyProperty("id")]
    public string Id { get; private set; }

    [OntologyRelation("locatedIn", typeof(Room))]
    public Room room { get; set; }

    protected virtual void Awake()
    {
        Id = Guid.NewGuid().ToString();
    }
}