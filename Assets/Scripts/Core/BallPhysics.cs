using UnityEngine;

namespace SwyPhexLeague.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class BallPhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        public float baseBounce = 0.8f;
        public float maxSpeed = 30f;
        public float curveFactor = 0.3f;
        public float gravityInfluence = 0.5f;
        public float magnetStrength = 50f;
        
        [Header("Visual Effects")]
        public TrailRenderer trail;
        public ParticleSystem hitParticles;
        public SpriteRenderer glowEffect;
        public Gradient speedGradient;
        
        [Header("State")]
        private Rigidbody2D rb;
        private CircleCollider2D col;
        private bool isMagnetized = false;
        private Transform magnetTarget = null;
        private float magnetDuration = 0f;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
            
            SetupPhysics();
        }
        
        private void SetupPhysics()
        {
            rb.mass = 0.5f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.2f;
            
            PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D()
            {
                bounciness = baseBounce,
                friction = 0.1f
            };
            col.sharedMaterial = bouncyMaterial;
        }
        
        private void FixedUpdate()
        {
            LimitSpeed();
            ApplyGravityInfluence();
            HandleMagnetism();
            UpdateVisuals();
        }
        
        private void LimitSpeed()
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
        
        private void ApplyGravityInfluence()
        {
            if (GravityManager.Instance)
            {
                Vector2 gravity = GravityManager.Instance.CurrentGravity;
                if (gravity != Physics2D.gravity)
                {
                    rb.AddForce(gravity * gravityInfluence * rb.mass);
                }
            }
        }
        
        private void HandleMagnetism()
        {
            if (isMagnetized && magnetTarget && magnetDuration > 0)
            {
                Vector2 direction = (Vector2)magnetTarget.position - rb.position;
                float distance = direction.magnitude;
                
                if (distance > 0.1f)
                {
                    float strength = magnetStrength / (distance * distance);
                    rb.AddForce(direction.normalized * strength);
                }
                
                magnetDuration -= Time.fixedDeltaTime;
                
                if (magnetDuration <= 0)
                {
                    RemoveMagnetism();
                }
            }
        }
        
        private void UpdateVisuals()
        {
            if (trail)
            {
                float speedRatio = rb.velocity.magnitude / maxSpeed;
                trail.time = Mathf.Lerp(0.1f, 0.5f, speedRatio);
                trail.startWidth = Mathf.Lerp(0.1f, 0.3f, speedRatio);
                
                if (speedGradient != null)
                {
                    trail.startColor = speedGradient.Evaluate(speedRatio);
                }
            }
            
            if (glowEffect)
            {
                float alpha = isMagnetized ? 
                    Mathf.PingPong(Time.time * 2f, 0.5f) + 0.5f : 
                    0.3f;
                    
                Color color = glowEffect.color;
                color.a = alpha;
                glowEffect.color = color;
                
                if (isMagnetized)
                {
                    glowEffect.color = Color.blue;
                }
                else
                {
                    glowEffect.color = Color.white;
                }
            }
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            float hitStrength = collision.relativeVelocity.magnitude;
            
            if (hitStrength > 3f)
            {
                Managers.AudioManager.Instance?.PlaySFX("BallHit", 
                    Mathf.Clamp01(hitStrength / 20f));
                    
                if (hitParticles)
                {
                    ContactPoint2D contact = collision.GetContact(0);
                    hitParticles.transform.position = contact.point;
                    hitParticles.Play();
                }
                
                CarController car = collision.gameObject.GetComponent<CarController>();
                if (car && hitStrength > 5f)
                {
                    BoostSystem boost = car.GetComponent<BoostSystem>();
                    boost?.RegenerateBoost(10f);
                    
                    if (hitStrength > 15f)
                    {
                        CameraShake.Instance?.Shake(0.1f, 0.3f);
                        SpawnImpactEffect(collision.GetContact(0).point);
                    }
                }
                
                ApplyCurveEffect();
            }
        }
        
        private void ApplyCurveEffect()
        {
            if (Mathf.Abs(rb.angularVelocity) > 100f)
            {
                Vector2 curveForce = new Vector2(
                    -rb.angularVelocity * curveFactor * rb.velocity.y * 0.01f,
                    rb.angularVelocity * curveFactor * rb.velocity.x * 0.01f
                );
                
                rb.AddForce(curveForce);
            }
        }
        
        private void SpawnImpactEffect(Vector2 position)
        {
            GameObject effect = Utilities.ObjectPool.Instance?.GetPooledObject("BallImpact");
            if (effect)
            {
                effect.transform.position = position;
                effect.SetActive(true);
            }
        }
        
        public void SetMagnetized(Transform target, float duration)
        {
            isMagnetized = true;
            magnetTarget = target;
            magnetDuration = duration;
            
            if (glowEffect)
            {
                glowEffect.color = Color.blue;
            }
        }
        
        private void RemoveMagnetism()
        {
            isMagnetized = false;
            magnetTarget = null;
            magnetDuration = 0f;
            
            if (glowEffect)
            {
                glowEffect.color = Color.white;
            }
        }
        
        public void ApplyGravityShock(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
            
            if (glowEffect)
            {
                StartCoroutine(GravityShockEffect());
            }
        }
        
        private System.Collections.IEnumerator GravityShockEffect()
        {
            if (!glowEffect) yield break;
            
            Color originalColor = glowEffect.color;
            glowEffect.color = Color.magenta;
            
            yield return new WaitForSeconds(1f);
            
            if (!isMagnetized)
            {
                glowEffect.color = originalColor;
            }
        }
        
        public void ResetBall(Vector2 position)
        {
            transform.position = position;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            RemoveMagnetism();
        }
        
        public Vector2 Velocity => rb.velocity;
        public bool IsMagnetized => isMagnetized;
        public float Speed => rb.velocity.magnitude;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("BoostOrb"))
            {
                other.gameObject.SetActive(false);
                Utilities.ObjectPool.Instance?.ReturnToPool("BoostOrb", other.gameObject);
                
                GameObject effect = Utilities.ObjectPool.Instance?.GetPooledObject("BoostPickup");
                if (effect)
                {
                    effect.transform.position = transform.position;
                    effect.SetActive(true);
                }
            }
        }
    }
}
