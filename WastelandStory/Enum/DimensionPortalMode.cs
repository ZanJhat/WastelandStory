using System;

namespace Game
{
    public enum DimensionPortalMode
    {
        AllReady, // Tất cả phải vào → mới teleport
        Follow // A vào → B cũng bị teleport theo
    }
}