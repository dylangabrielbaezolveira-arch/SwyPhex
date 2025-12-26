using UnityEngine;

namespace SwyPhexLeague.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CarController : MonoBehaviour
    {
        [Header("Car Stats")]
        [SerializeField] private float maxSpeed = 15f;
        [SerializeField] private float acceleration = 35f;
        [SerializeField] private float brakeForce = 45f;
        [SerializeField] private float turnSpeed = 3.5f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float airControl = 0.8f;
        [SerializeField] private float rotationSpeed = 180f;
        
        [Header("Components")]
        private Rigidbody2D rb;
        private BoostSystem boostSystem;
        private AbilitySystem abilitySystem;
        
        [Header("State")]
        private bool isGrounded;
        private bool isJumping;
        private float currentSpeed;
        private float horizontalInput;
        private bool jumpInput;
        private bool jumpHeld;
        private Vector2 groundNormal;
        
        [Header("Visual")]
        [SerializeField] private Transform carVisual;
        [SerializeField] private ParticleSystem boostParticles;
        [SerializeField] private TrailRenderer[] trails;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boostSystem = GetComponent<BoostSystem>();
            abilitySystem = GetComponent<AbilitySystem>();
            
            // Configurar física
            rb.mass = 1.2f;
            rb.drag = 0.4f;
            rb.angularDrag = 2f;
            rb.gravityScale = 1f;
        }
        
        private void Update()
        {
            GetInput();
            HandleJump();
            UpdateVisuals();
        }
        
        private void FixedUpdate()
        {
            CheckGrounded();
            HandleMovement();
            HandleRotation();
        }
        
        private void GetInput()
        {
            // Input móvil (configurable)
            horizontalInput = InputManager.Instance.HorizontalAxis;
            jumpInput = InputManager.Instance.JumpPressed;
            jumpHeld = InputManager.Instance.JumpHeld;
            
            // Boost activado
            if (InputManager.Instance.BoostHeld && boostSystem.HasBoost())
            {
                boostSystem.UseBoost();
                if (boostParticles && !boostParticles.isPlaying)
                    boostParticles.Play();
            }
            else if (boostParticles && boostParticles.isPlaying)
            {
                boostParticles.Stop();
            }
            
            // Habilidad
            if (InputManager.Instance.AbilityPressed)
            {
                abilitySystem?.ActivateAbility();
            }
        }
        
        private void CheckGrounded()
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                Vector2.down, 
                0.6f, 
                LayerMask.GetMask("Ground")
            );
            
            isGrounded = hit.collider != null;
            
            if (isGrounded)
            {
                groundNormal = hit.normal;
                isJumping = false;
                
                // Recuperar boost al tocar suelo
                if (rb.velocity.y <= 0.1f)
                    boostSystem?.RegenerateBoost(Time.fixedDeltaTime * 2f);
            }
        }
        
        private void HandleMovement()
        {
            float currentAcceleration = acceleration;
            float currentMaxSpeed = maxSpeed;
            
            // Modificadores
            if (!isGrounded)
            {
                currentAcceleration *= airControl;
                currentMaxSpeed *= 0.8f;
            }
            
            if (boostSystem.IsBoosting)
            {
                currentAcceleration *= 1.5f;
                currentMaxSpeed *= 1.3f;
            }
            
            // Fuerza de movimiento
            Vector2 moveForce = Vector2.right * horizontalInput * currentAcceleration;
            
            // Aplicar en dirección del suelo si está en pendiente
            if (isGrounded && Mathf.Abs(groundNormal.x) > 0.1f)
            {
                moveForce = Vector2.Perpendicular(groundNormal) * horizontalInput * currentAcceleration;
            }
            
            rb.AddForce(moveForce);
            
            // Limitar velocidad horizontal
            Vector2 velocity = rb.velocity;
            if (Mathf.Abs(velocity.x) > currentMaxSpeed)
            {
                velocity.x = Mathf.Sign(velocity.x) * currentMaxSpeed;
                rb.velocity = velocity;
            }
            
            // Frenado
            if (Mathf.Abs(horizontalInput) < 0.1f && isGrounded)
            {
                Vector2 brakeForceVector = -velocity * brakeForce * Time.fixedDeltaTime;
                brakeForceVector.y = 0;
                rb.AddForce(brakeForceVector);
            }
        }
        
        private void HandleRotation()
        {
            // Rotación en aire
            if (!isGrounded)
            {
                float rotationInput = 0f;
                
                if (horizontalInput != 0)
                    rotationInput = -Mathf.Sign(horizontalInput);
                
                if (jumpHeld)
                    rotationInput = 1f;
                
                rb.angularVelocity = rotationInput * rotationSpeed;
            }
            else
            {
                // Alinear con el suelo
                float targetAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
                
                rb.angularVelocity = -angleDiff * turnSpeed;
            }
        }
        
        private void HandleJump()
        {
            if (jumpInput && isGrounded)
            {
                float jumpPower = jumpForce;
                
                // Impulso extra si se mantiene
                if (jumpHeld)
                    jumpPower *= 1.2f;
                
                rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                isJumping = true;
                
                // Consumo de boost
                boostSystem?.UseBoost(5f);
                
                // Efectos
                AudioManager.Instance.PlaySFX("Jump");
                SpawnJumpParticles();
            }
        }
        
        private void UpdateVisuals()
        {
            // Trail effects
            bool shouldEmit = isGrounded && Mathf.Abs(rb.velocity.x) > 2f;
            foreach (var trail in trails)
            {
                trail.emitting = shouldEmit;
                if (boostSystem.IsBoosting)
                    trail.startColor = Color.cyan;
                else
                    trail.startColor = Color.white;
            }
        }
        
        private void SpawnJumpParticles()
        {
            // Pool de partículas
            GameObject particles = ObjectPool.Instance.GetPooledObject("JumpParticles");
            if (particles)
            {
                particles.transform.position = transform.position;
                particles.SetActive(true);
            }
        }
        
        public void ApplyDash(Vector2 direction, float force)
        {
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            AudioManager.Instance.PlaySFX("Dash");
        }
        
        public void ResetVelocity()
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Propiedades públicas
        public bool IsGrounded => isGrounded;
        public bool IsBoosting => boostSystem?.IsBoosting ?? false;
        public float CurrentSpeed => Mathf.Abs(rb.velocity.x);
        public Rigidbody2D Rigidbody => rb;
    }
}
