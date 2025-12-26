using UnityEngine;
using UnityEngine.UI;

namespace SwyPhexLeague.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Player HUD")]
        public Slider boostSlider;
        public Image boostFill;
        public Text boostText;
        public Image abilityIcon;
        public Image abilityCooldown;
        public Text abilityText;
        public Text speedText;
        
        [Header("Game HUD")]
        public Text timerText;
        public Text scoreText;
        public GameObject goalAnnouncement;
        public Text goalText;
        public Animator goalAnimator;
        
        [Header("References")]
        public Core.CarController playerCar;
        public Core.BoostSystem playerBoost;
        public Gameplay.AbilitySystem playerAbility;
        
        private void Start()
        {
            FindPlayerReferences();
        }
        
        private void FindPlayerReferences()
        {
            if (!playerCar)
            {
                playerCar = FindObjectOfType<Core.CarController>();
            }
            
            if (playerCar)
            {
                if (!playerBoost)
                {
                    playerBoost = playerCar.GetComponent<Core.BoostSystem>();
                }
                
                if (!playerAbility)
                {
                    playerAbility = playerCar.GetComponent<Gameplay.AbilitySystem>();
                }
            }
        }
        
        private void Update()
        {
            UpdatePlayerHUD();
        }
        
        private void UpdatePlayerHUD()
        {
            if (playerBoost)
            {
                UpdateBoostHUD();
            }
            
            if (playerAbility)
            {
                UpdateAbilityHUD();
            }
            
            if (playerCar)
            {
                UpdateSpeedHUD();
            }
        }
        
        private void UpdateBoostHUD()
        {
            if (boostSlider)
            {
                boostSlider.value = playerBoost.BoostPercentage;
            }
            
            if (boostFill)
            {
                boostFill.color = Color.Lerp(Color.red, Color.cyan, playerBoost.BoostPercentage);
            }
            
            if (boostText)
            {
                boostText.text = Mathf.RoundToInt(playerBoost.CurrentBoost).ToString();
            }
        }
        
        private void UpdateAbilityHUD()
        {
            if (playerAbility.CurrentAbility != null)
            {
                Gameplay.AbilitySystem.Ability ability = playerAbility.CurrentAbility;
                
                if (abilityIcon && ability.icon)
                {
                    abilityIcon.sprite = ability.icon;
                }
                
                if (abilityCooldown)
                {
                    abilityCooldown.fillAmount = ability.currentCooldown / ability.cooldown;
                }
                
                if (abilityText)
                {
                    if (ability.currentCooldown > 0)
                    {
                        abilityText.text = Mathf.CeilToInt(ability.currentCooldown).ToString();
                    }
                    else
                    {
                        abilityText.text = ability.name;
                    }
                }
            }
        }
        
        private void UpdateSpeedHUD()
        {
            if (speedText)
            {
                speedText.text = $"{Mathf.RoundToInt(playerCar.CurrentSpeed)}";
                
                if (playerCar.CurrentSpeed > 12f)
                {
                    speedText.color = Color.Lerp(Color.yellow, Color.red, 
                        (playerCar.CurrentSpeed - 12f) / 3f);
                }
                else
                {
                    speedText.color = Color.white;
                }
            }
        }
        
        public void ShowGoalAnnouncement(string scorer, string team)
        {
            if (goalAnnouncement && goalText)
            {
                goalText.text = $"{scorer} - {team}";
                goalAnnouncement.SetActive(true);
                
                if (goalAnimator)
                {
                    goalAnimator.SetTrigger("Show");
                }
                
                Invoke("HideGoalAnnouncement", 2f);
            }
        }
        
        private void HideGoalAnnouncement()
        {
            if (goalAnnouncement)
            {
                goalAnnouncement.SetActive(false);
            }
        }
        
        public void UpdateGameTimer(float time)
        {
            if (timerText)
            {
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }
        
        public void UpdateGameScore(int team1, int team2)
        {
            if (scoreText)
            {
                scoreText.text = $"{team1} - {team2}";
            }
        }
        
        public void ShowGravityWarning(string warning)
        {
            // Implementar warning de gravedad
            Debug.Log($"Gravity Warning: {warning}");
        }
    }
}
