using Engine;
using Engine.Graphics;

namespace Game;

public class TransferBlock : Block
{
    public const int Index = 335;

    public BlockMesh m_standaloneBlockMesh = new BlockMesh();

    public Color m_color;

    public TransferBlock()
    {
        DefaultDisplayName = "传送石";
        DefaultDescription = "点击传送方块门的门槛位置可召唤出传送门入口";
        DefaultCategory = "Items";
        CraftingId = "transfer";
        DisplayOrder = 1;
        IsPlaceable = false;
        FirstPersonScale = 0.4f;
        FirstPersonOffset = new Vector3(0.5f, -0.5f, -0.6f);
        InHandScale = 0.3f;
        InHandOffset = new Vector3(0f, 0.12f, 0f);
    }

    public override void Initialize()
    {
        m_color = new Color(192, 255, 128, 192);
        Model model = ContentManager.Get<Model>("Models/Diamond");
        Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Diamond").ParentBone);
        m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Diamond").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
        base.Initialize();
    }

    public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
    {
        BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_color, 2f * size, ref matrix, environmentData);
    }

    public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
    {
    }
}
