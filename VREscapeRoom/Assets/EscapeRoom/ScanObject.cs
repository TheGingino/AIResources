
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;
    
    [SerializeField] private GameObject scanEffect;
    
    private void Start()
    {
        if (scanner == null)
        {
            scanner = FindObjectOfType<Scanner>();
        }
    }

    /// <summary>
    /// Casts a ray forward when E is pressed and scans the first object with ScannableObject component.
    /// </summary>
     void Update()
     {
         if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || Input.GetKeyDown(KeyCode.E))
         {
             ScanTarget();
         }

         if (Input.GetKeyDown(KeyCode.S) && targetObj != null)
         {
             Debug.Log("Hellllooo");
             targetObj.gameObject.tag = "Scanned";
             Debug.Log($"Tagged {targetObj.gameObject.name} as {targetObj.gameObject.tag}");
         }
    }

    private void SetCurrentTarget(ScannableObject target)
    {
        targetObj = target;
    }

    public void ScanTarget()
    {
        //if (!Input.GetKeyDown(KeyCode.E)) return;
        Debug.Log("ScanTarget called");
        
        if (scanEffect != null)
        { 
            var effect = Instantiate(scanEffect, transform.position, Quaternion.identity); 
            Destroy(effect, 2f); // Destroy the effect after 2 seconds
        }
        
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 100f)) return;
        
        Debug.Log($"Raycast hit: {hit.transform.name}");
        var scannable = hit.transform.GetComponent<ScannableObject>() ?? hit.transform.GetComponentInParent<ScannableObject>();

        if (scannable == null) return;

        SetCurrentTarget(scannable);
        Debug.Log("E pressed");

        scanner.ScanAsync(targetObj);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 10f);
    }
}

