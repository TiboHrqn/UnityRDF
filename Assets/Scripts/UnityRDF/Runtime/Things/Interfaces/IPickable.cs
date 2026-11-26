[OntologyCapability("Pickable")]
public interface IPickable
{
    [OntologyProperty("isPickedUp")]
    bool IsPickedUp { get; }

    [OntologyAction("PickUp")]
    [OntologyParameter("actor", typeof(Agent))]
    [OntologyParameter("item", typeof(IPickable))]

    [OntologyPreCondition(false, "isPickedUp" , "item")]

    [OntologyAddEffect("isPickedUp", "item")]
    void PickUp(Agent actor);

    [OntologyAction("Drop")]
    [OntologyParameter("actor", typeof(Agent))]
    [OntologyParameter("item", typeof(IPickable))]

    [OntologyPreCondition(true, "isPickedUp" , "item")]

    [OntologyDeleteEffect("isPickedUp", "item")]
    void Drop(Agent actor);
}