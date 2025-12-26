using UnityEngine;
using UnityEngine.UI;

namespace SwyPhexLeague.Core
{
    public class BoostSystem : MonoBehaviour
    {
        [Header("Boost Settings")]
        public float maxBoost = 100f;
        public float startBoost = 50f;
        public float passiveRegen = 2f;
        public float boostConsumption = 12f;
        public float dashCost = 18f;
        public float abilityExtraCost = 5f;
        
        [Header("UI References")]
        public Image boostBar;
        public Text boostText;
        
        [Header("Effects")]
        public ParticleSystem boostParticles;
        public AudioSource boostAudio;
        
        [Header("State")]
        private float currentBoost;
        private bool isBoosting;
        private float penaltyTimer = 0f;
        private float boostMultiplier = 1f;
        
        private Color boostColor = Color.cyan;
        private float originalRegen;
        
        private void Start()
        {
            currentBoost = startBoost;
            originalRegen = passiveRegen;
            
            if (boostBar)
            {
                boostColor = boostBar.color;
            }
            
            UpdateUI();
        }
        
        private void Update()
        {
            HandleRegeneration();
            HandleBoostConsumption();
            HandlePenalty();
            
            UpdateUI();
            UpdateEffects();
        }
        
        private void HandleRegeneration()
        {
            if (!isBoosting && penaltyTimer <= 0f)
            {
                currentBoost += passiveRegen * Time.deltaTime;
                currentBoost = Mathf.Min(currentBoost, maxBoost);
            }
        }
        
        private void HandleBoostConsumption()
        {
            if (isBoosting)
            {
                currentBoost -= boostConsumption * Time.deltaTime * boostMultiplier;
                
                if (currentBoost <= 0f)
                {
                    currentBoost = 0f;
                    StopBoosting();
                    ApplyBoostPenalty();
                }
            }
        }
        
        private void HandlePenalty()
        {
            if (penaltyTimer > 0f)
            {
                penaltyTimer -= Time.deltaTime;
                
                if (penaltyTimer <= 0f)
                {
                    passiveRegen = originalRegen;
                }
            }
        }
        
        public void UseBoost(float amount = 0f)
        {
            if (amount == 0f)
            {
                if (currentBoost > 0)
                    isBoosting = true;
                return;
            }
            
            currentBoost = Mathf.Max(0, currentBoost - amount);
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
                CheckDashSpam();
                return true;
            }
            return false;
        }
        
        public bool UseAbilityBoost()
        {
            float totalCost = abilityExtraCost;
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
            Managers.AudioManager.Instance?.PlaySFX("BoostPickup");
            
            GameObject effect = Utilities.ObjectPool.Instance?.GetPooledObject("BoostPickup");
            if (effect)
            {
                effect.transform.position = transform.position;
                effect.SetActive(true);
            }
        }
        
        private void CheckDashSpam()
        {
            penaltyTimer += 0.5f;
            if (penaltyTimer >= 2f)
            {
                penaltyTimer = 4f;
                passiveRegen = 0f;
            }
        }
        
        private void ApplyBoostPenalty()
        {
            CarController car = GetComponent<CarController>();
            if (car)
            {
                StartCoroutine(SpeedPenaltyCoroutine(car));
            }
        }
        
        private System.Collections.IEnumerator SpeedPenaltyCoroutine(CarController car)
        {
            float originalMaxSpeed = car.maxSpeed;
            car.maxSpeed *= 0.7f;
            
            Vector2 oppositeForce = -car.Rigidbody.velocity * 0.3f;
            car.Rigidbody.AddForce(oppositeForce, ForceMode2D.Impulse);
            
            yield return new WaitForSeconds(2f);
            
            car.maxSpeed = originalMaxSpeed;
            penaltyTimer = 0f;
            passiveRegen = originalRegen;
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
                boostText.color = boostBar ? boostBar.color : Color.white;
            }
        }
        
        private void UpdateEffects()
        {
            if (boostParticles)
            {
                var emission = boostParticles.emission;
                emission.enabled = isBoosting && currentBoost > 0;
                
                var main = boostParticles.main;
                main.startColor = boostColor;
            }
            
            if (boostAudio)
            {
                if (isBoosting && currentBoost > 0)
                {
                    if (!boostAudio.isPlaying)
                        boostAudio.Play();
                        
                    boostAudio.volume = Mathf.Lerp(0.1f, 0.3f, currentBoost / maxBoost);
                }
                else if (boostAudio.isPlaying)
                {
                    boostAudio.Stop();
                }
            }
        }
        
        public bool HasBoost(float required = 20f)
        {
            return currentBoost >= required;
        }
        
        public void SetBoostColor(Color color)
        {
            boostColor = color;
            
            if (boostBar)
            {
                boostBar.color = boostColor;
            }
        }
        
        public void SetBoostParticles(ParticleSystem particles)
        {
            boostParticles = particles;
        }
        
        public void SetBoostSound(AudioClip clip)
        {
            if (boostAudio)
            {
                boostAudio.clip = clip;
            }
        }
        
        public float CurrentBoost => currentBoost;
        public float BoostPercentage => currentBoost / maxBoost;
        public bool IsBoosting => isBoosting;
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                RegenerateBoost(10f);
            }
        }
    }
}
