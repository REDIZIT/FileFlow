using FileFlow.Services;
using System;

namespace FileFlow.Extensions
{
    public static class EnumExtensions
    {
        public static string ToMessageString(this LoadStatus status)
        {
            switch (status)
            {
                case LoadStatus.Ok: return "Ok";
                case LoadStatus.Empty: return "Эта папка пуста";
                case LoadStatus.NoAuth: return "У вас нет доступа к этой папке";
                case LoadStatus.NotFound: return "Папка не найдена";
                default: throw new ArgumentException($"Unable to degine message text for {nameof(LoadStatus)} = {status}");
            }
        }
    }
}
