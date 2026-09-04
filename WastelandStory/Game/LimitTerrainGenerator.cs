using Engine;

namespace Game;

public class LimitTerrainGenerator : TerrainContentsGenerator23, ITerrainContentsGenerator
{
    public Vector3 playerSpawnPosition;

    public LimitTerrainGenerator(SubsystemTerrain subsystemTerrain)
        : base(subsystemTerrain)
    {
        playerSpawnPosition = FindCoarseSpawnPosition();
    }

    public new void GenerateChunkContentsPass1(TerrainChunk chunk)
    {
        NGenerateSurfaceParameters(chunk);
        NGenerateTerrain(chunk);
    }

    public new void GenerateChunkContentsPass2(TerrainChunk chunk)
    {
    }

    public new void GenerateChunkContentsPass3(TerrainChunk chunk)
    {
        GenerateCaves(chunk);
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
        UpdateFluidIsTop(chunk);
    }

    public void NGenerateSurfaceParameters(TerrainChunk chunk)
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int num = i + chunk.Origin.X;
                int num2 = j + chunk.Origin.Y;
                int temperature = CalculateTemperature(num, num2);
                int humidity = CalculateHumidity(num, num2);
                chunk.SetTemperatureFast(i, j, temperature);
                chunk.SetHumidityFast(i, j, humidity);
            }
        }
    }

    public void NGenerateTerrain(TerrainChunk chunk)
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int num = i + chunk.Origin.X;
                int num2 = j + chunk.Origin.Y;
                float num3 = (float)(int)((float)num - playerSpawnPosition.X) * ((float)num - playerSpawnPosition.X) + ((float)num2 - playerSpawnPosition.Z) * ((float)num2 - playerSpawnPosition.Z);
                for (int k = 0; k < 100; k++)
                {
                    int num4 = k - 34;
                    if (num3 < (float)(k * k) && num4 > 0)
                    {
                        chunk.SetCellValueFast(i, num4, j, 2);
                    }
                }
                for (int l = 0; l < 255; l++)
                {
                    if (num3 >= 10000f && num3 <= 11025f)
                    {
                        chunk.SetCellValueFast(i, l, j, 1023);
                    }
                }
            }
        }
    }
}
