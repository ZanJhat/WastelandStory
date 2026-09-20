using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemDimensionPortal : Subsystem, IUpdateable, IDrawable
    {
        public List<DimensionPortal> m_portals = new List<DimensionPortal>();
        
        private PrimitivesRenderer3D PrimitivesRenderer = new PrimitivesRenderer3D();

        public int[] DrawOrders => new int[1] { 110 };
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public void Update(float dt)
        {
            foreach (var portal in m_portals)
            {
                portal.Update(dt);
            }
        }

        public void Draw(Camera camera, int drawOrder)
        {
            foreach (var portal in m_portals)
            {
                portal.Draw(camera, PrimitivesRenderer);
            }
            
            PrimitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public void AddStandardPortal(Point3 point3, bool IsAxisX, WorldType worldType, DimensionPortalMode mode = DimensionPortalMode.AllReady)
        {
            Color color = StandardDimensionPortal.GetWorldDoorColor(worldType);
            Vector3 position = new Vector3(point3) + new Vector3(1f, 2.5f, 0.5f);
            
            // Tính toán Bounding Box
            Vector3 sizeOffset = IsAxisX ? new Vector3(1f, 3f, 0f) : new Vector3(0f, 3f, 1f);
            BoundingBox box = new BoundingBox(new Vector3(point3), new Vector3(point3) + sizeOffset);

            StandardDimensionPortal portal1 = new StandardDimensionPortal();
            portal1.Initialize(Project, position, box, color, worldType, mode);
            portal1.Up = new Vector3(0f, -1f, 0f);
            
            if (!IsAxisX)
            {
                portal1.Position = new Vector3(point3) + new Vector3(0.5f, 2.5f, 1f);
                portal1.Forward = new Vector3(-1f, 0f, 0f);
                portal1.Right = new Vector3(0f, 0f, -1f);
            }
            else
            {
                portal1.Forward = new Vector3(0f, 0f, -1f);
                portal1.Right = new Vector3(-1f, 0f, 0f);
            }

            m_portals.Add(portal1);
        }
    }
}
