using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneGoToDeliveryState : IDroneState
{
    private Transform _deliveryPostion;
    private Drone _drone;
    private NavMeshAgent _agent;
    private float _checkTime = 0.5f;
    private float _resetCheckTime;
    private float _currentYPos;

    public void EnterState(Drone drone)
    {
        _resetCheckTime = _checkTime;
        _drone = drone;
        _deliveryPostion = drone.CurrentOrderDestination;
        _agent = drone.Agent;
        UnityEngine.Debug.Log("drone go to delivery state");
        drone.SetDroneDestination(_deliveryPostion);
        drone.CrateOrder.SetActive(true);

        
    }

    public void ExitState()
    {
        if (_drone.BatteryIsDead)
        {
            if (_drone.transform.position.y < _currentYPos)
            {
                Vector3 position = _drone.transform.position;
                position.y = _currentYPos; // Resetuj Y poziciju na current
                _drone.transform.position = position;
            }
        }
    }

    public void UpdateState()
    {
        _checkTime -= Time.deltaTime;
        if (_checkTime <= 0)
        {
            _checkTime = _resetCheckTime;
            if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance && _agent.velocity.sqrMagnitude == 0)
            {
                _drone.DroneStateMachine.SetState(new DroneDeliveringPackageState(), _drone);
            }
            else if (_drone.HasLowBattery && _drone.BatteryIsDead == false)
            {
                bool hasFreeChargingStation = ChargingStationsManager.Instance.CheckIfThereIsFreeChargingStation(_drone);
                if (hasFreeChargingStation)
                {
                    _drone.DroneStateMachine.SetState(new DroneChargingStationState(), _drone);
                }
            }
            else if(_drone.BatteryIsDead)
            {
                _currentYPos = _drone.transform.position.y;

                bool hasFreeReparingDrone = DronesForRepairManager.Instance.CheckIfThereIsFreeChargingDrone(_drone);
                if(hasFreeReparingDrone)
                {
                    _drone.DroneStateMachine.SetState(new DroneWaitingForReparingState(), _drone);
                }
            }

        }
    }

}
