using UnityEngine;

namespace SwyPhexLeague.Gameplay.CarAbilities
{
    [CreateAssetMenu(fileName = "ShockDrop", menuName = "SwyPhex/Abilities/Shock Drop")]
    public class ShockDrop : AbilityData
    {
        [Header("Shock Drop Settings")]
        public float radius = 3f;
        public float duration = 2.5f;
        public float slowAmount = 0.4f;
        public float damage = 10f;
        
        public override void Activate(GameObject car)
        {
            Vector2 dropPosition = GetDropPosition(car);
            CreateShockZone(dropPosition);
            ApplyShockEffect(dropPosition);
        }
        
        private Vector2 GetDropPosition(GameObject car)
        {
            CarController controller = car.GetComponent<CarController>();
            if (!controller) return car.transform.position;
            
            if (!controller.IsGrounded)
            {
                controller.Rigidbody.velocity = new Vector2(
                    controller.Rigidbody.velocity.x,
                    -20f
                );
                return (Vector2)car.transform.position + Vector2.down * 2f;
            }
            
            return car.transform.position;
        }
        
        private void CreateShockZone(Vector2 position)
        {
            GameObject zone = new GameObject("ShockZone");
            zone.transform.position = position;
            
            CircleCollider2D collider = zone.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.isTrigger = true;
            
            SpriteRenderer renderer = zone.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateZoneSprite();
            renderer.color = new Color(1f, 0.5f, 0f, 0.3f);
            renderer.sortingOrder = -1;
            
            zone.AddComponent<ShockZoneController>().Initialize(this);
        }
        
        private Sprite CreateZoneSprite()
        {
            Texture2D texture = new Texture2D(256, 256);
            Color zoneColor = new Color(1f, 0.5f, 0f, 1f);
            
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Vector2 center = new Vector2(texture.width / 2, texture.height / 2);
                    float distance = Vector2.Distance(new Vector2(x, y), center) / (texture.width / 2);
                    
                    if (distance <= 1f)
                    {
                        float pattern = Mathf.Sin(x * 0.1f) * Mathf.Sin(y * 0.1f);
                        Color color = zoneColor * (1f - distance) * (0.7f + pattern * 0.3f);
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        
        private void ApplyShockEffect(Vector2 position)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
            
            foreach (var col in colliders)
            {
                CarController enemyCar = col.GetComponent<CarController>();
                if (enemyCar)
                {
                    enemyCar.Rigidbody.velocity *= (1f - slowAmount);
                    
                    AbilitySystem enemyAbility = enemyCar.GetComponent<AbilitySystem>();
                    if (enemyAbility)
                    {
                        enemyAbility.InterruptAbility();
                    }
                    
                    CreateShockEffect(enemyCar.transform.position);
                }
            }
        }
        
        private void CreateShockEffect(Vector2 position)
        {
            GameObject effect = new GameObject("ShockEffect");
            effect.transform.position = position;
            
            ParticleSystem particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startSpeed = 2f;
            main.startLifetime = 0.5f;
            main.startSize = 0.2f;
            main.startColor = new Color(1f, 0.8f, 0f, 0.8f);
            
            var emission = particles.emission;
            emission.rateOverTime = 20f;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            Destroy(effect, 1f);
        }
        
        public class ShockZoneController : MonoBehaviour
        {
            private ShockDrop ability;
            private float timer = 0f;
            
            public void Initialize(ShockDrop ability)
            {
                this.ability = ability;
                timer = ability.duration;
            }
            
            private void Update()
            {
                timer -= Time.deltaTime;
                
                if (timer <= 0f)
                {
                    Destroy(gameObject);
                }
                
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                if (renderer)
                {
                    float pulse = Mathf.PingPong(Time.time * 3f, 0.3f);
                    Color color = renderer.color;
                    color.a = 0.3f + pulse;
                    renderer.color = color;
                }
            }
            
            private void OnTriggerStay2D(Collider2D other)
            {
                CarController enemyCar = other.GetComponent<CarController>();
                if (enemyCar)
                {
                    enemyCar.Rigidbody.velocity *= (1f - ability.slowAmount * Time.deltaTime);
                }
            }
        }
    }
}
