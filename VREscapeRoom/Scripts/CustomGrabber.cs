using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CustomGrabber : MonoBehaviour
{
    // Inspector config
    public Transform handTransform;               // where grabbed objects become children (assign to hand)
    public KeyCode grabKey = KeyCode.E;           // keyboard fallback
    public bool useXRInput = true;                // enable XR input checks
    public bool useGripButton = true;             // if true - use grip, otherwise try trigger
    public string grabbableTag = "";              // optional: only accept objects with this tag when non-empty

    // runtime
    private Grabbable nearbyCandidate;
    private Grabbable grabbed;
    private Rigidbody grabbedRb;
    private List<Collider> grabbedColliders = new List<Collider>();
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;

    // For XR device lookup (simple, checks right/left controllers)
    private List<InputDevice> xrDevices = new List<InputDevice>();

    void Start()
    {
        if (handTransform == null) handTransform = transform;
        if (useXRInput) RefreshXRDevices();
    }

    void RefreshXRDevices()
    {
        xrDevices.Clear();
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand
                                     | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, xrDevices);
    }

    void OnEnable() => InputDevices.deviceConnected += OnDeviceConnected;
    void OnDisable() => InputDevices.deviceConnected -= OnDeviceConnected;
    void OnDeviceConnected(InputDevice device) => RefreshXRDevices();

    void OnTriggerEnter(Collider other)
    {
        var g = other.GetComponentInParent<Grabbable>();
        if (g == null) return;
        if (!string.IsNullOrEmpty(grabbableTag) && !other.CompareTag(grabbableTag)) return;
        nearbyCandidate = g;
    }

    void OnTriggerExit(Collider other)
    {
        var g = other.GetComponentInParent<Grabbable>();
        if (g == null) return;
        if (nearbyCandidate == g) nearbyCandidate = null;
    }

    void Update()
    {
        if (grabbed == null)
        {
            if (nearbyCandidate != null && (GetGrabPressedDown()))
            {
                DoGrab(nearbyCandidate);
            }
        }
        else
        {
            if (GetGrabReleased())
            {
                DoRelease();
            }
        }
    }

    bool GetGrabPressedDown()
    {
        if (Input.GetKeyDown(grabKey)) return true;
        if (!useXRInput) return false;

        foreach (var dev in xrDevices)
        {
            bool val;
            if (useGripButton)
            {
                if (dev.TryGetFeatureValue(CommonUsages.gripButton, out val) && val) return true;
            }
            else
            {
                if (dev.TryGetFeatureValue(CommonUsages.triggerButton, out val) && val) return true;
            }
        }
        return false;
    }

    bool GetGrabReleased()
    {
        if (Input.GetKeyUp(grabKey)) return true;
        if (!useXRInput) return false;

        foreach (var dev in xrDevices)
        {
            bool val;
            if (useGripButton)
            {
                // consider release when none of controllers report grip pressed
                if (dev.TryGetFeatureValue(CommonUsages.gripButton, out val) && !val) continue;
                if (dev.TryGetFeatureValue(CommonUsages.gripButton, out val) && val) return false;
            }
            else
            {
                if (dev.TryGetFeatureValue(CommonUsages.triggerButton, out val) && !val) continue;
                if (dev.TryGetFeatureValue(CommonUsages.triggerButton, out val) && val) return false;
            }
        }

        // If using XR, treat release when keyboard released as alternative:
        return Input.GetKeyUp(grabKey);
    }

    void DoGrab(Grabbable g)
    {
        if (g == null) return;
        grabbed = g;
        grabbedRb = grabbed.GetComponent<Rigidbody>();
        if (grabbedRb != null) grabbedRb.isKinematic = true;

        // cache colliders and disable to avoid physics bumps
        grabbedColliders.Clear();
        foreach (var col in grabbed.GetComponentsInChildren<Collider>())
        {
            grabbedColliders.Add(col);
            col.enabled = false;
        }

        // parent and snap to attachPoint if provided
        if (grabbed.attachPoint != null)
        {
            // preserve attachPoint local offset relative to object
            // we want attachPoint to match handTransform
            grabbed.transform.SetParent(handTransform, true);
            // compute desired local transform so attachPoint ends up at handTransform
            var worldPos = grabbed.attachPoint.position;
            var worldRot = grabbed.attachPoint.rotation;
            grabbed.transform.SetParent(null, true); // temporarily unparent to calculate
            grabbed.transform.position = grabbed.transform.position; // no-op but explicit
            // parent to hand
            grabbed.transform.SetParent(handTransform, true);
            // now shift object so attachPoint aligns with handTransform
            var attachLocalToObj = grabbed.attachPoint.localPosition;
            var attachLocalRot = grabbed.attachPoint.localRotation;
            // direct snap: set grabbed localPosition so that attachPoint is zeroed
            var deltaPos = grabbed.attachPoint.localPosition;
            var deltaRot = grabbed.attachPoint.localRotation;
            // simplest: set grabbed localPosition to negative attachPoint localPosition so attachPoint sits at hand
            grabbed.transform.localPosition -= deltaPos;
            grabbed.transform.localRotation = Quaternion.Inverse(deltaRot) * grabbed.transform.localRotation;
        }
        else
        {
            // preserve offset between object and hand
            savedLocalPosition = handTransform.InverseTransformPoint(grabbed.transform.position);
            savedLocalRotation = Quaternion.Inverse(handTransform.rotation) * grabbed.transform.rotation;
            grabbed.transform.SetParent(handTransform, true);
            grabbed.transform.localPosition = savedLocalPosition;
            grabbed.transform.localRotation = savedLocalRotation;
        }
    }

    void DoRelease()
    {
        if (grabbed == null) return;

        // unparent
        grabbed.transform.SetParent(null, true);

        // restore colliders
        foreach (var col in grabbedColliders)
        {
            if (col != null) col.enabled = true;
        }
        grabbedColliders.Clear();

        // restore physics
        if (grabbedRb != null)
        {
            grabbedRb.isKinematic = false;
            // optional: apply small throw velocity from hand motion if needed (not implemented)
        }

        grabbed = null;
        grabbedRb = null;
    }
}
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    // Optional transform that defines how the object should sit in the hand.
    // If null, the current world-to-hand offset is preserved on grab.
    public Transform attachPoint;

    // ...existing code...
}

