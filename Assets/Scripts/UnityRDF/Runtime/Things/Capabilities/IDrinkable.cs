[OntologyCapability("Drinkable")]
public interface IDrinkable
{
    [OntologyProperty("isFull")]
    bool IsFull { get; }


    [OntologyAction("Drink")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(true, "isFull", "target")]
    [OntologyPreCondition(true, "hasInInventory", "actor", "target")]

    [OntologyDeleteEffect("isFull", "target")]

    ActionResult Drink(Agent actor);
}