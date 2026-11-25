using UnityEngine;

[OntologyClass("Bottle")]
public class Bottle : SmartObject, IPickable, IDrinkable, IFillable
{
    [OntologyProperty("isPickedUp")]
    public bool IsPickedUp { get; private set; }

    [OntologyProperty("isFull")]
    public bool IsFull { get; private set; }

    public void PickUp(Agent actor)
    {
        IsPickedUp = true;
    }

    public void Drop(Agent actor)
    {
        IsPickedUp = false;
    }

    public void Drink(Agent actor)
    {
        if (IsFull)
            IsFull = false;
    }

    public void Fill(Agent actor)
    {
        if (!IsFull)
            IsFull = true;
    }
}
