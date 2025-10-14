
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;
    
    [SerializeField] private GameObject scanEffect;

    /// <summary>
    /// Casts a ray forward when E is pressed and scans the first object with ScannableObject component.
    /// </summary>
     void Update()
     {
         ScanTarget();
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

    private void ScanTarget()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

         if (scanEffect != null)
         {
             var effect = Instantiate(scanEffect, transform.position, Quaternion.identity);
             Destroy(effect, 2f); // Destroy the effect after 2 seconds
         }
        
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 10f)) return;
        
        var scannable = hit.transform.GetComponent<ScannableObject>()
                        ?? hit.transform.GetComponentInParent<ScannableObject>();
        
        if (scannable == null) return;

        SetCurrentTarget(scannable);
        Debug.Log("E pressed");

        scanner.ScanAsync(targetObj);
    }
}

