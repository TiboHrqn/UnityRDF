using UnityEngine;
using System.Collections.Generic;

[OntologyClass("Agent")]
public class Agent : SmartObject
{
    [Header("Movement")]
    [SerializeField]
    private bool canMove = true;

    [Header("Proximity")]
    [SerializeField]
    private float interactionRange = 2f;

    private readonly List<SmartObject> _inventory = new List<SmartObject>();
    private readonly List<SmartObject> _nearObjects = new List<SmartObject>();
    private readonly List<SmartObject> _reachableObjects = new List<SmartObject>();

    [OntologyProperty("canMove")]
    public bool CanMove => canMove;

    [OntologyRelation("hasInInventory", typeof(SmartObject))]
    public List<SmartObject> Inventory => _inventory;

    [OntologyRelation("isNear", typeof(SmartObject))]
    public IEnumerable<SmartObject> NearObjects => _nearObjects;

    [OntologyRelation("canReach", typeof(SmartObject))]
    public IEnumerable<SmartObject> ReachableObjects => _reachableObjects;

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void AddToInventory(SmartObject item)
    {
        if (item != null && !_inventory.Contains(item))
            _inventory.Add(item);
    }

    public void RemoveFromInventory(SmartObject item)
    {
        if (item != null)
            _inventory.Remove(item);
    }

    public bool HasInInventory(SmartObject item)
    {
        return item != null && _inventory.Contains(item);
    }

    public bool IsNear(SmartObject target)
    {
        return target != null && _nearObjects.Contains(target);
    }

    public bool CanReachObject(SmartObject target)
    {
        return target != null && _reachableObjects.Contains(target);
    }

    private void Update()
    {
        UpdateNearbyObjects();
        UpdateReachableObjects();
    }

    private void UpdateNearbyObjects()
    {
        //todo
    }

    private void UpdateReachableObjects()
    {
        //todo
    }
}