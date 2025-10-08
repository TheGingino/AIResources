using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar : MonoBehaviour
{
    public float BiasProgress, AccuracyProgress;


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            AddData();
        }
    }


    public void AddData()
    {
        GameManager.Instance.BiasValue += BiasProgress;
        GameManager.Instance.AccuracyValue += AccuracyProgress;
        gameObject.SetActive(false);
    }
}
