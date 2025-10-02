
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ScanObject : MonoBehaviour
{
    [SerializeField] private Scanner scanner;
    [SerializeField] private ScannableObject targetObj;

    /// <summary>
    /// Casts a ray forward when E is pressed and scans the first object with ScannableObject component.
    /// </summary>
    async void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 10f)) return;

        var scannable = hit.transform.GetComponent<ScannableObject>()
                        ?? hit.transform.GetComponentInParent<ScannableObject>();
        if (scannable == null) return;

        SetCurrentTarget(scannable);
        await scanner.ScanAsync(targetObj);
    }

    private void SetCurrentTarget(ScannableObject target)
    {
        targetObj = target;

    }
}
