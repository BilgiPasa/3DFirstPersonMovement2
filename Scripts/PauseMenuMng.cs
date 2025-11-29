using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuMng : MonoBehaviour
{
    //* Attach this script to the UserInterface game object.
    //* "PauseMenu"yü aktive et ve "SettingsButton"ın "Text"ine gel. O "Text"in "Outline" materyalinin "Face"inin "Dilate"sini 0.1 yap. "Outline"ının rengini bembeyaz yap ve "Thickness"ını 0.05 yap. Ardından "PauseMenu"yü inaktif yap.

    int[] last5FPS = new int[5];
    int dFOV = 90, dMaxFPS = 8, dMouseSensitivity = 100, dShowFPS = 1, dIncreasedSensitivity = -1, dDynamicFOV = 1, dShowSpeedText = 1, dPlayerSpeedTweak = 9, dPlayerThrowForceTweak = 60, dPlayerNormalJumpForceTweak = 20, dPlayerBouncyJumpForceTweak = 55, dPlayerNoFallDamageTweak = -1; // Default values
    int counter;
    bool gamePaused, dynamicFOV, settingsMenuOpened, cancelKeyPressed;
    RectTransform FPSTextRectTransform;
    TextMeshProUGUI speedText, FPSText;
    PlayerInteractionMng playerInteractionMng;
    PlayerMovementMng playerMovementMng;
    PlayerSpawnAndSaveMng playerSpawnAndSaveMng;
    PlayerStatusMng playerStatusMng;
    InputSystem_Actions inputActions;
    [SerializeField] GameObject playerObj, pauseMenuObj, settingsMenuObj, playerTweaksMenuObj, speedTextObj, FPSTextObj;
    [SerializeField] TextMeshProUGUI FOVText, mouseSensitivityText, maxFPSText, playerSpeedTweakText, playerThrowForceTweakText, playerNormalJumpForceTweakText, playerBouncyJumpForceTweakText, gameVersionText;
    [SerializeField] Toggle dynamicFOVToggle, showSpeedTextToggle, increasedSensitivityToggle, showFPSToggle, playerNoDamageTweakToggle;
    [SerializeField] Slider FOVSlider, mouseSensitivitySlider, maxFPSSlider, playerSpeedTweakSlider, playerThrowForceTweakSlider, playerNormalJumpForceSlider, playerBouncyJumpForceSlider;
    [SerializeField] PlayerCamMng playerCamMng;

    public bool GamePaused
    {
        get => gamePaused;
    }

    public bool DynamicFOV
    {
        get => dynamicFOV;
    }

    public bool SettingsMenuOpened
    {
        get => settingsMenuOpened;
    }

    void Start()
    {
        FPSTextRectTransform = FPSTextObj.GetComponent<RectTransform>();
        speedText = speedTextObj.GetComponent<TextMeshProUGUI>();
        FPSText = FPSTextObj.GetComponent<TextMeshProUGUI>();
        playerInteractionMng = playerObj.GetComponent<PlayerInteractionMng>();
        playerMovementMng = playerObj.GetComponent<PlayerMovementMng>();
        playerSpawnAndSaveMng = GetComponent<PlayerSpawnAndSaveMng>();
        playerStatusMng = GetComponent<PlayerStatusMng>();
        inputActions = new InputSystem_Actions();
        inputActions.UI.Enable();
        inputActions.UI.Cancel.performed += CancelInputPerformed;

        if (PlayerPrefs.GetInt("FOV") == 0)
        {
            PlayerPrefs.SetInt("FOV", dFOV);
        }

        if (PlayerPrefs.GetInt("maxFPS") == 0)
        {
            PlayerPrefs.SetInt("maxFPS", dMaxFPS);
        }

        if (PlayerPrefs.GetInt("mouseSensitivity") == 0)
        {
            PlayerPrefs.SetInt("mouseSensitivity", dMouseSensitivity);
        }

        if (PlayerPrefs.GetInt("showFPS") == 0)
        {
            PlayerPrefs.SetInt("showFPS", dShowFPS);
        }

        if (PlayerPrefs.GetInt("increasedSensitivity") == 0)
        {
            PlayerPrefs.SetInt("increasedSensitivity", dIncreasedSensitivity);
        }

        if (PlayerPrefs.GetInt("dynamicFOV") == 0)
        {
            PlayerPrefs.SetInt("dynamicFOV", dDynamicFOV);
        }

        if (PlayerPrefs.GetInt("showSpeedText") == 0)
        {
            PlayerPrefs.SetInt("showSpeedText", dShowSpeedText);
        }

        if (PlayerPrefs.GetInt("playerSpeedTweak") == 0)
        {
            PlayerPrefs.SetInt("playerSpeedTweak", dPlayerSpeedTweak);
        }

        if (PlayerPrefs.GetInt("playerThrowForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerThrowForceTweak", dPlayerThrowForceTweak);
        }

        if (PlayerPrefs.GetInt("playerNormalJumpForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerNormalJumpForceTweak", dPlayerNormalJumpForceTweak);
        }

        if (PlayerPrefs.GetInt("playerBouncyJumpForceTweak") == 0)
        {
            PlayerPrefs.SetInt("playerBouncyJumpForceTweak", dPlayerBouncyJumpForceTweak);
        }

        if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == 0)
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", dPlayerNoFallDamageTweak);
        }

        playerCamMng.NormalFOV = PlayerPrefs.GetInt("FOV");

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

        playerCamMng.Sensitivity = PlayerPrefs.GetInt("mouseSensitivity");

        if (PlayerPrefs.GetInt("showFPS") == 1)
        {
            FPSTextObj.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("showFPS") == -1)
        {
            FPSTextObj.SetActive(false);
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
            speedTextObj.SetActive(true);
        }
        else if (PlayerPrefs.GetInt("showSpeedText") == -1)
        {
            speedTextObj.SetActive(false);
        }

        speedText.text = "Speed: 0";
        gameVersionText.text = $"v{Application.version}";

        if (!(!speedTextObj.activeSelf && FPSTextObj.activeSelf))
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -90);
        }
        else
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -45);
        }

        playerMovementMng.NormalMoveSpeed = PlayerPrefs.GetInt("playerSpeedTweak");
        playerMovementMng.RunSpeed = playerMovementMng.NormalMoveSpeed * 4 / 3;
        playerMovementMng.CrouchSpeed = playerMovementMng.NormalMoveSpeed * 2 / 3;
        playerInteractionMng.ThrowForce = PlayerPrefs.GetInt("playerThrowForceTweak");
        playerMovementMng.NormalJumpForce = PlayerPrefs.GetInt("playerNormalJumpForceTweak");
        playerMovementMng.BouncyJumpForce = PlayerPrefs.GetInt("playerBouncyJumpForceTweak");

        if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == -1)
        {
            playerMovementMng.NoFallDamage = false;
        }
        else if (PlayerPrefs.GetInt("playerNoFallDamageTweak") == 1)
        {
            playerMovementMng.NoFallDamage = true;
        }
    }

    void CancelInputPerformed(InputAction.CallbackContext context)
    {
        cancelKeyPressed = true;
    }

    void Update()
    {
        if (!(gamePaused || playerSpawnAndSaveMng.PlayerDied))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            speedText.text = $"Speed: {playerStatusMng.FlatVelMag}";
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!gamePaused && FPSTextObj.activeSelf && 1 / Time.deltaTime <= int.MaxValue)
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

            if (!playerTweaksMenuObj.activeSelf)
            {
                if (!settingsMenuObj.activeSelf)
                {
                    if (!gamePaused)
                    {
                        if (!playerSpawnAndSaveMng.PlayerDied)
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
        playerSpawnAndSaveMng.SavingTheGame();
        pauseMenuObj.SetActive(true);
        Time.timeScale = 0;
        gamePaused = true;
    }

    public void Resume()
    {
        pauseMenuObj.SetActive(false);
        Time.timeScale = 1;
        gamePaused = false;
    }

    public void QuittingGame()
    {
        Application.Quit();
    }

    public void Settings()
    {
        FOVSlider.value = playerCamMng.NormalFOV;

        switch (playerCamMng.NormalFOV)
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
                FOVText.text = $"FOV: {playerCamMng.NormalFOV}";
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
            mouseSensitivitySlider.value = playerCamMng.Sensitivity;

            switch (playerCamMng.Sensitivity)
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
                    mouseSensitivityText.text = $"Mouse Sensitivity: {playerCamMng.Sensitivity}";
                    break;
            }
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            increasedSensitivityToggle.isOn = true;
            mouseSensitivitySlider.value = playerCamMng.Sensitivity - 200;

            switch (playerCamMng.Sensitivity)
            {
                case 300:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 400:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {playerCamMng.Sensitivity}";
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

        pauseMenuObj.SetActive(false);
        settingsMenuObj.SetActive(true);
        settingsMenuOpened = true;
    }

    public void FOV(float value)
    {
        PlayerPrefs.SetInt("FOV", (int)value);
        playerCamMng.NormalFOV = value;

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

        playerCamMng.Sensitivity = PlayerPrefs.GetInt("mouseSensitivity");
    }

    public void ShowFPSSwitch(bool active)
    {
        if (active)
        {
            PlayerPrefs.SetInt("showFPS", 1);
            FPSTextObj.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("showFPS", -1);
            FPSTextObj.SetActive(false);
        }

        if (!(!speedTextObj.activeSelf && FPSTextObj.activeSelf))
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

        playerCamMng.Sensitivity = PlayerPrefs.GetInt("mouseSensitivity");
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
            speedTextObj.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("showSpeedText", -1);
            speedTextObj.SetActive(false);
        }

        if (!(!speedTextObj.activeSelf && FPSTextObj.activeSelf))
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -90);
        }
        else
        {
            FPSTextRectTransform.anchoredPosition = new Vector2(FPSTextRectTransform.anchoredPosition.x, -45);
        }
    }

    public void ResetSettings()
    {
        PlayerPrefs.SetInt("FOV", dFOV);
        FOVSlider.value = dFOV;
        playerCamMng.NormalFOV = dFOV;

        switch (dFOV)
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
                FOVText.text = "FOV: " + dFOV;
                break;
        }

        PlayerPrefs.SetInt("maxFPS", dMaxFPS);
        maxFPSSlider.value = dMaxFPS;

        switch (dMaxFPS)
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

        PlayerPrefs.SetInt("mouseSensitivity", dMouseSensitivity);
        mouseSensitivitySlider.value = dMouseSensitivity;
        PlayerPrefs.SetInt("increasedSensitivity", dIncreasedSensitivity);

        if (PlayerPrefs.GetInt("increasedSensitivity") == -1)
        {
            switch (dMouseSensitivity)
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
                    mouseSensitivityText.text = $"Mouse Sensitivity: {dMouseSensitivity}";
                    break;
            }

            increasedSensitivityToggle.isOn = false;
        }
        else if (PlayerPrefs.GetInt("increasedSensitivity") == 1)
        {
            switch (dMouseSensitivity)
            {
                case 300:
                    mouseSensitivityText.text = "Mouse Sensitivity: VERY FAST";
                    break;
                case 400:
                    mouseSensitivityText.text = "Mouse Sensitivity: MAXIMUM";
                    break;
                default:
                    mouseSensitivityText.text = $"Mouse Sensitivity: {dMouseSensitivity}";
                    break;
            }

            increasedSensitivityToggle.isOn = true;
        }

        playerCamMng.Sensitivity = dMouseSensitivity;
        PlayerPrefs.SetInt("showFPS", dShowFPS);

        if (dShowFPS == 1)
        {
            showFPSToggle.isOn = true;
            FPSTextObj.SetActive(true);
        }
        else if (dShowFPS == -1)
        {
            showFPSToggle.isOn = false;
            FPSTextObj.SetActive(false);
        }

        PlayerPrefs.SetInt("dynamicFOV", dDynamicFOV);

        if (dDynamicFOV == 1)
        {
            dynamicFOVToggle.isOn = dynamicFOV = true;
        }
        else if (dDynamicFOV == -1)
        {
            dynamicFOVToggle.isOn = dynamicFOV = false;
        }

        PlayerPrefs.SetInt("showSpeedText", dShowSpeedText);

        if (dShowSpeedText == 1)
        {
            showSpeedTextToggle.isOn = true;
            speedTextObj.SetActive(true);
        }
        else if (dShowSpeedText == -1)
        {
            showSpeedTextToggle.isOn = false;
            speedTextObj.SetActive(false);
        }

        if (!(!speedTextObj.activeSelf && FPSTextObj.activeSelf))
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
        settingsMenuObj.SetActive(false);
        settingsMenuOpened = false;
        pauseMenuObj.SetActive(true);
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

        settingsMenuObj.SetActive(false);
        settingsMenuOpened = false;
        playerTweaksMenuObj.SetActive(true);
    }

    public void PlayerSpeedTweak(float value)
    {
        PlayerPrefs.SetInt("playerSpeedTweak", (int)value);
        playerMovementMng.NormalMoveSpeed = (int)value;
        playerMovementMng.RunSpeed = playerMovementMng.NormalMoveSpeed * 4 / 3;
        playerMovementMng.CrouchSpeed = playerMovementMng.NormalMoveSpeed * 2 / 3;
        playerSpeedTweakText.text = $"Player Speed: {(int)value}";
    }

    public void PlayerThrowForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerThrowForceTweak", (int)value);
        playerInteractionMng.ThrowForce = (int)value;
        playerThrowForceTweakText.text = $"Throw Force: {(int)value}";
    }

    public void PlayerNormalJumpForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerNormalJumpForceTweak", (int)value);
        playerMovementMng.NormalJumpForce = (int)value;
        playerNormalJumpForceTweakText.text = $"Normal Jump Force: {(int)value}";
    }

    public void PlayerBouncyJumpForceTweak(float value)
    {
        PlayerPrefs.SetInt("playerBouncyJumpForceTweak", (int)value);
        playerMovementMng.BouncyJumpForce = (int)value;
        playerBouncyJumpForceTweakText.text = $"Bouncy Jump Force: {(int)value}";
    }

    public void PlayerNoFallDamageTweak(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", -1);
            playerMovementMng.NoFallDamage = false;
        }
        else
        {
            PlayerPrefs.SetInt("playerNoFallDamageTweak", 1);
            playerMovementMng.NoFallDamage = true;
        }
    }

    public void ResetPlayerTweaks()
    {
        PlayerPrefs.SetInt("playerSpeedTweak", dPlayerSpeedTweak);
        playerSpeedTweakSlider.value = dPlayerSpeedTweak;
        playerMovementMng.NormalMoveSpeed = dPlayerSpeedTweak;
        playerMovementMng.RunSpeed = playerMovementMng.NormalMoveSpeed * 4 / 3;
        playerMovementMng.CrouchSpeed = playerMovementMng.NormalMoveSpeed * 2 / 3;
        playerSpeedTweakText.text = $"Player Speed: {dPlayerSpeedTweak}";
        PlayerPrefs.SetInt("playerThrowForceTweak", dPlayerThrowForceTweak);
        playerThrowForceTweakSlider.value = dPlayerThrowForceTweak;
        playerInteractionMng.ThrowForce = dPlayerThrowForceTweak;
        playerThrowForceTweakText.text = $"Throw Force: {dPlayerThrowForceTweak}";
        PlayerPrefs.SetInt("playerNormalJumpForceTweak", dPlayerNormalJumpForceTweak);
        playerNormalJumpForceSlider.value = dPlayerNormalJumpForceTweak;
        playerMovementMng.NormalJumpForce = dPlayerNormalJumpForceTweak;
        playerNormalJumpForceTweakText.text = $"Normal Jump Force: {dPlayerNormalJumpForceTweak}";
        PlayerPrefs.SetInt("playerBouncyJumpForceTweak", dPlayerBouncyJumpForceTweak);
        playerBouncyJumpForceSlider.value = dPlayerBouncyJumpForceTweak;
        playerMovementMng.BouncyJumpForce = dPlayerBouncyJumpForceTweak;
        playerBouncyJumpForceTweakText.text = $"Bouncy Jump Force: {dPlayerBouncyJumpForceTweak}";
        PlayerPrefs.SetInt("playerNoFallDamageTweak", dPlayerNoFallDamageTweak);

        if (dPlayerNoFallDamageTweak == -1)
        {
            playerNoDamageTweakToggle.isOn = playerMovementMng.NoFallDamage = false;
        }
        else if (dPlayerNoFallDamageTweak == 1)
        {
            playerNoDamageTweakToggle.isOn = playerMovementMng.NoFallDamage = true;
        }
    }

    public void GoBackToSettingsMenu()
    {
        playerTweaksMenuObj.SetActive(false);
        settingsMenuObj.SetActive(true);
        settingsMenuOpened = true;
    }
}
