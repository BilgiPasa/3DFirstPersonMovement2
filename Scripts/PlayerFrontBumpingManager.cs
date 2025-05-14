using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class PlayerFrontBumpingManager : MonoBehaviour
{
    //* In Unity Editor, layer 3 should be "Normal Layer".
    //* In Unity Editor, layer 6 should be "Bouncy Layer".

    public static bool frontBumping;
    BoxCollider frontBumpingDetectorBoxCollider;

    void Start()
    {
        frontBumpingDetectorBoxCollider = GetComponent<BoxCollider>();
        frontBumpingDetectorBoxCollider.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6 || other.gameObject.layer == 7)
        {
            frontBumping = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3 || other.gameObject.layer == 6 || other.gameObject.layer == 7)
        {
            frontBumping = false;
        }
    }
}
