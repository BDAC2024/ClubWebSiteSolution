namespace AnglingClubWebsite.Authentication
{
    public class AnonymousRoutes
    {
        private List<string> ANONYMOUS_ROUTES = new List<string>
        {
            "/news/"
        };

        public bool Contains(Uri requestUri) 
        {
            var exists = false;
            var requestedRoute = requestUri.ToString().ToLower();

            foreach (var route in ANONYMOUS_ROUTES)
            {
                exists = requestedRoute.EndsWith(route.ToLower());
                if (exists) break;
            }

            return exists;
        }
    }
}
