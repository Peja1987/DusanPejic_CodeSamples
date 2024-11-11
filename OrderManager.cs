using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [SerializeField] private float _orderInterval = 10.0f;
    [SerializeField] private float _retryInterval = 5.0f;
    [SerializeField] private List<OrderData> _orderDataList;

    private List<Drone> _drones = new List<Drone>();
    private List<BuildingForOrder> _allBuildingsForOrders = new List<BuildingForOrder>(); 
    private List<BuildingForOrder> _buildingsReadyForOrders = new List<BuildingForOrder>(); 
    private OrderFactory _orderFactory;

    private int _stageOpened = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _orderFactory = new OrderFactory(_orderDataList);
        StartCoroutine(GenerateOrders());

        Invoke("DelayToCheckIfSomeNewBuildingsAreReadyForOrder", 0.1f);
    }

    private void OnEnable()
    {
        EventManager.OpenNextStageForBuildings += OnOpenNextStageForBuildings;
    }


    private void OnDisable()
    {
        EventManager.OpenNextStageForBuildings -= OnOpenNextStageForBuildings;
    }
    private void OnOpenNextStageForBuildings(int index)
    {
        Invoke("DelayToCheckIfSomeNewBuildingsAreReadyForOrder", 0.1f);
    }

    private void SetBuildingsReadyForOrders()
    {
        _buildingsReadyForOrders.Clear();

        for (int i = 0; i < _allBuildingsForOrders.Count; i++)
        {
            if(_allBuildingsForOrders[i].CanReciveOrders)
            {
                _buildingsReadyForOrders.Add(_allBuildingsForOrders[i]);
            }
        }
    }

    private void DelayToCheckIfSomeNewBuildingsAreReadyForOrder()
    {
        SetBuildingsReadyForOrders();
    }

    public void RegisterDrone(Drone drone)
    {
        _drones.Add(drone);
    }
    public void RegisterBuilding(BuildingForOrder building)
    {
        _allBuildingsForOrders.Add(building);
    }

    private IEnumerator GenerateOrders()
    {
        while (true)
        {
            yield return new WaitForSeconds(_orderInterval);
            GenerateOrder();
        }
    }

    private void GenerateOrder()
    {

        BuildingForOrder building = _buildingsReadyForOrders[Random.Range(0, _buildingsReadyForOrders.Count)];

        if(building.CheckIfCanHaveOrder())
        {
            AssignOrderToDrone(building);
        }
    }
    private void AssignOrderToDrone(BuildingForOrder building)
    {
        Drone availableDrone = FindAvailableDrone(building);
        if (availableDrone != null)
        {
            Order order = _orderFactory.CreateRandomOrder(building.AcceptedDroneTypes);
            order.Destination = building.transform;
            building.AddOrder(order);
            availableDrone.AssignOrder(order);
        }
    }

    private Drone FindAvailableDrone(BuildingForOrder building)
    {
        foreach (Drone drone in _drones)
        {
            if (drone.IsAvailable && building.CanAcceptOrder(drone.GetDroneType()))
            {
                return drone;
            }
        }
        return null;
    }

    public void CompleteOrder(Order order, Vector3 positionWhereTheOrderIsDroped)
    {
        for (int i = 0; i < _allBuildingsForOrders.Count; i++)
        {
            _allBuildingsForOrders[i].RemoveOrder(order);
        }

        order.CompleteOrder(positionWhereTheOrderIsDroped);
        EventManager.OnOrderCompleted(order.Data.RewardMoney);
    }

    
}
