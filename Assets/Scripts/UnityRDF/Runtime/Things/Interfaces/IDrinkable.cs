public interface IDrinkable
{
    [OntologyProperty("isFull")]
    bool IsFull { get; }

    [OntologyAction("Drink")]
    [OntologyParameter("actor", typeof(Agent))]
    [OntologyParameter("bottle", typeof(IDrinkable))]

    [OntologyPreCondition(true, "isFull", "bottle")]

    [OntologyDeleteEffect("isFull", "bottle")]
    void Drink(Agent actor);
}