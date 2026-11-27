[OntologyCapability("Pickable")]
public interface IPickable
{
    [OntologyAction("PickUp")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(false, "hasInInventory", "actor", "target")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyAddEffect("hasInInventory", "actor", "target")]

    ActionResult PickUp(Agent actor);


    [OntologyAction("Drop")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(true, "hasInInventory", "actor", "target")]

    [OntologyDeleteEffect("hasInInventory", "actor", "target")]

    ActionResult Drop(Agent actor);
}
