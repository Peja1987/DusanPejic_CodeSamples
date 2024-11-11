using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Order 
{

    private Transform _destination;
    private bool _isAssigned;
    private OrderData _data;

    public Transform Destination { get => _destination; set => _destination = value; }

    public bool IsAssigned { get { return _isAssigned; } }

    public OrderData Data { get => _data; set => _data = value; }

    public virtual void AssignOrder()
    {
        int random = Random.Range(Data.MinMoney, Data.MaxMoney);
        Data.RewardMoney = random;
    }

    public abstract void CompleteOrder(Vector3 positionWhereOrderIsDroped);
}
