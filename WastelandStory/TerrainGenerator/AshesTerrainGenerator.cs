using Engine;

namespace Game;

public class AshesTerrainGenerator : TerrainContentsGenerator23, ITerrainContentsGenerator
{
    public AshesTerrainGenerator(SubsystemTerrain subsystemTerrain)
        : base(subsystemTerrain)
    {
    }

    public new void GenerateChunkContentsPass1(TerrainChunk chunk)
    {
        GenerateSurfaceParameters(chunk, 0, 0, 16, 8);
        NGenerateTerrain(chunk, 0, 0, 16, 8);
    }

    public new void GenerateChunkContentsPass2(TerrainChunk chunk)
    {
        GenerateSurfaceParameters(chunk, 0, 8, 16, 16);
        NGenerateTerrain(chunk, 0, 8, 16, 16);
    }

    public new void GenerateChunkContentsPass3(TerrainChunk chunk)
    {
        GenerateCaves(chunk);
        GeneratePockets(chunk);
        GenerateMinerals(chunk);
        PropagateFluidsDownwards(chunk);
    }

    public new void GenerateChunkContentsPass4(TerrainChunk chunk)
    {
        GenerateBedrockAndAir(chunk);
    }

    public void NGenerateTerrain(TerrainChunk chunk, int x1, int z1, int x2, int z2)
    {
        int num = x2 - x1;
        int num2 = z2 - z1;
        _ = m_subsystemTerrain.Terrain;
        int num3 = chunk.Origin.X + x1;
        int num4 = chunk.Origin.Y + z1;
        Grid2d grid2d = new Grid2d(num, num2);
        Grid2d grid2d2 = new Grid2d(num, num2);
        for (int i = 0; i < num2; i++)
        {
            for (int j = 0; j < num; j++)
            {
                grid2d.Set(j, i, CalculateOceanShoreDistance(j + num3, i + num4));
                grid2d2.Set(j, i, CalculateMountainRangeFactor(j + num3, i + num4));
            }
        }
        Grid3d grid3d = new Grid3d(num / 4 + 1, 33, num2 / 4 + 1);
        for (int k = 0; k < grid3d.SizeX; k++)
        {
            for (int l = 0; l < grid3d.SizeZ; l++)
            {
                int num5 = k * 4 + num3;
                int num6 = l * 4 + num4;
                float num7 = CalculateHeight(num5, num6);
                float v = CalculateMountainRangeFactor(num5, num6);
                float num8 = MathUtils.Lerp(TGMinTurbulence, 1f, TerrainContentsGenerator23.Squish(v, TGTurbulenceZero, 1f));
                for (int m = 0; m < grid3d.SizeY; m++)
                {
                    int num9 = m * 8;
                    float num10 = TGTurbulenceStrength * num8 * MathUtils.Saturate(num7 - (float)num9) * (2f * SimplexNoise.OctavedNoise(num5, num9, num6, TGTurbulenceFreq, TGTurbulenceOctaves, 4f, TGTurbulencePersistence) - 1f);
                    float num11 = (float)num9 + num10;
                    float num12 = num7 - num11;
                    num12 += MathUtils.Max(4f * (TGDensityBias - (float)num9), 0f);
                    grid3d.Set(k, m, l, num12);
                }
            }
        }
        int oceanLevel = base.OceanLevel;
        for (int n = 0; n < grid3d.SizeX - 1; n++)
        {
            for (int num13 = 0; num13 < grid3d.SizeZ - 1; num13++)
            {
                for (int num14 = 0; num14 < grid3d.SizeY - 1; num14++)
                {
                    grid3d.Get8(n, num14, num13, out var v2, out var v3, out var v4, out var v5, out var v6, out var v7, out var v8, out var v9);
                    float num15 = (v3 - v2) / 4f;
                    float num16 = (v5 - v4) / 4f;
                    float num17 = (v7 - v6) / 4f;
                    float num18 = (v9 - v8) / 4f;
                    float num19 = v2;
                    float num20 = v4;
                    float num21 = v6;
                    float num22 = v8;
                    for (int num23 = 0; num23 < 4; num23++)
                    {
                        float num24 = (num21 - num19) / 4f;
                        float num25 = (num22 - num20) / 4f;
                        float num26 = num19;
                        float num27 = num20;
                        for (int num28 = 0; num28 < 4; num28++)
                        {
                            float num29 = (num27 - num26) / 8f;
                            float num30 = num26;
                            int num31 = num23 + n * 4;
                            int num32 = num28 + num13 * 4;
                            int x3 = x1 + num31;
                            int z3 = z1 + num32;
                            float x4 = grid2d.Get(num31, num32);
                            float num33 = grid2d2.Get(num31, num32);
                            int temperatureFast = chunk.GetTemperatureFast(x3, z3);
                            int humidityFast = chunk.GetHumidityFast(x3, z3);
                            float f = num33 - 0.01f * (float)humidityFast;
                            float num34 = MathUtils.Lerp(100f, 0f, f);
                            float num35 = MathUtils.Lerp(300f, 30f, f);
                            bool flag = (temperatureFast > 8 && humidityFast < 8 && num33 < 0.97f) || (MathUtils.Abs(x4) < 16f && num33 < 0.97f);
                            int num36 = TerrainChunk.CalculateCellIndex(x3, 0, z3);
                            for (int num37 = 0; num37 < 8; num37++)
                            {
                                int num38 = num37 + num14 * 8;
                                int value = 0;
                                if (num30 < 0f)
                                {
                                    if (num38 <= oceanLevel)
                                    {
                                        value = 92;
                                    }
                                }
                                else
                                {
                                    value = (flag ? ((num30 < num34) ? 4 : ((!(num30 < num35)) ? 67 : 3)) : ((!(num30 < num35)) ? 67 : 3));
                                }
                                chunk.SetCellValueFast(num36 + num38, value);
                                num30 += num29;
                            }
                            num26 += num24;
                            num27 += num25;
                        }
                        num19 += num15;
                        num20 += num16;
                        num21 += num17;
                        num22 += num18;
                    }
                }
            }
        }
    }
}
