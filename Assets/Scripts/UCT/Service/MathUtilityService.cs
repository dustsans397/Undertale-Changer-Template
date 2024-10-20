﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    /// 数学相关函数
    /// </summary>
    public static class MathUtilityService
    {
        /// <summary>
        /// 随机获取-1或1
        /// </summary>
        public static int Get1Or_1()
        {
            int result;
            do
            {
                result = Random.Range(-1, 2);
            }
            while (result == 0);

            return result;
        }

        /// <summary>
        /// 传入数根据正负返回1/-1。
        /// 传0返1。
        /// </summary>
        public static int Get1Or_1(float input)
        {
            var result = input;
            
            if (result >= 0)
                result = 1;
            else
                result = -1;
            
            return (int)result;
        }

        /// <summary>
        /// 计算多边形中点
        /// </summary>
        public static Vector2 CalculatePolygonCenter(List<Vector2> vertexPoints)
        {
            var result = Vector2.zero;

            if (vertexPoints == null || vertexPoints.Count == 0)
            {
                return result;
            }

            result = vertexPoints.Aggregate(result, (current, vertex) => current + vertex);

            result /= vertexPoints.Count;

            return result;
        }

        /// <summary>
        /// 在球体表面上生成随机点
        /// </summary>
        public static Vector3 RandomPointOnSphereSurface(float sphereRadius, Vector3 sphereCenter)
        {
            var randomDirection = Random.onUnitSphere;

            randomDirection *= sphereRadius;

            var result = sphereCenter + randomDirection;

            return result;
        }
    }
}