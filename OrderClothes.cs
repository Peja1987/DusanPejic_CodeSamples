using UnityEngine;


public class OrderClothes : Order
{
    public override void CompleteOrder(Vector3 positionWhereOrderIsDroped)
    {
        Debug.Log("Clothes Delivered");
        int money = Data.RewardMoney;
        GameEconomy.Instance.AddMoney(money);
        TipsManager.Instance.ProcessTips(this, positionWhereOrderIsDroped);
        Debug.Log(money);
    }
}
