using UnityEngine;

namespace SwyPhexLeague.Utilities
{
    public static class GameExtensions
    {
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }
        
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }
        
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }
        
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }
        
        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }
        
        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }
        
        public static Color WithAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
        
        public static void ResetTransformation(this Transform t)
        {
            t.position = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
        
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }
        
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
        {
            return (layerMask.value & (1 << gameObject.layer)) != 0;
        }
        
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static float ClampAngle(this float angle, float min, float max)
        {
            if (angle < -360f) angle += 360f;
            if (angle > 360f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
        
        public static Vector2 PerpendicularClockwise(this Vector2 vector2)
        {
            return new Vector2(vector2.y, -vector2.x);
        }
        
        public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
        {
            return new Vector2(-vector2.y, vector2.x);
        }
        
        public static bool Approximately(this Vector2 a, Vector2 b, float tolerance = 0.01f)
        {
            return Vector2.Distance(a, b) <= tolerance;
        }
        
        public static bool Approximately(this Vector3 a, Vector3 b, float tolerance = 0.01f)
        {
            return Vector3.Distance(a, b) <= tolerance;
        }
        
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        
        public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        {
            return new Vector3(v.x, v.y, z);
        }
        
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
        
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
        
        public static float SmoothStep(this float t)
        {
            return t * t * (3f - 2f * t);
        }
        
        public static float SmootherStep(this float t)
        {
            return t * t * t * (t * (6f * t - 15f) + 10f);
        }
        
        public static Vector2 RandomPointInCircle(float radius)
        {
            return UnityEngine.Random.insideUnitCircle * radius;
        }
        
        public static Vector3 RandomPointInSphere(float radius)
        {
            return UnityEngine.Random.insideUnitSphere * radius;
        }
        
        public static Vector2 DirectionTo(this Transform from, Transform to)
        {
            return (to.position - from.position).ToVector2().normalized;
        }
        
        public static float DistanceTo(this Transform from, Transform to)
        {
            return Vector2.Distance(from.position, to.position);
        }
        
        public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
    }
}
