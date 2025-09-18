using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Interact : MonoBehaviour
{

    private bool isInteractable;
    public RenderTexture rt;
    private Camera cam;
    private PlayerCameraScript pcs;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chair"))
        {
            Debug.Log("Interacted with " + other.name);
            isInteractable = true;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chair"))
        {
            Debug.Log("Stopped interacting with " + other.name);
            isInteractable = false;
        }
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.E) && isInteractable)
        {
            Debug.Log("Sitting down");
            cam.targetTexture = rt;
            Cursor.lockState = CursorLockMode.None;
        }

    }
}


