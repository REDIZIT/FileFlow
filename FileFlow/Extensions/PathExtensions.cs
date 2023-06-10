namespace FileFlow.Extensions
{
    public static class PathExtensions
    {
        public static string CleanUp(this string path)
        {
            return path.Replace("\\", "/").Replace("//", "/");
        }
    }
}
