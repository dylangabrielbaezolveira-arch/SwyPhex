using UnityEngine;
using UnityEngine.UI;

namespace SwyPhexLeague.Gameplay
{
    public class AbilitySystem : MonoBehaviour
    {
        [System.Serializable]
        public class Ability
        {
            public string name;
            public float cooldown = 8f;
            public float boostCost = 5f;
            public Sprite icon;
            public GameObject effectPrefab;
            
            [HideInInspector] public float currentCooldown = 0f;
            [HideInInspector] public bool isActive = false;
        }
        
        [Header("Abilities")]
        public Ability[] abilities;
        public int currentAbilityIndex = 0;
        
        [Header("UI")]
        public Image abilityIcon;
        public Image cooldownOverlay;
        public Text cooldownText;
        
        [Header("Components")]
        private CarController carController;
        private Core.BoostSystem boostSystem;
        
        private void Awake()
        {
            carController = GetComponent<CarController>();
            boostSystem = GetComponent<Core.BoostSystem>();
            
            if (abilities.Length > 0)
            {
                InitializeAbilities();
            }
        }
        
        private void InitializeAbilities()
        {
            foreach (var ability in abilities)
            {
                ability.currentCooldown = 0f;
                ability.isActive = false;
            }
            
            UpdateUI();
        }
        
        private void Update()
        {
            UpdateCooldowns();
            UpdateUI();
        }
        
        private void UpdateCooldowns()
        {
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
        }
        
        public void ActivateAbility()
        {
            if (abilities.Length == 0) return;
            
            Ability ability = abilities[currentAbilityIndex];
            
            if (ability.currentCooldown > 0f) return;
            if (!boostSystem.HasBoost(ability.boostCost + 20f)) return;
            if (!boostSystem.UseAbilityBoost()) return;
            
            bool success = ActivateSpecificAbility(ability);
            
            if (success)
            {
                ability.currentCooldown = ability.cooldown;
                ability.isActive = true;
                
                Managers.AudioManager.Instance?.PlaySFX("AbilityActivate");
                
                if (ability.effectPrefab)
                {
                    GameObject effect = Instantiate(ability.effectPrefab, transform.position, Quaternion.identity);
                    Destroy(effect, 3f);
                }
            }
        }
        
        private bool ActivateSpecificAbility(Ability ability)
        {
            switch (ability.name)
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
                    Debug.LogWarning($"Unknown ability: {ability.name}");
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
            
            carController.ApplyExternalForce(dashDirection * 25f);
            StartCoroutine(BriefInvincibility(0.3f));
            
            return true;
        }
        
        private bool ActivateMagnetCore()
        {
            Core.BallPhysics ball = FindObjectOfType<Core.BallPhysics>();
            if (!ball) return false;
            
            ball.SetMagnetized(transform, 1.5f);
            carController.Rigidbody.velocity *= 0.8f;
            
            return true;
        }
        
        private bool ActivateGravityFlip()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 4f);
            
            foreach (var col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb && rb != carController.Rigidbody)
                {
                    Vector2 force = -Core.GravityManager.Instance.CurrentGravity * 20f;
                    rb.AddForce(force, ForceMode2D.Impulse);
                    
                    Core.BallPhysics ball = col.GetComponent<Core.BallPhysics>();
                    if (ball)
                    {
                        ball.ApplyGravityShock(force);
                    }
                }
            }
            
            GameObject effect = Utilities.ObjectPool.Instance?.GetPooledObject("GravityField");
            if (effect)
            {
                effect.transform.position = transform.position;
                effect.transform.localScale = Vector3.one * 8f;
                effect.SetActive(true);
            }
            
            return true;
        }
        
        private bool ActivateShockDrop()
        {
            Vector2 dropPosition = transform.position;
            
            if (!carController.IsGrounded)
            {
                carController.Rigidbody.velocity = new Vector2(
                    carController.Rigidbody.velocity.x,
                    -20f
                );
                dropPosition = (Vector2)transform.position + Vector2.down * 2f;
            }
            
            GameObject shockZone = Utilities.ObjectPool.Instance?.GetPooledObject("ShockZone");
            if (shockZone)
            {
                shockZone.transform.position = dropPosition;
                shockZone.SetActive(true);
                Destroy(shockZone, 2.5f);
            }
            
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
                    enemyCar.Rigidbody.velocity *= 0.6f;
                    
                    AbilitySystem enemyAbility = enemyCar.GetComponent<AbilitySystem>();
                    if (enemyAbility)
                    {
                        enemyAbility.InterruptAbility();
                    }
                    
                    GameObject shockEffect = Utilities.ObjectPool.Instance?.GetPooledObject("ShockEffect");
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
            if (!renderer) yield break;
            
            Color originalColor = renderer.color;
            float timer = 0f;
            
            while (timer < duration)
            {
                float alpha = Mathf.PingPong(timer * 10f, 1f);
                renderer.color = Color.Lerp(originalColor, Color.clear, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            
            renderer.color = originalColor;
        }
        
        public void InterruptAbility()
        {
            if (abilities.Length == 0) return;
            
            Ability ability = abilities[currentAbilityIndex];
            if (ability.isActive)
            {
                ability.currentCooldown = Mathf.Max(ability.currentCooldown - 2f, 0f);
                ability.isActive = false;
                
                Managers.AudioManager.Instance?.PlaySFX("AbilityInterrupted");
            }
        }
        
        private void UpdateUI()
        {
            if (abilities.Length == 0) return;
            
            Ability ability = abilities[currentAbilityIndex];
            
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
        
        public Ability CurrentAbility => 
            abilities.Length > 0 ? abilities[currentAbilityIndex] : null;
            
        public bool IsAbilityReady => 
            abilities.Length > 0 && abilities[currentAbilityIndex].currentCooldown <= 0f;
    }
}
