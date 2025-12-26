using UnityEngine;

namespace SwyPhexLeague.Core
{
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance { get; private set; }
        
        public enum GravityType
        {
            Normal,
            Low,
            High,
            Inverted,
            ZeroG
        }
        
        [Header("Settings")]
        public GravityType currentGravityType = GravityType.Normal;
        public float gravityTransitionTime = 0.5f;
        
        [Header("Variable Gravity")]
        public bool useVariableGravity = false;
        public float gravityChangeInterval = 30f;
        public GravityType[] gravitySequence;
        
        [Header("Effects")]
        public SpriteRenderer screenOverlay;
        public AudioSource gravitySound;
        
        private Vector2 currentGravity;
        private Vector2 targetGravity;
        private bool isTransitioning = false;
        private float transitionTimer = 0f;
        private float gravityTimer = 0f;
        private int sequenceIndex = 0;
        
        private Vector2 normalGravity = new Vector2(0, -9.8f);
        private Vector2 lowGravity = new Vector2(0, -3.92f);
        private Vector2 highGravity = new Vector2(0, -17.64f);
        private Vector2 invertedGravity = new Vector2(0, 9.8f);
        private Vector2 zeroGravity = Vector2.zero;
        
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
            
            InitializeGravity();
        }
        
        private void InitializeGravity()
        {
            currentGravity = GetGravityVector(currentGravityType);
            targetGravity = currentGravity;
            Physics2D.gravity = currentGravity;
            
            if (useVariableGravity && gravitySequence.Length > 0)
            {
                sequenceIndex = 0;
                gravityTimer = 0f;
            }
        }
        
        private void Update()
        {
            HandleGravityTransition();
            
            if (useVariableGravity)
            {
                HandleVariableGravity();
            }
        }
        
        private void HandleGravityTransition()
        {
            if (isTransitioning)
            {
                transitionTimer += Time.deltaTime / gravityTransitionTime;
                currentGravity = Vector2.Lerp(currentGravity, targetGravity, transitionTimer);
                Physics2D.gravity = currentGravity;
                
                UpdateTransitionEffects();
                
                if (transitionTimer >= 1f)
                {
                    isTransitioning = false;
                    transitionTimer = 0f;
                }
            }
        }
        
        private void HandleVariableGravity()
        {
            if (gravitySequence.Length == 0) return;
            
            gravityTimer += Time.deltaTime;
            
            if (gravityTimer >= gravityChangeInterval)
            {
                sequenceIndex = (sequenceIndex + 1) % gravitySequence.Length;
                SetGravity(gravitySequence[sequenceIndex]);
                gravityTimer = 0f;
            }
            
            if (gravityTimer >= gravityChangeInterval - 5f)
            {
                ShowGravityWarning(gravityChangeInterval - gravityTimer);
            }
        }
        
        public void SetGravity(GravityType type, bool immediate = false)
        {
            Vector2 newGravity = GetGravityVector(type);
            
            if (immediate)
            {
                currentGravity = newGravity;
                targetGravity = newGravity;
                Physics2D.gravity = newGravity;
                currentGravityType = type;
                isTransitioning = false;
            }
            else
            {
                targetGravity = newGravity;
                isTransitioning = true;
                transitionTimer = 0f;
                currentGravityType = type;
                
                PlayGravityChangeEffects(type);
            }
        }
        
        private Vector2 GetGravityVector(GravityType type)
        {
            return type switch
            {
                GravityType.Normal => normalGravity,
                GravityType.Low => lowGravity,
                GravityType.High => highGravity,
                GravityType.Inverted => invertedGravity,
                GravityType.ZeroG => zeroGravity,
                _ => normalGravity
            };
        }
        
        private void PlayGravityChangeEffects(GravityType newType)
        {
            if (gravitySound)
            {
                gravitySound.Play();
            }
            
            CameraShake.Instance?.Shake(0.15f, 0.5f);
            
            UI.UIManager.Instance?.PlayGravityTransition(newType);
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (PlayerPrefs.GetInt("HapticFeedback", 1) == 1)
                Handheld.Vibrate();
            #endif
        }
        
        private void UpdateTransitionEffects()
        {
            if (screenOverlay)
            {
                Color overlayColor = GetGravityColor(currentGravityType);
                overlayColor.a = Mathf.Lerp(0.3f, 0f, transitionTimer);
                screenOverlay.color = overlayColor;
            }
        }
        
        private Color GetGravityColor(GravityType type)
        {
            return type switch
            {
                GravityType.Normal => Color.white,
                GravityType.Low => Color.blue,
                GravityType.High => Color.red,
                GravityType.Inverted => Color.magenta,
                GravityType.ZeroG => Color.cyan,
                _ => Color.white
            };
        }
        
        private void ShowGravityWarning(float timeLeft)
        {
            if (screenOverlay && gravitySequence.Length > 0)
            {
                GravityType nextType = gravitySequence[(sequenceIndex + 1) % gravitySequence.Length];
                Color warningColor = GetGravityColor(nextType);
                warningColor.a = Mathf.PingPong(Time.time * 3f, 0.2f);
                screenOverlay.color = warningColor;
            }
            
            UI.UIManager.Instance?.ShowGravityWarning(timeLeft, 
                currentGravityType, 
                gravitySequence[(sequenceIndex + 1) % gravitySequence.Length]);
        }
        
        public void SetLocalGravity(Vector2 position, float radius, Vector2 gravity, float duration)
        {
            StartCoroutine(LocalGravityCoroutine(position, radius, gravity, duration));
        }
        
        private System.Collections.IEnumerator LocalGravityCoroutine(Vector2 position, float radius, Vector2 gravity, float duration)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
            
            foreach (var col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb)
                {
                    rb.gravityScale = 0f;
                    rb.AddForce(gravity * rb.mass, ForceMode2D.Force);
                }
            }
            
            yield return new WaitForSeconds(duration);
            
            foreach (var col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb)
                {
                    rb.gravityScale = 1f;
                }
            }
        }
        
        public Vector2 CurrentGravity => currentGravity;
        public GravityType CurrentGravityType => currentGravityType;
        public bool IsTransitioning => isTransitioning;
        
        public void ToggleVariableGravity(bool enabled)
        {
            useVariableGravity = enabled;
            if (enabled)
            {
                gravityTimer = 0f;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (useVariableGravity)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 1f);
                
                GUI.color = Color.yellow;
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.yellow;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                    $"Variable Gravity: {gravityTimer:F1}/{gravityChangeInterval}", style);
            }
        }
    }
}
