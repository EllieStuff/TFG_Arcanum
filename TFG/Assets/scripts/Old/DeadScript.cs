using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadScript : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0.0000001f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

}