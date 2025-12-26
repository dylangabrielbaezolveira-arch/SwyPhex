using UnityEngine;

namespace SwyPhexLeague.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "SwyPhex/Car Data")]
    public class CarData : ScriptableObject
    {
        [Header("Car Identity")]
        public string carId;
        public string carName;
        public string description;
        public Sprite icon;
        public GameObject carPrefab;
        public int unlockLevel = 1;
        public int cost = 500;
        
        [Header("Base Stats")]
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
        [Range(0.5f, 2f)] public float accelerationMultiplier = 1f;
        [Range(0.5f, 2f)] public float turnMultiplier = 1f;
        [Range(0.5f, 2f)] public float jumpMultiplier = 1f;
        [Range(0.5f, 2f)] public float boostMultiplier = 1f;
        
        [Header("Ability")]
        public string abilityName;
        public float abilityCooldown = 8f;
        public float abilityBoostCost = 5f;
        public Sprite abilityIcon;
        public GameObject abilityEffect;
        
        [Header("Visual Customization")]
        public Material[] availableMaterials;
        public Color[] availableColors;
        public GameObject[] availableDecals;
        
        [Header("Audio")]
        public AudioClip engineSound;
        public AudioClip boostSound;
        public AudioClip abilitySound;
        
        public float GetStatValue(CarStat stat)
        {
            switch (stat)
            {
                case CarStat.Speed: return 15f * speedMultiplier;
                case CarStat.Acceleration: return 35f * accelerationMultiplier;
                case CarStat.Turn: return 3.5f * turnMultiplier;
                case CarStat.Jump: return 12f * jumpMultiplier;
                case CarStat.Boost: return 12f * boostMultiplier;
                default: return 1f;
            }
        }
        
        public enum CarStat
        {
            Speed,
            Acceleration,
            Turn,
            Jump,
            Boost
        }
    }
}
