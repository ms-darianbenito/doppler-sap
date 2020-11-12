using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Doppler.Sap.DopplerSecurity
{
    public class IsSuperUserHandler : AuthorizationHandler<IsSuperUserRequirement>
    {
        private readonly ILogger<IsSuperUserHandler> _logger;

        public IsSuperUserHandler(ILogger<IsSuperUserHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsSuperUserRequirement requirement)
        {
            if (IsSuperUser(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsSuperUser(AuthorizationHandlerContext context)
        {
            if (!context.User.HasClaim(c => c.Type.Equals("isSU")))
            {
                _logger.LogDebug("The token hasn't super user permissions.");
                return false;
            }

            var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals("isSU")).Value);
            if (isSuperUser)
            {
                return true;
            }

            _logger.LogDebug("The token super user permissions is false.");
            return false;
        }
    }
}
