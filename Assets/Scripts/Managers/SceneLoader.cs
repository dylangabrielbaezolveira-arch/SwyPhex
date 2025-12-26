using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SwyPhexLeague.Managers
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        [Header("Loading Screen")]
        public GameObject loadingScreen;
        public UnityEngine.UI.Slider loadingSlider;
        public UnityEngine.UI.Text loadingText;
        public UnityEngine.UI.Text loadingPercentage;
        public float minLoadTime = 1f;
        
        [Header("Transition")]
        public Animator transitionAnimator;
        public float transitionTime = 0.5f;
        
        private AsyncOperation loadingOperation;
        private float loadStartTime;
        
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
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }
        
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }
        
        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            // Fade out
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeOut");
                yield return new WaitForSeconds(transitionTime);
            }
            
            // Mostrar pantalla de carga
            if (loadingScreen)
            {
                loadingScreen.SetActive(true);
            }
            
            loadStartTime = Time.time;
            
            // Cargar escena
            loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            loadingOperation.allowSceneActivation = false;
            
            while (!loadingOperation.isDone)
            {
                float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
                float elapsedTime = Time.time - loadStartTime;
                float displayProgress = Mathf.Min(progress, elapsedTime / minLoadTime);
                
                UpdateLoadingUI(displayProgress);
                
                if (progress >= 0.9f && elapsedTime >= minLoadTime)
                {
                    loadingOperation.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            // Ocultar pantalla de carga
            if (loadingScreen)
            {
                loadingScreen.SetActive(false);
            }
            
            // Fade in
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeIn");
                yield return new WaitForSeconds(transitionTime);
            }
        }
        
        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            // Fade out
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeOut");
                yield return new WaitForSeconds(transitionTime);
            }
            
            if (loadingScreen)
            {
                loadingScreen.SetActive(true);
            }
            
            loadStartTime = Time.time;
            
            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
            loadingOperation.allowSceneActivation = false;
            
            while (!loadingOperation.isDone)
            {
                float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
                float elapsedTime = Time.time - loadStartTime;
                float displayProgress = Mathf.Min(progress, elapsedTime / minLoadTime);
                
                UpdateLoadingUI(displayProgress);
                
                if (progress >= 0.9f && elapsedTime >= minLoadTime)
                {
                    loadingOperation.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            if (loadingScreen)
            {
                loadingScreen.SetActive(false);
            }
            
            if (transitionAnimator)
            {
                transitionAnimator.SetTrigger("FadeIn");
                yield return new WaitForSeconds(transitionTime);
            }
        }
        
        private void UpdateLoadingUI(float progress)
        {
            if (loadingSlider)
            {
                loadingSlider.value = progress;
            }
            
            if (loadingPercentage)
            {
                loadingPercentage.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            
            if (loadingText)
            {
                string[] loadingTips = {
                    "Use boost wisely to reach the ball faster!",
                    "Different cars have unique abilities",
                    "Master gravity changes to gain advantage",
                    "Try different control schemes in settings",
                    "Complete challenges to earn rewards"
                };
                
                int tipIndex = Mathf.FloorToInt(progress * loadingTips.Length) % loadingTips.Length;
                loadingText.text = loadingTips[tipIndex];
            }
        }
        
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void LoadNextScene()
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadScene(nextSceneIndex);
            }
            else
            {
                LoadScene(0); // Volver al menú principal
            }
        }
        
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }
        
        public bool IsLoading => loadingOperation != null && !loadingOperation.isDone;
    }
}
