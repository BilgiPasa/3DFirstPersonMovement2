using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractionMng : MonoBehaviour
{
    //* Attach this script to the Player game object.
    //* In Unity Editor, layer 7 should be "Movable Normal Layer".
    //* In Unity Editor, layer 8 should be "Movable Bouncy Layer".
    //* Make sure that movable objects have a Rigidbody.

    [Header("Holding and Throwing")]
    const int NormalObjHoldDistance = 4, HoldForce = 30, MaxObjHoldOffsetDistance = 10, MaxObjHoldDistance = 6, MinObjHoldDistance = 3;
    int throwForce; // If you want to change the value, change it from the PauseMenuManager script.
    const float ObjHoldScrollWheelSpeed = 0.4f, GrabbedObjVelReducerMult = 0.3f, CanReleaseTouchedObjCooldown = 0.2f, HoldAgainCooldown = 0.6f, CrosshairIsRedTime = 0.3f;
    float tempObjHoldDistance, mouseScrollY;
    bool canReleaseTouchedObj, readyToHold = true, interacionKeyPressed, throwKeyPressedWhenObjHold;
    Transform grabbedObjTransform, mainCamTransform;
    Rigidbody grabbedObjRb;
    RaycastHit holdInteractionHit;

    public int ThrowForce
    {
        get => throwForce;
        set { throwForce = value; }
    }

    public bool CanReleaseTouchedObj
    {
        get => canReleaseTouchedObj;
    }

    public Rigidbody GrabbedObjRb
    {
        get => grabbedObjRb;
    }

    [Header("Granade")]
    bool removePinWhenGranadeHold;

    [Header("Other Things")]
    PlayerMovementMng playerMovementMng;
    InputSystem_Actions inputActions;

    [Header("Inputs")]
    [SerializeField] Transform objHoldPosTransform;
    [SerializeField] Camera mainCam;
    [SerializeField] LayerMask movableNormalLayer, movableBouncyLayer;
    [SerializeField] Image crosshairImg;
    [SerializeField] PauseMenuMng pauseMenuMng;

    void Start()
    {
        mainCamTransform = mainCam.transform;
        crosshairImg.color = Color.black;
        tempObjHoldDistance = NormalObjHoldDistance;
        playerMovementMng = GetComponent<PlayerMovementMng>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Interact.performed += InteractInputPerformed;
        inputActions.Player.Throw.performed += ThrowInputPerformed;
        inputActions.Player.RemovePin.performed += RemovePinInputPerformed;
    }

    void InteractInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuMng.GamePaused)
        {
            interacionKeyPressed = true;
        }
    }

    void ThrowInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuMng.GamePaused && grabbedObjRb)
        {
            throwKeyPressedWhenObjHold = true;
        }
    }

    void RemovePinInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuMng.GamePaused && grabbedObjRb && grabbedObjRb.GetComponent<GranadeMng>())
        {
            removePinWhenGranadeHold = true;
        }
    }

    void Update()
    {
        if (!pauseMenuMng.GamePaused && grabbedObjRb)
        {
            mouseScrollY = inputActions.Player.MouseWheel.ReadValue<float>();

            if (mouseScrollY != 0)
            {
                tempObjHoldDistance += ObjHoldScrollWheelSpeed * mouseScrollY;
            }
        }
    }

    void FixedUpdate()
    {
        HoldingAndThrowingObj();

        if (removePinWhenGranadeHold)
        {
            removePinWhenGranadeHold = false;
            RemovingPinOfHoldedGranage();
        }
    }

    void HoldingAndThrowingObj()
    {
        if (grabbedObjRb)
        {
            grabbedObjRb.AddForce(HoldForce * (objHoldPosTransform.position - grabbedObjTransform.position), ForceMode.VelocityChange);
            grabbedObjRb.linearVelocity *= GrabbedObjVelReducerMult;
            grabbedObjRb.angularVelocity *= GrabbedObjVelReducerMult;

            if (throwKeyPressedWhenObjHold)
            {
                throwKeyPressedWhenObjHold = false;
                grabbedObjRb.AddForce(throwForce * mainCamTransform.forward, ForceMode.Impulse);
                ReleaseObj();
                return;
            }

            // Tutulan obje çok uzakta kalırsa (bir şeye sıkışır veya takılırsa) objenin hızını sıfırlayıp bıraksın.
            if ((objHoldPosTransform.position - grabbedObjTransform.position).magnitude > MaxObjHoldOffsetDistance)
            {
                ReleaseObjWithVelReset();
                return;
            }

            if (tempObjHoldDistance > MaxObjHoldDistance)
            {
                tempObjHoldDistance = MaxObjHoldDistance;
            }
            else if (tempObjHoldDistance < MinObjHoldDistance)
            {
                tempObjHoldDistance = MinObjHoldDistance;
            }

            objHoldPosTransform.localPosition = new Vector3(0, 0, tempObjHoldDistance);
        }

        if (interacionKeyPressed)
        {
            interacionKeyPressed = false;

            if (grabbedObjRb)
            {
                ReleaseObj();
                return;
            }

            if (Physics.Raycast(mainCamTransform.position, mainCamTransform.forward, out holdInteractionHit, MaxObjHoldDistance, movableNormalLayer | movableBouncyLayer) && readyToHold && holdInteractionHit.rigidbody && !holdInteractionHit.rigidbody.isKinematic && !holdInteractionHit.rigidbody.Equals(playerMovementMng.ObjRbThatPlayerStandingOn))
            {
                readyToHold = canReleaseTouchedObj = false;
                grabbedObjRb = holdInteractionHit.rigidbody;
                grabbedObjTransform = grabbedObjRb.transform;
                grabbedObjRb.useGravity = false;
                crosshairImg.color = Color.cyan;
                Invoke(nameof(CanReleaseTouchedObjActivator), CanReleaseTouchedObjCooldown);
                Invoke(nameof(HoldAgainReset), HoldAgainCooldown);
            }
            else if (readyToHold)
            {
                readyToHold = false;
                StartCoroutine(CrosshairIsRed());
                Invoke(nameof(HoldAgainReset), CrosshairIsRedTime);
            }
        }
    }

    public void ReleaseObj()
    {
        grabbedObjRb.useGravity = true;
        grabbedObjTransform = null;
        grabbedObjRb = null;
        tempObjHoldDistance = NormalObjHoldDistance;
        crosshairImg.color = Color.black;
    }

    public void ReleaseObjWithVelReset()
    {
        grabbedObjRb.linearVelocity = grabbedObjRb.angularVelocity = Vector3.zero;
        grabbedObjRb.useGravity = true;
        grabbedObjTransform = null;
        grabbedObjRb = null;
        tempObjHoldDistance = NormalObjHoldDistance;
        crosshairImg.color = Color.black;
    }

    void CanReleaseTouchedObjActivator()
    {
        canReleaseTouchedObj = true;
    }

    void HoldAgainReset()
    {
        readyToHold = true;
    }

    IEnumerator CrosshairIsRed()
    {
        crosshairImg.color = Color.red;
        yield return new WaitForSeconds(CrosshairIsRedTime);
        crosshairImg.color = Color.black;
    }

    void RemovingPinOfHoldedGranage()
    {
        grabbedObjRb.GetComponent<GranadeMng>().RemovePin = true;
    }
}
