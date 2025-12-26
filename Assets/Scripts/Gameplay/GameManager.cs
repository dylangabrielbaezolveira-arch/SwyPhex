using UnityEngine;
using System.Collections;

namespace SwyPhexLeague.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        public int maxScore = 5;
        public float matchTime = 120f;
        public Vector2 ballSpawnPoint = Vector2.zero;
        
        [Header("Teams")]
        public Team[] teams;
        
        [System.Serializable]
        public class Team
        {
            public string name = "Blue";
            public Color color = Color.blue;
            public int score = 0;
            public Transform[] spawnPoints;
        }
        
        [Header("State")]
        private float currentTime = 0f;
        private bool isGameActive = false;
        private bool isPaused = false;
        
        [Header("References")]
        public Core.BallPhysics ball;
        public GoalSystem goalSystem;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            if (!ball)
            {
                ball = FindObjectOfType<Core.BallPhysics>();
            }
            
            if (!goalSystem)
            {
                goalSystem = FindObjectOfType<GoalSystem>();
                if (goalSystem)
                {
                    goalSystem.OnGoalScored += HandleGoalScored;
                }
            }
            
            ResetTeams();
            SetupPlayers();
            
            currentTime = matchTime;
            isGameActive = true;
            
            UI.UIManager.Instance?.UpdateScore(teams[0].score, teams[1].score);
            UI.UIManager.Instance?.StartTimer(matchTime);
        }
        
        private void ResetTeams()
        {
            foreach (Team team in teams)
            {
                team.score = 0;
            }
        }
        
        private void SetupPlayers()
        {
            CarController[] players = FindObjectsOfType<CarController>();
            
            for (int i = 0; i < players.Length; i++)
            {
                Team team = teams[i % teams.Length];
                if (team.spawnPoints.Length > 0)
                {
                    int spawnIndex = i / teams.Length;
                    if (spawnIndex < team.spawnPoints.Length)
                    {
                        players[i].TeleportTo(team.spawnPoints[spawnIndex].position);
                    }
                }
            }
        }
        
        private void Update()
        {
            if (isGameActive && !isPaused)
            {
                UpdateTimer();
            }
        }
        
        private void UpdateTimer()
        {
            currentTime -= Time.deltaTime;
            UI.UIManager.Instance?.UpdateTimer(currentTime);
            
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                EndGame();
            }
        }
        
        public void HandleGoalScored(int teamNumber, string scorerName)
        {
            if (!isGameActive) return;
            
            if (teamNumber >= 0 && teamNumber < teams.Length)
            {
                teams[teamNumber].score++;
                UI.UIManager.Instance?.UpdateScore(teams[0].score, teams[1].score);
                
                if (teams[teamNumber].score >= maxScore)
                {
                    EndGame();
                    return;
                }
            }
            
            StartCoroutine(ResetAfterGoal());
        }
        
        private IEnumerator ResetAfterGoal()
        {
            isGameActive = false;
            
            yield return new WaitForSeconds(2f);
            
            ResetBall();
            ResetPlayers();
            
            isGameActive = true;
        }
        
        private void ResetBall()
        {
            if (ball)
            {
                ball.ResetBall(ballSpawnPoint);
            }
        }
        
        private void ResetPlayers()
        {
            SetupPlayers();
        }
        
        public void EndGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
            int winningTeam = -1;
            if (teams[0].score > teams[1].score)
            {
                winningTeam = 0;
            }
            else if (teams[1].score > teams[0].score)
            {
                winningTeam = 1;
            }
            
            ShowResults(winningTeam);
            SaveMatchResults(winningTeam);
            
            StartCoroutine(ReturnToMenuAfterDelay(5f));
        }
        
        private void ShowResults(int winningTeam)
        {
            string resultText = winningTeam == -1 ? "DRAW!" : $"{teams[winningTeam].name} WINS!";
            
            UI.UIManager.Instance?.ShowMatchResults(
                resultText,
                teams[0].score,
                teams[1].score
            );
            
            Managers.AudioManager.Instance?.PlaySFX(
                winningTeam == -1 ? "Draw" : "Victory"
            );
            
            if (winningTeam != -1)
            {
                CreateConfetti(teams[winningTeam].color);
            }
        }
        
        private void CreateConfetti(Color color)
        {
            GameObject confetti = Utilities.ObjectPool.Instance?.GetPooledObject("Confetti");
            if (confetti)
            {
                confetti.transform.position = Vector2.zero;
                
                ParticleSystem ps = confetti.GetComponent<ParticleSystem>();
                if (ps)
                {
                    var main = ps.main;
                    main.startColor = color;
                }
                
                confetti.SetActive(true);
            }
        }
        
        private void SaveMatchResults(int winningTeam)
        {
            // Guardar progreso
            Managers.SaveManager.Instance?.RecordMatch(
                winningTeam == 0, // Asumiendo que el jugador es equipo 0
                teams[0].score,
                0, // saves
                0, // dashes
                matchTime - currentTime
            );
        }
        
        private IEnumerator ReturnToMenuAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            UI.UIManager.Instance?.ReturnToMainMenu();
        }
        
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        
        public Vector2 BallSpawnPoint => ballSpawnPoint;
        public bool IsGameActive => isGameActive;
        
        private void OnDestroy()
        {
            if (goalSystem)
            {
                goalSystem.OnGoalScored -= HandleGoalScored;
            }
        }
    }
}
