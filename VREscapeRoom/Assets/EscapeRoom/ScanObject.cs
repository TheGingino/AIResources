
using System;
using Oculus.Interaction;
using UnityEngine;


public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;
    
    [SerializeField] private GameObject scanEffect;
    private ScannableObject currentScanned;
    
    private ScanResult scanResult;
    
    bool isGrabbed;

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
         if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && isGrabbed || Input.GetKeyDown(KeyCode.E))
         {
             ScanTarget();
         }
         if (OVRInput.GetDown(OVRInput.Button.One))
         {
             FlagTarget();
         }
         Debug.Log("isGrabbed:" + isGrabbed);
         if (OVRInput.GetUp( OVRInput.Button.SecondaryHandTrigger))
         {
             isGrabbed = false;
         }
    }

    private void SetCurrentTarget(ScannableObject target)
    {
        targetObj = target;
    }

    public async void ScanTarget()
    {
        Debug.Log("ScanTarget called");
        //if (grabbed == null) return;

        if (scanEffect != null)
        { 
            var effect = Instantiate(scanEffect, transform.position, Quaternion.identity); 
            Destroy(effect, 2f); // Destroy the effect after 2 seconds
        }
        
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 100f)) return;
        
        currentScanned = hit.transform.GetComponent<ScannableObject>() ?? hit.transform.GetComponentInParent<ScannableObject>();
        Debug.Log($"Raycast hit: {hit.transform.name}");

        if (currentScanned == null) return;

        SetCurrentTarget(currentScanned);
        Debug.Log("E pressed");

        //await scanner.ScanAsync(targetObj);
        scanner.OnObjectScanned(targetObj);
    }

    public void FlagTarget()
    {
       FillBar fillBar = currentScanned.GetComponent<FillBar>();
        fillBar.AddData();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            isGrabbed = true;
            Debug.Log("Object grabbed");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            isGrabbed = false;
            Debug.Log("Object released");
        }
    }
}

