namespace AnglingClubWebsite.Authentication
{
    public class AnonymousRoutes
    {
        private List<AnonymousRoute> ANONYMOUS_ROUTES = new List<AnonymousRoute>
        {
            new AnonymousRoute { Route = "/authenticate", Method = "POST" },
            new AnonymousRoute { Route = "/news", Method = "GET" },
        };

        public bool Contains(HttpRequestMessage request) 
        {
            var exists = false;
            var requestedRoute = request.RequestUri!.ToString().ToLower();
            var requestedMethod = request.Method.ToString().ToLower();

            foreach (var item in ANONYMOUS_ROUTES)
            {
                exists = requestedRoute.EndsWith(item.Route.ToLower()) && requestedMethod == item.Method.ToLower();
                if (exists) break;
            }

            return exists;
        }

        private class AnonymousRoute
        {
            public string Route { get; set; } = "";
            public string Method { get; set; } = "";
        }
    }
}
