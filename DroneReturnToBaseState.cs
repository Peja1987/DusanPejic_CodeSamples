using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneReturnToBaseState : IDroneState
{
    private Drone _drone;
    private Transform _baseLocation;
    private float _checkTime = 1.5f;
    private float _resetCheckTime;
    private NavMeshAgent _agent;
    private float _delayForNotifyRepairDrone;

    private const float DELAY_FOR_NOTIFY_REPAIR_DRONE = 0.5f;

    public void EnterState(Drone drone)
    {
        _delayForNotifyRepairDrone = DELAY_FOR_NOTIFY_REPAIR_DRONE;
        Debug.Log("drone return to base state");
        _resetCheckTime = _checkTime;
        _drone = drone;
        _baseLocation = drone.BaseLocation;
        _agent = drone.Agent;
        drone.SetDroneDestination(_baseLocation);
    }

    public void ExitState()
    {
    
    }

    public void UpdateState()
    {

        _checkTime -= Time.deltaTime;
        if (_checkTime <= 0)
        {
            _checkTime = _resetCheckTime;
            if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude == 0)
            {
                _drone.DroneStateMachine.SetState(new DroneIdleBaseState(), _drone);
                _drone.NotifyDroneAvailableAgain();
            }
            else if (_drone.HasLowBattery && _drone.BatteryIsDead == false && _drone.IsRepairDrone == false)
            {
                bool hasFreeChargingStation = ChargingStationsManager.Instance.CheckIfThereIsFreeChargingStation(_drone);
                if (hasFreeChargingStation)
                {
                    _drone.DroneStateMachine.SetState(new DroneChargingStationState(), _drone);
                }
            }
            else if (_drone.BatteryIsDead && _drone.IsRepairDrone == false)
            {
                bool hasFreeReparingDrone = DronesForRepairManager.Instance.CheckIfThereIsFreeChargingDrone(_drone);
                if (hasFreeReparingDrone)
                {
                    _drone.DroneStateMachine.SetState(new DroneWaitingForReparingState(), _drone);
                }
             
            }
        }

        if(_drone.IsRepairDrone)
        {
            _delayForNotifyRepairDrone -= Time.deltaTime;
            if(_delayForNotifyRepairDrone <= 0 && _drone.IsAvailable == false)
            {
                _drone.NotifyDroneAvailableAgain();
            }
        }

    }

    
}
