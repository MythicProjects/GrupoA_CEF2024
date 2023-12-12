using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RobotData/New robot")]    
public class RobotData : ScriptableObject
{
    public string robotType;
    public GameObject robotPrefab;
}
