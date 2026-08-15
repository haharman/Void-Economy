using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core;
using UnityEngine;
using R3;

namespace Model
{
    /// <summary>
    /// 星系の状態と、座標系の定義を保持するModel。
    ///
    /// 座標系は2種類：
    ///   Space  (PlanetId.Sun) … 恒星中心の直交座標。x, y ともにグローバル距離。
    ///   Surface(各惑星)        … 惑星表面に貼りついた極座標。
    ///                            x = 地表での弧長 (theta * radius)、y = 地表からの高度。
    ///                            theta は +Y 軸基準・時計回りが正。したがって
    ///                            惑星の最上点（グローバル Y が最大の点）が (0, 0) で、
    ///                            そこでの +x はグローバル +X（画面右）方向。
    ///
    /// 「いつ」変換するかは JetpackModel が判断し、「どう」変換するかはここが持つ。
    /// </summary>
    public class OrreryModel : IOrrerySource
    {
        public double OrreryCumulativeSeconds { get; private set; }
        public readonly Dictionary<CoordSystemId, PlanetModel> PlanetModelDictionary = new();

        public OrreryModel(OrrerySettingsSo settings)
        {
            foreach (var kvp in settings.Planets)
            {
                PlanetModelDictionary[kvp.Id] = kvp.Model;
            }
        }

        public void Init(double orreryCumulativeSeconds)
        {
            OrreryCumulativeSeconds = orreryCumulativeSeconds;
        }

        public void Tick(float deltaTime)
        {
            OrreryCumulativeSeconds += deltaTime;
            if (PlanetModelDictionary == null || PlanetModelDictionary.Count == 0) return;
            foreach (var planet in PlanetModelDictionary.Values)
            {
                planet.Tick(OrreryCumulativeSeconds);
            }
        }

        public (CoordSystemId id, IPlanetModel model, float distance) GetClosestPlanet(Vector2 position)
        {
            CoordSystemId closestId = CoordSystemId.Global;
            PlanetModel closestModel = null;
            float minSqr = float.MaxValue;

            foreach (var (id,model) in PlanetModelDictionary)
            {
                if (id == CoordSystemId.Global) continue;

                float sqr = (position - model.Position).sqrMagnitude;
                if (sqr < minSqr)
                {
                    minSqr = sqr;
                    closestId = id;
                    closestModel = model;
                }
            }

            return (closestId, closestModel, closestModel == null ? 0f : Mathf.Sqrt(minSqr));
        }

        public IPlanetModel GetPlanetModel(CoordSystemId id)
        {
            if (PlanetModelDictionary.TryGetValue(id, out PlanetModel planet))
                return planet;
            else
            {
                Debug.LogError("[OrreryModel] Planet not found");
                return null;
            }
        }

        // ------------------------------------------------------------------
        // 座標変換
        // ------------------------------------------------------------------

        /// <summary>
        /// Space座標 → Surface座標。位置と速度を必ずペアで変換する。
        /// 速度からは惑星の公転速度が差し引かれ、惑星に対する相対速度になる。
        /// </summary>
        public CoordPos ToSurfaceCoord(
            CoordSystemId id, Vector2 globalPosition, Vector2 globalVelocity, Vector2 globalForward)
        {
            var planet = PlanetModelDictionary[id];
            Vector2 planetPos    = planet.Position;
            float   planetRadius = planet.Radius;

            Vector2 d = globalPosition - planetPos;

            // ToGlobalCoord: x = r*sin(theta), y = r*cos(theta)  =>  theta = Atan2(dx, dy)
            // 中心と一致する場合は theta を定義できないので 0 に落とす
            float theta = (d.sqrMagnitude > 1e-12f) ? Mathf.Atan2(d.x, d.y) : 0f;

            Vector2 pos = new Vector2(theta * planetRadius, d.magnitude - planetRadius);

            float c = Mathf.Cos(theta), s = Mathf.Sin(theta);

            // ToGlobalCoord の M = [[c, s], [-s, c]] の逆行列は転置 Mt = [[c, -s], [s, c]]
            Vector2 rel = globalVelocity - planet.Velocity;
            Vector2 v = new Vector2(rel.x * c - rel.y * s,
                rel.x * s + rel.y * c);

            Vector2 f = new Vector2(globalForward.x * c - globalForward.y * s,
                globalForward.x * s + globalForward.y * c);

            return new CoordPos(id, pos, v, f);
        }
        
        public CoordPos ToGlobalCoord(
            CoordSystemId id, Vector2 surfacePosition, Vector2 surfaceVelocity, Vector2 surfaceForward)
        {
            var planet = PlanetModelDictionary[id];

            float theta  = surfacePosition.x / planet.Radius;
            float radius = planet.Radius + surfacePosition.y;

            float c = Mathf.Cos(theta), s = Mathf.Sin(theta);   // ← 先に出す

            Vector2 pos = planet.Position + new Vector2(radius * s, radius * c);

            // forward と同一の M を使う（旧: 成分別 sin/cos スケールは非可逆）
            Vector2 v = planet.Velocity + new Vector2(surfaceVelocity.x * c + surfaceVelocity.y * s,
                -surfaceVelocity.x * s + surfaceVelocity.y * c);

            Vector2 f = new Vector2(surfaceForward.x * c + surfaceForward.y * s,
                -surfaceForward.x * s + surfaceForward.y * c);

            return new CoordPos(CoordSystemId.Global, pos, v, f);
        }

        // ------------------------------------------------------------------
        // 重力
        // ------------------------------------------------------------------

        /// <summary>
        /// グローバル座標 position における重力加速度（Global空間のベクトル）。
        /// Surface空間の計算にそのまま足してはいけない。
        /// </summary>
        public Vector2 GetGravityAccelerationAt(Vector2 position)
        {
            Vector2 gravityAcceleration = Vector2.zero;
            foreach (var planet in PlanetModelDictionary.Values)
            {
                Vector2 d = planet.Position - position;
                float sqr = d.sqrMagnitude;
                if (sqr < 1e-6f) continue;

                // a = mu * d / |d|^3
                gravityAcceleration += planet.Mu * d / (sqr * Mathf.Sqrt(sqr));
            }

            return gravityAcceleration;
        }
    }
}
