using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.XR;

public abstract class Drone : MonoBehaviour
{
    [SerializeField] private Transform _currentOrderPosition;
    [SerializeField] private Transform _baseLocation;
    [SerializeField] private Transform _chargingStationPosition;
    [SerializeField] private Drone _repairingDrone;
    [SerializeField] private DroneType _droneType;
    [SerializeField] private GameObject _crateOrder;

    [SerializeField] private float _moveSpeed;
    [SerializeField] private List<DroneData> _droneDatas = new List<DroneData>();
    [SerializeField] private bool _isRepairDrone;
    [SerializeField] private int _indexOfBase;
    [SerializeField] private int _indexBaseParkingSpot;
    [SerializeField] private OrderMoneyEarnedUI _orderMoneyEarnedUIPrefab;
    [SerializeField] private Transform _postionForOrderMoneyUI;
    [SerializeField] private int RewardMoneyTest;
    [SerializeField] private Drone _droneToRepair;


    private DroneBateryHealth _bateryHealth;
    private DronePowerUpEffect _powerUpEffect;
    private NavMeshAgent _agent;
    private DroneStateMachine _droneStateMachine;
    private bool _isAvailable;
    private bool _hasLowBatery;
    private bool _bateryIsFull;
    private bool _bateryIsDead;
    private bool _droneIsMoving;
    private ChargingStation _currentChargingStation;
    public Order _droneOrder;
    private DroneData _droneData;
    private int _currentDroneLevel = 1;
    private GameObject _orderMoneyUIParent;
    private OrderMoneyEarnedUI _orderMoneyEarnedUI;
    private float _speedBeforePowerUpSpeedBoost;
    private bool _hasSpeedBoostPowerUpEffect;
    private float _speedBoostPowerUpMultiplier;


    public bool IsAvailable { get => _isAvailable; set => _isAvailable = value; }
    public int CurrentDroneLevel { get => _currentDroneLevel; set => _currentDroneLevel = value; }
    public DroneData DroneData { get => _droneData; set => _droneData = value; }
    public ChargingStation CurrentChargingStation => _currentChargingStation;
    public GameObject CrateOrder => _crateOrder;
    public bool HasLowBattery => _hasLowBatery;
    public bool BatteryIsFull => _bateryIsFull;
    public bool BatteryIsDead => _bateryIsDead;
    public bool IsRepairDrone => _isRepairDrone;
    public Drone RepairDrone => _repairingDrone;
    public Transform CurrentOrderDestination => _currentOrderPosition;
    public Transform BaseLocation { get => _baseLocation; set => _baseLocation = value; }
    public Transform ChargingStation => _chargingStationPosition;
    public NavMeshAgent Agent { get => _agent; set => _agent = value; }
    public DroneStateMachine DroneStateMachine { get => _droneStateMachine; set => _droneStateMachine = value; }
    public DroneBateryHealth BateryHealth => _bateryHealth;

    public Drone DroneToRepair { get => _droneToRepair; set => _droneToRepair = value; }

    private void Awake()
    {
        _bateryHealth = GetComponent<DroneBateryHealth>();
        _powerUpEffect = GetComponent<DronePowerUpEffect>();
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        _orderMoneyUIParent = GameObject.Find(StringHelper.ORDER_MONEY_TO_SHOW_PARENT);
        SetDroneData(_currentDroneLevel);

        _isAvailable = true;

        _bateryHealth.LowBatery += OnLowBatery;
        _bateryHealth.ChargingBateryComplete += OnChargingBateryComplete;
        _bateryHealth.BateryDead += OnBateryDead;

        _powerUpEffect.IncreaseDroneSpeed += OnIncreaseDroneSpeed;
        _powerUpEffect.ResetDroneSpeed += OnResetDroneSpeedAfterPowerUp;

        _powerUpEffect.BatteryDrainStop += OnBatteryDrainStop;
        _powerUpEffect.BatteryDrainContinue += OnBatteryDrainContinue;

        EventManager.UpgradeDrone += OnUpgradeDrone;

        OrderManager.Instance.RegisterDrone(this);

        DroneStateMachine droneStateMachine = new DroneStateMachine();
        droneStateMachine.SetState(new DroneIdleBaseState(), this);

        _droneStateMachine = droneStateMachine;

    }

  

    public virtual void OnDestroy()
    {
        _bateryHealth.LowBatery -= OnLowBatery;
        _bateryHealth.ChargingBateryComplete -= OnChargingBateryComplete;
        _bateryHealth.BateryDead -= OnBateryDead;
        EventManager.UpgradeDrone -= OnUpgradeDrone;
        _powerUpEffect.IncreaseDroneSpeed -= OnIncreaseDroneSpeed;
        _powerUpEffect.ResetDroneSpeed -= OnResetDroneSpeedAfterPowerUp;
        _powerUpEffect.BatteryDrainStop -= OnBatteryDrainStop;
        _powerUpEffect.BatteryDrainContinue -= OnBatteryDrainContinue;
    }


