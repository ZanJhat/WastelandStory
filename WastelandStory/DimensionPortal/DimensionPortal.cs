using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System.Collections.Generic;

namespace Game
{
    public abstract class DimensionPortal
    {
        public Project Project;

        // Các thông số không gian cơ bản
        public Vector3 Position { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public Color Color { get; set; }
        public WorldType TargetWorldType { get; set; }
        public DimensionPortalMode Mode { get; set; }

        // Các biến cần thiết
        public SubsystemBodies m_subsystemBodies;
        public SubsystemSky m_subsystemSky;
        public SubsystemDimensions m_subsystemDimensions;

        // Khởi tạo thông số cổng
        public virtual void Initialize(Project project, Vector3 position, BoundingBox box, Color color, WorldType targetWorld, DimensionPortalMode mode)
        {
            Project = project;
            Position = position;
            BoundingBox = box;
            Color = color;
            TargetWorldType = targetWorld;
            Mode = mode;

            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemDimensions = Project.FindSubsystem<SubsystemDimensions>(true);
        }

        // Xử lý logic mỗi khung hình (Hút entity, kiểm tra chạm box để chuyển map...)
        public virtual void Update(float dt)
        {
            DynamicArray<ComponentBody> bodiesInArea = new DynamicArray<ComponentBody>();

            // Tìm các thực thể nằm trong Bounding Box của cổng
            m_subsystemBodies.FindBodiesInArea(BoundingBox.Min.XZ, BoundingBox.Max.XZ, bodiesInArea);

            foreach (ComponentBody body in bodiesInArea)
            {
                Vector3 pos = body.Position;
                if (pos.X >= BoundingBox.Min.X && pos.Y >= BoundingBox.Min.Y && pos.Z >= BoundingBox.Min.Z &&
                    pos.X <= BoundingBox.Max.X && pos.Y <= BoundingBox.Max.Y && pos.Z <= BoundingBox.Max.Z)
                {
                    OnEntityEntered(body.Entity);
                }
            }
        }

        // Hàm được gọi khi có Entity bước vào Bounding Box
        protected virtual void OnEntityEntered(Entity entity)
        {
            // Bạn có thể gọi SubsystemDimensions để thực hiện logic dịch chuyển hay thực hiện bất kì gì
            // Có thể viết thêm logic phân loại xử lý cho người chơi/động vật ở đây
        }

        public DimensionPortalEntityInfo GetEntityInfo(Entity entity)
        {
            if (entity == null || m_subsystemDimensions == null)
                return null;

            ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>();
            ComponentCreature componentCreature = entity.FindComponent<ComponentCreature>();

            DimensionPortalEntityType entityType;

            if (componentPlayer != null)
            {
                entityType = DimensionPortalEntityType.Player;
            }
            else
            {
                if (componentCreature != null)
                {
                    entityType = DimensionPortalEntityType.Creature;
                }
                else
                {
                    entityType = DimensionPortalEntityType.Entity;
                }
            }

            ComponentBody componentBody = entity.FindComponent<ComponentBody>();

            if (componentBody == null)
                return null;

            Vector3 position = componentBody.Position;
            Vector3 velocity = componentBody.Velocity;

            return new DimensionPortalEntityInfo(
                entityType,
                componentPlayer,
                componentCreature,
                componentBody,
                entity,
                position,
                velocity,
                m_subsystemDimensions.WorldType,
                TargetWorldType);
        }

        // Bắt buộc các class kế thừa phải tự định nghĩa cách vẽ
        public abstract void Draw(Camera camera, PrimitivesRenderer3D renderer);
    }
}
