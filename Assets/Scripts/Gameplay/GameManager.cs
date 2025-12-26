using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SwyPhexLeague.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private int maxScore = 5;
        [SerializeField] private float goalResetTime = 2f;
        [SerializeField] private float matchEndDelay = 3f;
        
        [Header("References")]
        [SerializeField] private Transform ballSpawnPoint;
        [SerializeField] private Goal[] teamGoals;
        [SerializeField] private Transform[] playerSpawns;
        
        [Header("Game State")]
        private int[] teamScores = new int[2];
        private bool isGameActive = true;
        private BallPhysics ball;
        private List<CarController> players = new List<CarController>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            // Encontrar pelota
            ball = FindObjectOfType<BallPhysics>();
            if (!ball)
            {
                Debug.LogError("No ball found in scene!");
                return;
            }
            
            // Encontrar jugadores
            CarController[] foundPlayers = FindObjectsOfType<CarController>();
            players.AddRange(foundPlayers);
            
            // Configurar metas
            foreach (Goal goal in teamGoals)
            {
                goal.OnGoalScored += HandleGoalScored;
            }
            
            // Inicializar puntajes
            teamScores[0] = 0;
            teamScores[1] = 0;
            
            UpdateScoreUI();
            
            // Posicionar jugadores
            PositionPlayers();
            
            // Iniciar pelota
            ResetBall();
        }
        
        private void PositionPlayers()
        {
            if (playerSpawns.Length == 0 || players.Count == 0) return;
            
            for (int i = 0; i < players.Count; i++)
            {
                int spawnIndex = i % playerSpawns.Length;
                players[i].transform.position = playerSpawns[spawnIndex].position;
                players[i].ResetVelocity();
            }
        }
        
        public void HandleGoalScored(int teamNumber, string scorerName)
        {
            if (!isGameActive) return;
            
            // Incrementar puntaje
            teamScores[teamNumber]++;
            
            // Actualizar UI
            UpdateScoreUI();
            
            // Mostrar notificación
            string teamColor = teamNumber == 0 ? "Blue Team" : "Orange Team";
            UIManager.Instance.ShowGoalNotification(scorerName, teamColor);
            
            // Verificar si hay ganador
            if (teamScores[teamNumber] >= maxScore)
            {
                EndGame();
                return;
            }
            
            // Resetear después de delay
            StartCoroutine(ResetAfterGoal());
        }
        
        private IEnumerator ResetAfterGoal()
        {
            isGameActive = false;
            
            yield return new WaitForSeconds(goalResetTime);
            
            ResetBall();
            PositionPlayers();
            
            isGameActive = true;
        }
        
        public void ResetBall()
        {
            if (ball && ballSpawnPoint)
            {
                ball.ResetBall(ballSpawnPoint.position);
            }
        }
        
        private void UpdateScoreUI()
        {
            UIManager.Instance.UpdateScore(teamScores[0], teamScores[1]);
        }
        
        public void EndGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
            // Determinar ganador
            int winningTeam = teamScores[0] > teamScores[1] ? 0 : 
                             teamScores[1] > teamScores[0] ? 1 : -1;
            
            // Mostrar resultados
            ShowMatchResults(winningTeam);
            
            // Guardar progreso si es ranked
            if (IsRankedMatch())
            {
                SaveMatchResults(winningTeam);
            }
            
            // Volver al menú después de delay
            StartCoroutine(ReturnToMenuAfterDelay());
        }
        
        private void ShowMatchResults(int winningTeam)
        {
            string resultText;
            
            if (winningTeam == -1)
            {
                resultText = "DRAW!";
            }
            else
            {
                resultText = $"TEAM {winningTeam + 1} WINS!";
            }
            
            // Mostrar panel de resultados
            UIManager.Instance.ShowMatchResults(
                resultText, 
                teamScores[0], 
                teamScores[1]
            );
            
            // Efectos
            AudioManager.Instance.PlaySFX(winningTeam == -1 ? "Draw" : "Victory");
            
            if (winningTeam != -1)
            {
                // Confetti para el equipo ganador
                SpawnConfetti(winningTeam == 0 ? Color.blue : Color.orange);
            }
        }
        
        private void SpawnConfetti(Color teamColor)
        {
            GameObject confetti = ObjectPool.Instance.GetPooledObject("Confetti");
            if (confetti)
            {
                confetti.transform.position = Vector3.zero;
                
                // Configurar color del equipo
                ParticleSystem ps = confetti.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = teamColor;
                
                confetti.SetActive(true);
            }
        }
        
        private bool IsRankedMatch()
        {
            // Verificar si es partida ranked
            return SceneManager.GetActiveScene().name.Contains("Ranked");
        }
        
        private void SaveMatchResults(int winningTeam)
        {
            // Calcular cambio de RP
            int rpChange = CalculateRPChange(winningTeam);
            
            // Actualizar ranking
            SaveManager.Instance.UpdateRanking(rpChange);
            
            // Otorgar créditos
            int creditsEarned = CalculateCreditsEarned();
            SaveManager.Instance.AddCredits(creditsEarned);
            
            // Guardar progreso
            SaveManager.Instance.SaveGame();
        }
        
        private int CalculateRPChange(int winningTeam)
        {
            // Lógica simplificada de ELO
            int baseRP = 15;
            
            // Ajustar por diferencia de skill
            // Implementar lógica completa según sistema de ranking
            
            return winningTeam == 0 ? baseRP : -baseRP;
        }
        
        private int CalculateCreditsEarned()
        {
            int baseCredits = 25;
            
            // Bonus por victoria
            if (teamScores[0] > teamScores[1])
                baseCredits += 25;
            
            // Bonus por tiempo
            float timeBonus = Mathf.Clamp(UIManager.Instance.GameTimer / 120f, 0.5f, 1.5f);
            baseCredits = Mathf.RoundToInt(baseCredits * timeBonus);
            
            return baseCredits;
        }
        
        private IEnumerator ReturnToMenuAfterDelay()
        {
            yield return new WaitForSeconds(matchEndDelay);
            
            UIManager.Instance.ReturnToMenu();
        }
        
        private void OnDestroy()
        {
            // Desuscribirse de eventos
            foreach (Goal goal in teamGoals)
            {
                if (goal)
                    goal.OnGoalScored -= HandleGoalScored;
            }
        }
        
        // Propiedades públicas
        public bool IsGameActive => isGameActive;
        public BallPhysics Ball => ball;
        
        public void AddPlayer(CarController player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }
        
        public void RemovePlayer(CarController player)
        {
            players.Remove(player);
        }
    }
}
