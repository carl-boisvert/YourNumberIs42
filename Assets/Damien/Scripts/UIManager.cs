using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum UIType {
    Main,
    Options,
    Credits,
    HUD,
    Pause
}

public enum InteractionType {
    None,
    Flashlight,
    Interact,
    Manual
}

/// <summary>
/// <c>UIManager</c> controls all text alterations, UI switching, modification of preferences, etc
/// </summary>

[RequireComponent(typeof(Animator))]
public class UIManager : MonoBehaviour {
    #region Variables
    //Singleton for global access to public methods, using UIManager.Instance.x where x is the public method name
    private static UIManager instance;
    public static UIManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType<UIManager>();
                if (instance == null) {
                    GameObject managerClone = new GameObject();
                    managerClone.AddComponent<UIManager>();
                    managerClone.name = "UIManager";
                }
            }
            return instance;
        }
        //setter in Awake
    }

    [Header("Intialization")]
    [Tooltip("List of GameObjects that hold UI states to be switched between throughout the game loop")]
    [SerializeField] private List<GameObject> _uiStates = null;
    [Tooltip("Select a UI to start the game with, should be UIType.Main when shipping the game")]
    [SerializeField] private UIType _startingUI = UIType.HUD;
    [Tooltip("Enable to override input control for Escape (Pause) and Tab (Manual)")]
    [SerializeField] private bool _debugMode = false;
    [Space(15)]

    [Header("Audio")]
    [Tooltip("Sound effects used for interactable EventTriggers, currently given a random pitch to reuse effects without it being repetitve")]
    [SerializeField] private AudioClip[] _highlightSound;
    [Tooltip("A unique AudioSource intended for exclusive use for UI sound effects that isn't affected by the AudioListener being paused")]
    [SerializeField] private AudioSource _audioSource;
    [Space(15)]

    [Header("Cameras")]
    [SerializeField] private CinemachineBrain _brain;
    [SerializeField] private Animator _cameraAnimator;
    [Space(15)]

    [Header("Main Menu")]
    [Tooltip("Button to focus on when the Main Menu is called")]
    [SerializeField] private Button _startButton = null;
    [Tooltip("Button container that will be hidden when Quit Confirmation is called")]
    [SerializeField] private GameObject _mainButtons = null;
    [Tooltip("Quit Confirmation container that starts hidden, activated when the Quit Button is selected")]
    [SerializeField] private GameObject _quitConfirmation = null;
    //Needed to transition from the Start Button to the HUD"
    private Animator _introAnimator = null;
    [Space(15)]

    [Header("Credits")]
    [Tooltip("Button to focus on when the Credits UI is called")]
    [SerializeField] private Button _creditsBackButton = null;
    [Tooltip("")]
    [SerializeField] private GameObject _blackPanel;
    [SerializeField] private CanvasGroup _canvasGroup = null;
    private bool _isEndGame = false;
    [Space(15)]

    [Header("Options")]
    [Tooltip("Button to focus on when the Options UI is called")]
    [SerializeField] private Button _controlsButton = null;
    [Tooltip("Button to focus on when the Controls UI is called")]
    [SerializeField] private Button _backToOptionsButton = null;
    [Space(5)]
    [Tooltip("Screen that shows the control scheme, initially hidden")]
    [SerializeField] private GameObject _controlsScreen = null;
    [Tooltip("Container for player preference options")]
    [SerializeField] private GameObject _preferencesScreen = null;
    [Space(5)]
    [Tooltip("Invert vertical camera controls. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Toggle _invertedControlsToggle = null;
    [Tooltip("Enable/disable camera shake. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Toggle _cameraShakeToggle = null;
    [Space(5)]
    [Tooltip("Using the value of the slider, the Master volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _masterVolume = null;
    [Tooltip("Using the value of the slider, the Music volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _musicVolume = null;
    [Tooltip("Using the value of the slider, the Ambience volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _ambienceVolume = null;
    [Tooltip("Using the value of the slider, the SFX volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _sfxVolume = null;
    [Space(5)]
    [Tooltip("Using the value of the slider, the CinemachineVirualCamera Aim POV horizontal/vertical axis speeds are set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _lookSensitivity = null;
    [Tooltip("Using the value of the slider, the CinemachineVirtualCamera Lens Horizontal FOV is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _fieldOfView = null;
    [Tooltip("Using the value of the slider, the Global Volume Bloom intensity is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _bloom = null;
    [Tooltip("Using the value from the Dropdown, updates the current Quality Level of the project. Dropdown values match order and names shown in Edit/Project Settings/Quality. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private TMP_Dropdown _qualityDropdown = null;
    [Space(15)]

    [Header("Pause Menu")]
    [Tooltip("Button to focus on when the Pause UI is called")]
    [SerializeField] private Button _continueButton = null;
    [Tooltip("Button to focus on when the confirmation container pops up to go back to the Main Menu UI")]
    [SerializeField] private Button _noConfirmationButton = null;
    [Space(5)]
    [Tooltip("Interactables container that will be hidden when Main Menu confirmation is called")]
    [SerializeField] private GameObject _pauseInteractables = null;
    [Tooltip("Confirmation container to go back to the Main Menu UI")]
    [SerializeField] private GameObject _mainConfirmation = null;
    [Space(5)]
    [Tooltip("Invert vertical camera controls. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Toggle _invertedControlsTogglePaused = null;
    [Tooltip("Enable/disable camera shake. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Toggle _cameraShakeTogglePaused = null;
    [Space(5)]
    [Tooltip("Using the value of the slider, the Master volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _masterVolumePaused = null;
    [Tooltip("Using the value of the slider, the Music volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _musicVolumePaused = null;
    [Tooltip("Using the value of the slider, the Ambience volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _ambienceVolumePaused = null;
    [Tooltip("Using the value of the slider, the SFX volume is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _sfxVolumePaused = null;
    [Space(5)]
    [Tooltip("Using the value of the slider, the CinemachineVirualCamera Aim POV horizontal/vertical axis speeds are set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _lookSensitivityPaused = null;
    [Tooltip("Using the value of the slider, the CinemachineVirtualCamera Lens Horizontal FOV is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _fieldOfViewPaused = null;
    [Tooltip("Using the value of the slider, the Global Volume Bloom intensity is set and saved to PlayerPrefs. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private Slider _bloomPaused = null;
    [Tooltip("Using the value from the Dropdown, updates the current Quality Level of the project. Dropdown values match order and names shown in Edit/Project Settings/Quality. The word used before the underscore symbol in the name is the exact string used in the PreferenceManager.")]
    [SerializeField] private TMP_Dropdown _qualityDropdownPaused = null;
    [Space(15)]

    [Header("HUD")]
    [Tooltip("")]
    [SerializeField] private GameObject _hintContainer = null;
    [Tooltip("")]
    [SerializeField] private TextMeshProUGUI _hintText = null;
    [Space(5)]
    [Tooltip("")]
    [SerializeField] private GameObject _interactContainer = null;
    [Tooltip("")]
    [SerializeField] private TextMeshProUGUI _interactText = null;
    [Space(5)]
    [Tooltip("")]
    [SerializeField] private GameObject _reticleContainer = null;
    [Tooltip("")]
    [SerializeField] private Image _progressImage = null;
    [SerializeField] private Vector3 _maxReticleScale = Vector3.one;

    private UIType _currentType;
    private GameObject _currentState = null;
    private GameObject _previousState = null;
    private GameObject _lastElementSelected = null;
    private bool _progressStarted = false;
    #endregion

    #region Unity Events
    private void Awake() {
        instance = this;
        _introAnimator = GetComponent<Animator>();
        _brain = GameObject.FindObjectOfType<CinemachineBrain>();
    }

    private void Start() {
        InitUI();
        UIEventSubs(true);
    }

    private void OnDestroy() {
        UIEventSubs(false);
    }

    private void Update() {
        if (_debugMode) {
            if (Keyboard.current.escapeKey.wasPressedThisFrame) {
                ChangeUITypeOnPausePress();
            }
        }

        if (_currentType != UIType.HUD) {
            if (EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_lastElementSelected);
            }
        }
    }
    #endregion

    #region Private Methods
    private void UIEventSubs(bool isStart) {
        if (isStart) {
            // Subscribe to Events here
            Events.OnHoldProgress += OnProgressChange;
        }
        else {
            // Unsubscribe Events here
            Events.OnHoldProgress -= OnProgressChange;
        }
    }

    private void OnProgressChange(float progress) {
        _progressImage.fillAmount = progress;

        if (!_progressStarted) {
            _progressStarted = true;
            StartCoroutine(ScaleReticle());
        }

        if(progress == 0) {
            ScaleObject(_reticleContainer, _reticleContainer.transform.localScale, Vector3.one, 1f);
        }        
    }

    private void ScaleObject(GameObject objectToScale, Vector3 start, Vector3 end, float speed) {
        objectToScale.transform.localScale = Vector3.Lerp(start, end, speed);
    }

    private IEnumerator ScaleReticle() {
        while(_progressImage.fillAmount > 0f) {
            ScaleObject(_reticleContainer, _reticleContainer.transform.localScale, _maxReticleScale, 0.1f);
            yield return null;
        }

        ScaleObject(_reticleContainer, _reticleContainer.transform.localScale, Vector3.one, 1f);
        _progressStarted = false;
    }

    public void ChangeUITypeOnPausePress() {
        if (_currentType == UIType.Pause) {
            PauseMenu(false);
        }
        else if (_currentType == UIType.HUD) {
            PauseMenu(true);
        }
    }

    private void InitUI() {
        //Need to make sure that the UI SFX do not get paused when the game pauses
        _audioSource.ignoreListenerPause = true;

        //Reset all UI in case something was left open in-editor
        foreach (GameObject state in _uiStates) {
            state.SetActive(false);
        }

        _blackPanel.SetActive(true);
        _canvasGroup = _blackPanel.GetComponent<CanvasGroup>();
        _blackPanel.SetActive(false);
        
        _quitConfirmation.SetActive(false);
        _interactContainer.SetActive(false);
        _hintContainer.SetActive(false);
        _controlsScreen.SetActive(false);
        _preferencesScreen.SetActive(true);

        UpdateOptions(true);

        //Find and set starting UI
        _currentState = FindUI(_startingUI);
        if (_currentState != null) {
            UpdateUI(_startingUI);
            FocusOnButton(_startingUI);
        }

        //Finish up
        CheckTimeScale(_startingUI);
        _currentType = _startingUI;
    }

    private void CheckTimeScale(UIType type) {
        if (type == UIType.HUD) {
            Events.OnPauseGame?.Invoke(true);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputManager.Instance.SwitchMap("Player");
        }
        else {
            Events.OnPauseGame?.Invoke(false);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            InputManager.Instance.SwitchMap("UI");
        }

        SwitchCamera(type);
    }

    private void FocusOnButton(UIType type) {
        GameObject button = null;

        switch (type) {
            case UIType.Main:
                button = _startButton.gameObject;
                break;
            case UIType.Options:
                button = _controlsButton.gameObject;
                break;
            case UIType.Credits:
                button = _creditsBackButton.gameObject;
                break;
            case UIType.Pause:
                button = _continueButton.gameObject;
                break;
            case UIType.HUD:
            default:
                //Debug.Log("No button needed");
                break;
        }

        if (button != null) {
            if(_lastElementSelected != null) UnHighlightButton(_lastElementSelected);
            EventSystem.current.SetSelectedGameObject(button);
            HighlightButton(button);
        }        
    }

    private GameObject FindUI(UIType type) {
        foreach (GameObject state in _uiStates) {
            if (state.name.Contains(type.ToString())) {
                return state;
            }
        }

        return null;
    }

    private void UpdateOptions(bool isMainOptions = true) {
        if (isMainOptions) {
            _invertedControlsToggle.isOn = PreferenceManager.Instance.GetBoolPref("InvertedControls");
            _cameraShakeToggle.isOn = PreferenceManager.Instance.GetBoolPref("CameraShake");
            _masterVolume.value = PreferenceManager.Instance.GetFloatPref("Master");
            _musicVolume.value = PreferenceManager.Instance.GetFloatPref("Music");
            _ambienceVolume.value = PreferenceManager.Instance.GetFloatPref("Ambience");
            _sfxVolume.value = PreferenceManager.Instance.GetFloatPref("SFX");
            _lookSensitivity.value = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
            _fieldOfView.value = PreferenceManager.Instance.GetFloatPref("FieldOfView");
            _bloom.value = PreferenceManager.Instance.GetFloatPref("Bloom");
            _qualityDropdown.value = PreferenceManager.Instance.GetIntPref("Quality");
        }
        else {
            _invertedControlsTogglePaused.isOn = PreferenceManager.Instance.GetBoolPref("InvertedControls");
            _cameraShakeTogglePaused.isOn = PreferenceManager.Instance.GetBoolPref("CameraShake");
            _masterVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Master");
            _musicVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Music");
            _ambienceVolumePaused.value = PreferenceManager.Instance.GetFloatPref("Ambience");
            _sfxVolumePaused.value = PreferenceManager.Instance.GetFloatPref("SFX");
            _lookSensitivityPaused.value = PreferenceManager.Instance.GetFloatPref("LookSensitivity");
            _fieldOfViewPaused.value = PreferenceManager.Instance.GetFloatPref("FieldOfView");
            _bloomPaused.value = PreferenceManager.Instance.GetFloatPref("Bloom");
            _qualityDropdownPaused.value = PreferenceManager.Instance.GetIntPref("Quality");
        }  
    }
    #endregion

    #region Public Methods
    public void FadeFromBlack() {
        
    }

    public void HideHintText() {
        _hintText.text = "";

        if (_hintContainer.activeInHierarchy) {
            _hintContainer.SetActive(false);
        }
    }

    public void RevealHintText(InteractionType type = InteractionType.None, string hintText = "") {
        HideHintText();

        if (hintText.Length == 0) {
            _hintText.text = "Why does this happen to me!?";
        }
        else {
            _hintText.text = hintText;
        }

        if (!_hintContainer.activeInHierarchy) {
            _hintContainer.SetActive(true);
        }
    }

    public void RevealInteractionText(string text = "") {
        if (text.Length == 0) {
            _interactText.text = "Weirder things are happening right now, I'd worry about the knocking if I were you...";
        }
        else {
            _interactText.text = text;
        }

        if (!_interactContainer.activeInHierarchy) {
            _interactContainer.SetActive(true);
        }
    }

    public void HideInteractionText() {
        _interactText.text = "";

        if (_interactContainer.activeInHierarchy) {
            _interactContainer.SetActive(false);
        }        
    }

    public void UpdateUI(UIType type, bool startofGame = false) {
        _previousState = _currentState;
        _currentState = FindUI(type);

        if (_currentState != null) {
            if (!(type == UIType.HUD && startofGame)) {
                _previousState.SetActive(false);
            }
            
            _currentState.SetActive(true);

            FocusOnButton(type);
            _currentType = type;
            CheckTimeScale(type);
        }
    }

    private void SwitchCamera(UIType type) {
        string animationName = "";

        switch (type) {
            case UIType.Main:
                animationName = "MainMenu";
                break;
            case UIType.Options:
                animationName = "OptionsMenu";
                break;
            case UIType.Credits:
                animationName = "CreditsMenu";
                break;
            case UIType.HUD:
            case UIType.Pause:
                animationName = "PlayerCam";
                break;
            default:
                break;
        }

        if (animationName == "") {
            return;
        }

        StartCoroutine(WaitForCameraTransition(animationName));
    }

    private IEnumerator WaitForCameraTransition(string name) {
        Debug.Log("Transition Camera" + name);
        _cameraAnimator.Play(name);

        while (_brain.IsBlending) {
            yield return new WaitForEndOfFrame();
        }
    }

    public UIType GetCurrentUIType() {
        return _currentType;
    }

    public void PauseMenu(bool gamePaused) {
        if (gamePaused) {
            UpdateOptions(false);
            AudioManager.Instance.PauseAudio(true);
            UpdateUI(UIType.Pause);
        }
        else {
            AudioManager.Instance.PauseAudio(false);
            UpdateUI(UIType.HUD);
        }
    }

    public void ElementHighlighted() {
        if(_highlightSound != null) {
            if (_highlightSound.Length > 0) {
                int index = Random.Range(0, _highlightSound.Length);
                _audioSource.pitch = Random.Range(0f, 3f);
                _audioSource.PlayOneShot(_highlightSound[index]);
            }            
        }
    }

    public void SetLastElementSelected(GameObject element) {
        _lastElementSelected = element;
    }

    public void HighlightButton(GameObject button) {
        TextHelper text = button.GetComponentInChildren<TextHelper>();
        if(text == null) {
            return;
        }

        text.SetToMax();

        //Image buttonImage = button.gameObject.GetComponent<Image>();
        //if (buttonImage != null) {
        //    var currentColor = buttonImage.color;

        //    Color32 color = new Color32((byte)(currentColor[0] * 255f), (byte)(currentColor[1] * 255f), (byte)(currentColor[2] * 255f), 255);
        //    buttonImage.color = color;
        //}
    }

    public void UnHighlightButton(GameObject button) {
        TextHelper text = button.GetComponentInChildren<TextHelper>();
        if (text == null) {
            return;
        }

        text.SetToMin();

        //Image buttonImage = button.gameObject.GetComponent<Image>();
        //if (buttonImage != null) {
        //    var currentColor = buttonImage.color;

        //    Color32 color = new Color32((byte)(currentColor[0] * 255f), (byte)(currentColor[1] * 255f), (byte)(currentColor[2] * 255f), 0);
        //    buttonImage.color = color;
        //}
    }    
    #endregion

    #region Inspector Event Triggers
    public void StartGame() {
        UpdateUI(UIType.HUD, true);
        _audioSource.volume = 0f;        
        _introAnimator.Play("RevealTicket");
    }

    public void CleanupStartGame() {
        _audioSource.volume = 1f;
        FindUI(UIType.Main).SetActive(false);
        Events.OnGameStart?.Invoke();
    }

    public void GoToCredits() {
        UpdateUI(UIType.Credits);
    }

    public void BackToMain() {
        UpdateUI(UIType.Main);        
    }

    public void BackToMainFromOptions() {
        UpdateUI(UIType.Main);
    }

    public void BackToMainFromPause() {
        _pauseInteractables.SetActive(true);
        _mainConfirmation.SetActive(false);
        AudioManager.Instance.PauseAudio(false);
        HideHintText();
        //UpdateUI(UIType.Main);
        SceneManager.LoadScene("MAIN");
    }

    public void EndGame() {
        StartCoroutine(EndCredits());
    }

    private IEnumerator EndCredits() {
        _blackPanel.SetActive(true);
        UpdateUI(UIType.Credits);

        yield return new WaitForSecondsRealtime(2f);

        float step = 1f;
        while (_canvasGroup.alpha > 0f) {
            step -= 0.5f * Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, step);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
    }

    public void GoToControls() {
        _preferencesScreen.SetActive(false);
        _controlsScreen.SetActive(true);

        if (_backToOptionsButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(_backToOptionsButton.gameObject);
            HighlightButton(_backToOptionsButton.gameObject);
        }
    }

    public void GoToOptionsFromControls() {
        _controlsScreen.SetActive(false);
        _preferencesScreen.SetActive(true);

        if (_controlsButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }
            EventSystem.current.SetSelectedGameObject(_controlsButton.gameObject);
            HighlightButton(_controlsButton.gameObject);
        }
    }

    public void GoToOptionsFromMain() {
        UpdateOptions(true);
        UpdateUI(UIType.Options);
    }

    public void AreYouSure(Button focusedButton) {
        if (_currentType == UIType.Main) {
            _mainButtons.SetActive(false);
            _quitConfirmation.SetActive(true);            
            
        }
        else if (_currentType == UIType.Pause) {
            _pauseInteractables.SetActive(false);
            _mainConfirmation.SetActive(true);
        }

        if (focusedButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(focusedButton.gameObject);
            HighlightButton(focusedButton.gameObject);
        }
    }

    public void YesQuit() {
        Debug.LogWarning("Quit");
        Application.Quit();
    }

    public void NoQuit(Button focusedButton) {
        if (_currentType == UIType.Main) {
            _quitConfirmation.SetActive(false);
            _mainButtons.SetActive(true);
        }
        else if (_currentType == UIType.Pause) {
            _mainConfirmation.SetActive(false);
            _pauseInteractables.SetActive(true);
        }

        if (focusedButton != null) {
            if (_lastElementSelected != null) {
                UnHighlightButton(_lastElementSelected);
            }                
            EventSystem.current.SetSelectedGameObject(focusedButton.gameObject);
            HighlightButton(focusedButton.gameObject);
        }
    }

    public void ChangeAudio(Slider slider) {
        AudioManager.Instance.SetVolume(slider.name, slider.value, true);
    }

    public void ChangeVolume(Slider slider) {
        Events.OnBloomChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences<float>(slider.name, PreferenceType.floatType, slider.value);
    }

    public void ChangeLookSensitivity(Slider slider) {
        Events.OnLookSensitivityChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences<float>(slider.name, PreferenceType.floatType, slider.value);
    }

    public void ChangeInvertedControls(Toggle toggle) {
        Events.OnInvertedControlsChange?.Invoke(toggle.isOn);
        PreferenceManager.Instance.UpdatePreferences<bool>(toggle.name, PreferenceType.boolType, toggle.isOn);
    }

    public void ChangeCameraShake(Toggle toggle) {
        Events.OnCameraShakeChange?.Invoke(toggle.isOn);
        PreferenceManager.Instance.UpdatePreferences<bool>(toggle.name, PreferenceType.boolType, toggle.isOn);
    }

    public void ChangeFOV(Slider slider) {
        Events.OnFOVChange?.Invoke(slider.value);
        PreferenceManager.Instance.UpdatePreferences<float>(slider.name, PreferenceType.floatType, slider.value);
    }

    public void ChangeQualitySettings(TMP_Dropdown dropdown) {
        QualitySettings.SetQualityLevel(dropdown.value);
        PreferenceManager.Instance.UpdatePreferences<int>(dropdown.name, PreferenceType.intType, dropdown.value);
    }
    #endregion
}
