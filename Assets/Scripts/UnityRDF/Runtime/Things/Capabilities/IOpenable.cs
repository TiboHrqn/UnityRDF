
[OntologyCapability("Openable")]
public interface IOpenable
{
    [OntologyProperty("isOpen")]
    bool IsOpen { get; }

    [OntologyProperty("isLocked")]
    bool IsLocked { get; }

    [OntologyRelation("requiresKey", typeof(Key))]
    Key RequiredKey { get; }


    // UNLOCK
    [OntologyAction("Unlock")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]
    [OntologyActionParameter("key", typeof(Key), 1)]

    [OntologyPreCondition(true, "isLocked", "target")]
    [OntologyPreCondition(false, "isOpen", "target")]
    [OntologyPreCondition(true, "requiresKey", "target", "key")]
    [OntologyPreCondition(true, "hasInInventory", "actor", "key")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyDeleteEffect("isLocked", "target")]

    ActionResult Unlock(Agent actor, Key key);


    // LOCK
    [OntologyAction("Lock")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]
    [OntologyActionParameter("key", typeof(Key), 1)]

    [OntologyPreCondition(false, "isLocked", "target")]
    [OntologyPreCondition(false, "isOpen", "target")]
    [OntologyPreCondition(true, "requiresKey", "target", "key")]
    [OntologyPreCondition(true, "hasInInventory", "actor", "key")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyAddEffect("isLocked", "target")]

    ActionResult Lock(Agent actor, Key key);


    // OPEN
    [OntologyAction("Open")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(false, "isOpen", "target")]
    [OntologyPreCondition(false, "isLocked", "target")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyAddEffect("isOpen", "target")]

    ActionResult Open(Agent actor);


    // CLOSE
    [OntologyAction("Close")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(true, "isOpen", "target")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyDeleteEffect("isOpen", "target")]

    ActionResult Close(Agent actor);
}
