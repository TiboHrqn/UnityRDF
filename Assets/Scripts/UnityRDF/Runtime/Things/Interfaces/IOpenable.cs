[OntologyCapability("Openable")]
public interface IOpenable
{
    [OntologyProperty("isOpen")]
    bool IsOpen { get; }

    [OntologyRelation("requiresKey", typeof(Key))]
    Key RequiredKey { get; }

    [OntologyAction("Open")]
    [OntologyParameter("actor", typeof(Agent))]
    [OntologyParameter("door", typeof(IOpenable))]
    [OntologyParameter("key", typeof(Key))]

    [OntologyPreCondition(false, "isOpen", "door")]
    [OntologyPreCondition(true, "requiresKey", "door", "key")]
    [OntologyPreCondition(true, "hasInInventory" , "actor", "key")]

    [OntologyAddEffect("isOpen", "door")]
    void Open(Agent actor, Key key);
}
