using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Range(30, 120)]
    public int frameRate = 60;
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}
