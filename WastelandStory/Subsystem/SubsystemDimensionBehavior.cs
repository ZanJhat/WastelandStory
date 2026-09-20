using System;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemDimensionBehavior : SubsystemDimensions, IUpdateable
    {
        public SubsystemWeather subsystemWeather;

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            subsystemWeather = base.Project.FindSubsystem<SubsystemWeather>(throwOnError: true);
        }

        public override void OnEntityAdded(Entity entity)
        {
            base.OnEntityAdded(entity);
        }

        public void ChangeCreatureTypes()
        {
            SubsystemCreatureSpawn subsystemCreatureSpawn = base.Project.FindSubsystem<SubsystemCreatureSpawn>(throwOnError: true);
            subsystemCreatureSpawn.m_creatureTypes.Clear();

            subsystemCreatureSpawn.m_creatureTypes.Add(new SubsystemCreatureSpawn.CreatureType("Seagull", SpawnLocationType.Surface, randomSpawn: true, constantSpawn: false)
            {
                SpawnSuitabilityFunction = delegate (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point)
                {
                    float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
                    return (num < 8f) ? 5f : 0f;
                },
                SpawnFunction = (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point) => subsystemCreatureSpawn.SpawnCreatures(creatureType, "Seagull", point, 1).Count
            });
            subsystemCreatureSpawn.m_creatureTypes.Add(new SubsystemCreatureSpawn.CreatureType("White Tigers", SpawnLocationType.Surface, randomSpawn: true, constantSpawn: false)
            {
                SpawnSuitabilityFunction = delegate (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point)
                {
                    float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
                    return (num > 8f) ? 5f : 0f;
                },
                SpawnFunction = (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point) => subsystemCreatureSpawn.SpawnCreatures(creatureType, "Tiger_White", point, 1).Count
            });
            subsystemCreatureSpawn.m_creatureTypes.Add(new SubsystemCreatureSpawn.CreatureType("White Bull", SpawnLocationType.Surface, randomSpawn: true, constantSpawn: false)
            {
                SpawnSuitabilityFunction = delegate (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point)
                {
                    float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
                    return (num > 8f) ? 5f : 0f;
                },
                SpawnFunction = (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point) => subsystemCreatureSpawn.SpawnCreatures(creatureType, "Bull_White", point, 1).Count
            });
            subsystemCreatureSpawn.m_creatureTypes.Add(new SubsystemCreatureSpawn.CreatureType("Polar Bears", SpawnLocationType.Surface, randomSpawn: true, constantSpawn: false)
            {
                SpawnSuitabilityFunction = delegate (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point)
                {
                    float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
                    return (num > 8f) ? 5f : 0f;
                },
                SpawnFunction = (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point) => subsystemCreatureSpawn.SpawnCreatures(creatureType, "Bear_Polar", point, 1).Count
            });
            subsystemCreatureSpawn.m_creatureTypes.Add(new SubsystemCreatureSpawn.CreatureType("White Horse", SpawnLocationType.Surface, randomSpawn: true, constantSpawn: false)
            {
                SpawnSuitabilityFunction = delegate (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point)
                {
                    float num = m_subsystemTerrain.TerrainContentsGenerator.CalculateOceanShoreDistance(point.X, point.Z);
                    return (num > 8f) ? 5f : 0f;
                },
                SpawnFunction = (SubsystemCreatureSpawn.CreatureType creatureType, Point3 point) => subsystemCreatureSpawn.SpawnCreatures(creatureType, "Horse_White", point, 1).Count
            });
        }
    }
}
