using UnityEngine;

[OntologyClass("Key")]
public class Key : SmartObject, IPickable
{
    [OntologyProperty("isPickedUp")]
    public bool IsPickedUp { get; private set; }

    public void PickUp(Agent actor)
    {
        IsPickedUp = true;
    }

    public void Drop(Agent actor)
    {
        IsPickedUp = false;
    }
}