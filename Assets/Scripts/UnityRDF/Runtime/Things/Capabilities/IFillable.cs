
[OntologyCapability("Fillable")]
public interface IFillable
{
    [OntologyProperty("isFull")]
    bool IsFull { get; }

    [OntologyAction("Fill")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]

    [OntologyPreCondition(false, "isFull", "target")]
    [OntologyPreCondition(true, "isNear", "actor", "target")]

    [OntologyAddEffect("isFull", "target")]

    ActionResult Fill(Agent actor);
}
