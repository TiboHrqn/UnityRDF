
using UnityEngine;

[OntologyClass("Key")]
public class Key : SmartObject, IPickable
{
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
    
}
