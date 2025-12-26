using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace SwyPhexLeague.Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [System.Serializable]
        public class GameData
        {
            // Player Progression
            public int playerLevel = 1;
            public int playerXP = 0;
            public int totalCredits = 0;
            public int neonTokens = 0;
            
            // Unlocks
            public List<string> unlockedCars = new List<string>();
            public List<string> unlockedSkins = new List<string>();
            public List<string> unlockedTrails = new List<string>();
            public List<string> unlockedAvatars = new List<string>();
            
            // Settings
            public string selectedCar = "PHEX-01";
            public string selectedSkin = "Default";
            public string selectedTrail = "Default";
            public string selectedAvatar = "Default";
            
            // Statistics
            public int matchesPlayed = 0;
            public int matchesWon = 0;
            public int goalsScored = 0;
            public int savesMade = 0;
            public int dashCount = 0;
            public float timePlayed = 0f;
            
            // Ranking
            public string currentLeague = "Bronze";
            public int leagueDivision = 1;
            public int rankingPoints = 0;
            public int highestLeague = 0;
            
            // Achievements
            public Dictionary<string, bool> achievements = new Dictionary<string, bool>();
            
            // Season Progress
            public int seasonNumber = 1;
            public int seasonProgress = 0;
            public bool[] seasonRewardsClaimed;
            
            // Tutorial Completion
            public bool tutorialCompleted = false;
        }
        
        private GameData gameData;
        private string savePath;
        private bool isInitialized = false;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            #if UNITY_EDITOR
            savePath = Application.dataPath + "/Saves/";
            #else
            savePath = Application.persistentDataPath + "/Saves/";
            #endif
            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            savePath += "gamesave.json";
            
            LoadGame();
            isInitialized = true;
        }
        
        public void SaveGame()
        {
            if (!isInitialized || gameData == null) return;
            
            try
            {
                string jsonData = JsonUtility.ToJson(gameData, true);
                File.WriteAllText(savePath, jsonData);
                Debug.Log("Game saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }
        
        public void LoadGame()
        {
            if (File.Exists(savePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(savePath);
                    gameData = JsonUtility.FromJson<GameData>(jsonData);
                    Debug.Log("Game loaded successfully");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Load failed: {e.Message}");
                    CreateNewGame();
                }
            }
            else
            {
                CreateNewGame();
            }
        }
        
        private void CreateNewGame()
        {
            gameData = new GameData();
            
            // Unlock default car
            gameData.unlockedCars.Add("PHEX-01");
            gameData.selectedCar = "PHEX-01";
            
            // Initialize achievements
            InitializeAchievements();
            
            SaveGame();
        }
        
        private void InitializeAchievements()
        {
            gameData.achievements["FirstWin"] = false;
            gameData.achievements["Score10Goals"] = false;
            gameData.achievements["Play10Matches"] = false;
            gameData.achievements["ReachGold"] = false;
            gameData.achievements["CompleteTutorial"] = false;
        }
        
        #region Progression
        
        public void AddXP(int xp)
        {
            gameData.playerXP += xp;
            
            int xpForNextLevel = CalculateXPForLevel(gameData.playerLevel);
            while (gameData.playerXP >= xpForNextLevel)
            {
                gameData.playerXP -= xpForNextLevel;
                gameData.playerLevel++;
                xpForNextLevel = CalculateXPForLevel(gameData.playerLevel);
                
                OnLevelUp(gameData.playerLevel);
            }
            
            SaveGame();
        }
        
        private int CalculateXPForLevel(int level)
        {
            return 1000 + (level - 1) * 250;
        }
        
        private void OnLevelUp(int newLevel)
        {
            AddCredits(50);
            
            if (newLevel % 5 == 0)
            {
                UnlockRandomCosmetic();
            }
            
            if (newLevel == 10 || newLevel == 30 || newLevel == 50)
            {
                UnlockCarForLevel(newLevel);
            }
            
            // Notificar UI
            UI.UIManager.Instance?.ShowLevelUpNotification(newLevel);
        }
        
        #endregion
        
        #region Economy
        
        public void AddCredits(int amount)
        {
            gameData.totalCredits += amount;
            SaveGame();
        }
        
        public bool SpendCredits(int amount)
        {
            if (gameData.totalCredits >= amount)
            {
                gameData.totalCredits -= amount;
                SaveGame();
                return true;
            }
            return false;
        }
        
        public void AddNeonTokens(int amount)
        {
            gameData.neonTokens += amount;
            SaveGame();
        }
        
        public bool SpendNeonTokens(int amount)
        {
            if (gameData.neonTokens >= amount)
            {
                gameData.neonTokens -= amount;
                SaveGame();
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region Unlocks
        
        public void UnlockCar(string carId)
        {
            if (!gameData.unlockedCars.Contains(carId))
            {
                gameData.unlockedCars.Add(carId);
                SaveGame();
            }
        }
        
        public bool IsCarUnlocked(string carId)
        {
            return gameData.unlockedCars.Contains(carId);
        }
        
        private void UnlockCarForLevel(int level)
        {
            string carId = level switch
            {
                10 => "NEON_WRAITH",
                30 => "GRAVRIDER",
                50 => "STREET_BRUISER",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(carId))
            {
                UnlockCar(carId);
            }
        }
        
        private void UnlockRandomCosmetic()
        {
            // Implementar lógica para desbloquear cosmético aleatorio
            // Basado en probabilidades
        }
        
        public void UnlockSkin(string skinId)
        {
            if (!gameData.unlockedSkins.Contains(skinId))
            {
                gameData.unlockedSkins.Add(skinId);
                SaveGame();
            }
        }
        
        public void UnlockTrail(string trailId)
        {
            if (!gameData.unlockedTrails.Contains(trailId))
            {
                gameData.unlockedTrails.Add(trailId);
                SaveGame();
            }
        }
        
        public bool IsLevelUnlocked(int levelIndex)
        {
            // Niveles 0-3 desbloqueados por defecto
            if (levelIndex < 4) return true;
            
            // Niveles posteriores requieren progresión
            return gameData.playerLevel >= levelIndex * 5;
        }
        
        #endregion
        
        #region Statistics
        
        public void RecordMatch(bool won, int goals, int saves, int dashes, float time)
        {
            gameData.matchesPlayed++;
            if (won) gameData.matchesWon++;
            gameData.goalsScored += goals;
            gameData.savesMade += saves;
            gameData.dashCount += dashes;
            gameData.timePlayed += time;
            
            // XP por partida
            int xpEarned = 100 + (won ? 50 : 0) + (goals * 10) + (saves * 5);
            AddXP(xpEarned);
            
            // Créditos por partida
            int creditsEarned = 25 + (won ? 25 : 0);
            AddCredits(creditsEarned);
            
            // Logros
            CheckAchievements();
            
            SaveGame();
        }
        
        private void CheckAchievements()
        {
            if (!gameData.achievements["FirstWin"] && gameData.matchesWon > 0)
            {
                UnlockAchievement("FirstWin");
            }
            
            if (!gameData.achievements["Score10Goals"] && gameData.goalsScored >= 10)
            {
                UnlockAchievement("Score10Goals");
            }
            
            if (!gameData.achievements["Play10Matches"] && gameData.matchesPlayed >= 10)
            {
                UnlockAchievement("Play10Matches");
            }
        }
        
        private void UnlockAchievement(string achievementId)
        {
            if (gameData.achievements.ContainsKey(achievementId))
            {
                gameData.achievements[achievementId] = true;
                
                // Recompensa por logro
                switch (achievementId)
                {
                    case "FirstWin":
                        AddCredits(100);
                        break;
                    case "Score10Goals":
                        AddCredits(250);
                        break;
                    case "Play10Matches":
                        AddNeonTokens(50);
                        break;
                }
                
                SaveGame();
                
                // Notificar UI
                UI.UIManager.Instance?.ShowAchievementUnlocked(achievementId);
            }
        }
        
        #endregion
        
        #region Ranking
        
        public void UpdateRanking(int rpChange)
        {
            gameData.rankingPoints += rpChange;
            gameData.rankingPoints = Mathf.Max(0, gameData.rankingPoints);
            
            UpdateLeague();
            
            int currentLeagueValue = LeagueToValue(gameData.currentLeague);
            if (currentLeagueValue > gameData.highestLeague)
            {
                gameData.highestLeague = currentLeagueValue;
            }
            
            SaveGame();
        }
        
        private void UpdateLeague()
        {
            (string league, int division) = GetLeagueForRP(gameData.rankingPoints);
            gameData.currentLeague = league;
            gameData.leagueDivision = division;
        }
        
        private (string, int) GetLeagueForRP(int rp)
        {
            if (rp < 800) return ("Bronze", Mathf.Clamp(rp / 267, 1, 3));
            if (rp < 1200) return ("Silver", Mathf.Clamp((rp - 800) / 134, 1, 3));
            if (rp < 1600) return ("Gold", Mathf.Clamp((rp - 1200) / 134, 1, 3));
            if (rp < 2000) return ("Platinum", Mathf.Clamp((rp - 1600) / 134, 1, 3));
            if (rp < 2400) return ("Neon", Mathf.Clamp((rp - 2000) / 134, 1, 3));
            return ("Phex", 1);
        }
        
        private int LeagueToValue(string league)
        {
            return league switch
            {
                "Bronze" => 1,
                "Silver" => 2,
                "Gold" => 3,
                "Platinum" => 4,
                "Neon" => 5,
                "Phex" => 6,
                _ => 0
            };
        }
        
        #endregion
        
        #region Getters
        
        public int PlayerLevel => gameData.playerLevel;
        public int Credits => gameData.totalCredits;
        public int NeonTokens => gameData.neonTokens;
        public int RankingPoints => gameData.rankingPoints;
        public string CurrentLeague => gameData.currentLeague;
        public int LeagueDivision => gameData.leagueDivision;
        
        public string SelectedCar
        {
            get => gameData.selectedCar;
            set
            {
                gameData.selectedCar = value;
                SaveGame();
            }
        }
        
        public string SelectedSkin
        {
            get => gameData.selectedSkin;
            set
            {
                gameData.selectedSkin = value;
                SaveGame();
            }
        }
        
        public List<string> GetUnlockedCars() => new List<string>(gameData.unlockedCars);
        public List<string> GetUnlockedSkins() => new List<string>(gameData.unlockedSkins);
        
        public GameData GetGameData() => gameData;
        
        #endregion
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }
        
        private void OnApplicationQuit()
        {
            SaveGame();
        }
        
        public void ResetSave()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            
            CreateNewGame();
            LoadGame();
        }
    }
}
