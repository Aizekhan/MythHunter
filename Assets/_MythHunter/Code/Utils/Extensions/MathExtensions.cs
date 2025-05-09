// Шлях: Assets/_MythHunter/Code/Utils/Extensions/MathExtensions.cs
using Unity.Mathematics;
using UnityEngine;

namespace MythHunter.Utils.Extensions
{
    /// <summary>
    /// Розширення для математичних типів Unity.Mathematics
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Конвертує float3 в Vector3
        /// </summary>
        public static Vector3 ToVector3(this float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }

        /// <summary>
        /// Конвертує Vector3 в float3
        /// </summary>
        public static float3 ToFloat3(this Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }

        /// <summary>
        /// Отримує дистанцію між двома позиціями
        /// </summary>
        public static float Distance(this float3 a, float3 b)
        {
            return math.distance(a, b);
        }

        /// <summary>
        /// Лінійна інтерполяція між двома значеннями
        /// </summary>
        public static float3 Lerp(this float3 a, float3 b, float t)
        {
            return math.lerp(a, b, t);
        }

        /// <summary>
        /// Отримує напрямок від однієї точки до іншої
        /// </summary>
        public static float3 DirectionTo(this float3 from, float3 to)
        {
            return math.normalize(to - from);
        }
    }
}
