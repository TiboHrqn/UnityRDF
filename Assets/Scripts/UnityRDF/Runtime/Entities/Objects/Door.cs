using UnityEngine;

[OntologyClass("Door")]
public class Door : Object
{
    [OntologyProperty("isOpen")]
    public bool isOpen { get; set; }

    [OntologyProperty("isLocked")]
    public bool isLocked { get; set; }

}
