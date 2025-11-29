using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamMng : MonoBehaviour
{
    //* Attach this script to the CameraHolder game object.

    int sensitivity, normalCameraRotationMult = 1, zoomingSpeed = 10;
    float xRotation, yRotation, normalFOV, zoomedCameraRotationMult = 0.5f, theCameraRotationMult, sprintFOV, zoomFOV, zoomSprintFOV;
    bool zoomingInput;
    Transform cameraHolderTransform;
    PauseMenuMng pauseMenuMng;
    PlayerSpawnAndSaveMng playerSpawnAndSaveMng;
    PlayerStatusMng playerStatusMng;
    InputSystem_Actions inputActions;
    [SerializeField] GameObject userInterfaceObj;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform;
    [SerializeField] Camera mainCam;
    [SerializeField] PlayerMovementMng playerMovementMng;

    public int Sensitivity
    {
        get => sensitivity;
        set { sensitivity = value; }
    }

    public float XRotation
    {
        get => xRotation;
        set { xRotation = value; }
    }

    public float YRotation
    {
        get => yRotation;
        set { yRotation = value; }
    }

    public float NormalFOV
    {
        get => normalFOV;
        set { normalFOV = value; }
    }

    void Start()
    {
        mainCam.nearClipPlane = 0.1f;
        mainCam.fieldOfView = normalFOV;
        cameraHolderTransform = transform;
        theCameraRotationMult = normalCameraRotationMult;
        pauseMenuMng = userInterfaceObj.GetComponent<PauseMenuMng>();
        playerSpawnAndSaveMng = userInterfaceObj.GetComponent<PlayerSpawnAndSaveMng>();
        playerStatusMng = userInterfaceObj.GetComponent<PlayerStatusMng>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.CameraZoom.performed += CameraZoomInputPerformed;
        inputActions.Player.CameraZoom.canceled += CameraZoomInputCancelled;
    }

    void CameraZoomInputPerformed(InputAction.CallbackContext context)
    {
        zoomingInput = true;
    }

    void CameraZoomInputCancelled(InputAction.CallbackContext context)
    {
        zoomingInput = false;
    }

    void Update()
    {
        if (!pauseMenuMng.GamePaused)
        {
            CameraLook();

            if (!playerSpawnAndSaveMng.PlayerDied)
            {
                FOVChange();
            }
        }

        if (pauseMenuMng.SettingsMenuOpened)
        {
            mainCam.fieldOfView = normalFOV;
        }
    }

    void LateUpdate()
    {// I didn't pause the camera position change for not to see your body when you pause the game.
        cameraHolderTransform.position = cameraPositionTransform.position;
    }

    void CameraLook()
    {
        if (!playerSpawnAndSaveMng.PlayerDied)
        {
            yRotation += inputActions.Player.Look.ReadValue<Vector2>().x * sensitivity * theCameraRotationMult * 0.001f;
            xRotation -= inputActions.Player.Look.ReadValue<Vector2>().y * sensitivity * theCameraRotationMult * 0.001f;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
        }

        cameraHolderTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void FOVChange()
    {
        if (!zoomingInput)
        {
            theCameraRotationMult = normalCameraRotationMult;

            if (!(pauseMenuMng.DynamicFOV && (playerStatusMng.Running || (playerStatusMng.Sliding && playerStatusMng.FlatVelMag > playerMovementMng.RunSpeed))))
            {
                mainCam.fieldOfView = mainCam.fieldOfView > normalFOV - 0.01f && mainCam.fieldOfView < normalFOV + 0.01f ? normalFOV : Mathf.Lerp(mainCam.fieldOfView, normalFOV, zoomingSpeed * Time.deltaTime);
            }
            else
            {
                sprintFOV = normalFOV + 10;
                mainCam.fieldOfView = mainCam.fieldOfView > sprintFOV - 0.01f ? sprintFOV : Mathf.Lerp(mainCam.fieldOfView, sprintFOV, zoomingSpeed * Time.deltaTime);
            }
        }
        else
        {
            theCameraRotationMult = zoomedCameraRotationMult;

            if (!(pauseMenuMng.DynamicFOV && (playerStatusMng.Running || (playerStatusMng.Sliding && playerStatusMng.FlatVelMag > playerMovementMng.RunSpeed))))
            {
                zoomFOV = normalFOV / 5;
                mainCam.fieldOfView = mainCam.fieldOfView < zoomFOV + 0.01f ? zoomFOV : Mathf.Lerp(mainCam.fieldOfView, zoomFOV, zoomingSpeed * Time.deltaTime);
            }
            else
            {
                zoomSprintFOV = (normalFOV + 10) / 5;
                mainCam.fieldOfView = mainCam.fieldOfView > zoomSprintFOV - 0.01f && mainCam.fieldOfView < zoomSprintFOV + 0.01f ? zoomSprintFOV : Mathf.Lerp(mainCam.fieldOfView, zoomSprintFOV, zoomingSpeed * Time.deltaTime);
            }
        }
    }
}
