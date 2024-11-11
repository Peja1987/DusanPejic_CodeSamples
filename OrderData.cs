using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOrderData", menuName = "Order/Order Data")]
public class OrderData : ScriptableObject
{
    [SerializeField] private string _type; // Tip porudžbine
    [SerializeField] private string _orderName;
    [SerializeField] private int _minMoney;
    [SerializeField] private int _maxMoney;
    [SerializeField] private float _tipsChance;

    private int _rewardMoney;

    public string Type { get => _type; set => _type = value; }
    public string OrderName { get => _orderName; set => _orderName = value; }
    public int MinMoney { get => _minMoney; set => _minMoney = value; }
    public int MaxMoney { get => _maxMoney; set => _maxMoney = value; }
    public int RewardMoney { get => _rewardMoney; set => _rewardMoney = value; }
    public float TipsChance { get => _tipsChance; set => _tipsChance = value; }
}
