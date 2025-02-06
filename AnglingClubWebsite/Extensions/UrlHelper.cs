namespace AnglingClubWebsite.Extensions
{
    public static class UrlHelper
    {
        public static string SanitizeUrl(this string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return uriResult.ToString();
            }
            throw new ArgumentException("Invalid URL");
        }
    }

}
