using log4net.Config;
using log4net;

namespace Playlist_Pro
{
    public static class Log4NetConfig
    {
        public static ILog GetLogger()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            return LogManager.GetLogger(typeof(Log4NetConfig));
        }
    }
}