    // Update is called once per frame
    public virtual void Update()
    {
        _droneStateMachine.Update();
        IfDroneIsMoving();

    }
    public void SetDroneDestination(Transform destination)
    {
        if (NavMesh.SamplePosition(destination.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            Debug.Log($"Valid destination set at {hit.position}");
        }
        else
        {
            Debug.LogError("Destination is not on the NavMesh." + " " + destination.position);

        }

        _agent.ResetPath();
        _agent.SetDestination(destination.position);
        _isAvailable = false;
    }

    public void AssignOrder(Order order)
    {
        _droneOrder = order;
        _currentOrderPosition = order.Destination;
        _droneStateMachine.SetState(new DroneGoToDeliveryState(), this);

        _orderMoneyEarnedUI = Instantiate(_orderMoneyEarnedUIPrefab);
        _orderMoneyEarnedUI.SetUp(order.Data.RewardMoney, _postionForOrderMoneyUI);
        _orderMoneyEarnedUI.transform.SetParent(_orderMoneyUIParent.transform);
        _orderMoneyEarnedUI.transform.localScale = Vector3.one;
    }
    //EventSubscription

    private void OnBatteryDrainContinue()
    {
        _bateryHealth.IsBatteryPowerUpActivated(false);
    }

    private void OnBatteryDrainStop()
    {
        _bateryHealth.IsBatteryPowerUpActivated(true);
    }

    private void OnResetDroneSpeedAfterPowerUp()
    {
        _hasSpeedBoostPowerUpEffect = false;
        _agent.speed = _speedBeforePowerUpSpeedBoost;
    }

    private void OnIncreaseDroneSpeed(float speedMultiplier)
    {
        _hasSpeedBoostPowerUpEffect = true;
        _speedBoostPowerUpMultiplier = speedMultiplier;
        _speedBeforePowerUpSpeedBoost = _agent.speed;
        _agent.speed *= speedMultiplier;
    }
    private void OnLowBatery()
    {
        _hasLowBatery = true;
        _bateryIsFull = false;
    }
    private void OnChargingBateryComplete()
    {
        _bateryIsFull = true;
        _hasLowBatery = false;
        _bateryIsDead = false;
    }
    private void OnBateryDead()
    {
       
        _hasLowBatery = false;
        _bateryIsDead = true;
        _agent.isStopped = true;
    }
    public virtual void OnUpgradeDrone(DroneData upgradedDroneData, DroneBatteryData upgradedDroneBatteryData, int baseIndex, int baseSlotIndex, Button button, bool batteryUpgrade)
    {
        if (_indexOfBase == baseIndex && _indexBaseParkingSpot == baseSlotIndex && batteryUpgrade == false)
        {
            SetDroneData(upgradedDroneData.Level);
        }
        else if (_indexOfBase == baseIndex && _indexBaseParkingSpot == baseSlotIndex && batteryUpgrade == true)
        {
            _bateryHealth.SetBateryData(upgradedDroneBatteryData.Level);
        }

    }

    public void SetChargingStationPositionAndChargingTime(ChargingStation chargingStation, float chargingTime, Transform chargingSpot)
    {
        _chargingStationPosition = chargingSpot;
        _currentChargingStation = chargingStation;
        _bateryHealth.SetChargingTimeForBatery(chargingTime);
    }


    public void SetDroneData(int currentLevel)
    {
        DroneData droneData = new DroneData();

        for (int i = 0; i < _droneDatas.Count; i++)
        {
            if (_droneDatas[i].Level == currentLevel)
            {
                droneData = _droneDatas[i];
                break;
            }
        }

        _droneData = droneData;
        _agent.speed = droneData.Speed;

        if(_hasSpeedBoostPowerUpEffect)
        {
            OnIncreaseDroneSpeed(_speedBoostPowerUpMultiplier);
        }
    }

    public DroneType GetDroneType()
    {
        return _droneType;
    }

    public void NotifyOrderCompleted()
    {
        OrderManager.Instance.CompleteOrder(_droneOrder, transform.position);
        _currentOrderPosition = null;
        _droneOrder = null;
        Destroy(_orderMoneyEarnedUI.gameObject);
    }

    public void NotifyDroneAvailableAgain()
    {
        _isAvailable = true;
    }
    public void RemoveDroneFromChargingStation()
    {
        ChargingStationsManager.Instance.RemoveDroneFromStation(this, _currentChargingStation, _chargingStationPosition);
        _currentChargingStation = null;
        _chargingStationPosition = null;
    }

    public void RemoveDroneFromRepairingDrone()
    {
        DronesForRepairManager.Instance.RemoveDroneFromRepairingDrone(_repairingDrone);
        _repairingDrone = null;
    }
    public void StartChargingBattery()
    {
        _bateryHealth.StartCharging();
    }

    private void IfDroneIsMoving()
    {
        if (_agent.enabled)
        {
            if (_agent.velocity.sqrMagnitude > 0.1f && _droneIsMoving == false)
            {
                _droneIsMoving = true;
                _bateryHealth.DroneIsMoving(true);
            }
            else if (_droneIsMoving && _agent.velocity.sqrMagnitude <= 0)
            {
                _droneIsMoving = false;
                _bateryHealth.DroneIsMoving(false);
            }
        }

    }
    public void DroneSetUp(Transform parkingSpotInBase, int indexForBase, int indexForBaseSlot)
    {
        BaseLocation = parkingSpotInBase;
        transform.position = new Vector3(BaseLocation.transform.position.x, transform.position.y, BaseLocation.transform.position.z);
        transform.rotation = BaseLocation.transform.rotation;

        _indexOfBase = indexForBase;
        _indexBaseParkingSpot = indexForBaseSlot;

        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = true;
    }
    public virtual void AssignDroneToRepair(Drone drone)
    {
    }
    public virtual void RemoveDroneFromRepairing()
    {
    }

    public void SetReapirDroneAndChargingTime(Drone drone, float chargingTime)
    {
        _repairingDrone = drone;
        _bateryHealth.SetChargingTimeForBatery(chargingTime);
    }

    public void DestroyParticleForCharging(GameObject particle)
    {
        Destroy(particle);
    }
}

public enum DroneType
{
    STANDARD,
    HEAVY,
    FAST,
    LONGRANGE,
    REPAIR
}
