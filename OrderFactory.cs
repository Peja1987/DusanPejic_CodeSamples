using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OrderFactory
{
    private List<OrderData> _orderDataList; // Lista za različite tipove porudžbina

    public OrderFactory(List<OrderData> orderDataList)
    {
        _orderDataList = orderDataList; // Inicijalizuj listu
    }

    public T CreateOrder<T>(OrderData orderData) where T : Order, new()
    {
        //T order = new T();
        //order.Data = orderData; // Dodeli podatke o porudžbini
        //return order;
        T order = new T();
        // Kreiraj novu instancu OrderData kako bi izbegao deljenje referenci
        OrderData newData = new OrderData
        {
            Type = orderData.Type,
            MinMoney = orderData.MinMoney,
            MaxMoney = orderData.MaxMoney,
            RewardMoney = orderData.RewardMoney,
            TipsChance = orderData.TipsChance
            
        };
        order.Data = newData;
        return order;

    }


    public Order CreateRandomOrder(List<DroneType> supportedDroneTypes)
    {
        //// Filtriraj porudžbine na osnovu tipova dronova koje zgrada može primiti
        //List<OrderData> validOrders = new List<OrderData>();

        //foreach (OrderData orderData in _orderDataList)
        //{
        //    if (CanDroneHandleOrder(orderData, supportedDroneTypes))
        //    {
        //        validOrders.Add(orderData);
        //    }
        //}

        //// Ako postoje validne porudžbine, kreiraj jednu nasumično
        //if (validOrders.Count > 0)
        //{
        //    int orderType = UnityEngine.Random.Range(0, validOrders.Count);
        //    OrderData orderData = validOrders[orderType];

        //    switch (orderData.Type)
        //    {
        //        case "Food": return CreateOrder<OrderFood>(orderData);
        //        case "Drinks": return CreateOrder<OrderDrinks>(orderData);
        //        case "Clothes": return CreateOrder<OrderClothes>(orderData);
        //        case "Electronics": return CreateOrder<OrderElectronics>(orderData);
        //        case "MedicalSupplies": return CreateOrder<OrderMedicalSupplies>(orderData);
        //        default: return CreateOrder<OrderFood>(orderData); // Default fallback
        //    }
        //}

        //return null; // Ako nema validnih porudžbina

        List<OrderData> validOrders = new List<OrderData>();
        foreach (OrderData orderData in _orderDataList)
        {
            if (CanDroneHandleOrder(orderData, supportedDroneTypes))
            {
                validOrders.Add(orderData);
            }
        }
        if (validOrders.Count > 0)
        {
            int orderType = UnityEngine.Random.Range(0, validOrders.Count);
            OrderData orderData = validOrders[orderType];
            Order newOrder = null;

            switch (orderData.Type)
            {
                case "Food": newOrder = CreateOrder<OrderFood>(orderData); break;
                case "Drinks": newOrder = CreateOrder<OrderDrinks>(orderData); break;
                case "Clothes": newOrder = CreateOrder<OrderClothes>(orderData); break;
                case "Electronics": newOrder = CreateOrder<OrderElectronics>(orderData); break;
                case "MedicalSupplies": newOrder = CreateOrder<OrderMedicalSupplies>(orderData); break;
                default: newOrder = CreateOrder<OrderFood>(orderData); break; // Default fallback
            }

            newOrder.AssignOrder(); // Postavi RewardMoney odmah nakon kreiranja
            return newOrder;
        }
        return null; // Ako nema validnih porudžbina
    }
    private bool CanDroneHandleOrder(OrderData orderData, List<DroneType> supportedDroneTypes)
    {
        // Na osnovu tipa porudžbine, proveri da li dronovi mogu da obavljaju tu porudžbinu
        switch (orderData.Type)
        {
            case "Food":
            case "Drinks":
            case "Clothes":
                return supportedDroneTypes.Contains(DroneType.STANDARD) || supportedDroneTypes.Contains(DroneType.FAST);

            case "Electronics":
                return supportedDroneTypes.Contains(DroneType.HEAVY);

            case "MedicalSupplies":
                return supportedDroneTypes.Contains(DroneType.LONGRANGE);

            default:
                return false;
        }
    }

}
