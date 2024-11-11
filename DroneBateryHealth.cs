using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBateryHealth : MonoBehaviour
{
    [SerializeField] float _maxBateryLife = 100;
    [SerializeField] float _bateryLife;
    [SerializeField] float _batteryConsumptionRate = 1;
    [SerializeField] float _lowBatteryThreshold = 20;
    [SerializeField] private bool _hasBoost;
    [SerializeField] private float _boostAmount;
    [SerializeField] private float _boostCoolDown;
    [SerializeField] private List<DroneBatteryData> _droneBatteryDatas = new List<DroneBatteryData>();

    public event Action LowBatery;
    public event Action BateryDead;
    public event Action ChargingBateryComplete;

    private int _currentBateryLevel = 1;
    private float _nextBoostTime = 0;
   [SerializeField] private float _chargingRate;
    private bool _droneIsMoving;
   [SerializeField] private bool _isCharging;
    [SerializeField] private bool _batteryPowerUpActivated;

    public float MaxBateryLife => _maxBateryLife;
    public float BateryLife => _bateryLife;

    // Start is called before the first frame update
    void Start()
    {
        SetBateryData(_currentBateryLevel);
    }

    // Update is called once per frame
    void Update()
    {
        BatteryLife();
    }

    protected virtual void BatteryLife()
    {
        if(_isCharging)
        {
            _bateryLife += _chargingRate * Time.deltaTime;
            if (_bateryLife > _maxBateryLife)
            {
                _bateryLife = _maxBateryLife;
                ChargingBateryComplete?.Invoke();
                StopCharging();
            }
        }
        else if(_batteryPowerUpActivated)
        {
            return;
        }
        else if (_droneIsMoving)
        {

            _bateryLife -= _batteryConsumptionRate * Time.deltaTime;
            if (_bateryLife < _lowBatteryThreshold)
            {
                LowBatery?.Invoke();
            }

            if (_bateryLife <= 0)
            {
                _bateryLife = 0;
                TryBoostBattery();
            }
        }
    }

    public void SetBateryData(int currentLevel)
    {
        DroneBatteryData droneBateryData = new DroneBatteryData();

        for (int i = 0; i < _droneBatteryDatas.Count; i++)
        {
            if (_droneBatteryDatas[i].Level == currentLevel)
            {
                droneBateryData = _droneBatteryDatas[i];
                break;
            }
        }

        _maxBateryLife = droneBateryData.MaxBatteryLife;
        _bateryLife = _maxBateryLife;
        _batteryConsumptionRate = droneBateryData.BatteryConsumptionRate;
        _lowBatteryThreshold = droneBateryData.LowBatteryThreshold;
        _hasBoost = droneBateryData.HasBoost;
        _boostAmount = droneBateryData.BoostAmount;
        _boostCoolDown = droneBateryData.BoostCoolDown;

    }

    private void TryBoostBattery()
    {
        if (_hasBoost && Time.time >= _nextBoostTime)
        {
            _bateryLife += _boostAmount;
            _nextBoostTime = Time.time + _boostCoolDown;
        }
        else
        {
            BateryDead?.Invoke();
        }
    }

    public void DroneIsMoving(bool moving)
    {
        _droneIsMoving = moving;
    }
    public void SetChargingTimeForBatery(float chargingRate)
    {
        _chargingRate = chargingRate;
    }

    public void StartCharging()
    {
        _isCharging = true;
    }

    public void StopCharging()
    {
        _isCharging = false;
    }

    public void IsBatteryPowerUpActivated(bool isBatteryPowerUpActivated)
    {
        _batteryPowerUpActivated = isBatteryPowerUpActivated;
    }
}
