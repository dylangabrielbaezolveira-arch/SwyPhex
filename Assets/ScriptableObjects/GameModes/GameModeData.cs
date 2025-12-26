using UnityEngine;

namespace SwyPhexLeague.ScriptableObjects.GameModes
{
    [CreateAssetMenu(fileName = "GameModeData", menuName = "SwyPhex/Game Mode")]
    public class GameModeData : ScriptableObject
    {
        [Header("Mode Information")]
        public string modeId = "Ranked1v1";
        public string displayName = "Ranked 1v1";
        public string description = "Competitive 1v1 matches";
        public Sprite icon;
        
        [Header("Game Rules")]
        public int teamSize = 1;
        public int maxScore = 5;
        public float matchTime = 120f;
        public bool overtimeEnabled = true;
        public float overtimeDuration = 60f;
        public bool suddenDeath = false;
        
        [Header("Restrictions")]
        public bool abilitiesAllowed = true;
        public bool boostOrbsEnabled = false;
        public bool variableGravityAllowed = true;
        public string[] allowedMapIds;
        
        [Header("Rewards")]
        public int creditsPerWin = 25;
        public int creditsPerLoss = 10;
        public int xpPerWin = 100;
        public int xpPerLoss = 50;
        public int rankingPointsPerWin = 15;
        public int rankingPointsPerLoss = -10;
        
        [Header("Matchmaking")]
        public int minPlayers = 2;
        public int maxPlayers = 2;
        public float matchmakingTimeout = 60f;
        public bool fillWithBots = false;
        
        public bool IsMapAllowed(string mapId)
        {
            if (allowedMapIds.Length == 0) return true;
            
            foreach (string allowedId in allowedMapIds)
            {
                if (allowedId == mapId) return true;
            }
            return false;
        }
        
        public int CalculateRewards(bool won, int performanceScore)
        {
            int baseReward = won ? creditsPerWin : creditsPerLoss;
            int performanceBonus = Mathf.RoundToInt(baseReward * (performanceScore / 100f));
            
            return baseReward + performanceBonus;
        }
        
        public enum ModeType
        {
            Ranked,
            Casual,
            Training,
            Tournament,
            Event
        }
    }
}
