using System;

namespace FileFlow
{
    [Serializable]
    public class Appearance
    {
        public bool useWallpaper;
        public string wallpaperPath;
        public float wallpaperOpacity = 0.9f;
        public float wallpaperDimmerOpacity = 0.07843f;
    }
}
