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
    const int normalHoldingObjectDistance = 4, movingHoldingObjectWithScrollWheelSpeed = 4, holdForce = 30, maxHoldingObjectCanBeOffsetDistance = 10, maxHoldingObjectDistance = 6, minHoldingObjectDistance = 3;
    const float grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier = 0.3f, canReleaseHoldedObjectWhenTouchedToPlayerCooldown = 0.1f, holdAgainCooldown = 0.6f, crosshairBeingRedTime = 0.4f;
    float tempHoldingObjectDistance;
    bool readyToHold = true, interacionKeyPressed, throwKeyPressedWhileHoldingAnObject;
    Transform grabbedObjectTransform, mainCameraTransform;
    RaycastHit holdInteractionHit;

    [Header("Granade")]
    bool removingPinKeyPressedWhileHoldingGranade;

    [Header("Keybinds")]
    KeyCode interactionKey = KeyCode.E, throwKey = KeyCode.Mouse0, removingPinKey = KeyCode.Mouse1;

    [Header("Other Things")]
    PlayerMovementManager playerMovementManagerScript;

    [Header("Inputs")]
    [SerializeField] int throwForce = 60;
    [SerializeField] Transform holdedObjectPositionTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask movableNormalLayer, movableBouncyLayer;
    [SerializeField] Image crosshairImage;
    [SerializeField] PauseMenuManager pauseMenuManagerScript;

    void Start()
    {
        mainCameraTransform = mainCamera.transform;
        crosshairImage.color = Color.black;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
        playerMovementManagerScript = GetComponent<PlayerMovementManager>();
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

        if (removingPinKeyPressedWhileHoldingGranade)
        {
            removingPinKeyPressedWhileHoldingGranade = false;
            RemovingPinOfHoldedGranage();
        }
    }

    void InteractionInputs()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            interacionKeyPressed = true;
        }

        if (grabbedObjectRigidbody)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                tempHoldingObjectDistance += movingHoldingObjectWithScrollWheelSpeed * Input.GetAxis("Mouse ScrollWheel");
            }

            if (Input.GetKeyDown(throwKey))
            {
                throwKeyPressedWhileHoldingAnObject = true;
            }

            if (Input.GetKeyDown(removingPinKey) && grabbedObjectRigidbody.GetComponent<GranadeManager>())
            {
                removingPinKeyPressedWhileHoldingGranade = true;
            }
        }
    }

    void HoldingAndThrowingObject()
    {
        if (grabbedObjectRigidbody)
        {
            grabbedObjectRigidbody.AddForce(holdForce * (holdedObjectPositionTransform.position - grabbedObjectTransform.position), ForceMode.VelocityChange);
            grabbedObjectRigidbody.linearVelocity *= grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;
            grabbedObjectRigidbody.angularVelocity *= grabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;

            if (throwKeyPressedWhileHoldingAnObject)
            {
                throwKeyPressedWhileHoldingAnObject = false;
                grabbedObjectRigidbody.AddForce(throwForce * mainCameraTransform.forward, ForceMode.Impulse);
                ReleaseObject();
                return;
            }

            // Tutulan obje çok uzakta kalırsa (bir şeye sıkışır veya takılırsa) objenin hızını sıfırlayıp bıraksın.
            if ((holdedObjectPositionTransform.position - grabbedObjectTransform.position).magnitude > maxHoldingObjectCanBeOffsetDistance)
            {
                ReleaseObjectWithResettingLinearAndAngularVelocity();
                return;
            }

            if (tempHoldingObjectDistance > maxHoldingObjectDistance)
            {
                tempHoldingObjectDistance = maxHoldingObjectDistance;
            }
            else if (tempHoldingObjectDistance < minHoldingObjectDistance)
            {
                tempHoldingObjectDistance = minHoldingObjectDistance;
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

            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out holdInteractionHit, maxHoldingObjectDistance, movableNormalLayer | movableBouncyLayer) && readyToHold && holdInteractionHit.rigidbody && !holdInteractionHit.rigidbody.isKinematic && !holdInteractionHit.rigidbody.Equals(playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn))
            {
                readyToHold = canReleaseHoldedObjectWhenTouchedToPlayer = false;
                grabbedObjectRigidbody = holdInteractionHit.rigidbody;
                grabbedObjectTransform = grabbedObjectRigidbody.transform;
                grabbedObjectRigidbody.useGravity = false;
                crosshairImage.color = Color.cyan;
                Invoke(nameof(CanReleaseHoldedObjectWhenTouchedToPlayerActivator), canReleaseHoldedObjectWhenTouchedToPlayerCooldown);
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
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectTransform = null;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
        crosshairImage.color = Color.black;
    }

    public void ReleaseObjectWithResettingLinearAndAngularVelocity()
    {
        grabbedObjectRigidbody.linearVelocity = grabbedObjectRigidbody.angularVelocity = Vector3.zero;
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectTransform = null;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = normalHoldingObjectDistance;
        crosshairImage.color = Color.black;
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

    void RemovingPinOfHoldedGranage()
    {
        grabbedObjectRigidbody.GetComponent<GranadeManager>().removePin = true;
    }
}
