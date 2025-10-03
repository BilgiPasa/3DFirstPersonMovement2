using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class PlayerFrontBumpingManager : MonoBehaviour
{
    //* Attach this script to the FrontBumpingDetector game object.
    //* In Unity Editor, layer 3 should be "Static Normal Layer".
    //* In Unity Editor, layer 6 should be "Static Bouncy Layer".
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".

    bool frontBumping; // This value has getter and setter.
    BoxCollider frontBumpingDetectorBoxCollider;

    public bool FrontBumping
    {
        get => frontBumping;
        set { frontBumping = value; }
    }

    void Start()
    {
        frontBumpingDetectorBoxCollider = GetComponent<BoxCollider>();
        frontBumpingDetectorBoxCollider.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6 || other.gameObject.layer == 7 || other.gameObject.layer == 8)
        {
            frontBumping = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6 || other.gameObject.layer == 7 || other.gameObject.layer == 8)
        {
            frontBumping = false;
        }
    }
}
