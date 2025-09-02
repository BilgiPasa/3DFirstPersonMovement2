using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* PauseMenu'yü aktive et ve SettingsButton'ın Text'ine gel. O text'in Outline materyalinin Face'inin Dilate'sini 0.05 yap. Outline'ının rengini bembeyaz yap ve Thickness'ını 0.05 yap. Ardından PauseMenu'yü inaktif yap.

    [HideInInspector] public bool gamePaused, dynamicFOV, settingsMenuOpened;
    int[] last5FPS = new int[5];
    int counter;
    bool cancelKeyPressed;
    RectTransform FPSTextRectTransform;
    TextMeshProUGUI speedText, FPSText;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;
    InputSystem_Actions inputActions;
    [SerializeField] GameObject pauseMenuObject, settingsMenuObject, speedTextObject, FPSTextObject;
    [SerializeField] TextMeshProUGUI FOVText, mouseSensitivityText, maxFPSText;
    [SerializeField] Toggle dynamicFOVToggle, speedTextToggle, increasedSensitivityToggle, showFPSToggle;
    [SerializeField] Slider FOVSlider, mouseSensitivitySlider, maxFPSSlider;
    [SerializeField] PlayerCameraManager playerCameraManagerScript;

    void Start()
    {
        FPSTextRectTransform = FPSTextObject.GetComponent<RectTransform>();
        speedText = speedTextObject.GetComponent<TextMeshProUGUI>();
        FPSText = FPSTextObject.GetComponent<TextMeshProUGUI>();
        playerSpawnAndSaveManagerScript = GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = GetComponent<PlayerStatusManager>();
        inputActions = new InputSystem_Actions();
        inputActions.UI.Enable();
        inputActions.UI.Cancel.performed += CancelInputPerformed;

        if (PlayerPrefs.GetInt("FOV") == 0)
        {
            PlayerPrefs.SetInt("FOV", 90);
        }

        if (PlayerPrefs.GetInt("maxFPS") == 0)
        {
            PlayerPrefs.SetInt("maxFPS", 8);
        }

        if (PlayerPrefs.GetInt("mouseSensitivity") == 0)
        {
            PlayerPrefs.SetInt("mouseSensitivity", 100);
        }

        if (PlayerPrefs.GetInt("showFPS") == 0)
        {
            PlayerPrefs.SetInt("showFPS", 1);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == 0)
        {
            PlayerPrefs.SetInt("increasedSensitivity", -1);
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 0)
        {
            PlayerPrefs.SetInt("dynamicFOV", 1);
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 0)
        {
            PlayerPrefs.SetInt("speedTextObjectActive", 1);
        }

        playerCameraManagerScript.normalFOV = PlayerPrefs.GetInt("FOV");
        playerCameraManagerScript.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");

        switch (PlayerPrefs.GetInt("maxFPS"))
        {
            case 9:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                break;
            case 8:
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
                break;
            case 7:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 240;
                break;
            case 6:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 180;
                break;
            case 5:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 165;
                break;
            case 4:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 144;
                break;
            case 3:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 120;
                break;
            case 2:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 75;
                break;
            case 1:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                break;
        }

        speedText.text = "Speed: 0";
    }

    void CancelInputPerformed(InputAction.CallbackContext context)
    {
        cancelKeyPressed = true;
    }

    void Update()
    {
        if (!(gamePaused || playerSpawnAndSaveManagerScript.playerDied))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            speedText.text = $"Speed: {playerStatusManagerScript.flatVelocityMagnitude}";
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!gamePaused && FPSTextObject.activeSelf && 1 / Time.deltaTime <= int.MaxValue)
        {
            if (counter < last5FPS.Length - 1)
            {
                counter++;
            }
            else
            {
                counter = 0;
            }

            last5FPS[counter] = (int)(1 / Time.deltaTime);
            FPSText.text = $"FPS: {(int)Queryable.Average(last5FPS.AsQueryable())}";
        }

        if (playerSpawnAndSaveManagerScript.playerDied)
        {
            return;
        }

        if (cancelKeyPressed)
        {
            cancelKeyPressed = false;

            if (!settingsMenuObject.activeSelf)
            {
                if (!gamePaused)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                GoBackToPauseMenu();
            }
        }

        playerCameraManagerScript.normalFOV = PlayerPrefs.GetInt("FOV");
        playerCameraManagerScript.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");

        if (PlayerPrefs.GetInt("dynamicFOV") == 1)
        {
            dynamicFOV = true;
        }
        else if (PlayerPrefs.GetInt("dynamicFOV") == -1)
        {
            dynamicFOV = false;
        }

        if (PlayerPrefs.GetInt("showFPS") == 1)
        {
            FPSTextObject.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("showFPS") == -1)
        {
            FPSTextObject.SetActive(false);
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 1)
        {
            speedTextObject.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("speedTextObjectActive") == -1)
        {
            speedTextObject.SetActive(false);
        }

        if (!(!speedTextObject.activeSelf && FPSTextObject.activeSelf))
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -90);
        }
        else
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -45);
        }
    }

    void Pause()
    {
        playerSpawnAndSaveManagerScript.SavingTheGame();
        pauseMenuObject.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    public void Resume()
    {
        pauseMenuObject.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }

    public void GoBackToPauseMenu()
    {
        settingsMenuObject.SetActive(false);
        settingsMenuOpened = false;
        pauseMenuObject.SetActive(true);
    }

    public void Settings()
    {
        switch (playerCameraManagerScript.normalFOV)
        {
            case 90:
                FOVText.text = "FOV: Normal";
                break;
            case 110:
                FOVText.text = "FOV: WIDE";
                break;
            case 30:
                FOVText.text = "FOV: Telescope";
                break;
            default:
                FOVText.text = $"FOV: {playerCameraManagerScript.normalFOV}";
                break;
        }

        switch (PlayerPrefs.GetInt("maxFPS"))
        {
            case 9:
                maxFPSText.text = "Max FPS: Unlimited";
                maxFPSSlider.value = 9;
                break;
            case 8:
                maxFPSText.text = "Max FPS: V-Sync";
                maxFPSSlider.value = 8;
                break;
            case 7:
                maxFPSText.text = "Max FPS: 240";
                maxFPSSlider.value = 7;
                break;
            case 6:
                maxFPSText.text = "Max FPS: 180";
                maxFPSSlider.value = 6;
                break;
            case 5:
                maxFPSText.text = "Max FPS: 165";
                maxFPSSlider.value = 5;
                break;
            case 4:
                maxFPSText.text = "Max FPS: 144";
                maxFPSSlider.value = 4;
                break;
            case 3:
                maxFPSText.text = "Max FPS: 120";
                maxFPSSlider.value = 3;
                break;
            case 2:
                maxFPSText.text = "Max FPS: 75";
                maxFPSSlider.value = 2;
                break;
            case 1:
                maxFPSText.text = "Max FPS: 60";
                maxFPSSlider.value = 1;
                break;
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            increasedSensitivityToggle.isOn = false;

            switch (playerCameraManagerScript.sensitivity)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {playerCameraManagerScript.sensitivity}";
                    break;
            }

            mouseSensitivitySlider.value = playerCameraManagerScript.sensitivity;
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            increasedSensitivityToggle.isOn = true;

            switch (playerCameraManagerScript.sensitivity)
            {
                case 300:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 400:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {playerCameraManagerScript.sensitivity}";
                    break;
            }

            mouseSensitivitySlider.value = playerCameraManagerScript.sensitivity - 200;
        }

        if (PlayerPrefs.GetInt("showFPS") == 1)
        {
            showFPSToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("showFPS") == -1)
        {
            showFPSToggle.isOn = false;
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 1)
        {
            dynamicFOVToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("dynamicFOV") == -1)
        {
            dynamicFOVToggle.isOn = false;
        }

        if (PlayerPrefs.GetInt("speedTextObjectActive") == 1)
        {
            speedTextToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("speedTextObjectActive") == -1)
        {
            speedTextToggle.isOn = false;
        }

        FOVSlider.value = playerCameraManagerScript.normalFOV;
        pauseMenuObject.SetActive(false);
        settingsMenuObject.SetActive(true);
        settingsMenuOpened = true;
    }

    public void FOV(float value)
    {
        switch (value)
        {
            case 90:
                FOVText.text = "FOV: Normal";
                break;
            case 110:
                FOVText.text = "FOV: WIDE";
                break;
            case 30:
                FOVText.text = "FOV: Telescope";
                break;
            default:
                FOVText.text = "FOV: " + Mathf.RoundToInt(value);
                break;
        }

        PlayerPrefs.SetInt("FOV", (int)FOVSlider.value);
    }

    public void MaxFPS(float value)
    {
        switch (value)
        {
            case 9:
                maxFPSText.text = "Max FPS: Unlimited";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                PlayerPrefs.SetInt("maxFPS", 9);
                break;
            case 8:
                maxFPSText.text = "Max FPS: V-Sync";
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
                PlayerPrefs.SetInt("maxFPS", 8);
                break;
            case 7:
                maxFPSText.text = "Max FPS: 240";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 240;
                PlayerPrefs.SetInt("maxFPS", 7);
                break;
            case 6:
                maxFPSText.text = "Max FPS: 180";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 180;
                PlayerPrefs.SetInt("maxFPS", 6);
                break;
            case 5:
                maxFPSText.text = "Max FPS: 165";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 165;
                PlayerPrefs.SetInt("maxFPS", 5);
                break;
            case 4:
                maxFPSText.text = "Max FPS: 144";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 144;
                PlayerPrefs.SetInt("maxFPS", 4);
                break;
            case 3:
                maxFPSText.text = "Max FPS: 120";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 120;
                PlayerPrefs.SetInt("maxFPS", 3);
                break;
            case 2:
                maxFPSText.text = "Max FPS: 75";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 75;
                PlayerPrefs.SetInt("maxFPS", 2);
                break;
            case 1:
                maxFPSText.text = "Max FPS: 60";
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                PlayerPrefs.SetInt("maxFPS", 1);
                break;
        }
    }

    public void Sensitivity(float value)
    {
        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(value);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(value + 200);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)(mouseSensitivitySlider.value + 200));
        }
    }

    public void ShowFPSSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("showFPS", 1);
        }
        else
        {
            PlayerPrefs.SetInt("showFPS", -1);
        }
    }

    public void IncreasedSensitivitySwitch(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetInt("increasedSensitivity", -1);
        }
        else
        {
            PlayerPrefs.SetInt("increasedSensitivity", 1);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (mouseSensitivitySlider.value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: Normal";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: FAST";
                    break;
                case 1:
                    mouseSensitivityText.text = "Mouse Sensitivity: Snail";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(mouseSensitivitySlider.value);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (mouseSensitivitySlider.value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + Mathf.RoundToInt(mouseSensitivitySlider.value + 200);
                    break;
            }

            PlayerPrefs.SetInt("mouseSensitivity", (int)(mouseSensitivitySlider.value + 200));
        }
    }

    public void DynamicFOVSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("dynamicFOV", 1);
        }
        else
        {
            PlayerPrefs.SetInt("dynamicFOV", -1);
        }
    }

    public void SpeedTextSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("speedTextObjectActive", 1);
        }
        else
        {
            PlayerPrefs.SetInt("speedTextObjectActive", -1);
        }
    }

    public void QuittingGame()
    {
        Application.Quit();
    }
}
