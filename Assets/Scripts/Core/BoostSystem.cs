using UnityEngine;
using UnityEngine.UI;

namespace SwyPhexLeague.Core
{
    public class BoostSystem : MonoBehaviour
    {
        [Header("Boost Settings")]
        [SerializeField] private float maxBoost = 100f;
        [SerializeField] private float startBoost = 50f;
        [SerializeField] private float passiveRegen = 2f; // por segundo
        [SerializeField] private float boostConsumption = 12f; // por segundo
        [SerializeField] private float dashCost = 18f;
        
        [Header("UI Reference")]
        [SerializeField] private Image boostBar;
        [SerializeField] private Text boostText;
        
        [Header("State")]
        private float currentBoost;
        private bool isBoosting;
        private float boostMultiplier = 1f;
        private float penaltyTimer = 0f;
        
        // Propiedades cosméticas
        private Color boostColor = Color.cyan;
        private ParticleSystem boostParticles;
        private AudioSource boostSound;
        
        private void Start()
        {
            currentBoost = startBoost;
            UpdateUI();
            
            boostParticles = GetComponentInChildren<ParticleSystem>();
            boostSound = GetComponent<AudioSource>();
            
            if (boostSound)
            {
                boostSound.loop = true;
                boostSound.volume = 0.3f;
            }
        }
        
        private void Update()
        {
            // Regeneración pasiva
            if (!isBoosting && penaltyTimer <= 0f)
            {
                RegenerateBoost(passiveRegen * Time.deltaTime);
            }
            
            // Consumo activo
            if (isBoosting)
            {
                float consumption = boostConsumption * Time.deltaTime * boostMultiplier;
                UseBoost(consumption);
            }
            
            // Penalización por spam
            if (penaltyTimer > 0f)
            {
                penaltyTimer -= Time.deltaTime;
            }
            
            UpdateUI();
            UpdateEffects();
        }
        
        public void UseBoost(float amount = 0f)
        {
            if (amount == 0f)
            {
                isBoosting = true;
                return;
            }
            
            currentBoost -= amount;
            if (currentBoost <= 0f)
            {
                currentBoost = 0f;
                isBoosting = false;
                ApplyBoostPenalty();
            }
        }
        
        public void StopBoosting()
        {
            isBoosting = false;
        }
        
        public void RegenerateBoost(float amount)
        {
            currentBoost = Mathf.Min(currentBoost + amount, maxBoost);
        }
        
        public bool UseDash()
        {
            if (currentBoost >= dashCost)
            {
                currentBoost -= dashCost;
                
                // Verificar spam de dash
                CheckDashSpam();
                
                return true;
            }
            return false;
        }
        
        public bool UseAbilityBoost(float extraCost)
        {
            float totalCost = 5f + extraCost;
            if (currentBoost >= totalCost)
            {
                currentBoost -= totalCost;
                return true;
            }
            return false;
        }
        
        public void AddBoostOrb(float amount = 30f)
        {
            RegenerateBoost(amount);
            AudioManager.Instance.PlaySFX("BoostPickup");
            
            // Efecto visual
            GameObject effect = ObjectPool.Instance.GetPooledObject("BoostPickup");
            if (effect)
            {
                effect.transform.position = transform.position;
                effect.SetActive(true);
            }
        }
        
        private void CheckDashSpam()
        {
            // Lógica de detección de spam (simplificada)
            penaltyTimer += 0.5f;
            if (penaltyTimer >= 2f)
            {
                penaltyTimer = 4f; // Penalización máxima
                passiveRegen = 0f; // Sin regeneración por 4s
            }
        }
        
        private void ApplyBoostPenalty()
        {
            // Reducción de velocidad cuando se acaba el boost
            CarController car = GetComponent<CarController>();
            if (car)
            {
                StartCoroutine(SpeedPenaltyCoroutine(car));
            }
        }
        
        private System.Collections.IEnumerator SpeedPenaltyCoroutine(CarController car)
        {
            float originalMaxSpeed = 15f;
            car.GetComponent<CarController>().enabled = false;
            
            // Aplicar fuerza contraria
            Vector2 oppositeForce = -car.Rigidbody.velocity * 0.3f;
            car.Rigidbody.AddForce(oppositeForce, ForceMode2D.Impulse);
            
            yield return new WaitForSeconds(2f);
            
            car.GetComponent<CarController>().enabled = true;
            penaltyTimer = 0f;
            passiveRegen = 2f; // Restaurar regeneración
        }
        
        private void UpdateUI()
        {
            if (boostBar)
            {
                float fillAmount = currentBoost / maxBoost;
                boostBar.fillAmount = fillAmount;
                boostBar.color = Color.Lerp(Color.red, boostColor, fillAmount);
            }
            
            if (boostText)
            {
                boostText.text = Mathf.RoundToInt(currentBoost).ToString();
            }
        }
        
        private void UpdateEffects()
        {
            // Partículas de boost
            if (boostParticles)
            {
                var emission = boostParticles.emission;
                emission.enabled = isBoosting && currentBoost > 0;
                
                var main = boostParticles.main;
                main.startColor = boostColor;
            }
            
            // Sonido de boost
            if (boostSound)
            {
                if (isBoosting && currentBoost > 0 && !boostSound.isPlaying)
                {
                    boostSound.Play();
                }
                else if ((!isBoosting || currentBoost <= 0) && boostSound.isPlaying)
                {
                    boostSound.Stop();
                }
            }
        }
        
        // Propiedades públicas
        public float CurrentBoost => currentBoost;
        public float BoostPercentage => currentBoost / maxBoost;
        public bool IsBoosting => isBoosting;
        public bool HasBoost(float required = 20f) => currentBoost >= required;
        
        // Personalización
        public void SetBoostColor(Color color)
        {
            boostColor = color;
        }
        
        public void SetBoostParticles(ParticleSystem particles)
        {
            boostParticles = particles;
        }
        
        public void SetBoostSound(AudioClip clip)
        {
            if (boostSound)
            {
                boostSound.clip = clip;
            }
        }
    }
}
