using UnityEngine;

namespace SwyPhexLeague.Gameplay.CarAbilities
{
    [CreateAssetMenu(fileName = "GravityFlip", menuName = "SwyPhex/Abilities/Gravity Flip")]
    public class GravityFlip : AbilityData
    {
        [Header("Gravity Flip Settings")]
        public float radius = 4f;
        public float forceMultiplier = 20f;
        public float duration = 3f;
        
        public override void Activate(GameObject car)
        {
            CreateGravityField(car.transform.position);
            
            Collider2D[] colliders = Physics2D.OverlapCircleAll(car.transform.position, radius);
            
            foreach (var col in colliders)
            {
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb && rb.gameObject != car)
                {
                    Vector2 gravityForce = -Core.GravityManager.Instance.CurrentGravity * forceMultiplier;
                    rb.AddForce(gravityForce, ForceMode2D.Impulse);
                    
                    Core.BallPhysics ball = col.GetComponent<Core.BallPhysics>();
                    if (ball)
                    {
                        ball.ApplyGravityShock(gravityForce);
                    }
                }
            }
            
            Core.GravityManager.Instance?.SetLocalGravity(
                car.transform.position, 
                radius, 
                -Core.GravityManager.Instance.CurrentGravity * 2f, 
                duration
            );
        }
        
        private void CreateGravityField(Vector2 position)
        {
            GameObject field = new GameObject("GravityField");
            field.transform.position = position;
            
            CircleCollider2D collider = field.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.isTrigger = true;
            
            SpriteRenderer renderer = field.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateFieldSprite();
            renderer.color = new Color(1f, 0f, 1f, 0.4f);
            renderer.sortingOrder = -1;
            
            field.AddComponent<GravityFieldController>().Initialize(this);
        }
        
        private Sprite CreateFieldSprite()
        {
            Texture2D texture = new Texture2D(256, 256);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Vector2 center = new Vector2(texture.width / 2, texture.height / 2);
                    float distance = Vector2.Distance(new Vector2(x, y), center) / (texture.width / 2);
                    
                    float wave = Mathf.Sin(distance * Mathf.PI * 4f) * 0.5f + 0.5f;
                    Color color = Color.Lerp(Color.magenta, Color.clear, distance) * wave;
                    
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        
        public class GravityFieldController : MonoBehaviour
        {
            private GravityFlip ability;
            private float timer = 0f;
            
            public void Initialize(GravityFlip ability)
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
                    float pulse = Mathf.PingPong(Time.time * 2f, 0.2f);
                    Color color = renderer.color;
                    color.a = 0.4f + pulse;
                    renderer.color = color;
                }
            }
        }
    }
}
