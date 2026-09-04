using System;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game;

public class SubsystemDimensionBehavior : SubsystemDimensions, IUpdateable
{
    public SubsystemTerrain subsystemTerrain;

    public SubsystemWeather subsystemWeather;

    public WorldType worldType;

    public override void Update(float dt)
    {
        base.Update(dt);
        if (worldType == WorldType.Default)
        {
            return;
        }
        if (worldType == WorldType.Snowfield)
        {
            subsystemWeather.m_precipitationStartTime = 0.0;
        }
        else if (worldType == WorldType.Limit && m_componentPlayer != null)
        {
            m_componentPlayer.ComponentFlu.m_fluDuration = 0f;
            m_componentPlayer.ComponentFlu.m_coughDuration = 0f;
            m_componentPlayer.ComponentSickness.m_sicknessDuration = 0f;
            m_componentPlayer.ComponentSickness.m_greenoutDuration = 0f;
            m_componentPlayer.ComponentVitalStats.Sleep = 1f;
            m_componentPlayer.ComponentVitalStats.Stamina = 1f;
            m_componentPlayer.ComponentVitalStats.Temperature = 12f;
            m_componentPlayer.ComponentVitalStats.Wetness = 8f;
            if (m_componentPlayer.ComponentInput.PlayerInput.Jump && m_componentPlayer.ComponentLocomotion.m_falling)
            {
                Vector3 velocity = m_componentPlayer.ComponentBody.Velocity;
                m_componentPlayer.ComponentBody.Velocity = new Vector3(velocity.X, 7.5f, velocity.Z);
            }
        }
    }

    public override void Load(ValuesDictionary valuesDictionary)
    {
        base.Load(valuesDictionary);
        subsystemTerrain = m_subsystemTerrain;
        subsystemWeather = base.Project.FindSubsystem<SubsystemWeather>(throwOnError: true);
        worldType = m_worldType;
        switch (worldType)
        {
            case WorldType.Ashes:
                subsystemTerrain.TerrainContentsGenerator = new AshesTerrainGenerator(subsystemTerrain);
                break;
            case WorldType.Desert:
                subsystemTerrain.TerrainContentsGenerator = new DesertTerrainGenerator(subsystemTerrain);
                break;
            case WorldType.Snowfield:
                subsystemTerrain.TerrainContentsGenerator = new SnowfieldTerrainGenerator(subsystemTerrain);
                break;
            case WorldType.Limit:
                subsystemTerrain.TerrainContentsGenerator = new LimitTerrainGenerator(subsystemTerrain);
                break;
            case WorldType.Exist:
                subsystemTerrain.TerrainContentsGenerator = new TerrainContentsGeneratorFlat(subsystemTerrain);
                break;
        }
        if (worldType == WorldType.Ashes)
        {
            base.Project.FindSubsystem<SubsystemBlocksTexture>(throwOnError: true).BlocksTexture = ContentManager.Get<Texture2D>("寂静岭材质包");
            base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.TimeOfDayMode = TimeOfDayMode.Sunset;
            base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true).WorldSettings.AreWeatherEffectsEnabled = false;
        }
        else if (worldType == WorldType.Snowfield)
        {
            ChangeCreatureTypes();
        }
    }

    public override void OnEntityAdded(Entity entity)
    {
        if (worldType == WorldType.Ashes)
        {
            ComponentCreature componentCreature = entity.FindComponent<ComponentCreature>();
            ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>();
            if (componentCreature != null && componentPlayer == null)
            {
                componentCreature.ComponentCreatureModel.TextureOverride = ContentManager.Get<Texture2D>("Textures/Creatures/Jaguar");
                componentCreature.ComponentLocomotion.FlySpeed = componentCreature.ComponentLocomotion.WalkSpeed * 3f;
                componentCreature.ComponentLocomotion.WalkSpeed = componentCreature.ComponentLocomotion.WalkSpeed * 2f;
                ComponentHealth componentHealth = componentCreature.ComponentHealth;
                componentHealth.Attacked = (Action<ComponentCreature>)Delegate.Combine(componentHealth.Attacked, (Action<ComponentCreature>)delegate
                {
                    Random random = new Random();
                    if (random.Float(0f, 1f) <= 0.04f)
                    {
                        Vector3 position = componentCreature.ComponentBody.Position;
                        base.Project.FindSubsystem<SubsystemPickables>().AddPickable(111, 1, position, null, null);
                    }
                });
            }
        }
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
