using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game;

public class SubsystemDimensionPortal : Subsystem, IDrawable
{
    public SubsystemSky m_subsystemSky;

    public Dictionary<Point3, DimensionPortal[]> m_dimensionPortals = new Dictionary<Point3, DimensionPortal[]>();

    private PrimitivesRenderer3D PrimitivesRenderer = new PrimitivesRenderer3D();

    private TexturedBatch3D BatchesByType = new TexturedBatch3D();

    public int[] DrawOrders => new int[1] { 110 };

    public void Draw(Camera camera, int drawOrder)
    {
        foreach (Point3 key in m_dimensionPortals.Keys)
        {
            DimensionPortal[] array = m_dimensionPortals[key];
            DimensionPortal[] array2 = array;
            foreach (DimensionPortal chartlet in array2)
            {
                Vector3 vector = chartlet.Position - camera.ViewPosition;
                float num = Vector3.Dot(vector, camera.ViewDirection);
                if (!(num > 0.01f))
                {
                    continue;
                }
                float num2 = vector.Length();
                if (num2 < m_subsystemSky.VisibilityRange)
                {
                    float num3 = chartlet.Size;
                    if (chartlet.FarDistance > 0f)
                    {
                        num3 += (chartlet.FarSize - chartlet.Size) * MathUtils.Saturate(num2 / chartlet.FarDistance);
                    }
                    Vector3 vector2 = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                    Vector3 p = chartlet.Position + num3 * (-chartlet.Right - chartlet.Up) + vector2;
                    Vector3 p2 = chartlet.Position + num3 * (chartlet.Right - chartlet.Up) + vector2;
                    Vector3 p3 = chartlet.Position + num3 * (chartlet.Right + chartlet.Up) + vector2;
                    Vector3 p4 = chartlet.Position + num3 * (-chartlet.Right + chartlet.Up) + vector2;
                    BatchesByType.QueueQuad(p, p2, p3, p4, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), chartlet.Color);
                }
            }
        }
        PrimitivesRenderer.Flush(camera.ViewProjectionMatrix);
    }

    public override void Load(ValuesDictionary valuesDictionary)
    {
        m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
        BatchesByType = PrimitivesRenderer.TexturedBatch(ContentManager.Get<Texture2D>("传送门贴图"), useAlphaTest: false, 0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.AlphaBlend, SamplerState.LinearClamp);
    }

    public void CreateChartlet(Point3 point3, bool IsAxisX, WorldType worldType)
    {
        DimensionPortal[] array = new DimensionPortal[2];
        DimensionPortal chartlet = new DimensionPortal();
        DimensionPortal chartlet2 = new DimensionPortal();
        Color worldDoorColor = SubsystemDimensions.GetWorldDoorColor(worldType);
        Vector3 position = new Vector3(point3) + new Vector3(1f, 2.5f, 0.5f);
        Vector3 forward = new Vector3(0f, 0f, -1f);
        Vector3 up = new Vector3(0f, -1f, 0f);
        Vector3 right = new Vector3(-1f, 0f, 0f);
        Vector3 forward2 = new Vector3(0f, 0f, -1f);
        Vector3 up2 = new Vector3(0f, -1f, 0f);
        Vector3 right2 = new Vector3(1f, 0f, 0f);
        if (!IsAxisX)
        {
            position = new Vector3(point3) + new Vector3(0.5f, 2.5f, 1f);
            forward = new Vector3(-1f, 0f, 0f);
            up = new Vector3(0f, -1f, 0f);
            right = new Vector3(0f, 0f, -1f);
            forward2 = new Vector3(-1f, 0f, 0f);
            up2 = new Vector3(0f, -1f, 0f);
            right2 = new Vector3(0f, 0f, 1f);
        }
        chartlet.Position = position;
        chartlet.Forward = forward;
        chartlet.Up = up;
        chartlet.Right = right;
        chartlet.Color = worldDoorColor;
        chartlet2.Position = position;
        chartlet2.Forward = forward2;
        chartlet2.Up = up2;
        chartlet2.Right = right2;
        chartlet2.Color = worldDoorColor;
        array[0] = chartlet;
        array[1] = chartlet2;
        if (!m_dimensionPortals.ContainsKey(point3))
        {
            m_dimensionPortals.Add(point3, array);
        }
    }
}
