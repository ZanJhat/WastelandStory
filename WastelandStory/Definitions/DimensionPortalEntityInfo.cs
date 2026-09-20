using System;
using Engine;
using TemplatesDatabase;
using GameEntitySystem;

namespace Game
{
    public class DimensionPortalEntityInfo
    {
        public DimensionPortalEntityType EntityType;

        public ComponentPlayer ComponentPlayer;

        public ComponentCreature ComponentCreature;

        public ComponentBody ComponentBody;

        public Entity Entity;

        public Vector3 Position;

        public Vector3 Velocity;

        public WorldType CurrentWorldType;

        public WorldType TargetWorldType;

        public DimensionPortalEntityInfo(
            DimensionPortalEntityType entityType,
            ComponentPlayer componentPlayer,
            ComponentCreature componentCreature,
            ComponentBody componentBody,
            Entity entity,
            Vector3 position,
            Vector3 velocity,
            WorldType currentWorldType,
            WorldType targetWorldType)
        {
            EntityType = entityType;
            ComponentPlayer = componentPlayer;
            ComponentCreature = componentCreature;
            ComponentBody = componentBody;
            Entity = entity;
            Position = position;
            Velocity = velocity;
            CurrentWorldType = currentWorldType;
            TargetWorldType = targetWorldType;
        }
    }
}