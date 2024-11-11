using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneStateMachine
{
    private IDroneState _currentState;

    public void SetState(IDroneState newState, Drone drone)
    {
        _currentState?.ExitState();
        _currentState = newState;
        _currentState.EnterState(drone);
    }

    public void Update()
    {
        _currentState?.UpdateState();
    }
}
