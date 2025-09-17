using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInteractionManager : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* Make sure that movable objects have a Rigidbody.

    [Header("Holding and Throwing")]
    [HideInInspector] public int throwForce = 60;
    [HideInInspector] public bool canReleaseHoldedObjectWhenTouchedToPlayer;
    [HideInInspector] public Rigidbody grabbedObjectRigidbody;
    const int NormalHoldingObjectDistance = 4, HoldForce = 30, MaxHoldingObjectCanBeOffsetDistance = 10, MaxHoldingObjectDistance = 6, MinHoldingObjectDistance = 3;
    const float MovingHoldingObjectWithScrollWheelSpeed = 0.4f, GrabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier = 0.3f, CanReleaseHoldedObjectWhenTouchedToPlayerCooldown = 0.1f, HoldAgainCooldown = 0.6f, CrosshairBeingRedTime = 0.3f;
    float tempHoldingObjectDistance, mouseScrollY;
    bool readyToHold = true, interacionKeyPressed, throwKeyPressedWhileHoldingAnObject;
    Transform grabbedObjectTransform, mainCameraTransform;
    RaycastHit holdInteractionHit;

    [Header("Granade")]
    bool removingPinKeyPressedWhileHoldingGranade;

    [Header("Other Things")]
    PlayerMovementManager playerMovementManagerScript;
    InputSystem_Actions inputActions;

    [Header("Inputs")]
    [SerializeField] Transform holdedObjectPositionTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask movableNormalLayer, movableBouncyLayer;
    [SerializeField] Image crosshairImage;
    [SerializeField] PauseMenuManager pauseMenuManagerScript;

    void Start()
    {
        mainCameraTransform = mainCamera.transform;
        crosshairImage.color = Color.black;
        tempHoldingObjectDistance = NormalHoldingObjectDistance;
        playerMovementManagerScript = GetComponent<PlayerMovementManager>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += InteractInputPerformed;
        inputActions.Player.Throw.performed += ThrowInputPerformed;
        inputActions.Player.RemovePin.performed += RemovePinInputPerformed;
    }

    void InteractInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.gamePaused)
        {
            interacionKeyPressed = true;
        }
    }

    void ThrowInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.gamePaused && grabbedObjectRigidbody)
        {
            throwKeyPressedWhileHoldingAnObject = true;
        }
    }

    void RemovePinInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.gamePaused && grabbedObjectRigidbody && grabbedObjectRigidbody.GetComponent<GranadeManager>())
        {
            removingPinKeyPressedWhileHoldingGranade = true;
        }
    }

    void Update()
    {
        if (!pauseMenuManagerScript.gamePaused && grabbedObjectRigidbody)
        {
            mouseScrollY = inputActions.Player.MouseWheel.ReadValue<float>();

            if (mouseScrollY != 0)
            {
                tempHoldingObjectDistance += MovingHoldingObjectWithScrollWheelSpeed * mouseScrollY;
            }
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

    void HoldingAndThrowingObject()
    {
        if (grabbedObjectRigidbody)
        {
            grabbedObjectRigidbody.AddForce(HoldForce * (holdedObjectPositionTransform.position - grabbedObjectTransform.position), ForceMode.VelocityChange);
            grabbedObjectRigidbody.linearVelocity *= GrabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;
            grabbedObjectRigidbody.angularVelocity *= GrabbedObjectLinearVelocityAndAngularVelocitySlowingMultiplier;

            if (throwKeyPressedWhileHoldingAnObject)
            {
                throwKeyPressedWhileHoldingAnObject = false;
                grabbedObjectRigidbody.AddForce(throwForce * mainCameraTransform.forward, ForceMode.Impulse);
                ReleaseObject();
                return;
            }

            // Tutulan obje çok uzakta kalırsa (bir şeye sıkışır veya takılırsa) objenin hızını sıfırlayıp bıraksın.
            if ((holdedObjectPositionTransform.position - grabbedObjectTransform.position).magnitude > MaxHoldingObjectCanBeOffsetDistance)
            {
                ReleaseObjectWithResettingLinearAndAngularVelocity();
                return;
            }

            if (tempHoldingObjectDistance > MaxHoldingObjectDistance)
            {
                tempHoldingObjectDistance = MaxHoldingObjectDistance;
            }
            else if (tempHoldingObjectDistance < MinHoldingObjectDistance)
            {
                tempHoldingObjectDistance = MinHoldingObjectDistance;
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

            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out holdInteractionHit, MaxHoldingObjectDistance, movableNormalLayer | movableBouncyLayer) && readyToHold && holdInteractionHit.rigidbody && !holdInteractionHit.rigidbody.isKinematic && !holdInteractionHit.rigidbody.Equals(playerMovementManagerScript.objectRigidbodyThatPlayerIsStandingOn))
            {
                readyToHold = canReleaseHoldedObjectWhenTouchedToPlayer = false;
                grabbedObjectRigidbody = holdInteractionHit.rigidbody;
                grabbedObjectTransform = grabbedObjectRigidbody.transform;
                grabbedObjectRigidbody.useGravity = false;
                crosshairImage.color = Color.cyan;
                Invoke(nameof(CanReleaseHoldedObjectWhenTouchedToPlayerActivator), CanReleaseHoldedObjectWhenTouchedToPlayerCooldown);
                Invoke(nameof(HoldAgainReset), HoldAgainCooldown);
            }
            else if (readyToHold)
            {
                readyToHold = false;
                StartCoroutine(CrosshairBeingRed());
                Invoke(nameof(HoldAgainReset), CrosshairBeingRedTime);
            }
        }
    }

    public void ReleaseObject()
    {
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectTransform = null;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = NormalHoldingObjectDistance;
        crosshairImage.color = Color.black;
    }

    public void ReleaseObjectWithResettingLinearAndAngularVelocity()
    {
        grabbedObjectRigidbody.linearVelocity = grabbedObjectRigidbody.angularVelocity = Vector3.zero;
        grabbedObjectRigidbody.useGravity = true;
        grabbedObjectTransform = null;
        grabbedObjectRigidbody = null;
        tempHoldingObjectDistance = NormalHoldingObjectDistance;
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
        yield return new WaitForSeconds(CrosshairBeingRedTime);
        crosshairImage.color = Color.black;
    }

    void RemovingPinOfHoldedGranage()
    {
        grabbedObjectRigidbody.GetComponent<GranadeManager>().removePin = true;
    }
}
