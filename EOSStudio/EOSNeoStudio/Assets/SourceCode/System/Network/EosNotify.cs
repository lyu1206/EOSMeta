using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EosPropertyNotify
{
    LocalPosition = 1000,
    LocalRotation = 1002,
    LocalScale = 1003,
    WorldPosition = 1004,
    WorldRotation = 1005,
    HumanoidID = 1006,
    HumanoidUpDirection = 1007,
    HumanoidMoveDirection = 1008,
    HumanoidJump = 1009,
    HumanoidActionLayer = 1010,
    HumanoidBehavior = 1011,
}

public enum EosObjectAction
{
    StateChange = 1,
    PlayNode = 2,
    
    
    // system action
    HumanoidCollided,
    Landing,
}