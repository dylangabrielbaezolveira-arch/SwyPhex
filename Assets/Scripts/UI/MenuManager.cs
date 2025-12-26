using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SwyPhexLeague.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Main Menu")]
        public GameObject mainMenu;
        public Button playButton;
        public Button customizationButton;
        public Button settingsButton;
        public Button quitButton;
        public Text versionText;
        
        [Header("Level Select")]
        public GameObject levelSelect;
        public Button[] levelButtons;
        public Button backFromLevelsButton;
        
        [Header("Customization")]
        public GameObject customization;
        public Button backFromCustomizationButton;
        public CarCustomizationUI carCustomizer;
        
        [Header("Settings")]
        public GameObject settings;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Toggle hapticToggle;
        public Dropdown controlSchemeDropdown;
        public Button backFromSettingsButton;
        
        [Header("Loading")]
        public GameObject loadingScreen;
        public Slider loadingProgress;
        public Text loadingText;
        
        private void Start()
        {
            SetupButtons();
            ShowMainMenu();
            
            if (versionText)
            {
                versionText.text = $"v{Application.version}";
            }
        }
        
        private void SetupButtons()
        {
            playButton.onClick.AddListener(ShowLevelSelect);
            customizationButton.onClick.AddListener(ShowCustomization);
            settingsButton.onClick.AddListener(ShowSettings);
            quitButton.onClick.AddListener(QuitGame);
            
            backFromLevelsButton.onClick.AddListener(ShowMainMenu);
            backFromCustomizationButton.onClick.AddListener(ShowMainMenu);
            backFromSettingsButton.onClick.AddListener(ShowMainMenu);
            
            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i;
                levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
            }
            
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            hapticToggle.onValueChanged.AddListener(SetHapticFeedback);
            controlSchemeDropdown.onValueChanged.AddListener(SetControlScheme);
            
            LoadSettings();
        }
        
        public void ShowMainMenu()
        {
            mainMenu.SetActive(true);
            levelSelect.SetActive(false);
            customization.SetActive(false);
            settings.SetActive(false);
            loadingScreen.SetActive(false);
        }
        
        public void ShowLevelSelect()
        {
            mainMenu.SetActive(false);
            levelSelect.SetActive(true);
            
            UpdateLevelButtons();
        }
        
        private void UpdateLevelButtons()
        {
            for (int i = 0; i < levelButtons.Length; i++)
            {
                Button button = levelButtons[i];
                bool isUnlocked = Managers.SaveManager.Instance?.IsLevelUnlocked(i) ?? true;
                
                button.interactable = isUnlocked;
                
                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText)
                {
                    buttonText.color = isUnlocked ? Color.white : Color.gray;
                }
            }
        }
        
        public void ShowCustomization()
        {
            mainMenu.SetActive(false);
            customization.SetActive(true);
            
            if (carCustomizer)
            {
                carCustomizer.LoadCarData();
            }
        }
        
        public void ShowSettings()
        {
            mainMenu.SetActive(false);
            settings.SetActive(true);
        }
        
        private void LoadSettings()
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            hapticToggle.isOn = PlayerPrefs.GetInt("HapticFeedback", 1) == 1;
            controlSchemeDropdown.value = PlayerPrefs.GetInt("ControlScheme", 0);
            
            ApplySettings();
        }
        
        private void ApplySettings()
        {
            SetMusicVolume(musicSlider.value);
            SetSFXVolume(sfxSlider.value);
            SetHapticFeedback(hapticToggle.isOn);
            SetControlScheme(controlSchemeDropdown.value);
        }
        
        public void SetMusicVolume(float volume)
        {
            Managers.AudioManager.Instance?.SetMusicVolume(volume);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
        
        public void SetSFXVolume(float volume)
        {
            Managers.AudioManager.Instance?.SetSFXVolume(volume);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
        
        public void SetHapticFeedback(bool enabled)
        {
            PlayerPrefs.SetInt("HapticFeedback", enabled ? 1 : 0);
        }
        
        public void SetControlScheme(int schemeIndex)
        {
            Core.InputManager.Instance?.SetControlScheme(schemeIndex);
            PlayerPrefs.SetInt("ControlScheme", schemeIndex);
        }
        
        public void LoadLevel(int levelIndex)
        {
            string[] levelScenes = { "NeonDocks", "FluxArena", "VoidPit", "UndergroundX" };
            
            if (levelIndex >= 0 && levelIndex < levelScenes.Length)
            {
                StartCoroutine(LoadSceneAsync(levelScenes[levelIndex]));
            }
        }
        
        private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            loadingScreen.SetActive(true);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (loadingProgress)
                {
                    loadingProgress.value = progress;
                }
                
                if (loadingText)
                {
                    loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
                }
                
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        public void ShowAchievements()
        {
            // Implementar logros
            Debug.Log("Showing achievements");
        }
        
        public void ShowLeaderboards()
        {
            // Implementar tablas de clasificación
            Debug.Log("Showing leaderboards");
        }
        
        public void ShowShop()
        {
            // Implementar tienda
            Debug.Log("Showing shop");
        }
    }
}
