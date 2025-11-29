using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class PlayerFrontBumpingMng : MonoBehaviour
{
    //* Attach this script to the FrontBumpingDetector game object.
    //* In Unity Editor, layer 3 should be "Static Normal Layer".
    //* In Unity Editor, layer 6 should be "Static Bouncy Layer".
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".

    bool frontBumping;
    BoxCollider frontBumpingDetectorBoxColl;

    public bool FrontBumping
    {
        get => frontBumping;
    }

    void Start()
    {
        frontBumpingDetectorBoxColl = GetComponent<BoxCollider>();
        frontBumpingDetectorBoxColl.isTrigger = true;
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
