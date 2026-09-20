using Engine;
using Engine.Graphics;

namespace Game
{
    public class StandardDimensionPortal : DimensionPortal
    {
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public float Size = 1.5f;
        public float FarSize = 1.5f;
        public float FarDistance = 1f;

        public override void Draw(Camera camera, PrimitivesRenderer3D renderer)
        {
            Vector3 vector = Position - camera.ViewPosition;
            float num = Vector3.Dot(vector, camera.ViewDirection);
            
            // Xóa dòng "if (num <= 0.01f) return;" vì bây giờ chúng ta cần render cổng ngay cả khi người chơi đứng ở mặt sau của nó (Vector.Dot âm).

            float num2 = vector.Length();
            
            if (num2 < m_subsystemSky.VisibilityRange)
            {
                float currentSize = Size;
                
                if (FarDistance > 0f)
                    currentSize += (FarSize - Size) * MathUtils.Saturate(num2 / FarDistance);
                
                // Vector2 ở đây dùng để triệt tiêu Z-fighting (nhấp nháy khi cổng sát tường)
                // Sử dụng MathF.Abs(num) để cổng không bị lỗi offset khi đứng ở mặt sau
                Vector3 vector2 = (0f - (0.01f + 0.02f * MathF.Abs(num))) / num2 * vector;

                Texture2D texture = ContentManager.Get<Texture2D>("StandardDimensionPortal");
                TexturedBatch3D batch = renderer.TexturedBatch(texture, useAlphaTest: false, 0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.AlphaBlend, SamplerState.LinearClamp);
                
                // 1. Vẽ mặt trước (Right bình thường)
                Vector3 p = Position + currentSize * (-Right - Up) + vector2;
                Vector3 p2 = Position + currentSize * (Right - Up) + vector2;
                Vector3 p3 = Position + currentSize * (Right + Up) + vector2;
                Vector3 p4 = Position + currentSize * (-Right + Up) + vector2;
                
                batch.QueueQuad(p, p2, p3, p4, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), Color);

                // 2. Vẽ mặt sau (Đảo ngược trục Right)
                Vector3 backRight = -Right;
                Vector3 bp = Position + currentSize * (-backRight - Up) + vector2;
                Vector3 bp2 = Position + currentSize * (backRight - Up) + vector2;
                Vector3 bp3 = Position + currentSize * (backRight + Up) + vector2;
                Vector3 bp4 = Position + currentSize * (-backRight + Up) + vector2;
                
                // Đảo tọa độ UV X (từ 0->1 thành 1->0) để hình ảnh mặt sau không bị lật ngược trái/phải
                batch.QueueQuad(bp, bp2, bp3, bp4, new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), Color);
            }
        }
        
        public static int GetWorldDoorBlock(WorldType worldType)
        {
            int result = 0;
            string[][] collections = WorldParameter.Collections;
            foreach (string[] array in collections)
            {
                if (worldType.ToString() == array[0])
                {
                    result = int.Parse(array[2]);
                    break;
                }
            }
            return result;
        }

        public static Color GetWorldDoorColor(WorldType worldType)
        {
            Color result = Color.White;
            string[][] collections = WorldParameter.Collections;
            foreach (string[] array in collections)
            {
                if (worldType.ToString() == array[0])
                {
                    string[] array2 = array[3].Split(',');
                    int r = int.Parse(array2[0]);
                    int g = int.Parse(array2[1]);
                    int b = int.Parse(array2[2]);
                    int a = int.Parse(array2[3]);
                    result = new Color(r, g, b, a);
                    break;
                }
            }
            return result;
        }
    }
}
