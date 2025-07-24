using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionManager : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* Make sure that movable objects have a Rigidbody.

    [Header("Holding and Throwing")]
    [HideInInspector] public bool canReleaseHoldedObjectWhenTouchedToPlayer;
    [HideInInspector] public Rigidbody grabbedObjectRigidbody;
    const int holdForce = 30;
    const float grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier = 0.3f, movingHoldingObjectWithScrollWheelSpeed = 7.5f, canReleaseHoldedObjectWhenTouchedToPlayerCooldown = 0.3f, holdAgainCooldown = 0.6f, crosshairBeingRedTime = 0.2f;
    float tempHoldingObjectDistance;
    bool readyToHold = true, interacionKeyPressed, throwKeyPressedWhileHoldingAnObject;
    Transform grabbedObjectTransform;
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
    [SerializeField] Image crosshairImage;
    [SerializeField] PauseMenuManager pauseMenuManagerScript;

    void Start()
    {
        crosshairImage.color = Color.black;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
    }

    void Update()
    {
        if (!pauseMenuManagerScript.gamePaused)
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
            grabbedObjectRigidbody.AddForce(holdForce * (holdedObjectPositionTransform.position - grabbedObjectTransform.position), ForceMode.Impulse);
            grabbedObjectRigidbody.linearVelocity *= grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;
            grabbedObjectRigidbody.angularVelocity *= grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;

            if (throwKeyPressedWhileHoldingAnObject)
            {
                throwKeyPressedWhileHoldingAnObject = false;
                grabbedObjectRigidbody.AddForce(throwForce * mainCamera.transform.forward, ForceMode.Impulse);
                ReleaseObject();
                return;
            }

            if ((holdedObjectPositionTransform.position - grabbedObjectTransform.position).magnitude > maxHoldingObjectCanBeOffsetDistance)
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
                    crosshairImage.color = Color.cyan;
                    grabbedObjectTransform = grabbedObjectRigidbody.transform;
                    grabbedObjectRigidbody.linearVelocity = Vector3.zero;
                    grabbedObjectRigidbody.useGravity = false;
                    Invoke(nameof(CanReleaseHoldedObjectWhenTouchedToPlayerActivator), canReleaseHoldedObjectWhenTouchedToPlayerCooldown);
                }

                Invoke(nameof(HoldAgainReset), holdAgainCooldown);
            }
            else if (readyToHold)
            {
                readyToHold = false;
                StartCoroutine(CrosshairBeingRed());
                Invoke(nameof(HoldAgainReset), crosshairBeingRedTime);
            }
        }
    }

    public void ReleaseObject()
    {
        crosshairImage.color = Color.black;
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectTransform = null;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
        canReleaseHoldedObjectWhenTouchedToPlayer = false;
    }

    void CanReleaseHoldedObjectWhenTouchedToPlayerActivator()
    {
        canReleaseHoldedObjectWhenTouchedToPlayer = true;
    }

    void HoldAgainReset()
    {
        readyToHold = true;
    }

    IEnumerator CrosshairBeingRed()
    {
        crosshairImage.color = Color.red;
        yield return new WaitForSeconds(crosshairBeingRedTime);
        crosshairImage.color = Color.black;
    }
}
