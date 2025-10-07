
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
    async void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        
        scanEffect.SetActive(true);
        await Task.Delay(5000);
        scanEffect.SetActive(false);
        
        RaycastHit hit;
        
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 10f)) return;
        Debug.Log("Mashallah"); 

        var scannable = hit.transform.GetComponent<ScannableObject>()
                        ?? hit.transform.GetComponentInParent<ScannableObject>();
        
        if (scannable == null) return;

        SetCurrentTarget(scannable);
        await scanner.ScanAsync(targetObj);
        if (Input.GetKeyDown(KeyCode.S))
        {
            targetObj.gameObject.tag = "Scanned";
        }
    }

    private void SetCurrentTarget(ScannableObject target)
    {
        targetObj = target;

    }
}

