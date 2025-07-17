using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    //* Attach this script to the Player gameobject.
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* Make sure that movable objects have a Rigidbody.

    [Header("Holding and Throwing")]
    public static Rigidbody grabbedObjectRigidbody;
    const int holdForce = 30;
    const float holdAgainCooldown = 0.6f;
    bool readyToHold = true, interacionKeyPressed, throwKeyPressedWhileHoldingAnObject;
    RaycastHit holdInteractionHit;

    [Header("Keybinds")]
    KeyCode interactionKey = KeyCode.E, throwKey = KeyCode.Mouse0;

    [Header("Inputs")]
    [SerializeField] int maxHoldingObjectDistance = 8;
    [SerializeField] int throwForce = 60;
    [SerializeField] Transform holdedObjectPositionTransform;
    [SerializeField] Transform cameraHolderTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask movableNormalLayer, movableBouncyLayer;

    void Update()
    {
        InteractionInputs();
    }

    void FixedUpdate()
    {
        HoldingAndThrowingObject();
    }

    void InteractionInputs()
    {
        if (PauseMenuManager.gamePaused)
        {
            return;
        }

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

            if ((holdedObjectPositionTransform.position - grabbedObjectRigidbody.position).magnitude > maxHoldingObjectDistance)
            {
                ReleaseObject();
            }
        }

        if (interacionKeyPressed)
        {
            interacionKeyPressed = false;

            if (grabbedObjectRigidbody)
            {
                ReleaseObject();
                return;
            }

            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out holdInteractionHit, maxHoldingObjectDistance, movableNormalLayer | movableBouncyLayer) && readyToHold)
            {
                readyToHold = false;
                grabbedObjectRigidbody = holdInteractionHit.rigidbody;

                if (grabbedObjectRigidbody)
                {
                    grabbedObjectRigidbody.linearVelocity = Vector3.zero;
                    grabbedObjectRigidbody.useGravity = false;
                }

                Invoke(nameof(HoldAgainReset), holdAgainCooldown);
            }
        }
    }

    public void ReleaseObject()
    {
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectRigidbody = null;
    }

    void HoldAgainReset()
    {
        readyToHold = true;
    }
}
