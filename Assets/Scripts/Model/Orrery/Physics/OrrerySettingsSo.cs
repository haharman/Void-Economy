using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "OrrerySettings", menuName = "NewOrrerySettings")]
    public class OrrerySettingsSo : ScriptableObject
    {
        [Serializable]
        public class PlanetKvp
        {
            public CoordSystemId Id;
            public PlanetModel Model;
        }
        public List<PlanetKvp> Planets = new List<PlanetKvp>();

        public IReadOnlyDictionary<CoordSystemId, PlanetModel> GetPlanetModelDictionary()
        {
            var dictionary = new Dictionary<CoordSystemId, PlanetModel>();
            foreach (var planet in Planets)
            {
                dictionary[planet.Id] = planet.Model;
            }
            return dictionary;
        }
    }
}