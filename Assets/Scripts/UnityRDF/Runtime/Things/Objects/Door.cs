using UnityEngine;

[OntologyClass("Door")]
public class Door : SmartObject, IOpenable
{
    [OntologyProperty("isOpen")]
    public bool IsOpen { get; private set; }

    [OntologyRelation("requiresKey", typeof(Key))]
    public Key RequiredKey { get; private set; }

    public void Initialize(Key requiredKey)
    {
        RequiredKey = requiredKey;
    }

    public void Open(Agent actor, Key key)
    {
        IsOpen = true;
    }
}
