using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SwyPhexLeague.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Main Menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button customizationButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Header("HUD In-Game")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private Text timerText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text boostText;
        [SerializeField] private Image abilityCooldown;
        [SerializeField] private Text abilityText;
        [SerializeField] private GameObject goalNotification;
        [SerializeField] private Text goalText;
        
        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        
        [Header("Customization")]
        [SerializeField] private GameObject customizationPanel;
        [SerializeField] private Transform carGrid;
        [SerializeField] private GameObject carButtonPrefab;
        [SerializeField] private ColorSelect colorSelector;
        
        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle hapticToggle;
        [SerializeField] private Dropdown controlSchemeDropdown;
        
        [Header("Transition Effects")]
        [SerializeField] private Image screenFade;
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private GameObject gravityWarning;
        [SerializeField] private Text gravityWarningText;
        
        private float gameTimer = 120f; // 2 minutos
        private bool isPaused = false;
        private bool timerRunning = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            SetupButtons();
            LoadSettings();
        }
        
        private void Start()
        {
            ShowMainMenu();
        }
        
        private void Update()
        {
            if (timerRunning)
            {
                gameTimer -= Time.deltaTime;
                UpdateTimerDisplay();
                
                if (gameTimer <= 0f)
                {
                    gameTimer = 0f;
                    timerRunning = false;
                    GameManager.Instance.EndGame();
                }
            }
            
            // Pause con botón físico
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        private void SetupButtons()
        {
            // Main Menu
            playButton.onClick.AddListener(StartGame);
            customizationButton.onClick.AddListener(ShowCustomization);
            settingsButton.onClick.AddListener(ShowSettings);
            quitButton.onClick.AddListener(QuitGame);
            
            // Pause Menu
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartGame);
            menuButton.onClick.AddListener(ReturnToMenu);
            
            // Settings
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            hapticToggle.onValueChanged.AddListener(SetHapticFeedback);
            controlSchemeDropdown.onValueChanged.AddListener(SetControlScheme);
        }
        
        #region SCENE MANAGEMENT
        
        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            hudPanel.SetActive(false);
            pausePanel.SetActive(false);
            customizationPanel.SetActive(false);
            settingsPanel.SetActive(false);
            
            Cursor.visible = true;
            Time.timeScale = 1f;
        }
        
        public void StartGame()
        {
            StartCoroutine(LoadGameScene("NeonDocks"));
        }
        
        private IEnumerator LoadGameScene(string sceneName)
        {
            // Fade out
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeOut");
                yield return new WaitForSeconds(0.5f);
            }
            
            // Cargar escena
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Setup HUD
            mainMenuPanel.SetActive(false);
            hudPanel.SetActive(true);
            
            // Iniciar timer
            gameTimer = 120f;
            timerRunning = true;
            
            // Fade in
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeIn");
            }
        }
        
        public void TogglePause()
        {
            if (!hudPanel.activeSelf) return;
            
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
            
            if (isPaused)
            {
                AudioManager.Instance.PauseAll();
            }
            else
            {
                AudioManager.Instance.ResumeAll();
            }
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            AudioManager.Instance.ResumeAll();
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
        
        #endregion
        
        #region HUD UPDATES
        
        public void UpdateTimerDisplay()
        {
            if (!timerText) return;
            
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Cambiar color cuando queda poco tiempo
            if (gameTimer <= 10f)
            {
                timerText.color = Color.Lerp(Color.red, Color.white, 
                    Mathf.PingPong(Time.time * 2f, 1f));
                    
                if (gameTimer <= 5f && Mathf.FloorToInt(gameTimer) != Mathf.FloorToInt(gameTimer + Time.deltaTime))
                {
                    AudioManager.Instance.PlaySFX("Countdown");
                }
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        
        public void UpdateScore(int team1Score, int team2Score)
        {
            if (scoreText)
            {
                scoreText.text = $"{team1Score} - {team2Score}";
            }
        }
        
        public void UpdateBoost(float currentBoost, float maxBoost)
        {
            if (boostText)
            {
                boostText.text = Mathf.RoundToInt(currentBoost).ToString();
                boostText.color = Color.Lerp(Color.red, Color.cyan, currentBoost / maxBoost);
            }
        }
        
        public void UpdateAbility(float cooldown, float maxCooldown, string abilityName)
        {
            if (abilityCooldown)
            {
                abilityCooldown.fillAmount = cooldown / maxCooldown;
            }
            
            if (abilityText)
            {
                if (cooldown > 0f)
                {
                    abilityText.text = Mathf.CeilToInt(cooldown).ToString();
                }
                else
                {
                    abilityText.text = abilityName;
                }
            }
        }
        
        public void ShowGoalNotification(string scorerName, string teamColor)
        {
            if (!goalNotification) return;
            
            goalText.text = $"{scorerName} scored for {teamColor}!";
            goalNotification.SetActive(true);
            
            // Ocultar después de 2 segundos
            StartCoroutine(HideGoalNotification());
        }
        
        private IEnumerator HideGoalNotification()
        {
            yield return new WaitForSeconds(2f);
            goalNotification.SetActive(false);
        }
        
        public void ShowGravityWarning(float timeLeft, GravityManager.GravityType current, GravityManager.GravityType next)
        {
            if (!gravityWarning) return;
            
            gravityWarning.SetActive(true);
            gravityWarningText.text = $"Gravity change in {Mathf.CeilToInt(timeLeft)}s\n{next} gravity incoming!";
            
            // Parpadeo
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            Color warningColor = gravityWarningText.color;
            warningColor.a = alpha;
            gravityWarningText.color = warningColor;
        }
        
        public void HideGravityWarning()
        {
            if (gravityWarning)
            {
                gravityWarning.SetActive(false);
            }
        }
        
        public void PlayGravityTransition(GravityManager.GravityType newType)
        {
            // Efecto de pantalla completa
            StartCoroutine(GravityTransitionEffect(newType));
        }
        
        private IEnumerator GravityTransitionEffect(GravityManager.GravityType newType)
        {
            if (!screenFade) yield break;
            
            Color targetColor = GetGravityColor(newType);
            targetColor.a = 0.3f;
            
            // Fade in
            float duration = 0.2f;
            float timer = 0f;
            Color startColor = screenFade.color;
            startColor.a = 0f;
            
            while (timer < duration)
            {
                screenFade.color = Color.Lerp(startColor, targetColor, timer / duration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            // Mantener breve momento
            yield return new WaitForSeconds(0.1f);
            
            // Fade out
            timer = 0f;
            while (timer < duration)
            {
                screenFade.color = Color.Lerp(targetColor, startColor, timer / duration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            screenFade.color = startColor;
        }
        
        private Color GetGravityColor(GravityManager.GravityType type)
        {
            switch (type)
            {
                case GravityManager.GravityType.Normal: return Color.white;
                case GravityManager.GravityType.Low: return Color.blue;
                case GravityManager.GravityType.High: return Color.red;
                case GravityManager.GravityType.Inverted: return Color.magenta;
                case GravityManager.GravityType.ZeroG: return Color.cyan;
                default: return Color.white;
            }
        }
        
        #endregion
        
        #region CUSTOMIZATION
        
        public void ShowCustomization()
        {
            mainMenuPanel.SetActive(false);
            customizationPanel.SetActive(true);
            
            // Cargar autos disponibles
            LoadCarSelection();
        }
        
        private void LoadCarSelection()
        {
            // Limpiar grid
            foreach (Transform child in carGrid)
            {
                Destroy(child.gameObject);
            }
            
            // Cargar autos del ScriptableObject database
            CarDatabase carDB = Resources.Load<CarDatabase>("CarDatabase");
            if (!carDB) return;
            
            foreach (var carData in carDB.cars)
            {
                GameObject buttonObj = Instantiate(carButtonPrefab, carGrid);
                CarButton carButton = buttonObj.GetComponent<CarButton>();
                
                if (carButton)
                {
                    carButton.Setup(carData, OnCarSelected);
                }
            }
        }
        
        private void OnCarSelected(CarData carData)
        {
            // Guardar selección
            SaveManager.Instance.SelectedCar = carData.carId;
            
            // Mostrar preview
            UpdateCarPreview(carData);
        }
        
        private void UpdateCarPreview(CarData carData)
        {
            // Implementar preview 3D/2D del auto
            Debug.Log($"Car selected: {carData.carName}");
        }
        
        #endregion
        
        #region SETTINGS
        
        public void ShowSettings()
        {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
        
        private void LoadSettings()
        {
            // Cargar desde PlayerPrefs
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            hapticToggle.isOn = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
            controlSchemeDropdown.value = PlayerPrefs.GetInt("ControlScheme", 0);
            
            // Aplicar valores
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);
        }
        
        public void SetMusicVolume(float volume)
        {
            AudioManager.Instance.SetMusicVolume(volume);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
        
        public void SetSFXVolume(float volume)
        {
            AudioManager.Instance.SetSFXVolume(volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
        
        public void SetHapticFeedback(bool enabled)
        {
            // Implementar feedback háptico
            PlayerPrefs.SetInt("HapticFeedback", enabled ? 1 : 0);
        }
        
        public void SetControlScheme(int schemeIndex)
        {
            InputManager.Instance.SetControlScheme(schemeIndex);
            PlayerPrefs.SetInt("ControlScheme", schemeIndex);
        }
        
        #endregion
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        // Propiedades públicas
        public bool IsPaused => isPaused;
    }
}
