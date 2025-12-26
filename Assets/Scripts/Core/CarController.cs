using UnityEngine;

namespace SwyPhexLeague.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CarController : MonoBehaviour
    {
        [Header("Car Stats")]
        public float maxSpeed = 15f;
        public float acceleration = 35f;
        public float brakeForce = 45f;
        public float turnSpeed = 3.5f;
        public float jumpForce = 12f;
        public float airControl = 0.8f;
        public float rotationSpeed = 180f;
        public float dashForce = 25f;
        
        [Header("Components")]
        private Rigidbody2D rb;
        private BoostSystem boostSystem;
        private Gameplay.AbilitySystem abilitySystem;
        private SpriteRenderer spriteRenderer;
        
        [Header("State")]
        private bool isGrounded;
        private bool isJumping;
        private float lastJumpTime;
        private float jumpGracePeriod = 0.15f;
        private Vector2 groundNormal;
        private bool canDash = true;
        private float dashCooldown = 1.5f;
        
        [Header("Visual")]
        public Transform carVisual;
        public ParticleSystem boostParticles;
        public ParticleSystem jumpParticles;
        public TrailRenderer[] trails;
        
        [Header("Input")]
        private float horizontalInput;
        private bool jumpInput;
        private bool jumpHeld;
        private bool boostInput;
        private bool abilityInput;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boostSystem = GetComponent<BoostSystem>();
            abilitySystem = GetComponent<Gameplay.AbilitySystem>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            SetupPhysics();
        }
        
        private void SetupPhysics()
        {
            rb.mass = 1.2f;
            rb.drag = 0.4f;
            rb.angularDrag = 2f;
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        private void Update()
        {
            GetInput();
            HandleJumpInput();
            HandleAbility();
            UpdateVisuals();
        }
        
        private void FixedUpdate()
        {
            CheckGrounded();
            HandleMovement();
            HandleRotation();
            HandleBoost();
        }
        
        private void GetInput()
        {
            horizontalInput = InputManager.Instance.GetHorizontalAxis();
            jumpInput = InputManager.Instance.GetJumpDown();
            jumpHeld = InputManager.Instance.GetJump();
            boostInput = InputManager.Instance.GetBoost();
            abilityInput = InputManager.Instance.GetAbilityDown();
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
                
                if (Time.time - lastJumpTime > 0.2f)
                    isJumping = false;
                    
                boostSystem?.RegenerateBoost(Time.fixedDeltaTime * 2f);
            }
            
            Debug.DrawRay(transform.position, Vector2.down * 0.6f, 
                         isGrounded ? Color.green : Color.red);
        }
        
        private void HandleMovement()
        {
            float currentAcceleration = acceleration;
            float currentMaxSpeed = maxSpeed;
            
            if (!isGrounded)
            {
                currentAcceleration *= airControl;
                currentMaxSpeed *= 0.8f;
            }
            
            if (boostSystem?.IsBoosting == true)
            {
                currentAcceleration *= 1.5f;
                currentMaxSpeed *= 1.3f;
            }
            
            Vector2 moveForce = Vector2.right * horizontalInput * currentAcceleration;
            
            if (isGrounded && Mathf.Abs(groundNormal.x) > 0.1f)
            {
                moveForce = Vector2.Perpendicular(groundNormal) * horizontalInput * currentAcceleration;
            }
            
            rb.AddForce(moveForce);
            
            Vector2 velocity = rb.velocity;
            if (Mathf.Abs(velocity.x) > currentMaxSpeed)
            {
                velocity.x = Mathf.Sign(velocity.x) * currentMaxSpeed;
                rb.velocity = velocity;
            }
            
            if (Mathf.Abs(horizontalInput) < 0.1f && isGrounded)
            {
                Vector2 brakeForceVector = -velocity * brakeForce * Time.fixedDeltaTime;
                brakeForceVector.y = 0;
                rb.AddForce(brakeForceVector);
            }
        }
        
        private void HandleRotation()
        {
            if (!isGrounded)
            {
                float rotationInput = 0f;
                
                if (horizontalInput != 0)
                    rotationInput = -Mathf.Sign(horizontalInput);
                    
                if (jumpHeld)
                    rotationInput = 1f;
                    
                transform.Rotate(0, 0, rotationInput * rotationSpeed * Time.fixedDeltaTime);
            }
            else
            {
                float targetAngle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
                
                transform.Rotate(0, 0, -angleDiff * turnSpeed * Time.fixedDeltaTime);
            }
        }
        
        private void HandleJumpInput()
        {
            if (jumpInput && (isGrounded || Time.time - lastJumpTime < jumpGracePeriod))
            {
                PerformJump();
            }
            
            if (InputManager.Instance.GetDoubleTapJump() && canDash)
            {
                PerformDash();
            }
        }
        
        private void PerformJump()
        {
            float jumpPower = jumpForce;
            
            if (jumpHeld)
                jumpPower *= 1.2f;
                
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            isJumping = true;
            lastJumpTime = Time.time;
            
            boostSystem?.UseBoost(5f);
            
            Managers.AudioManager.Instance?.PlaySFX("Jump");
            
            if (jumpParticles)
            {
                jumpParticles.Play();
            }
        }
        
        private void PerformDash()
        {
            if (!boostSystem?.UseDash() ?? true) return;
            
            Vector2 dashDirection = rb.velocity.normalized;
            if (dashDirection.magnitude < 0.1f)
            {
                dashDirection = transform.right;
            }
            
            rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
            canDash = false;
            Invoke(nameof(ResetDash), dashCooldown);
            
            Managers.AudioManager.Instance?.PlaySFX("Dash");
        }
        
        private void ResetDash()
        {
            canDash = true;
        }
        
        private void HandleBoost()
        {
            if (boostInput && boostSystem?.HasBoost() == true)
            {
                boostSystem.UseBoost();
                if (boostParticles && !boostParticles.isPlaying)
                    boostParticles.Play();
            }
            else if (boostParticles && boostParticles.isPlaying)
            {
                boostParticles.Stop();
            }
        }
        
        private void HandleAbility()
        {
            if (abilityInput)
            {
                abilitySystem?.ActivateAbility();
            }
        }
        
        private void UpdateVisuals()
        {
            bool shouldEmit = isGrounded && Mathf.Abs(rb.velocity.x) > 2f;
            foreach (var trail in trails)
            {
                if (trail)
                {
                    trail.emitting = shouldEmit;
                    trail.startColor = (boostSystem?.IsBoosting == true) ? 
                        Color.cyan : Color.white;
                }
            }
            
            if (spriteRenderer && boostSystem?.IsBoosting == true)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.cyan, 
                    Mathf.PingPong(Time.time * 2f, 0.3f));
            }
            else if (spriteRenderer)
            {
                spriteRenderer.color = Color.white;
            }
        }
        
        public void ApplyExternalForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            rb.AddForce(force, mode);
        }
        
        public void ResetVelocity()
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        public void TeleportTo(Vector2 position)
        {
            rb.position = position;
            ResetVelocity();
        }
        
        public bool IsGrounded => isGrounded;
        public bool IsJumping => isJumping;
        public bool IsBoosting => boostSystem?.IsBoosting ?? false;
        public float CurrentSpeed => Mathf.Abs(rb.velocity.x);
        public Rigidbody2D Rigidbody => rb;
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ball"))
            {
                Managers.AudioManager.Instance?.PlaySFX("CarHit", 
                    Mathf.Clamp01(collision.relativeVelocity.magnitude / 15f));
            }
        }
    }
}
