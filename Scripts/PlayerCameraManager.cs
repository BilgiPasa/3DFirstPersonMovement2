using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraManager : MonoBehaviour
{
    //* Attach this script to the CameraHolder game object.

    [HideInInspector] public int sensitivity;
    [HideInInspector] public float xRotation, yRotation, normalFOV;
    int normalCameraRotationMultiplier = 1, zoomingSpeed = 10;
    float zoomedCameraRotationMultiplier = 0.5f, theCameraRotationMultiplier, sprintFOV, zoomFOV, zoomSprintFOV;
    bool zoomingInput;
    Transform cameraHolderTransform;
    PauseMenuManager pauseMenuManagerScript;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;
    InputSystem_Actions inputActions;
    [SerializeField] GameObject userInterfaceObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] PlayerMovementManager playerMovementManagerScript;

    void Start()
    {
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.fieldOfView = normalFOV;
        cameraHolderTransform = transform;
        theCameraRotationMultiplier = normalCameraRotationMultiplier;
        pauseMenuManagerScript = userInterfaceObject.GetComponent<PauseMenuManager>();
        playerSpawnAndSaveManagerScript = userInterfaceObject.GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = userInterfaceObject.GetComponent<PlayerStatusManager>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.CameraZoom.performed += CameraZoomInputPerformed;
        inputActions.Player.CameraZoom.canceled += CameraZoomInputCancelled;
    }

    void CameraZoomInputPerformed(InputAction.CallbackContext context)
    {
        if (!pauseMenuManagerScript.gamePaused && !playerSpawnAndSaveManagerScript.playerDied)
        {
            zoomingInput = true;
        }
    }

    void CameraZoomInputCancelled(InputAction.CallbackContext context)
    {
        zoomingInput = false;
    }

    void Update()
    {
        if (!pauseMenuManagerScript.gamePaused)
        {
            CameraLook();

            if (!playerSpawnAndSaveManagerScript.playerDied)
            {
                FOVChange();
            }
        }

        if (pauseMenuManagerScript.settingsMenuOpened)
        {
            mainCamera.fieldOfView = normalFOV;
        }
    }

    void LateUpdate()
    {// I didn't pause the camera position change for not to see your body when you pause the game.
        cameraHolderTransform.position = cameraPositionTransform.position;
    }

    void CameraLook()
    {
        if (!playerSpawnAndSaveManagerScript.playerDied)
        {
            yRotation += inputActions.Player.Look.ReadValue<Vector2>().x * sensitivity * theCameraRotationMultiplier * 0.001f;
            xRotation -= inputActions.Player.Look.ReadValue<Vector2>().y * sensitivity * theCameraRotationMultiplier * 0.001f;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
        }

        cameraHolderTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void FOVChange()
    {
        if (!zoomingInput)
        {
            theCameraRotationMultiplier = normalCameraRotationMultiplier;

            if (!(pauseMenuManagerScript.dynamicFOV && (playerStatusManagerScript.running || (playerStatusManagerScript.sliding && playerStatusManagerScript.flatVelocityMagnitude > playerMovementManagerScript.runSpeed))))
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView > normalFOV - 0.01f && mainCamera.fieldOfView < normalFOV + 0.01f ? normalFOV : Mathf.Lerp(mainCamera.fieldOfView, normalFOV, zoomingSpeed * Time.deltaTime);
            }
            else
            {
                sprintFOV = normalFOV + 10;
                mainCamera.fieldOfView = mainCamera.fieldOfView > sprintFOV - 0.01f ? sprintFOV : Mathf.Lerp(mainCamera.fieldOfView, sprintFOV, zoomingSpeed * Time.deltaTime);
            }
        }
        else
        {
            theCameraRotationMultiplier = zoomedCameraRotationMultiplier;

            if (!(pauseMenuManagerScript.dynamicFOV && (playerStatusManagerScript.running || (playerStatusManagerScript.sliding && playerStatusManagerScript.flatVelocityMagnitude > playerMovementManagerScript.runSpeed))))
            {
                zoomFOV = normalFOV / 5;
                mainCamera.fieldOfView = mainCamera.fieldOfView < zoomFOV + 0.01f ? zoomFOV : Mathf.Lerp(mainCamera.fieldOfView, zoomFOV, zoomingSpeed * Time.deltaTime);
            }
            else
            {
                zoomSprintFOV = (normalFOV + 10) / 5;
                mainCamera.fieldOfView = mainCamera.fieldOfView > zoomSprintFOV - 0.01f && mainCamera.fieldOfView < zoomSprintFOV + 0.01f ? zoomSprintFOV : Mathf.Lerp(mainCamera.fieldOfView, zoomSprintFOV, zoomingSpeed * Time.deltaTime);
            }
        }
    }
}
