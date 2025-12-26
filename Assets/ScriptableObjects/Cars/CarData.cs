using UnityEngine;

namespace SwyPhexLeague.ScriptableObjects.Cars
{
    [CreateAssetMenu(fileName = "CarData", menuName = "SwyPhex/Car Data")]
    public class CarData : ScriptableObject
    {
        [Header("Identification")]
        public string carId = "PHEX-01";
        public string displayName = "PHEX-01";
        public string description = "Balanced car with Pulse Dash ability";
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Unlock Requirements")]
        public int unlockLevel = 1;
        public int cost = 0;
        public bool isPremium = false;
        
        [Header("Base Stats")]
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
        [Range(0.5f, 2f)] public float accelerationMultiplier = 1f;
        [Range(0.5f, 2f)] public float turnMultiplier = 1f;
        [Range(0.5f, 2f)] public float jumpMultiplier = 1f;
        [Range(0.5f, 2f)] public float boostMultiplier = 1f;
        [Range(0.5f, 2f)] public float weightMultiplier = 1f;
        
        [Header("Ability")]
        public string abilityName = "PulseDash";
        public float abilityCooldown = 8f;
        public float abilityBoostCost = 5f;
        public Sprite abilityIcon;
        public GameObject abilityPrefab;
        
        [Header("Visual Customization")]
        public Material[] availableMaterials;
        public Color[] availableColors;
        public GameObject[] availableDecals;
        public ParticleSystem[] availableTrails;
        
        [Header("Audio")]
        public AudioClip engineSound;
        public AudioClip boostSound;
        public AudioClip abilitySound;
        public AudioClip collisionSound;
        
        [Header("UI")]
        public Color uiColor = Color.white;
        public Sprite statChart;
        
        public float GetStatValue(StatType stat)
        {
            return stat switch
            {
                StatType.Speed => 15f * speedMultiplier,
                StatType.Acceleration => 35f * accelerationMultiplier,
                StatType.Turn => 3.5f * turnMultiplier,
                StatType.Jump => 12f * jumpMultiplier,
                StatType.Boost => 12f * boostMultiplier,
                StatType.Weight => 1.2f * weightMultiplier,
                _ => 1f
            };
        }
        
        public float[] GetAllStats()
        {
            return new float[]
            {
                speedMultiplier,
                accelerationMultiplier,
                turnMultiplier,
                jumpMultiplier,
                boostMultiplier,
                weightMultiplier
            };
        }
        
        public string[] GetStatNames()
        {
            return new string[]
            {
                "Speed",
                "Acceleration",
                "Turn",
                "Jump",
                "Boost",
                "Weight"
            };
        }
        
        public enum StatType
        {
            Speed,
            Acceleration,
            Turn,
            Jump,
            Boost,
            Weight
        }
    }
}
