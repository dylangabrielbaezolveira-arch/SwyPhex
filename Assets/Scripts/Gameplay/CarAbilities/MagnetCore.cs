using UnityEngine;

namespace SwyPhexLeague.Gameplay.CarAbilities
{
    [CreateAssetMenu(fileName = "MagnetCore", menuName = "SwyPhex/Abilities/Magnet Core")]
    public class MagnetCore : AbilityData
    {
        [Header("Magnet Core Settings")]
        public float magnetStrength = 50f;
        public float duration = 1.5f;
        public float speedPenalty = 0.2f;
        public float attractionRadius = 3f;
        
        public override void Activate(GameObject car)
        {
            CarController controller = car.GetComponent<CarController>();
            if (!controller) return;
            
            BallPhysics ball = FindObjectOfType<BallPhysics>();
            if (ball)
            {
                ball.SetMagnetized(car.transform, duration);
            }
            
            controller.Rigidbody.velocity *= (1f - speedPenalty);
            
            GameObject fieldEffect = CreateMagneticField(car.transform.position);
            if (fieldEffect)
            {
                Destroy(fieldEffect, duration);
            }
        }
        
        private GameObject CreateMagneticField(Vector2 position)
        {
            GameObject field = new GameObject("MagneticField");
            field.transform.position = position;
            
            CircleCollider2D collider = field.AddComponent<CircleCollider2D>();
            collider.radius = attractionRadius;
            collider.isTrigger = true;
            
            SpriteRenderer renderer = field.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateFieldSprite();
            renderer.color = new Color(0, 0.5f, 1f, 0.3f);
            renderer.sortingOrder = -1;
            
            field.AddComponent<MagneticFieldController>().Initialize(this);
            
            return field;
        }
        
        private Sprite CreateFieldSprite()
        {
            Texture2D texture = new Texture2D(256, 256);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float distance = Vector2.Distance(
                        new Vector2(x, y), 
                        new Vector2(texture.width / 2, texture.height / 2)
                    ) / (texture.width / 2);
                    
                    Color color = Color.Lerp(Color.blue, Color.clear, distance);
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        
        public class MagneticFieldController : MonoBehaviour
        {
            private MagnetCore ability;
            private float timer = 0f;
            
            public void Initialize(MagnetCore ability)
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
                    Color color = renderer.color;
                    color.a = Mathf.Lerp(0.3f, 0f, 1f - (timer / ability.duration));
                    renderer.color = color;
                }
            }
            
            private void OnTriggerStay2D(Collider2D other)
            {
                BallPhysics ball = other.GetComponent<BallPhysics>();
                if (ball && ball.IsMagnetized)
                {
                    Vector2 direction = (transform.position - ball.transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, ball.transform.position);
                    float strength = ability.magnetStrength / (distance * distance);
                    
                    ball.GetComponent<Rigidbody2D>().AddForce(direction * strength);
                }
            }
        }
    }
}
