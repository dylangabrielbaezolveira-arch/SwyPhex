using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SwyPhexLeague.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Main Menu")]
        public GameObject mainMenuPanel;
        public Button playButton;
        public Button customizationButton;
        public Button settingsButton;
        public Button quitButton;
        
        [Header("HUD")]
        public GameObject hudPanel;
        public Text timerText;
        public Text scoreText;
        public Text boostText;
        public Image abilityIcon;
        public Image abilityCooldown;
        public Text abilityText;
        public GameObject goalNotification;
        public Text goalText;
        public GameObject gravityWarning;
        public Text gravityWarningText;
        
        [Header("Pause Menu")]
        public GameObject pausePanel;
        public Button resumeButton;
        public Button restartButton;
        public Button menuButton;
        
        [Header("Results")]
        public GameObject resultsPanel;
        public Text resultsText;
        public Text team1ScoreText;
        public Text team2ScoreText;
        
        [Header("Transition")]
        public Image screenFade;
        public Animator transitionAnimator;
        
        private float gameTimer = 0f;
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
                return;
            }
            
            SetupButtons();
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
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Escape) && hudPanel.activeSelf)
            {
                TogglePause();
            }
        }
        
        private void SetupButtons()
        {
            playButton.onClick.AddListener(StartGame);
            customizationButton.onClick.AddListener(ShowCustomization);
            settingsButton.onClick.AddListener(ShowSettings);
            quitButton.onClick.AddListener(QuitGame);
            
            resumeButton.onClick.AddListener(ResumeGame);
            restartButton.onClick.AddListener(RestartGame);
            menuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            hudPanel.SetActive(false);
            pausePanel.SetActive(false);
            resultsPanel.SetActive(false);
            
            Time.timeScale = 1f;
        }
        
        public void StartGame()
        {
            StartCoroutine(LoadGameScene("NeonDocks"));
        }
        
        private System.Collections.IEnumerator LoadGameScene(string sceneName)
        {
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeOut");
                yield return new WaitForSeconds(0.5f);
            }
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            mainMenuPanel.SetActive(false);
            hudPanel.SetActive(true);
            
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeIn");
            }
        }
        
        public void TogglePause()
        {
            bool isPaused = !pausePanel.activeSelf;
            pausePanel.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
            
            if (isPaused)
            {
                Managers.AudioManager.Instance?.PauseAll();
            }
            else
            {
                Managers.AudioManager.Instance?.ResumeAll();
            }
        }
        
        public void ResumeGame()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            Managers.AudioManager.Instance?.ResumeAll();
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
        
        public void StartTimer(float time)
        {
            gameTimer = time;
            timerRunning = true;
            UpdateTimerDisplay();
        }
        
        public void UpdateTimer(float time)
        {
            gameTimer = time;
            UpdateTimerDisplay();
        }
        
        private void UpdateTimerDisplay()
        {
            if (!timerText) return;
            
            int minutes = Mathf.FloorToInt(gameTimer / 60f);
            int seconds = Mathf.FloorToInt(gameTimer % 60f);
            
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (gameTimer <= 10f)
            {
                timerText.color = Color.Lerp(Color.red, Color.white, 
                    Mathf.PingPong(Time.time * 2f, 1f));
                    
                if (gameTimer <= 5f && Mathf.FloorToInt(gameTimer) != 
                    Mathf.FloorToInt(gameTimer + Time.deltaTime))
                {
                    Managers.AudioManager.Instance?.PlaySFX("Countdown");
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
        
        public void ShowGoalNotification(string scorerName, string teamName)
        {
            if (!goalNotification) return;
            
            goalText.text = $"{scorerName} scored for {teamName}!";
            goalNotification.SetActive(true);
            
            StartCoroutine(HideGoalNotification());
        }
        
        private System.Collections.IEnumerator HideGoalNotification()
        {
            yield return new WaitForSeconds(2f);
            goalNotification.SetActive(false);
        }
        
        public void ShowGravityWarning(float timeLeft, Core.GravityManager.GravityType current, 
            Core.GravityManager.GravityType next)
        {
            if (!gravityWarning) return;
            
            gravityWarning.SetActive(true);
            gravityWarningText.text = $"Gravity change in {Mathf.CeilToInt(timeLeft)}s";
            
            float alpha = Mathf.PingPong(Time.time * 3f, 1f);
            Color color = gravityWarningText.color;
            color.a = alpha;
            gravityWarningText.color = color;
        }
        
        public void HideGravityWarning()
        {
            if (gravityWarning)
            {
                gravityWarning.SetActive(false);
            }
        }
        
        public void PlayGravityTransition(Core.GravityManager.GravityType newType)
        {
            StartCoroutine(GravityTransitionEffect(newType));
        }
        
        private System.Collections.IEnumerator GravityTransitionEffect(Core.GravityManager.GravityType newType)
        {
            if (!screenFade) yield break;
            
            Color targetColor = GetGravityColor(newType);
            targetColor.a = 0.3f;
            
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
            
            yield return new WaitForSeconds(0.1f);
            
            timer = 0f;
            while (timer < duration)
            {
                screenFade.color = Color.Lerp(targetColor, startColor, timer / duration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            
            screenFade.color = startColor;
        }
        
        private Color GetGravityColor(Core.GravityManager.GravityType type)
        {
            switch (type)
            {
                case Core.GravityManager.GravityType.Normal: return Color.white;
                case Core.GravityManager.GravityType.Low: return Color.blue;
                case Core.GravityManager.GravityType.High: return Color.red;
                case Core.GravityManager.GravityType.Inverted: return Color.magenta;
                case Core.GravityManager.GravityType.ZeroG: return Color.cyan;
                default: return Color.white;
            }
        }
        
        public void ShowMatchResults(string result, int team1Score, int team2Score)
        {
            resultsPanel.SetActive(true);
            resultsText.text = result;
            team1ScoreText.text = team1Score.ToString();
            team2ScoreText.text = team2Score.ToString();
        }
        
        public void ShowCustomization()
        {
            // Implementar panel de personalización
            Debug.Log("Opening customization panel");
        }
        
        public void ShowSettings()
        {
            // Implementar panel de configuración
            Debug.Log("Opening settings panel");
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        public float GameTimer => gameTimer;
    }
}
