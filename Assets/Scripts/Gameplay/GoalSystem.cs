using UnityEngine;

namespace SwyPhexLeague.Gameplay
{
    public class GoalSystem : MonoBehaviour
    {
        [System.Serializable]
        public class Goal
        {
            public Collider2D goalCollider;
            public int teamNumber = 0;
            public string teamName = "Blue";
            public Color teamColor = Color.blue;
            public ParticleSystem goalEffect;
            public AudioClip goalSound;
        }
        
        [Header("Goals")]
        public Goal[] goals;
        
        [Header("Settings")]
        public float goalResetTime = 2f;
        public float celebrationDuration = 1f;
        
        public delegate void GoalScoredHandler(int teamNumber, string scorerName);
        public event GoalScoredHandler OnGoalScored;
        
        private void Start()
        {
            foreach (Goal goal in goals)
            {
                if (goal.goalCollider)
                {
                    goal.goalCollider.isTrigger = true;
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ball"))
            {
                Goal goal = GetGoalFromCollider(other);
                if (goal != null)
                {
                    ScoreGoal(goal);
                }
            }
        }
        
        private Goal GetGoalFromCollider(Collider2D collider)
        {
            foreach (Goal goal in goals)
            {
                if (goal.goalCollider == collider)
                {
                    return goal;
                }
            }
            return null;
        }
        
        private void ScoreGoal(Goal goal)
        {
            string scorerName = GetLastToucher();
            
            PlayGoalEffects(goal);
            
            OnGoalScored?.Invoke(goal.teamNumber, scorerName);
            
            StartCoroutine(ResetAfterGoal());
        }
        
        private string GetLastToucher()
        {
            // Implementar lógica para rastrear último jugador que tocó la pelota
            return "Player";
        }
        
        private void PlayGoalEffects(Goal goal)
        {
            if (goal.goalEffect)
            {
                goal.goalEffect.Play();
            }
            
            Managers.AudioManager.Instance?.PlaySFX("Goal");
            
            CameraShake.Instance?.Shake(0.2f, 0.5f);
            
            UI.UIManager.Instance?.ShowGoalNotification(
                GetLastToucher(), 
                goal.teamName
            );
        }
        
        private System.Collections.IEnumerator ResetAfterGoal()
        {
            yield return new WaitForSeconds(goalResetTime);
            
            ResetBall();
            
            yield return new WaitForSeconds(celebrationDuration);
            
            GameManager.Instance?.ResumeGame();
        }
        
        private void ResetBall()
        {
            Core.BallPhysics ball = FindObjectOfType<Core.BallPhysics>();
            if (ball && GameManager.Instance)
            {
                ball.ResetBall(GameManager.Instance.BallSpawnPoint);
            }
        }
        
        public void RegisterGoal(Collider2D collider, int teamNumber, string teamName, Color teamColor)
        {
            Goal newGoal = new Goal
            {
                goalCollider = collider,
                teamNumber = teamNumber,
                teamName = teamName,
                teamColor = teamColor
            };
            
            System.Array.Resize(ref goals, goals.Length + 1);
            goals[goals.Length - 1] = newGoal;
        }
    }
}
