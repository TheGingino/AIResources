using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    public TextMeshProUGUI countdown;
    public float timeleft;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        timeleft -= Time.deltaTime;
        countdown.text = ((int)timeleft).ToString();

    }
}
