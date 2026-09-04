using Engine;

namespace Game;

public class DesertTerrainGenerator : TerrainContentsGenerator23, ITerrainContentsGenerator
{
    public DesertTerrainGenerator(SubsystemTerrain subsystemTerrain)
        : base(subsystemTerrain)
    {
    }

    public new void GenerateChunkContentsPass1(TerrainChunk chunk)
    {
        NGenerateSurfaceParameters(chunk, 0, 0, 16, 8);
        NGenerateTerrain(chunk, 0, 0, 16, 8);
    }

    public new void GenerateChunkContentsPass2(TerrainChunk chunk)
    {
        NGenerateSurfaceParameters(chunk, 0, 8, 16, 16);
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
        int x = chunk.Coords.X;
        int y = chunk.Coords.Y;
        if ((y - 4 * x) % 5 != 0 || (y + 7 * x) % 4 != 0 || (int)MathUtils.Sqrt(x * x + y * y) % 7 != 0)
        {
            GenerateCacti(chunk);
            return;
        }
        int num = 10;
        bool flag = true;
        for (int num2 = 200; num2 > 10; num2--)
        {
            if (m_subsystemTerrain.Terrain.GetCellContentsFast(chunk.Origin.X, num2, chunk.Origin.Y) != 0)
            {
                num = num2;
                break;
            }
        }
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                int cellContentsFast = m_subsystemTerrain.Terrain.GetCellContentsFast(chunk.Origin.X + i, num, chunk.Origin.Y + j);
                int cellContentsFast2 = m_subsystemTerrain.Terrain.GetCellContentsFast(chunk.Origin.X + i, num + 1, chunk.Origin.Y + j);
                if (cellContentsFast == 0 || cellContentsFast == 18 || cellContentsFast2 != 0)
                {
                    flag = false;
                    break;
                }
            }
        }
        if (flag)
        {
            string text = ContentManager.Get<string>("沙漠房子");
            text = text.Replace("\n", "#");
            string[] array = text.Split('#');
            string[] array2 = array;
            foreach (string text2 in array2)
            {
                string[] array3 = text2.Split(',');
                if (array3.Length > 3)
                {
                    int num3 = int.Parse(array3[0]);
                    int num4 = int.Parse(array3[1]);
                    int num5 = int.Parse(array3[2]);
                    int value = int.Parse(array3[3]);
                    m_subsystemTerrain.Terrain.SetCellValueFast(chunk.Origin.X + num3, num + 1 + num4, chunk.Origin.Y + num5, value);
                }
            }
        }
        else
        {
            GenerateCacti(chunk);
        }
    }

    public void NGenerateSurfaceParameters(TerrainChunk chunk, int x1, int z1, int x2, int z2)
    {
        for (int i = x1; i < x2; i++)
        {
            for (int j = z1; j < z2; j++)
            {
                int num = i + chunk.Origin.X;
                int num2 = j + chunk.Origin.Y;
                int temperature = MathUtils.Clamp((int)(MathUtils.Saturate(3f * SimplexNoise.OctavedNoise((float)num + m_temperatureOffset.X, (float)num2 + m_temperatureOffset.Y, 0.0015f / TGBiomeScaling, 5, 2f, 0.6f) - 1.1f + m_worldSettings.TemperatureOffset / 16f) * 16f), 12, 15);
                int humidity = MathUtils.Clamp((int)(MathUtils.Saturate(3f * SimplexNoise.OctavedNoise((float)num + m_humidityOffset.X, (float)num2 + m_humidityOffset.Y, 0.0012f / TGBiomeScaling, 5, 2f, 0.6f) - 0.9f + m_worldSettings.HumidityOffset / 16f) * 16f), 0, 3);
                chunk.SetTemperatureFast(i, j, temperature);
                chunk.SetHumidityFast(i, j, humidity);
            }
        }
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
                                if (num30 >= 0f)
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
