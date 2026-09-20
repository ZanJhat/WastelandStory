using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;
using XmlUtilities;

namespace Game
{
    public class AirWallBlock : AlphaTestCubeBlock
    {
        public static int Index = 1023;

        public AirWallBlock()
        {
            DefaultCreativeData = -1;
        }

        public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue, int x, int y, int z)
        {
            return false;
        }

        public override float GetDigResilience(int value)
        {
            return float.PositiveInfinity;
        }

        public override float GetProjectileResilience(int value)
        {
            return float.PositiveInfinity;
        }

        public override float GetExplosionResilience(int value)
        {
            return float.PositiveInfinity;
        }
    }
}
