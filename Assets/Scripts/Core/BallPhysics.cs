using UnityEngine;

namespace SwyPhexLeague.Core
{
    public class BallPhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private float baseBounce = 0.8f;
        [SerializeField] private float maxSpeed = 30f;
        [SerializeField] private float curveFactor = 0.3f;
        [SerializeField] private float gravityInfluence = 0.5f;
        
        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private GameObject glowEffect;
        
        private Rigidbody2D rb;
        private Collider2D col;
        private Vector2 lastVelocity;
        private Vector2 gravityDirection = Vector2.down;
        
        // Estado especial
        private bool isMagnetized = false;
        private Transform magnetTarget = null;
        private float magnetStrength = 0f;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            
            // Configurar física
            rb.mass = 0.5f;
            rb.drag = 0.1f;
            rb.angularDrag = 0.2f;
            rb.sharedMaterial = new PhysicsMaterial2D()
            {
                bounciness = baseBounce,
                friction = 0.1f
            };
        }
        
        private void FixedUpdate()
        {
            // Limitar velocidad máxima
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
            
            // Aplicar influencia de gravedad
            ApplyGravityInfluence();
            
            // Efecto de magnetismo
            if (isMagnetized && magnetTarget)
            {
                ApplyMagnetism();
            }
            
            // Efecto visual de velocidad
            UpdateTrail();
            
            lastVelocity = rb.velocity;
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Calcular fuerza del golpe
            float hitStrength = collision.relativeVelocity.magnitude;
            
            // Efecto de sonido
            AudioManager.Instance.PlaySFX("BallHit", Mathf.Clamp01(hitStrength / 20f));
            
            // Partículas
            if (hitParticles && hitStrength > 3f)
            {
                ContactPoint2D contact = collision.GetContact(0);
                hitParticles.transform.position = contact.point;
                hitParticles.Play();
            }
            
            // Bonus de boost para el jugador que golpea
            CarController car = collision.gameObject.GetComponent<CarController>();
            if (car && hitStrength > 5f)
            {
                car.GetComponent<BoostSystem>()?.RegenerateBoost(10f);
                
                // Efecto visual de golpe fuerte
                if (hitStrength > 15f)
                {
                    CameraShake.Instance.Shake(0.1f, 0.3f);
                    SpawnHitEffect(collision.GetContact(0).point);
                }
            }
            
            // Efecto de curva (Magnus effect simplificado)
            if (rb.angularVelocity != 0 && hitStrength > 8f)
            {
                ApplyCurveEffect();
            }
        }
        
        private void ApplyGravityInfluence()
        {
            Vector2 currentGravity = GravityManager.Instance.CurrentGravity;
            
            // Solo aplicar si no es gravedad normal
            if (currentGravity != Vector2.down)
            {
                rb.AddForce(currentGravity * gravityInfluence * rb.mass);
            }
        }
        
        private void ApplyCurveEffect()
        {
            // Efecto Magnus simplificado
            Vector2 curveForce = new Vector2(
                -rb.angularVelocity * curveFactor * rb.velocity.y,
                rb.angularVelocity * curveFactor * rb.velocity.x
            ) * 0.01f;
            
            rb.AddForce(curveForce);
        }
        
        private void ApplyMagnetism()
        {
            Vector2 direction = (magnetTarget.position - transform.position);
            float distance = direction.magnitude;
            
            if (distance > 0.1f)
            {
                float strength = magnetStrength / (distance * distance);
                rb.AddForce(direction.normalized * strength);
            }
        }
        
        private void UpdateTrail()
        {
            if (trail)
            {
                float speedRatio = rb.velocity.magnitude / maxSpeed;
                trail.time = Mathf.Lerp(0.1f, 0.5f, speedRatio);
                trail.startWidth = Mathf.Lerp(0.1f, 0.3f, speedRatio);
                
                // Cambiar color según velocidad
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(
                            Color.Lerp(Color.yellow, Color.red, speedRatio), 
                            1f
                        )
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0f),
                        new GradientAlphaKey(0f, 1f)
                    }
                );
                trail.colorGradient = gradient;
            }
        }
        
        private void SpawnHitEffect(Vector2 position)
        {
            GameObject effect = ObjectPool.Instance.GetPooledObject("BallImpact");
            if (effect)
            {
                effect.transform.position = position;
                effect.SetActive(true);
            }
        }
        
        // API pública para efectos especiales
        public void SetMagnetized(Transform target, float strength, float duration)
        {
            isMagnetized = true;
            magnetTarget = target;
            magnetStrength = strength;
            
            CancelInvoke("RemoveMagnetism");
            Invoke("RemoveMagnetism", duration);
            
            // Efecto visual
            if (glowEffect)
            {
                glowEffect.SetActive(true);
                glowEffect.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        
        private void RemoveMagnetism()
        {
            isMagnetized = false;
            magnetTarget = null;
            magnetStrength = 0f;
            
            if (glowEffect)
            {
                glowEffect.SetActive(false);
            }
        }
        
        public void ApplyGravityShock(Vector2 gravityDirection, float duration)
        {
            Vector2 force = gravityDirection * 15f;
            rb.AddForce(force, ForceMode2D.Impulse);
            
            // Efecto visual
            StartCoroutine(GravityShockEffect(duration));
        }
        
        private System.Collections.IEnumerator GravityShockEffect(float duration)
        {
            if (glowEffect)
            {
                glowEffect.SetActive(true);
                glowEffect.GetComponent<SpriteRenderer>().color = Color.magenta;
                
                yield return new WaitForSeconds(duration);
                
                if (!isMagnetized)
                {
                    glowEffect.SetActive(false);
                }
            }
        }
        
        public void ResetBall(Vector2 position)
        {
            transform.position = position;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            isMagnetized = false;
            magnetTarget = null;
            
            if (glowEffect)
            {
                glowEffect.SetActive(false);
            }
        }
        
        // Propiedades públicas
        public Vector2 Velocity => rb.velocity;
        public bool IsMagnetized => isMagnetized;
        public float Speed => rb.velocity.magnitude;
    }
}
