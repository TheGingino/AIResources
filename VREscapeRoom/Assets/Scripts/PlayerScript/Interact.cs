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

    [SerializeField]private PlayerMovement _playerMovement;
    [SerializeField]private PlayerCameraScript _playerCameraScript;
    [SerializeField]private CharacterController character;
    [SerializeField]private Rigidbody _rigidbody;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Chair")) return;
        
        Debug.Log("Interacted with " + other.name);
        isInteractable = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Chair")) return;
        
        Debug.Log("Stopped interacting with " + other.name);
        isInteractable = false;
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.E) && isInteractable)
        {
            Debug.Log("Sitting down");
            //cam.targetTexture = rt;
            Cursor.lockState = CursorLockMode.None;
            _playerMovement.enabled = false;
            _playerCameraScript.enabled = false;
            character.enabled = false;
            _rigidbody.isKinematic = true;

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _playerMovement.enabled = true;
            _playerCameraScript.enabled = true;
            character.enabled = true;
            _rigidbody.isKinematic = false;
        }
    }
}


