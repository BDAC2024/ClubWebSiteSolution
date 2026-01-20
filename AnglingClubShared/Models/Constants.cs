namespace AnglingClubShared.Models
{
    public static class Constants
    {
        public const string VIDEO_BASE_URL = "https://www.youtube.com/embed";
        public const string MAP_DIRECTIONS_BASE_URL = "https://www.google.co.uk/maps/dir";

        public const string AUTH_KEY = "currentMember";
        public const string HTTP_CLIENT_KEY = "ServerApi";
        public const string HTTP_CLIENT_KEY_LONG_RUNNING = "ServerApiLongRunning";
        public const string API_ROOT_KEY = "ServerUrl";
        public const string AUTH_EXPIRED = "EXPIRED";

        public const string API_AUTHENTICATE = "authenticate";
        public const string API_PIN_RESET_REQUEST = "/pinresetrequest";

        public const string API_NEWS = "";
        public const string API_WATERS = "";
        public const string API_REF_DATA = "";
        public const string API_CLUB_EVENTS = ""; 
        public const string API_WATERS_UPDATE = "UpdateDescription";

        public const string API_DOCUMENT = "";
        public const string API_DOCUMENT_GETUPLOADURL = "GetUploadUrl";
        public const string API_DOCUMENT_READ = "GetDocuments";

        public const string API_TMPFILE = "";
        public const string API_TMPFILE_GETUPLOADURL = "GetUploadUrl";

        public const int MINUTES_TO_EXPIRE_LINKS = 5;

    }
}
