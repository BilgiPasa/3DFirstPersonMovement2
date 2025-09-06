using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* "PauseMenu"yü aktive et ve "SettingsButton"ın "Text"ine gel. O "Text"in "Outline" materyalinin "Face"inin "Dilate"sini 0.1 yap. "Outline"ının rengini bembeyaz yap ve "Thickness"ını 0.05 yap. Ardından "PauseMenu"yü inaktif yap.

    [HideInInspector] public bool gamePaused, dynamicFOV, settingsMenuOpened;
    int[] last5FPS = new int[5];
    int defaultFOV = 90, defaultMaxFPS = 8, defaultMouseSensitivity = 100, defaultShowFPS = 1, defaultIncreasedSensitivity = -1, defaultDynamicFOV = 1, defaultShowSpeedText = 1, defaultPlayerSpeedTweak = 9, defaultPlayerThrowForceTweak = 60, defaultPlayerNormalJumpForceTweak = 21, defaultPlayerBouncyJumpForceTweak = 56, defaultPlayerNoFallDamageTweak = -1, counter;
    bool cancelKeyPressed;
    RectTransform FPSTextRectTransform;
    TextMeshProUGUI speedText, FPSText;
    PlayerInteractionManager playerInteractionManagerScript;
    PlayerMovementManager playerMovementManagerScript;
    PlayerSpawnAndSaveManager playerSpawnAndSaveManagerScript;
    PlayerStatusManager playerStatusManagerScript;
    InputSystem_Actions inputActions;
    [SerializeField] GameObject playerObject, pauseMenuObject, settingsMenuObject, playerTweaksMenuObject, speedTextObject, FPSTextObject;
    [SerializeField] TextMeshProUGUI FOVText, mouseSensitivityText, maxFPSText, playerSpeedTweakText, playerThrowForceTweakText, playerNormalJumpForceTweakText, playerBouncyJumpForceTweakText;
    [SerializeField] Toggle dynamicFOVToggle, showSpeedTextToggle, increasedSensitivityToggle, showFPSToggle, playerNoDamageTweakToggle;
    [SerializeField] Slider FOVSlider, mouseSensitivitySlider, maxFPSSlider, playerSpeedTweakSlider, playerThrowForceTweakSlider, playerNormalJumpForceSlider, playerBouncyJumpForceSlider;
    [SerializeField] PlayerCameraManager playerCameraManagerScript;

    void Start()
    {
        FPSTextRectTransform = FPSTextObject.GetComponent<RectTransform>();
        speedText = speedTextObject.GetComponent<TextMeshProUGUI>();
        FPSText = FPSTextObject.GetComponent<TextMeshProUGUI>();
        playerInteractionManagerScript = playerObject.GetComponent<PlayerInteractionManager>();
        playerMovementManagerScript = playerObject.GetComponent<PlayerMovementManager>();
        playerSpawnAndSaveManagerScript = GetComponent<PlayerSpawnAndSaveManager>();
        playerStatusManagerScript = GetComponent<PlayerStatusManager>();
        inputActions = new InputSystem_Actions();
        inputActions.UI.Enable();
        inputActions.UI.Cancel.performed += CancelInputPerformed;

        if (PlayerPrefs.GetInt("FOV") == 0)
        {
            PlayerPrefs.SetInt("FOV", defaultFOV);
        }

        if (PlayerPrefs.GetInt("maxFPS") == 0)
        {
            PlayerPrefs.SetInt("maxFPS", defaultMaxFPS);
        }

        if (PlayerPrefs.GetInt("mouseSensitivity") == 0)
        {
            PlayerPrefs.SetInt("mouseSensitivity", defaultMouseSensitivity);
        }

        if (PlayerPrefs.GetInt("showFPS") == 0)
        {
            PlayerPrefs.SetInt("showFPS", defaultShowFPS);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == 0)
        {
            PlayerPrefs.SetInt("increasedSensitivity", defaultIncreasedSensitivity);
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 0)
        {
            PlayerPrefs.SetInt("dynamicFOV", defaultDynamicFOV);
        }

        if (PlayerPrefs.GetInt("showSpeedText") == 0)
        {
            PlayerPrefs.SetInt("showSpeedText", defaultShowSpeedText);
        }

        if (PlayerPrefs.GetInt("playerSpeedTweak") == 0)
        {
            PlayerPrefs.SetInt("playerSpeedTweak", defaultPlayerSpeedTweak);
        }

        if (PlayerPrefs.GetInt("playerThrowForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerThrowForceTweak", defaultPlayerThrowForceTweak);
        }

        if (PlayerPrefs.GetInt("playerNormalJumpForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerNormalJumpForceTweak", defaultPlayerNormalJumpForceTweak);
        }

        if (PlayerPrefs.GetInt("playerBouncyJumpForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerBouncyJumpForceTweak", defaultPlayerBouncyJumpForceTweak);
        }

        if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == 0)
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", defaultPlayerNoFallDamageTweak);
        }

        playerCameraManagerScript.normalFOV = PlayerPrefs.GetInt("FOV");

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

        playerCameraManagerScript.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");

        if (PlayerPrefs.GetInt("showFPS") == 1)
        {
            FPSTextObject.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("showFPS") == -1)
        {
            FPSTextObject.SetActive(false);
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 1)
        {
            dynamicFOV = true;
        }
        else if (PlayerPrefs.GetInt("dynamicFOV") == -1)
        {
            dynamicFOV = false;
        }

        if (PlayerPrefs.GetInt("showSpeedText") == 1)
        {
            speedTextObject.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("showSpeedText") == -1)
        {
            speedTextObject.SetActive(false);
        }

        speedText.text = "Speed: 0";

        if (!(!speedTextObject.activeSelf && FPSTextObject.activeSelf))
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -90);
        }
        else
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -45);
        }

        playerMovementManagerScript.normalMoveSpeed = PlayerPrefs.GetInt("playerSpeedTweak");
        playerMovementManagerScript.runSpeed = playerMovementManagerScript.normalMoveSpeed * 4 / 3;
        playerMovementManagerScript.crouchSpeed = playerMovementManagerScript.normalMoveSpeed * 2 / 3;
        playerInteractionManagerScript.throwForce = PlayerPrefs.GetInt("playerThrowForceTweak");
        playerMovementManagerScript.normalJumpForce = PlayerPrefs.GetInt("playerNormalJumpForceTweak");
        playerMovementManagerScript.bouncyJumpForce = PlayerPrefs.GetInt("playerBouncyJumpForceTweak");

        if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == -1)
        {
            playerMovementManagerScript.noFallDamage = false;
        }
        else if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == 1)
        {
            playerMovementManagerScript.noFallDamage = true;
        }
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

        if (cancelKeyPressed)
        {
            cancelKeyPressed = false;

            if (!playerTweaksMenuObject.activeSelf)
            {
                if (!settingsMenuObject.activeSelf)
                {
                    if (!gamePaused)
                    {
                        if (!playerSpawnAndSaveManagerScript.playerDied)
                        {
                            Pause();
                        }
                        else
                        {
                            QuittingGame();
                        }
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
            else
            {
                GoBackToSettingsMenu();
            }
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

    public void QuittingGame()
    {
        Application.Quit();
    }

    public void Settings()
    {
        FOVSlider.value = playerCameraManagerScript.normalFOV;

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
                maxFPSSlider.value = 9;
                maxFPSText.text = "Max FPS: Unlimited";
                break;
            case 8:
                maxFPSSlider.value = 8;
                maxFPSText.text = "Max FPS: V-Sync";
                break;
            case 7:
                maxFPSSlider.value = 7;
                maxFPSText.text = "Max FPS: 240";
                break;
            case 6:
                maxFPSSlider.value = 6;
                maxFPSText.text = "Max FPS: 180";
                break;
            case 5:
                maxFPSSlider.value = 5;
                maxFPSText.text = "Max FPS: 165";
                break;
            case 4:
                maxFPSSlider.value = 4;
                maxFPSText.text = "Max FPS: 144";
                break;
            case 3:
                maxFPSSlider.value = 3;
                maxFPSText.text = "Max FPS: 120";
                break;
            case 2:
                maxFPSSlider.value = 2;
                maxFPSText.text = "Max FPS: 75";
                break;
            case 1:
                maxFPSSlider.value = 1;
                maxFPSText.text = "Max FPS: 60";
                break;
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            increasedSensitivityToggle.isOn = false;
            mouseSensitivitySlider.value = playerCameraManagerScript.sensitivity;

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
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            increasedSensitivityToggle.isOn = true;
            mouseSensitivitySlider.value = playerCameraManagerScript.sensitivity - 200;

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

        if (PlayerPrefs.GetInt("showSpeedText") == 1)
        {
            showSpeedTextToggle.isOn = true;
        }
        else if (PlayerPrefs.GetInt("showSpeedText") == -1)
        {
            showSpeedTextToggle.isOn = false;
        }

        pauseMenuObject.SetActive(false);
        settingsMenuObject.SetActive(true);
        settingsMenuOpened = true;
    }

    public void FOV(float value)
    {
        PlayerPrefs.SetInt("FOV", (int)value);
        playerCameraManagerScript.normalFOV = value;

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
                FOVText.text = "FOV: " + (int)value;
                break;
        }
    }

    public void MaxFPS(float value)
    {
        switch (value)
        {
            case 9:
                PlayerPrefs.SetInt("maxFPS", 9);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                maxFPSText.text = "Max FPS: Unlimited";
                break;
            case 8:
                PlayerPrefs.SetInt("maxFPS", 8);
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
                maxFPSText.text = "Max FPS: V-Sync";
                break;
            case 7:
                PlayerPrefs.SetInt("maxFPS", 7);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 240;
                maxFPSText.text = "Max FPS: 240";
                break;
            case 6:
                PlayerPrefs.SetInt("maxFPS", 6);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 180;
                maxFPSText.text = "Max FPS: 180";
                break;
            case 5:
                PlayerPrefs.SetInt("maxFPS", 5);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 165;
                maxFPSText.text = "Max FPS: 165";
                break;
            case 4:
                PlayerPrefs.SetInt("maxFPS", 4);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 144;
                maxFPSText.text = "Max FPS: 144";
                break;
            case 3:
                PlayerPrefs.SetInt("maxFPS", 3);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 120;
                maxFPSText.text = "Max FPS: 120";
                break;
            case 2:
                PlayerPrefs.SetInt("maxFPS", 2);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 75;
                maxFPSText.text = "Max FPS: 75";
                break;
            case 1:
                PlayerPrefs.SetInt("maxFPS", 1);
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                maxFPSText.text = "Max FPS: 60";
                break;
        }
    }

    public void Sensitivity(float value)
    {
        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            PlayerPrefs.SetInt("mouseSensitivity", (int)value);

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
                    mouseSensitivityText.text = "Mouse Sensitivity: " + (int)value;
                    break;
            }
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            PlayerPrefs.SetInt("mouseSensitivity", (int)(value + 200));

            switch (value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + (int)(value + 200);
                    break;
            }
        }

        playerCameraManagerScript.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");
    }

    public void ShowFPSSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("showFPS", 1);
            FPSTextObject.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("showFPS", -1);
            FPSTextObject.SetActive(false);
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

    public void IncreasedSensitivitySwitch(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);

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
                    mouseSensitivityText.text = "Mouse Sensitivity: " + (int)mouseSensitivitySlider.value;
                    break;
            }

            PlayerPrefs.SetInt("increasedSensitivity", -1);
        }
        else
        {
            PlayerPrefs.SetInt("mouseSensitivity", (int)(mouseSensitivitySlider.value + 200));

            switch (mouseSensitivitySlider.value)
            {
                case 100:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 200:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = "Mouse Sensitivity: " + (int)(mouseSensitivitySlider.value + 200);
                    break;
            }

            PlayerPrefs.SetInt("increasedSensitivity", 1);
        }

        playerCameraManagerScript.sensitivity = PlayerPrefs.GetInt("mouseSensitivity");
    }

    public void DynamicFOVSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("dynamicFOV", 1);
            dynamicFOV = true;
        }
        else
        {
            PlayerPrefs.SetInt("dynamicFOV", -1);
            dynamicFOV = false;
        }
    }

    public void ShowSpeedTextSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("showSpeedText", 1);
            speedTextObject.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("showSpeedText", -1);
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

    public void ResetSettingsToDefault()
    {
        PlayerPrefs.SetInt("FOV", defaultFOV);
        FOVSlider.value = defaultFOV;
        playerCameraManagerScript.normalFOV = defaultFOV;

        switch (defaultFOV)
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
                FOVText.text = "FOV: " + defaultFOV;
                break;
        }

        PlayerPrefs.SetInt("maxFPS", defaultMaxFPS);

        switch (defaultMaxFPS)
        {
            case 9:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                maxFPSText.text = "Max FPS: Unlimited";
                break;
            case 8:
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
                maxFPSText.text = "Max FPS: V-Sync";
                break;
            case 7:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 240;
                maxFPSText.text = "Max FPS: 240";
                break;
            case 6:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 180;
                maxFPSText.text = "Max FPS: 180";
                break;
            case 5:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 165;
                maxFPSText.text = "Max FPS: 165";
                break;
            case 4:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 144;
                maxFPSText.text = "Max FPS: 144";
                break;
            case 3:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 120;
                maxFPSText.text = "Max FPS: 120";
                break;
            case 2:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 75;
                maxFPSText.text = "Max FPS: 75";
                break;
            case 1:
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                maxFPSText.text = "Max FPS: 60";
                break;
        }

        PlayerPrefs.SetInt("mouseSensitivity", defaultMouseSensitivity);
        mouseSensitivitySlider.value = defaultMouseSensitivity;
        PlayerPrefs.SetInt("increasedSensitivity", defaultIncreasedSensitivity);

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (defaultMouseSensitivity)
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
                    mouseSensitivityText.text = $"Mouse Sensitivity: {defaultMouseSensitivity}";
                    break;
            }

            increasedSensitivityToggle.isOn = false;
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (defaultMouseSensitivity)
            {
                case 300:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 400:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {defaultMouseSensitivity}";
                    break;
            }

            increasedSensitivityToggle.isOn = true;
        }

        playerCameraManagerScript.sensitivity = defaultMouseSensitivity;
        PlayerPrefs.SetInt("showFPS", defaultShowFPS);

        if (defaultShowFPS == 1)
        {
            showFPSToggle.isOn = true;
            FPSTextObject.SetActive(true);
        }
        else if (defaultShowFPS == -1)
        {
            showFPSToggle.isOn = false;
            FPSTextObject.SetActive(false);
        }

        PlayerPrefs.SetInt("dynamicFOV", defaultDynamicFOV);

        if (defaultDynamicFOV == 1)
        {
            dynamicFOVToggle.isOn = dynamicFOV = true;
        }
        else if (defaultDynamicFOV == -1)
        {
            dynamicFOVToggle.isOn = dynamicFOV = false;
        }

        PlayerPrefs.SetInt("showSpeedText", defaultShowSpeedText);

        if (defaultShowSpeedText == 1)
        {
            showSpeedTextToggle.isOn = true;
            speedTextObject.SetActive(true);
        }
        else if (defaultShowSpeedText == -1)
        {
            showSpeedTextToggle.isOn = false;
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

    public void GoBackToPauseMenu()
    {
        settingsMenuObject.SetActive(false);
        settingsMenuOpened = false;
        pauseMenuObject.SetActive(true);
    }

    public void PlayerTweaks()
    {
        playerSpeedTweakSlider.value = PlayerPrefs.GetInt("playerSpeedTweak");
        playerSpeedTweakText.text = $"Player Speed: {PlayerPrefs.GetInt("playerSpeedTweak")}";
        playerThrowForceTweakSlider.value = PlayerPrefs.GetInt("playerThrowForceTweak");
        playerThrowForceTweakText.text = $"Throw Force: {PlayerPrefs.GetInt("playerThrowForceTweak")}";
        playerNormalJumpForceSlider.value = PlayerPrefs.GetInt("playerNormalJumpForceTweak");
        playerNormalJumpForceTweakText.text = $"Normal Jump Force: {PlayerPrefs.GetInt("playerNormalJumpForceTweak")}";
        playerBouncyJumpForceSlider.value = PlayerPrefs.GetInt("playerBouncyJumpForceTweak");
        playerBouncyJumpForceTweakText.text = $"Bouncy Jump Force: {PlayerPrefs.GetInt("playerBouncyJumpForceTweak")}";

        if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == -1)
        {
            playerNoDamageTweakToggle.isOn = false;
        }
        else if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == 1)
        {
            playerNoDamageTweakToggle.isOn = true;
        }

        settingsMenuObject.SetActive(false);
        settingsMenuOpened = false;
        playerTweaksMenuObject.SetActive(true);
    }

    public void PlayerSpeedTweak(float value)
    {
        PlayerPrefs.SetInt("playerSpeedTweak", (int)value);
        playerMovementManagerScript.normalMoveSpeed = (int)value;
        playerMovementManagerScript.runSpeed = playerMovementManagerScript.normalMoveSpeed * 4 / 3;
        playerMovementManagerScript.crouchSpeed = playerMovementManagerScript.normalMoveSpeed * 2 / 3;
        playerSpeedTweakText.text = $"Player Speed: {(int)value}";
    }

    public void PlayerThrowForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerThrowForceTweak", (int)value);
        playerInteractionManagerScript.throwForce = (int)value;
        playerThrowForceTweakText.text = $"Throw Force: {(int)value}";
    }

    public void PlayerNormalJumpForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerNormalJumpForceTweak", (int)value);
        playerMovementManagerScript.normalJumpForce = (int)value;
        playerNormalJumpForceTweakText.text = $"Normal Jump Force: {(int)value}";
    }

    public void PlayerBouncyJumpForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerBouncyJumpForceTweak", (int)value);
        playerMovementManagerScript.bouncyJumpForce = (int)value;
        playerBouncyJumpForceTweakText.text = $"Bouncy Jump Force: {(int)value}";
    }

    public void PlayerNoFallDamageTweak(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", -1);
            playerMovementManagerScript.noFallDamage = false;
        }
        else
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", 1);
            playerMovementManagerScript.noFallDamage = true;
        }
    }

    public void ResetPlayerTweaksToDefault()
    {
        PlayerPrefs.SetInt("playerSpeedTweak", defaultPlayerSpeedTweak);
        playerSpeedTweakSlider.value = defaultPlayerSpeedTweak;
        playerMovementManagerScript.normalMoveSpeed = defaultPlayerSpeedTweak;
        playerMovementManagerScript.runSpeed = playerMovementManagerScript.normalMoveSpeed * 4 / 3;
        playerMovementManagerScript.crouchSpeed = playerMovementManagerScript.normalMoveSpeed * 2 / 3;
        playerSpeedTweakText.text = $"Player Speed: {defaultPlayerSpeedTweak}";
        PlayerPrefs.SetInt("playerThrowForceTweak", defaultPlayerThrowForceTweak);
        playerThrowForceTweakSlider.value = defaultPlayerThrowForceTweak;
        playerInteractionManagerScript.throwForce = defaultPlayerThrowForceTweak;
        playerThrowForceTweakText.text = $"Throw Force: {defaultPlayerThrowForceTweak}";
        PlayerPrefs.SetInt("playerNormalJumpForceTweak", defaultPlayerNormalJumpForceTweak);
        playerNormalJumpForceSlider.value = defaultPlayerNormalJumpForceTweak;
        playerMovementManagerScript.normalJumpForce = defaultPlayerNormalJumpForceTweak;
        playerNormalJumpForceTweakText.text = $"Normal Jump Force: {defaultPlayerNormalJumpForceTweak}";
        PlayerPrefs.SetInt("playerBouncyJumpForceTweak", defaultPlayerBouncyJumpForceTweak);
        playerBouncyJumpForceSlider.value = defaultPlayerBouncyJumpForceTweak;
        playerMovementManagerScript.bouncyJumpForce = defaultPlayerBouncyJumpForceTweak;
        playerBouncyJumpForceTweakText.text = $"Bouncy Jump Force: {defaultPlayerBouncyJumpForceTweak}";
        PlayerPrefs.SetInt("playerNoFallDamageTweak", defaultPlayerNoFallDamageTweak);

        if (defaultPlayerNoFallDamageTweak == -1)
        {
            playerNoDamageTweakToggle.isOn = playerMovementManagerScript.noFallDamage = false;
        }
        else if (defaultPlayerNoFallDamageTweak == 1)
        {
            playerNoDamageTweakToggle.isOn = playerMovementManagerScript.noFallDamage = true;
        }
    }

    public void GoBackToSettingsMenu()
    {
        playerTweaksMenuObject.SetActive(false);
        settingsMenuObject.SetActive(true);
        settingsMenuOpened = true;
    }
}
