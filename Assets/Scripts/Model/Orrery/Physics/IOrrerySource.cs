using System.Collections.Generic;
using Core;

namespace Model
{
    public interface IOrrerySource
    {
        double OrreryCumulativeSeconds { get; }
        IPlanetModel GetPlanetModel(CoordSystemId planetId);
    }
}