using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnglingClubShared.Exceptions
{
    public class UserSessionExpiredException : Exception
    {
        public UserSessionExpiredException()
        {
        }

        public UserSessionExpiredException(string message)
            : base(message)
        {
        }

        public UserSessionExpiredException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
