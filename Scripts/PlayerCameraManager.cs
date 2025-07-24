using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    //* Attach this script to the CameraHolder game object.

    [HideInInspector] public int sensitivity;
    [HideInInspector] public float xRotation, yRotation, normalFOV;
    float sprintFOV, zoomFOV, zoomSprintFOV;
    KeyCode zoomKey = KeyCode.C;
    Transform cameraHolderTransform;
    PauseMenuManager pauseMenuManagerScript;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;
    [SerializeField] GameObject userInterfaceObject;
    [SerializeField] Transform playerColliderTransform, cameraPositionTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] PlayerMovementManager playerMovementManagerScript;

    void Start()
    {
        cameraHolderTransform = transform;
        mainCamera.fieldOfView = normalFOV;
        mainCamera.nearClipPlane = 0.1f;
        pauseMenuManagerScript = userInterfaceObject.GetComponent<PauseMenuManager>();
        playerSpawnAndSaveManagerScript = userInterfaceObject.GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = userInterfaceObject.GetComponent<PlayerStatusManager>();
    }

    void Update()
    {
        CameraFOVAssign();

        if (!pauseMenuManagerScript.gamePaused)
        {
            CameraLook();

            if (!playerSpawnAndSaveManagerScript.playerDied)
            {
                FOVChange();
            }
        }
    }

    void LateUpdate()
    {// I didn't pause the camera movement for not to see your body when you pause the game
        CameraMovement();
    }

    void CameraFOVAssign()
    {
        if (pauseMenuManagerScript.settingsMenuOpened && normalFOV != mainCamera.fieldOfView)
        {
            mainCamera.fieldOfView = normalFOV;
        }
    }

    void CameraLook()
    {
        if (!playerSpawnAndSaveManagerScript.playerDied)
        {
            yRotation += Input.GetAxisRaw("Mouse X") * sensitivity * 0.02f;
            xRotation -= Input.GetAxisRaw("Mouse Y") * sensitivity * 0.02f;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
        }

        cameraHolderTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerColliderTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void FOVChange()
    {
        sprintFOV = normalFOV + 10;
        zoomFOV = normalFOV / 5;
        zoomSprintFOV = sprintFOV / 5;

        if (!Input.GetKey(zoomKey))
        {
            if (!(pauseMenuManagerScript.dynamicFOV && (playerStatusManagerScript.running || (playerStatusManagerScript.sliding && playerStatusManagerScript.flatVelocityMagnitude > playerMovementManagerScript.runSpeed))))
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView > normalFOV - 0.01f && mainCamera.fieldOfView < normalFOV + 0.01f ? mainCamera.fieldOfView = normalFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, normalFOV, 7.5f * Time.deltaTime);
            }
            else
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView > sprintFOV - 0.01f ? mainCamera.fieldOfView = sprintFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, sprintFOV, 7.5f * Time.deltaTime);
            }
        }
        else
        {
            if (!(pauseMenuManagerScript.dynamicFOV && (playerStatusManagerScript.running || (playerStatusManagerScript.sliding && playerStatusManagerScript.flatVelocityMagnitude > playerMovementManagerScript.runSpeed))))
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView < zoomFOV + 0.01f ? mainCamera.fieldOfView = zoomFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFOV, 7.5f * Time.deltaTime);
            }
            else
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView > zoomSprintFOV - 0.01f && mainCamera.fieldOfView < zoomSprintFOV + 0.01f ? mainCamera.fieldOfView = zoomSprintFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomSprintFOV, 7.5f * Time.deltaTime);
            }
        }
    }

    void CameraMovement()
    {
        cameraHolderTransform.position = cameraPositionTransform.position;
    }
}
