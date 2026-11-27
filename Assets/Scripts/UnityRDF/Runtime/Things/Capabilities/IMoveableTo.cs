
[OntologyCapability("MoveableTo")]
public interface IMoveableTo
{
    [OntologyAction("MoveTo")]
    [OntologyActionParameter("actor", typeof(Agent), 0)]
    [OntologyPreCondition(true, "canMove", "actor")]
    [OntologyPreCondition(true, "canReach", "actor", "target")]
    [OntologyAddEffect("isNear", "actor", "target")]
    ActionResult MoveTo(Agent actor);
}

