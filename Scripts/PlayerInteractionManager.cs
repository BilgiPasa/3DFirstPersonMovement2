using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* Make sure that movable objects have a Rigidbody.

    [Header("Holding and Throwing")]
    public static bool canReleaseHoldedObjectWhenTouchedToPlayer;
    public static Rigidbody grabbedObjectRigidbody;
    const int holdForce = 30;
    const float holdAgainCooldown = 0.6f, canReleaseHoldedObjectWhenTouchedToPlayerCooldown = 0.3f, movingHoldingObjectWithScrollWheelSpeed = 7.5f;
    float tempHoldingObjectDistance;
    bool readyToHold = true, interacionKeyPressed, throwKeyPressedWhileHoldingAnObject;
    RaycastHit holdInteractionHit;

    [Header("Keybinds")]
    KeyCode interactionKey = KeyCode.E, throwKey = KeyCode.Mouse0;

    [Header("Inputs")]
    [SerializeField] int throwForce = 60;
    [SerializeField] int maxHoldingObjectCanBeOffsetDistance = 10;
    [SerializeField] int normalHoldingObjectDistance = 4;
    [SerializeField] float maxHoldingObjectDistance = 6, minHoldingObjectDistance = 2.5f;
    [SerializeField] Transform holdedObjectPositionTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask movableNormalLayer, movableBouncyLayer;

    void Start()
    {
        tempHoldingObjectDistance = normalHoldingObjectDistance;
    }

    void Update()
    {
        if (!PauseMenuManager.gamePaused)
        {
            InteractionInputs();
        }
    }

    void FixedUpdate()
    {
        HoldingAndThrowingObject();
    }

    void InteractionInputs()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            interacionKeyPressed = true;
        }

        if (Input.GetKeyDown(throwKey) && grabbedObjectRigidbody)
        {
            throwKeyPressedWhileHoldingAnObject = true;
        }
    }

    void HoldingAndThrowingObject()
    {
        if (grabbedObjectRigidbody)
        {
            grabbedObjectRigidbody.AddForce(holdForce * (holdedObjectPositionTransform.position - grabbedObjectRigidbody.position), ForceMode.Impulse);
            grabbedObjectRigidbody.linearVelocity *= 0.25f;
            grabbedObjectRigidbody.angularVelocity *= 0.25f;

            if (throwKeyPressedWhileHoldingAnObject)
            {
                throwKeyPressedWhileHoldingAnObject = false;
                grabbedObjectRigidbody.AddForce(throwForce * mainCamera.transform.forward, ForceMode.Impulse);
                ReleaseObject();
                return;
            }

            if ((holdedObjectPositionTransform.position - grabbedObjectRigidbody.position).magnitude > maxHoldingObjectCanBeOffsetDistance)
            {
                ReleaseObject();
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                if (tempHoldingObjectDistance >= maxHoldingObjectDistance && Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    tempHoldingObjectDistance = maxHoldingObjectDistance;
                }
                else if (tempHoldingObjectDistance <= minHoldingObjectDistance && Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    tempHoldingObjectDistance = minHoldingObjectDistance;
                }
                else
                {
                    tempHoldingObjectDistance += movingHoldingObjectWithScrollWheelSpeed * Input.GetAxis("Mouse ScrollWheel");
                }
            }

            holdedObjectPositionTransform.localPosition = new Vector3(0, 0, tempHoldingObjectDistance);
        }

        if (interacionKeyPressed)
        {
            interacionKeyPressed = false;

            if (grabbedObjectRigidbody)
            {
                ReleaseObject();
                return;
            }

            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out holdInteractionHit, maxHoldingObjectDistance, movableNormalLayer | movableBouncyLayer) && readyToHold && !holdInteractionHit.rigidbody.isKinematic)
            {
                readyToHold = false;
                grabbedObjectRigidbody = holdInteractionHit.rigidbody;

                if (grabbedObjectRigidbody)
                {
                    grabbedObjectRigidbody.linearVelocity = Vector3.zero;
                    grabbedObjectRigidbody.useGravity = false;
                    Invoke(nameof(HoldAgainReset), holdAgainCooldown);
                    Invoke(nameof(CanReleaseHoldedObjectWhenTouchedToPlayerActivator), canReleaseHoldedObjectWhenTouchedToPlayerCooldown);
                }
            }
        }
    }

    public void ReleaseObject()
    {
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
        canReleaseHoldedObjectWhenTouchedToPlayer = false;
    }

    void HoldAgainReset()
    {
        readyToHold = true;
    }

    void CanReleaseHoldedObjectWhenTouchedToPlayerActivator()
    {
        canReleaseHoldedObjectWhenTouchedToPlayer = true;
    }
}
