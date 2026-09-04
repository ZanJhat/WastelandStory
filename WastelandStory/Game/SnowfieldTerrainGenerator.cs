namespace Game;

public class SnowfieldTerrainGenerator : TerrainContentsGenerator23, ITerrainContentsGenerator
{
    public SnowfieldTerrainGenerator(SubsystemTerrain subsystemTerrain)
        : base(subsystemTerrain)
    {
    }

    public new void GenerateChunkContentsPass1(TerrainChunk chunk)
    {
        NGenerateSurfaceParameters(chunk, 0, 0, 16, 8);
        GenerateTerrain(chunk, 0, 0, 16, 8);
    }

    public new void GenerateChunkContentsPass2(TerrainChunk chunk)
    {
        NGenerateSurfaceParameters(chunk, 0, 8, 16, 16);
        GenerateTerrain(chunk, 0, 8, 16, 16);
    }

    public new void GenerateChunkContentsPass3(TerrainChunk chunk)
    {
        GenerateCaves(chunk);
        GeneratePockets(chunk);
        GenerateMinerals(chunk);
        GenerateSurface(chunk);
        PropagateFluidsDownwards(chunk);
    }

    public new void GenerateChunkContentsPass4(TerrainChunk chunk)
    {
        GenerateGrassAndPlants(chunk);
        GenerateLogs(chunk);
        GenerateTrees(chunk);
        GenerateCacti(chunk);
        GeneratePumpkins(chunk);
        GenerateKelp(chunk);
        GenerateSeagrass(chunk);
        GenerateBottomSuckers(chunk);
        GenerateTraps(chunk);
        GenerateIvy(chunk);
        GenerateGraves(chunk);
        GenerateSnowAndIce(chunk);
        GenerateBedrockAndAir(chunk);
        UpdateFluidIsTop(chunk);
    }

    public void NGenerateSurfaceParameters(TerrainChunk chunk, int x1, int z1, int x2, int z2)
    {
        for (int i = x1; i < x2; i++)
        {
            for (int j = z1; j < z2; j++)
            {
                int num = i + chunk.Origin.X;
                int num2 = j + chunk.Origin.Y;
                int temperature = 0;
                int humidity = 15;
                chunk.SetTemperatureFast(i, j, temperature);
                chunk.SetHumidityFast(i, j, humidity);
            }
        }
    }
}
