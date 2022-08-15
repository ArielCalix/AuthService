using AuthService.Context;
using AuthService.Models;

namespace AuthService.Helpers
{
    public static class ValidationHelper
    {
        public static Employed TokenValidate(string token, userDbContext context)
        {
            if (context.UserTokens == null)
            {
                return new Employed();
            }
            else
            {
                UserToken userToken = context.UserTokens.Where(c => c.HashToken.Equals(token)).FirstOrDefault();
                if(userToken == null) return new Employed();
                return context.Employeds.Where(c => c.Identification.Equals(userToken.UserIdentity)).FirstOrDefault();
            }
        }
    }
}
