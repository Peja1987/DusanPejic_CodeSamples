using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class DroneRepair: Drone
{
    [SerializeField] private Animator _animator;

    public override void Start()
    {
        IsAvailable = true;
        _animator = GetComponent<Animator>();
        //Agent = GetComponent<NavMeshAgent>();
        SetDroneData(CurrentDroneLevel);
        DronesForRepairManager.Instance.RegisterRepairingDrones(this);

        DroneStateMachine droneStateMachine = new DroneStateMachine();
        droneStateMachine.SetState(new DroneIdleBaseState(), this);

        DroneStateMachine = droneStateMachine;

        EventManager.UpgradeDrone += OnUpgradeDrone;
    }

    public override void Update()
    {
        DroneStateMachine.Update();
    }
    public override void OnDestroy()
    {
        EventManager.UpgradeDrone -= OnUpgradeDrone;
    }
    public override void AssignDroneToRepair(Drone drone)
    {
        DroneToRepair = drone;
        DroneStateMachine.SetState(new DroneGoToRepairDroneState(), this);
        drone.SetReapirDroneAndChargingTime(this, DroneData.ChargingSpeed);
        _animator.SetBool("LightsOn", true);
    }

    public override void RemoveDroneFromRepairing()
    {
        DroneToRepair = null;
        _animator.SetBool("LightsOn", false);
    }
    public override void OnUpgradeDrone(DroneData upgradedDroneData, DroneBatteryData upgradedDroneBatteryData, int baseIndex, int baseSlotIndex, Button button, bool batteryUpgrade)
    {
        base.OnUpgradeDrone(upgradedDroneData, upgradedDroneBatteryData, baseIndex, baseSlotIndex, button, batteryUpgrade);
    }

}

