using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerResult : MonoBehaviour
{
    public List<GameObject> scannedImages = new List<GameObject>();
    public List<GameObject> correctImages = new List<GameObject>();
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
         
            GameObject[] scannedObjects = GameObject.FindGameObjectsWithTag("Scanned");
            foreach (var item in scannedObjects)
            {
                if (!scannedImages.Contains(item))
                {
                    scannedImages.Add(item);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            bool correctAnswer = scannedImages.Count == correctImages.Count && !scannedImages.Except(correctImages).Any() && !correctImages.Except(scannedImages).Any();

            if (correctAnswer == true) 
            {
                Debug.Log("splendid");
            }
            else
            {
                Debug.Log("Not Splendid");
            }
        }
    }
}
