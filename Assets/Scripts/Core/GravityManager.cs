using UnityEngine;

namespace SwyPhexLeague.Core
{
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance { get; private set; }
        
        [Header("Gravity Settings")]
        [SerializeField] private Vector2 baseGravity = new Vector2(0, -9.8f);
        [SerializeField] private float gravityTransitionTime = 0.5f;
        
        [Header("Current State")]
        [SerializeField] private Vector2 currentGravity;
        [SerializeField] private GravityType currentGravityType = GravityType.Normal;
        
        [Header("Variable Gravity Settings")]
        [SerializeField] private bool isVariableGravity = false;
        [SerializeField] private float gravityChangeInterval = 30f;
        [SerializeField] private GravityType[] gravitySequence;
        
        private float gravityTimer = 0f;
        private int currentSequenceIndex = 0;
        private Vector2 targetGravity;
        private bool isTransitioning = false;
        private float transitionProgress = 0f;
        
        public enum GravityType
        {
            Normal,
            Low,
            High,
            Inverted,
            ZeroG
        }
        
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
            
            currentGravity = baseGravity;
            targetGravity = baseGravity;
            Physics2D.gravity = currentGravity;
        }
        
        private void Start()
        {
            if (isVariableGravity && gravitySequence.Length > 0)
            {
                StartVariableGravity();
            }
        }
        
        private void Update()
        {
            // Manejar transiciones de gravedad
            if (isTransitioning)
            {
                transitionProgress += Time.deltaTime / gravityTransitionTime;
                currentGravity = Vector2.Lerp(
                    currentGravity, 
                    targetGravity, 
                    transitionProgress
                );
                
                Physics2D.gravity = currentGravity;
                
                if (transitionProgress >= 1f)
                {
                    isTransitioning = false;
                    transitionProgress = 0f;
                }
            }
            
            // Gravedad variable
            if (isVariableGravity)
            {
                gravityTimer += Time.deltaTime;
                
                if (gravityTimer >= gravityChangeInterval)
                {
                    ChangeToNextGravity();
                    gravityTimer = 0f;
                }
                
                // Aviso visual (últimos 5 segundos)
                if (gravityTimer >= gravityChangeInterval - 5f)
                {
                    ShowGravityWarning();
                }
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
                transitionProgress = 0f;
                currentGravityType = type;
                
                // Efectos de transición
                OnGravityChangeStart(type);
            }
        }
        
        private Vector2 GetGravityVector(GravityType type)
        {
            switch (type)
            {
                case GravityType.Normal:
                    return new Vector2(0, -9.8f);
                case GravityType.Low:
                    return new Vector2(0, -3.92f); // 40%
                case GravityType.High:
                    return new Vector2(0, -17.64f); // 180%
                case GravityType.Inverted:
                    return new Vector2(0, 9.8f);
                case GravityType.ZeroG:
                    return Vector2.zero;
                default:
                    return baseGravity;
            }
        }
        
        private void StartVariableGravity()
        {
            currentSequenceIndex = 0;
            SetGravity(gravitySequence[0], true);
            
            // Programar siguiente cambio
            gravityTimer = 0f;
            isVariableGravity = true;
        }
        
        private void ChangeToNextGravity()
        {
            if (gravitySequence.Length == 0) return;
            
            currentSequenceIndex = (currentSequenceIndex + 1) % gravitySequence.Length;
            SetGravity(gravitySequence[currentSequenceIndex]);
        }
        
        private void ShowGravityWarning()
        {
            // Cambiar color de borde de pantalla
            UIManager.Instance.ShowGravityWarning(
                gravityChangeInterval - gravityTimer,
                currentGravityType,
                gravitySequence[currentSequenceIndex]
            );
            
            // Sonido de advertencia
            if (!AudioManager.Instance.IsPlaying("GravityWarning"))
            {
                AudioManager.Instance.PlaySFX("GravityWarning");
            }
        }
        
        private void OnGravityChangeStart(GravityType newType)
        {
            // Efectos visuales
            CameraShake.Instance.Shake(0.15f, 0.5f);
            
            // Sonido
            AudioManager.Instance.PlaySFX("GravityChange");
            
            // Efecto de pantalla completa
            UIManager.Instance.PlayGravityTransition(newType);
            
            // Vibración (si está soportada)
            #if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
            #endif
        }
        
        public void SetLocalGravity(Vector2 position, float radius, Vector2 gravity, float duration)
        {
            StartCoroutine(LocalGravityEffect(position, radius, gravity, duration));
        }
        
        private System.Collections.IEnumerator LocalGravityEffect(
            Vector2 position, 
            float radius, 
            Vector2 gravity, 
            float duration)
        {
            // Marcar objetos en la zona
            Collider2D[] objectsInZone = Physics2D.OverlapCircleAll(position, radius);
            
            // Aplicar gravedad local a cada objeto
            Dictionary<Rigidbody2D, Vector2> originalGravities = new Dictionary<Rigidbody2D, Vector2>();
            
            foreach (var col in objectsInZone)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb && !originalGravities.ContainsKey(rb))
                {
                    originalGravities[rb] = Physics2D.gravity;
                    
                    // Para objetos con gravedad personalizada
                    if (col.CompareTag("Ball") || col.CompareTag("Car"))
                    {
                        rb.gravityScale = 0f;
                        rb.AddForce(gravity * rb.mass, ForceMode2D.Force);
                    }
                }
            }
            
            // Efecto visual de zona
            GameObject zoneEffect = ObjectPool.Instance.GetPooledObject("GravityZone");
            if (zoneEffect)
            {
                zoneEffect.transform.position = position;
                zoneEffect.transform.localScale = Vector3.one * radius * 2f;
                zoneEffect.SetActive(true);
            }
            
            yield return new WaitForSeconds(duration);
            
            // Restaurar gravedades
            foreach (var kvp in originalGravities)
            {
                if (kvp.Key)
                {
                    kvp.Key.gravityScale = 1f;
                }
            }
            
            if (zoneEffect)
            {
                zoneEffect.SetActive(false);
            }
        }
        
        // Propiedades públicas
        public Vector2 CurrentGravity => currentGravity;
        public GravityType CurrentGravityType => currentGravityType;
        public bool IsTransitioning => isTransitioning;
        
        public float GravityStrength => currentGravity.magnitude;
        public Vector2 GravityDirection => currentGravity.normalized;
    }
}
