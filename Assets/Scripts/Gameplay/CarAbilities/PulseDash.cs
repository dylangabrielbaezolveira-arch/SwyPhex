using UnityEngine;

namespace SwyPhexLeague.Gameplay.CarAbilities
{
    [CreateAssetMenu(fileName = "PulseDash", menuName = "SwyPhex/Abilities/Pulse Dash")]
    public class PulseDash : AbilityData
    {
        [Header("Pulse Dash Settings")]
        public float dashForce = 25f;
        public float invincibilityDuration = 0.3f;
        public float ballPushMultiplier = 1.5f;
        
        public override void Activate(GameObject car)
        {
            CarController controller = car.GetComponent<CarController>();
            if (!controller) return;
            
            Vector2 dashDirection = controller.Rigidbody.velocity.normalized;
            if (dashDirection.magnitude < 0.1f)
            {
                dashDirection = car.transform.right;
            }
            
            controller.ApplyExternalForce(dashDirection * dashForce);
            
            AbilitySystem abilitySystem = car.GetComponent<AbilitySystem>();
            if (abilitySystem)
            {
                abilitySystem.StartCoroutine(InvincibilityCoroutine(car, invincibilityDuration));
            }
        }
        
        private System.Collections.IEnumerator InvincibilityCoroutine(GameObject car, float duration)
        {
            CarController controller = car.GetComponent<CarController>();
            if (!controller) yield break;
            
            controller.gameObject.layer = LayerMask.NameToLayer("Invincible");
            
            SpriteRenderer renderer = car.GetComponentInChildren<SpriteRenderer>();
            Color originalColor = renderer ? renderer.color : Color.white;
            
            float timer = 0f;
            while (timer < duration)
            {
                if (renderer)
                {
                    float alpha = Mathf.PingPong(timer * 10f, 1f);
                    renderer.color = Color.Lerp(originalColor, Color.clear, alpha);
                }
                timer += Time.deltaTime;
                yield return null;
            }
            
            if (renderer)
            {
                renderer.color = originalColor;
            }
            
            controller.gameObject.layer = LayerMask.NameToLayer("Car");
        }
        
        public override void OnBallHit(GameObject car, GameObject ball)
        {
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb)
            {
                Vector2 pushDirection = ballRb.velocity.normalized;
                ballRb.AddForce(pushDirection * ballPushMultiplier * 10f, ForceMode2D.Impulse);
            }
        }
    }
}
