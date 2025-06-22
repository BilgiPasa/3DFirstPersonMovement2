using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    public static int sensitivity;
    public static float xRotation, yRotation, normalFOV;
    float sprintFOV, zoomFOV, zoomSprintFOV;
    KeyCode zoomKey = KeyCode.C;
    [SerializeField] Transform playerModelTransform, cameraPosition, cameraHolder;
    [SerializeField] Camera mainCamera;

    void Start()
    {
        mainCamera.fieldOfView = normalFOV;
        mainCamera.nearClipPlane = 0.1f;
    }

    void Update()
    {
        CameraFOVAssign();

        if (!PauseMenuManager.gamePaused)
        {
            CameraLook();

            if (!PlayerSpawnAndSaveManager.playerDied)
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
        if (PauseMenuManager.settingsMenuOpened && normalFOV != mainCamera.fieldOfView)
        {
            mainCamera.fieldOfView = normalFOV;
        }
    }

    void CameraMovement()
    {
        cameraHolder.position = cameraPosition.position;
    }

    void CameraLook()
    {
        if (!PlayerSpawnAndSaveManager.playerDied)
        {
            yRotation += Input.GetAxisRaw("Mouse X") * sensitivity * 0.02f;
            xRotation -= Input.GetAxisRaw("Mouse Y") * sensitivity * 0.02f;
            xRotation = Mathf.Clamp(xRotation, -90, 90);
        }

        cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        playerModelTransform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void FOVChange()
    {
        sprintFOV = normalFOV + 10;
        zoomFOV = normalFOV / 5;
        zoomSprintFOV = sprintFOV / 5;

        if (!Input.GetKey(zoomKey))
        {
            if (!(PauseMenuManager.dynamicFOV && (PlayerStatusManager.running || (PlayerStatusManager.sliding && PlayerStatusManager.flatVelocityMagnitude > PlayerMovementManager.runSpeed))))
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
            if (!(PauseMenuManager.dynamicFOV && (PlayerStatusManager.running || (PlayerStatusManager.sliding && PlayerStatusManager.flatVelocityMagnitude > PlayerMovementManager.runSpeed))))
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView < zoomFOV + 0.01f ? mainCamera.fieldOfView = zoomFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomFOV, 7.5f * Time.deltaTime);
            }
            else
            {
                mainCamera.fieldOfView = mainCamera.fieldOfView > zoomSprintFOV - 0.01f && mainCamera.fieldOfView < zoomSprintFOV + 0.01f ? mainCamera.fieldOfView = zoomSprintFOV : mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, zoomSprintFOV, 7.5f * Time.deltaTime);
            }
        }
    }
}
