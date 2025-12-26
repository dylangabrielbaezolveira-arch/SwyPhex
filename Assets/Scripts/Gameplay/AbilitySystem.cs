using UnityEngine;
using UnityEngine.UI;

namespace SwyPhexLeague.Gameplay
{
    public class AbilitySystem : MonoBehaviour
    {
        [System.Serializable]
        public class AbilityData
        {
            public string abilityName;
            public float cooldown = 8f;
            public float boostCost = 5f;
            public Sprite icon;
            public GameObject visualEffect;
            
            [HideInInspector] public float currentCooldown = 0f;
            [HideInInspector] public bool isActive = false;
        }
        
        [Header("Ability Settings")]
        [SerializeField] private AbilityData[] abilities;
        [SerializeField] private int currentAbilityIndex = 0;
        
        [Header("UI References")]
        [SerializeField] private Image abilityIcon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text cooldownText;
        
        [Header("Components")]
        private CarController carController;
        private BoostSystem boostSystem;
        
        private void Awake()
        {
            carController = GetComponent<CarController>();
            boostSystem = GetComponent<BoostSystem>();
            
            // Inicializar habilidades
            foreach (var ability in abilities)
            {
                ability.currentCooldown = 0f;
            }
            
            UpdateUI();
        }
        
        private void Update()
        {
            // Actualizar cooldowns
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].currentCooldown > 0f)
                {
                    abilities[i].currentCooldown -= Time.deltaTime;
                    if (abilities[i].currentCooldown <= 0f)
                    {
                        abilities[i].currentCooldown = 0f;
                        abilities[i].isActive = false;
                    }
                }
            }
            
            UpdateUI();
        }
        
        public void ActivateAbility()
        {
            if (abilities.Length == 0) return;
            
            AbilityData ability = abilities[currentAbilityIndex];
            
            // Verificar condiciones
            if (ability.currentCooldown > 0f) return;
            if (!boostSystem.HasBoost(ability.boostCost + 20f)) return;
            
            // Consumir boost extra
            if (!boostSystem.UseAbilityBoost(ability.boostCost)) return;
            
            // Activar habilidad específica
            bool activated = ActivateSpecificAbility(ability);
            
            if (activated)
            {
                // Iniciar cooldown
                ability.currentCooldown = ability.cooldown;
                ability.isActive = true;
                
                // Efectos de sonido
                AudioManager.Instance.PlaySFX("AbilityActivate");
                
                // Efecto visual
                if (ability.visualEffect)
                {
                    GameObject effect = Instantiate(
                        ability.visualEffect, 
                        transform.position, 
                        Quaternion.identity
                    );
                    Destroy(effect, 3f);
                }
            }
        }
        
        private bool ActivateSpecificAbility(AbilityData ability)
        {
            switch (ability.abilityName)
            {
                case "PulseDash":
                    return ActivatePulseDash();
                case "MagnetCore":
                    return ActivateMagnetCore();
                case "GravityFlip":
                    return ActivateGravityFlip();
                case "ShockDrop":
                    return ActivateShockDrop();
                default:
                    Debug.LogWarning($"Habilidad desconocida: {ability.abilityName}");
                    return false;
            }
        }
        
        private bool ActivatePulseDash()
        {
            Vector2 dashDirection = carController.Rigidbody.velocity.normalized;
            if (dashDirection.magnitude < 0.1f)
            {
                dashDirection = transform.right;
            }
            
            carController.ApplyDash(dashDirection, 25f);
            
            // Efecto de invencibilidad breve
            StartCoroutine(BriefInvincibility(0.3f));
            
            return true;
        }
        
        private bool ActivateMagnetCore()
        {
            // Encontrar pelota en escena
            BallPhysics ball = FindObjectOfType<BallPhysics>();
            if (!ball) return false;
            
            // Aplicar magnetismo a la pelota
            ball.SetMagnetized(transform, 50f, 1.5f);
            
            // Penalización de velocidad
            carController.Rigidbody.velocity *= 0.8f;
            
            return true;
        }
        
        private bool ActivateGravityFlip()
        {
            // Invertir gravedad en área
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position, 
                4f
            );
            
            foreach (var col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb && rb != carController.Rigidbody)
                {
                    // Aplicar fuerza de gravedad invertida
                    Vector2 force = -GravityManager.Instance.CurrentGravity * 20f;
                    rb.AddForce(force, ForceMode2D.Impulse);
                    
                    // Efecto en pelota
                    BallPhysics ball = col.GetComponent<BallPhysics>();
                    if (ball)
                    {
                        ball.ApplyGravityShock(-GravityManager.Instance.CurrentGravity, 1f);
                    }
                }
            }
            
            // Efecto visual de área
            GameObject areaEffect = ObjectPool.Instance.GetPooledObject("GravityField");
            if (areaEffect)
            {
                areaEffect.transform.position = transform.position;
                areaEffect.transform.localScale = Vector3.one * 8f;
                areaEffect.SetActive(true);
            }
            
            return true;
        }
        
        private bool ActivateShockDrop()
        {
            // Crear zona de choque en el suelo
            Vector2 dropPosition = transform.position;
            
            // Si está en aire, caer rápidamente
            if (!carController.IsGrounded)
            {
                carController.Rigidbody.velocity = new Vector2(
                    carController.Rigidbody.velocity.x,
                    -20f
                );
                dropPosition = (Vector2)transform.position + Vector2.down * 2f;
            }
            
            // Crear zona de efecto
            GameObject shockZone = ObjectPool.Instance.GetPooledObject("ShockZone");
            if (shockZone)
            {
                shockZone.transform.position = dropPosition;
                shockZone.SetActive(true);
                
                // Configurar duración
                Destroy(shockZone, 2.5f);
            }
            
            // Aplicar efecto a enemigos cercanos
            ApplyShockEffect(dropPosition);
            
            return true;
        }
        
        private void ApplyShockEffect(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 3f);
            
            foreach (var col in colliders)
            {
                CarController enemyCar = col.GetComponent<CarController>();
                if (enemyCar && enemyCar != carController)
                {
                    // Ralentizar
                    enemyCar.Rigidbody.velocity *= 0.6f;
                    
                    // Interrumpir habilidades
                    AbilitySystem enemyAbility = enemyCar.GetComponent<AbilitySystem>();
                    if (enemyAbility)
                    {
                        enemyAbility.InterruptAbility();
                    }
                    
                    // Efecto visual
                    GameObject shockEffect = ObjectPool.Instance.GetPooledObject("ShockEffect");
                    if (shockEffect)
                    {
                        shockEffect.transform.position = enemyCar.transform.position;
                        shockEffect.SetActive(true);
                        Destroy(shockEffect, 1f);
                    }
                }
            }
        }
        
        private System.Collections.IEnumerator BriefInvincibility(float duration)
        {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            Color originalColor = renderer.color;
            
            float timer = 0f;
            while (timer < duration)
            {
                renderer.color = Color.Lerp(originalColor, Color.clear, Mathf.PingPong(timer * 10f, 1f));
                timer += Time.deltaTime;
                yield return null;
            }
            
            renderer.color = originalColor;
        }
        
        public void InterruptAbility()
        {
            if (abilities.Length == 0) return;
            
            AbilityData ability = abilities[currentAbilityIndex];
            if (ability.isActive)
            {
                ability.currentCooldown = Mathf.Max(ability.currentCooldown - 2f, 0f);
                ability.isActive = false;
                
                // Efecto de interrupción
                AudioManager.Instance.PlaySFX("AbilityInterrupted");
            }
        }
        
        private void UpdateUI()
        {
            if (abilities.Length == 0) return;
            
            AbilityData ability = abilities[currentAbilityIndex];
            
            if (abilityIcon && ability.icon)
            {
                abilityIcon.sprite = ability.icon;
            }
            
            if (cooldownOverlay)
            {
                float cooldownRatio = ability.currentCooldown / ability.cooldown;
                cooldownOverlay.fillAmount = cooldownRatio;
                cooldownOverlay.gameObject.SetActive(cooldownRatio > 0);
            }
            
            if (cooldownText)
            {
                if (ability.currentCooldown > 0)
                {
                    cooldownText.text = Mathf.CeilToInt(ability.currentCooldown).ToString();
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }
        
        public void SetAbility(int index)
        {
            if (index >= 0 && index < abilities.Length)
            {
                currentAbilityIndex = index;
                UpdateUI();
            }
        }
        
        // Propiedades públicas
        public AbilityData CurrentAbility => 
            abilities.Length > 0 ? abilities[currentAbilityIndex] : null;
            
        public bool IsAbilityReady => 
            abilities.Length > 0 && abilities[currentAbilityIndex].currentCooldown <= 0f;
    }
}
