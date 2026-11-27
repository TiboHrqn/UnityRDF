
using UnityEngine;

[OntologyClass("Bottle")]
public class Bottle : SmartObject, IPickable, IDrinkable, IFillable
{
    [OntologyProperty("isFull")]
    public bool IsFull { get; private set; }


    public ActionResult PickUp(Agent actor)
    {
        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Bottle is too far away.");

        actor.AddToInventory(this);

        return ActionResult.Success("Bottle picked up.");
    }

    public ActionResult Drop(Agent actor)
    {
        if (!actor.HasInInventory(this))
            return ActionResult.Fail(ActionStatus.InvalidState, "Bottle is not in inventory.");

        actor.RemoveFromInventory(this);

        return ActionResult.Success("Bottle dropped.");
    }

    public ActionResult Drink(Agent actor)
    {
        if (!IsFull)
            return ActionResult.Fail(ActionStatus.InvalidState, "Bottle is empty.");

        if (!actor.HasInInventory(this))
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Bottle is not in inventory.");

        IsFull = false;

        return ActionResult.Success("Bottle emptied.");
    }


    public ActionResult Fill(Agent actor)
    {
        if (IsFull)
            return ActionResult.Fail(ActionStatus.InvalidState, "Bottle is already full.");

        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Bottle is too far away.");

        IsFull = true;

        return ActionResult.Success("Bottle filled.");
    }
}
