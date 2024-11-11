using System.Collections.Generic;
using UnityEngine;

public abstract class ChargingStation : MonoBehaviour
{
    [SerializeField] private int _stationIndex;
    [SerializeField] private int _capacity;
    [SerializeField] private float _chargingSpeed;
    [SerializeField] private List<ChargingStationData> _chargingStationData = new List<ChargingStationData>();
    [SerializeField] private List<Transform> _chargingSpots;
    [SerializeField] private List<Transform> _spotsToAddToChargingSpots;
    [SerializeField] private List<Transform> _busySpots;
    [SerializeField] private bool _stationPurchased;
    [SerializeField] private int _costForPurchasing;
    [SerializeField] private GameObject _uiElementAboveBuildingPrefab;
    [SerializeField] private Transform _uiElementPositionOnBuilding;

    [SerializeField] private int _currentChargeCount;
    private int _currentChargingStationLevel = 1;
    private ChargingStationData _currentChargingStationData;
    [SerializeField] private List<Drone> _dronesInQueue = new List<Drone>();

    public int Index => _stationIndex;
    public int Capacity => _capacity;
    public bool StationPurchased => _stationPurchased;
    public int CostForPurchasing => _costForPurchasing;
    public float ChargingSpeed => _chargingSpeed;
    public int CurrentChargeCount => _currentChargeCount;
    public ChargingStationData CurrentChargingStationData => _currentChargingStationData;

    public abstract void UpgradeStation();
    public abstract bool IsAvailable();
    public abstract void AssignDrone(Drone drone);

    private void Start()
    {
        SetData(_currentChargingStationLevel);
        ChargingStationsManager.Instance.RegisterChargingStation(this);
        EventManager.UpgradeChargingStation += OnUpgradeChargingStation;
        EventManager.StationPurchased += OnStationPurchased;

        if (_stationPurchased == false)
        {
            for (int i = 0; i < _chargingSpots.Count; i++)
            {
                _chargingSpots[i].gameObject.SetActive(false);
            }
        }
        else
        {
            InstantiateUIAboveBuilding();
        }


    }


    private void OnDestroy()
    {
        EventManager.UpgradeChargingStation -= OnUpgradeChargingStation;
        EventManager.StationPurchased -= OnStationPurchased;
    }
    private void OnStationPurchased(int index)
    {
        if (index != _stationIndex)
            return;

        _stationPurchased = true;
        InstantiateUIAboveBuilding();

        for (int i = 0; i < _chargingSpots.Count; i++)
        {
            _chargingSpots[i].gameObject.SetActive(true);
        }
    }

    private void OnUpgradeChargingStation(ChargingStationData chargingStationData, int index)
    {
        if (_stationIndex == index)
        {
            SetData(chargingStationData.Level);
        }
    }

    protected void AddDroneToStation(Drone drone)
    {
        _currentChargeCount++;
        _dronesInQueue.Add(drone);
        Transform posInStation = PosInStation();
        drone.SetChargingStationPositionAndChargingTime(this, ChargingSpeed, posInStation);
    }

    public void RemoveDroneFromStation(Drone drone, Transform chargingSpot)
    {
        _currentChargeCount--;
        _dronesInQueue.Remove(drone);


        for (int i = 0; i < _busySpots.Count; i++)
        {
            if (_busySpots[i] == chargingSpot)
            {
                _chargingSpots.Add(chargingSpot);
                _busySpots.Remove(_busySpots[i]);
                break;
            }

        }

    }

    private Transform PosInStation()
    {
        Transform pos = null;

        for (int i = 0; i < _chargingSpots.Count; i++)
        {
            pos = _chargingSpots[0];
            _busySpots.Add(pos);
            _chargingSpots.Remove(pos);
            break;
        }

        return pos;
    }

    protected bool HasCapacity()
    {
        return _currentChargeCount < _capacity;
    }
    private void SetData(int currentLevel)
    {

        for (int i = 0; i < _chargingStationData.Count; i++)
        {
            if (_chargingStationData[i].Level == currentLevel)
            {
                _currentChargingStationData = _chargingStationData[i];
                break;
            }
        }

        _capacity = _currentChargingStationData.DroneCapacity;
        _chargingSpeed = _currentChargingStationData.ChargingSpeed;

        SetAvailableChargingSpots(_capacity);
    }
    private void InstantiateUIAboveBuilding()
    {
        Instantiate(_uiElementAboveBuildingPrefab, _uiElementPositionOnBuilding.position, Quaternion.identity);

    }

    private void SetAvailableChargingSpots(int capacity)
    {
        int spots = capacity - (_chargingSpots.Count + _busySpots.Count);

        for (int i = 0; i < spots; i++)
        {
            _spotsToAddToChargingSpots[i].gameObject.SetActive(true);
            _chargingSpots.Add(_spotsToAddToChargingSpots[i]);
        }

        _spotsToAddToChargingSpots.RemoveAll(spot => _chargingSpots.Contains(spot));
    }
}
