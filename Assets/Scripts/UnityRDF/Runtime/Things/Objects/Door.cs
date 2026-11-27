
using UnityEngine;

[OntologyClass("Door")]
public class Door : SmartObject, IOpenable
{
    [OntologyProperty("isOpen")]
    public bool IsOpen { get; private set; }

    [OntologyProperty("isLocked")]
    public bool IsLocked { get; private set; }

    [OntologyRelation("requiresKey", typeof(Key))]
    public Key RequiredKey { get; private set; }

    public void Initialize(Key requiredKey)
    {
        RequiredKey = requiredKey;
        IsLocked = requiredKey != null;
        IsOpen = false;
    }

    public ActionResult Unlock(Agent actor, Key key)
    {
        if (!IsLocked)
            return ActionResult.Fail(ActionStatus.InvalidState, "Door is already unlocked.");

        if (key == null || key != RequiredKey)
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Incorrect key.");

        if (!actor.HasInInventory(key))
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Key is not in inventory.");

        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Door is too far away.");

        IsLocked = false;

        return ActionResult.Success("Door unlocked.");
    }

    public ActionResult Lock(Agent actor, Key key)
    {
        if (IsLocked)
            return ActionResult.Fail(ActionStatus.InvalidState, "Door is already locked.");

        if (IsOpen)
            return ActionResult.Fail(ActionStatus.InvalidState, "Close the door before locking it.");

        if (key == null || key != RequiredKey)
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Incorrect key.");

        if (!actor.HasInInventory(key))
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Key is not in inventory.");

        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Door is too far away.");

        IsLocked = true;

        return ActionResult.Success("Door locked.");
    }

    public ActionResult Open(Agent actor)
    {
        if (IsOpen)
            return ActionResult.Fail(ActionStatus.InvalidState, "Door is already open.");

        if (IsLocked)
            return ActionResult.Fail(ActionStatus.MissingRequirement, "Door is locked.");

        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Door is too far away.");

        IsOpen = true;

        return ActionResult.Success("Door opened.");
    }


    public ActionResult Close(Agent actor)
    {
        if (!IsOpen)
            return ActionResult.Fail(ActionStatus.InvalidState, "Door is already closed.");

        if (!actor.IsNear(this))
            return ActionResult.Fail(ActionStatus.OutOfRange, "Door is too far away.");

        IsOpen = false;

        return ActionResult.Success("Door closed.");
    }
}
