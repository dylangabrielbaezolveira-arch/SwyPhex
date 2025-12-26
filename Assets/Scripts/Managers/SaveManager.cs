using UnityEngine;
using System;
using System.IO;

namespace SwyPhexLeague.Managers
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [System.Serializable]
        public class GameData
        {
            // Progresión
            public int playerLevel = 1;
            public int playerXP = 0;
            public int totalCredits = 0;
            public int neonTokens = 0;
            
            // Ranking
            public string currentLeague = "Bronze";
            public int leagueDivision = 1;
            public int rankingPoints = 0;
            public int highestLeague = 0;
            
            // Desbloqueos
            public string[] unlockedCars;
            public string[] unlockedSkins;
            public string[] unlockedTrails;
            public string[] unlockedAvatars;
            
            // Estadísticas
            public int matchesPlayed = 0;
            public int matchesWon = 0;
            public int goalsScored = 0;
            public int savesMade = 0;
            public int dashCount = 0;
            public float timePlayed = 0f;
            
            // Configuración
            public string selectedCar = "PHEX-01";
            public string selectedSkin = "Default";
            public string selectedTrail = "Default";
            public string selectedAvatar = "Default";
            
            // Logros
            public bool[] completedAchievements;
            
            // Temporada actual
            public int seasonNumber = 1;
            public int seasonProgress = 0;
            public bool[] seasonRewardsClaimed;
        }
        
        private GameData gameData;
        private string savePath;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeSaveSystem()
        {
            #if UNITY_EDITOR
            savePath = Application.dataPath + "/Saves/gamesave.json";
            #else
            savePath = Application.persistentDataPath + "/gamesave.json";
            #endif
            
            LoadGame();
            
            // Crear datos iniciales si no existen
            if (gameData == null)
            {
                gameData = new GameData();
                SaveGame();
            }
        }
        
        public void SaveGame()
        {
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
                    gameData = new GameData();
                }
            }
            else
            {
                gameData = new GameData();
            }
        }
        
        #region PROGRESSION
        
        public void AddXP(int xp)
        {
            gameData.playerXP += xp;
            
            // Check level up
            int xpForNextLevel = GetXPForLevel(gameData.playerLevel);
            while (gameData.playerXP >= xpForNextLevel)
            {
                gameData.playerXP -= xpForNextLevel;
                gameData.playerLevel++;
                xpForNextLevel = GetXPForLevel(gameData.playerLevel);
                
                OnLevelUp(gameData.playerLevel);
            }
            
            SaveGame();
        }
        
        private int GetXPForLevel(int level)
        {
            return 1000 + (level - 1) * 250;
        }
        
        private void OnLevelUp(int newLevel)
        {
            // Recompensas por nivel
            AddCredits(50);
            
            if (newLevel % 5 == 0)
            {
                // Caja cosmética cada 5 niveles
                UnlockRandomCosmetic();
            }
            
            if (newLevel == 10 || newLevel == 30 || newLevel == 50)
            {
                // Auto nuevo en niveles específicos
                UnlockCarForLevel(newLevel);
            }
            
            UIManager.Instance.ShowLevelUpNotification(newLevel);
        }
        
        #endregion
        
        #region ECONOMY
        
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
        
        #endregion
        
        #region UNLOCKS
        
        public void UnlockCar(string carId)
        {
            if (!IsCarUnlocked(carId))
            {
                Array.Resize(ref gameData.unlockedCars, 
                    (gameData.unlockedCars?.Length ?? 0) + 1);
                gameData.unlockedCars[gameData.unlockedCars.Length - 1] = carId;
                SaveGame();
            }
        }
        
        public bool IsCarUnlocked(string carId)
        {
            if (gameData.unlockedCars == null) return false;
            
            foreach (string unlockedCar in gameData.unlockedCars)
            {
                if (unlockedCar == carId) return true;
            }
            return false;
        }
        
        private void UnlockRandomCosmetic()
        {
            // Implementar lógica para desbloquear cosmético aleatorio
            // Basado en probabilidades y lo que ya está desbloqueado
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
        
        #endregion
        
        #region RANKING
        
        public void UpdateRanking(int rpChange)
        {
            gameData.rankingPoints += rpChange;
            
            // Actualizar liga si es necesario
            UpdateLeague();
            
            // Actualizar máxima liga
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
        
        #region STATISTICS
        
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
            
            SaveGame();
        }
        
        #endregion
        
        // Propiedades públicas
        public GameData CurrentData => gameData;
        
        public string SelectedCar
        {
            get => gameData.selectedCar;
            set
            {
                gameData.selectedCar = value;
                SaveGame();
            }
        }
        
        public int PlayerLevel => gameData.playerLevel;
        public int Credits => gameData.totalCredits;
        public int RankingPoints => gameData.rankingPoints;
        public string CurrentLeague => gameData.currentLeague;
    }
}
