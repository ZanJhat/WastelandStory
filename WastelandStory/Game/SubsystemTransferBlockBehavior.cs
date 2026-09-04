using Engine;
using TemplatesDatabase;
using Game;

namespace Game;

public class SubsystemTransferBlockBehavior : SubsystemBlockBehavior
{
    public SubsystemDimensions m_subsystemDimensions;

    public SubsystemDimensionPortal m_subsystemDimensionPortal;

    public SubsystemTerrain m_subsystemTerrain;

    public SubsystemParticles m_subsystemParticles;

    public override int[] HandledBlocks => new int[1] { BlocksManager.GetBlockIndex<TransferBlock>() };

    public override void Load(ValuesDictionary valuesDictionary)
    {
        m_subsystemDimensions = base.Project.FindSubsystem<SubsystemDimensions>(throwOnError: true);
        m_subsystemDimensionPortal = base.Project.FindSubsystem<SubsystemDimensionPortal>(throwOnError: true);
        m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
        m_subsystemParticles = base.Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
    }

    public override bool OnUse(Ray3 ray, ComponentMiner componentMiner)
    {
        object obj = componentMiner.Raycast(ray, RaycastMode.Interaction);
        if (obj is TerrainRaycastResult)
        {
            CellFace cellFace = ((TerrainRaycastResult)obj).CellFace;
            if (cellFace.Face != 4)
            {
                return false;
            }
            Point3 point = cellFace.Point;
            int cellContents = m_subsystemTerrain.Terrain.GetCellContents(point.X, point.Y, point.Z);
            string[][] collections = WorldParameter.Collections;
            foreach (string[] array in collections)
            {
                if (cellContents == int.Parse(array[2]))
                {
                    string[] array2 = array[3].Split(',');
                    if (array2.Length > 3)
                    {
                        Color color = new Color(int.Parse(array2[0]), int.Parse(array2[1]), int.Parse(array2[2]), int.Parse(array2[3]));
                        m_subsystemParticles.AddParticleSystem(new FireworksParticleSystem(new Vector3(point) + new Vector3(0.5f, 1f, 0.5f), color, FireworksBlock.Shape.SmallBurst, 0.8f, 0.3f));
                        break;
                    }
                }
            }
            Point3 point2 = point + new Point3(1, 0, 0);
            if (CreateEntrance(point, point2, skipJudge: false, WorldType.Default))
            {
                return true;
            }
            point2 = point + new Point3(-1, 0, 0);
            if (CreateEntrance(point, point2, skipJudge: false, WorldType.Default))
            {
                return true;
            }
            point2 = point + new Point3(0, 0, 1);
            if (CreateEntrance(point, point2, skipJudge: false, WorldType.Default))
            {
                return true;
            }
            point2 = point + new Point3(0, 0, -1);
            if (CreateEntrance(point, point2, skipJudge: false, WorldType.Default))
            {
                return true;
            }
        }
        return false;
    }

    public bool JudgePass(Point3 point, Point3 point2, int cid)
    {
        for (int i = 0; i < 5; i++)
        {
            int num = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y + i, point.Z));
            int num2 = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(point2.X, point2.Y + i, point2.Z));
            if ((i == 0 || i == 4) && (num != cid || num2 != cid))
            {
                return false;
            }
            if (i > 0 && i < 4 && (num != 0 || num2 != 0))
            {
                return false;
            }
        }
        Point3 point3 = point2 - point;
        point -= point3;
        point2 += point3;
        for (int j = 0; j < 5; j++)
        {
            int num3 = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y + j, point.Z));
            int num4 = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(point2.X, point2.Y + j, point2.Z));
            if (num3 != cid || num4 != cid)
            {
                return false;
            }
        }
        return true;
    }

    public bool CreateEntrance(Point3 point, Point3 point2, bool skipJudge, WorldType fastType)
    {
        string[][] collections = WorldParameter.Collections;
        foreach (string[] array in collections)
        {
            int cid = int.Parse(array[2]);
            bool flag = JudgePass(point, point2, cid);
            if (flag || skipJudge)
            {
                WorldDoor worldDoor = new WorldDoor();
                if (point.X - point2.X > 0 || point.Y - point2.Y > 0 || point.Z - point2.Z > 0)
                {
                    Point3 point3 = point;
                    point = point2;
                    point2 = point3;
                }
                bool isAxisX = false;
                Vector3 vector = new Vector3(1f, 0f, 0f);
                Vector3 vector2 = new Vector3(0f, 3f, 1f);
                if (point2.X - point.X > 0 && point2.Z - point.Z == 0)
                {
                    isAxisX = true;
                    vector = new Vector3(0f, 0f, 1f);
                    vector2 = new Vector3(1f, 3f, 0f);
                }
                worldDoor.WorldType = (skipJudge ? fastType : SubsystemDimensions.GetWorldType(array[0]));
                worldDoor.MinPoint = new Vector3(point) + new Vector3(0f, 1f, 0f) + vector * 0.4f;
                worldDoor.MaxPoint = new Vector3(point2) + new Vector3(0f, 1f, 0f) + vector * 0.6f + vector2;
                if (!m_subsystemDimensions.m_worldDoors.ContainsKey(point))
                {
                    m_subsystemDimensions.m_worldDoors.Add(point, worldDoor);
                    m_subsystemDimensionPortal.CreateChartlet(point, isAxisX, worldDoor.WorldType);
                }
                return true;
            }
        }
        return false;
    }
}
