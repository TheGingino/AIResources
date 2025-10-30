
using System;
using Oculus.Interaction;
using UnityEngine;


public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;
    
    [SerializeField] private GameObject scanEffect;
    
    [SerializeField] private OVRGrabber rightGrabber;       // assign your RIGHT hand OVRGrabber in Inspector

    private Rigidbody rb;

    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (scanner == null)
        {
            scanner = FindObjectOfType<Scanner>();
        }
        if (rightGrabber == null)
        {
            // Try to auto-find a right-hand grabber in scene
            var grabbers = FindObjectsOfType<OVRGrabber>();
            foreach (var g in grabbers)
            {
                // Heuristic: right controllers often include "Right" in the GO name
                if (g.name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rightGrabber = g;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Casts a ray forward when E is pressed and scans the first object with ScannableObject component.
    /// </summary>
     void Update()
     {
         float rt = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
         if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && Mathf.Approximately(rt, 1)|| Input.GetKeyDown(KeyCode.E))
         {
             ScanTarget();
         }

         if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)&& rb.isKinematic)
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
        Debug.Log("ScanTarget called");
        var grabbed = rightGrabber ? rightGrabber.grabbedObject : null;
        //if (grabbed == null) return;

        if (scanEffect != null)
        { 
            var effect = Instantiate(scanEffect, transform.position, Quaternion.identity); 
            Destroy(effect, 2f); // Destroy the effect after 2 seconds
        }
        
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 100f)) return;
        
        var scannable = hit.transform.GetComponent<ScannableObject>() ?? hit.transform.GetComponentInParent<ScannableObject>();
        Debug.Log($"Raycast hit: {hit.transform.name}");

        if (scannable == null) return;

        SetCurrentTarget(scannable);
        Debug.Log("E pressed");

        scanner.ScanAsync(targetObj);
    }
    
}

