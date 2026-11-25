public interface IFillable
{
    [OntologyProperty("isFull")]
    bool IsFull { get; }

    [OntologyAction("Fill")]
    [OntologyParameter("actor" , typeof(Agent))]
    [OntologyParameter("container" , typeof(IFillable))]

    [OntologyPreCondition(false, "isFull" , "container")]

    [OntologyAddEffect("isFull" , "container")]
    void Fill(Agent actor);
}
