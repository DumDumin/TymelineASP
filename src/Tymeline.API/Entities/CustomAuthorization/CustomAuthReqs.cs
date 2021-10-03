#region Assembly Microsoft.AspNetCore.Authorization, Version=5.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
// Microsoft.AspNetCore.Authorization.dll
#endregion

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using ApiServer.Policies;

namespace Microsoft.AspNetCore.Authorization.Infrastructure
{
    //
    // Summary:
    //     Implements an Microsoft.AspNetCore.Authorization.IAuthorizationHandler and Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
    //     which requires at least one instance of the specified claim type, and, if allowed
    //     values are specified, the claim value must be any of the allowed values.
    public class CustomAuthReqs : AuthorizationHandler<CustomRoleRequirement>
    {

        public CustomAuthReqs(){
            // ClaimType = claimType;
            // AllowedValues = allowedValues;
        }

        //
        // Summary:
        //     Gets the claim type that must be present.
        public string ClaimType { get; }
        //
        // Summary:
        //     Gets the optional list of claim values, which, if present, the claim must match.
        public IEnumerable<string>? AllowedValues { get; }

      
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRoleRequirement requirement){
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

      
    }
}