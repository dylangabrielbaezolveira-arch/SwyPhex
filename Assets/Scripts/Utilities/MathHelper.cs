using UnityEngine;

namespace SwyPhexLeague.Utilities
{
    public static class MathHelper
    {
        public const float DEG2RAD = Mathf.PI / 180f;
        public const float RAD2DEG = 180f / Mathf.PI;
        public const float TWO_PI = Mathf.PI * 2f;
        
        public static float LinearToDecibel(float linear)
        {
            if (linear <= 0f) return -144f;
            return 20f * Mathf.Log10(linear);
        }
        
        public static float DecibelToLinear(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }
        
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, 
            float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
        {
            if (deltaTime < 0f) deltaTime = Time.deltaTime;
            
            target = current + Mathf.DeltaAngle(current, target);
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }
        
        public static Vector2 ParabolicCurve(float t, Vector2 start, Vector2 end, float height)
        {
            float parabolicT = t * 2f - 1f;
            Vector2 travelDirection = end - start;
            Vector2 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1f) * height;
            return result;
        }
        
        public static Vector3 ParabolicCurve(float t, Vector3 start, Vector3 end, float height)
        {
            float parabolicT = t * 2f - 1f;
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1f) * height;
            return result;
        }
        
        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
        
        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return c3 * t * t * t - c1 * t * t;
        }
        
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        
        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            
            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }
        
        public static float BounceOut(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            
            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
        
        public static float BounceIn(float t)
        {
            return 1f - BounceOut(1f - t);
        }
        
        public static float BounceInOut(float t)
        {
            return t < 0.5f
                ? (1f - BounceOut(1f - 2f * t)) / 2f
                : (1f + BounceOut(2f * t - 1f)) / 2f;
        }
        
        public static float ElasticIn(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            
            return t == 0f ? 0f : t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
        }
        
        public static float ElasticOut(float t)
        {
            const float c4 = (2f * Mathf.PI) / 3f;
            
            return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
        
        public static float ElasticInOut(float t)
        {
            const float c5 = (2f * Mathf.PI) / 4.5f;
            
            return t == 0f ? 0f : t == 1f ? 1f : t < 0.5f
                ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
                : (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
        }
        
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }
        
        public static float AngleDifference(float a, float b)
        {
            float diff = (b - a + 180f) % 360f - 180f;
            return diff < -180f ? diff + 360f : diff;
        }
        
        public static float SmoothApproach(float pastPosition, float pastTargetPosition, 
            float targetPosition, float speed, float deltaTime)
        {
            float t = deltaTime * speed;
            float v = (targetPosition - pastTargetPosition) / t;
            float f = pastPosition - pastTargetPosition + v;
            return targetPosition - v + f * Mathf.Exp(-t);
        }
        
        public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s1 = c.y - a.y;
            float s2 = c.x - a.x;
            float s3 = b.y - a.y;
            float s4 = p.y - a.y;
            
            float w1 = (a.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (b.x - a.x) * s1);
            float w2 = (s4 - w1 * s3) / s1;
            
            return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
        }
        
        public static Vector2 ClosestPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            line.Normalize();
            
            Vector2 v = point - lineStart;
            float d = Vector2.Dot(v, line);
            d = Mathf.Clamp(d, 0f, lineLength);
            
            return lineStart + line * d;
        }
        
        public static float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return Vector2.Distance(point, ClosestPointOnLine(point, lineStart, lineEnd));
        }
        
        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
        
        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }
        
        public static float Gaussian(float x, float mean, float stdDev)
        {
            float variance = stdDev * stdDev;
            float coefficient = 1f / Mathf.Sqrt(2f * Mathf.PI * variance);
            float exponent = -((x - mean) * (x - mean)) / (2f * variance);
            return coefficient * Mathf.Exp(exponent);
        }
        
        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        
        public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }
        
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t
            );
        }
        
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (Mathf.Approximately(a, b)) return 0f;
            return (value - a) / (b - a);
        }
    }
}
