using System;

namespace QingTing.Fm.Utility
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// 缓存路径
        /// </summary>
        public static string CachePath = string.Empty;
        public Settings()
        {
            CachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\QingTing.Fm\");
        }

    }
}
