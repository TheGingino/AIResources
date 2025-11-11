using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrab : MonoBehaviour
{
    public Transform handTransform;                     // optional: specific hand transform to attach to
    public KeyCode grabKey = KeyCode.E;                       // keyboard grab key
    [SerializeField] private OVRInput.Button ovrGrabButton = OVRInput.Button.SecondaryHandTrigger; // Oculus grab button
    [SerializeField] private bool isGrabbed;
    private Collider nearbyHand;                               // collider currently in trigger (hand candidate)
    private Rigidbody rb;
    private Transform originalParent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Release when grabbed and key pressed
        if (isGrabbed && OVRInput.GetDown(ovrGrabButton) || Input.GetKeyDown(grabKey))
        {
            Release();
            return;
        }

        // Grab when not grabbed, a hand is nearby and key pressed
        if (!isGrabbed && nearbyHand != null && OVRInput.GetDown(ovrGrabButton) || Input.GetKeyDown(grabKey))
        {
            Grab();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            nearbyHand = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (nearbyHand == other)
        {
            nearbyHand = null;
        }
    }

    private void Grab()
    {
        // choose the target parent: inspector handTransform has priority, otherwise use nearby hand transform
        Transform targetHand = handTransform != null ? handTransform : (nearbyHand != null ? nearbyHand.transform : null);
        if (targetHand == null) return;

        originalParent = transform.parent;
        transform.SetParent(targetHand);
        // snap to hand
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isGrabbed = true;
        if (rb != null) rb.isKinematic = true;
    }

    private void Release()
    {
        // unparent back to original parent (or to null if none)
        transform.SetParent(originalParent, true);

        isGrabbed = false;
        if (rb != null) rb.isKinematic = false;
    }
}
