using System.Configuration;

namespace UniAlltid.Language.API.Code
{
    public class AppSettingsReader
    {
        public static bool UpdateQueryId4
        {
            get
            {
                var val = ConfigurationManager.AppSettings["useUpdatedQueryId4"];
                bool update;
                if (bool.TryParse(val, out update))
                    return update;

                return false;
            }
        }

        public static string SeqApiKey => ConfigurationManager.AppSettings["seqApiKey"];

        public static string SeqUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:5341/";
#endif
                return ConfigurationManager.AppSettings["seqUrl"];
            }
        }
    }
}