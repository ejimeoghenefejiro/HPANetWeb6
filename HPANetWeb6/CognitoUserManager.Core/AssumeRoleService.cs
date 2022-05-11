using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CognitoUserManager.Contracts
{
    public class AssumeRoleService //: IAssumeRoleService
    {
        private readonly IHttpContextAccessor _accessor;
        private const string KEY_COGNITO_USERID = "cognito:username";
        private const string KEY_COGNITO_PREFERRED_ROLE = "cognito:preferred_role";
        private const string KEY_COGNITO_ROLES = "cognito:roles";

        public AssumeRoleService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        // Get the User ObjectId from Claims
        private string GetRoleSessionName(IEnumerable<Claim> claims)
        {
            var roleSessionNameClaims = claims.FirstOrDefault(x =>
                      x.Type == KEY_COGNITO_USERID);
            if (roleSessionNameClaims != null)
            {
                return roleSessionNameClaims.Value;
            }
            return string.Empty;
        }

        // Get Role from Claims
        private string GetRole(IEnumerable<Claim> claims)
        {
            var preferredRoleClaims = claims.FirstOrDefault(x =>
                    x.Type == KEY_COGNITO_PREFERRED_ROLE);
            if (preferredRoleClaims != null)
            {
                return preferredRoleClaims.Value;
            }
            else
            {
                var roleClaims = claims.Where(x => x.Type == KEY_COGNITO_ROLES);
                if (roleClaims != null && roleClaims.Any())
                {
                    return roleClaims.First().Value;
                }
            }
            return string.Empty;
        }

        public async Task<Credentials> GetAssumedRoleCredentialsAsync()
        {
            var role = GetRole(_accessor.HttpContext.User.Claims);

            var stsClient = new AmazonSecurityTokenServiceClient();
            var assumeRoleReq = new AssumeRoleRequest()
            {
                DurationSeconds = 3600,
                RoleArn = role,
                RoleSessionName = GetRoleSessionName(_accessor.HttpContext.User.Claims),
            };

            AssumeRoleResponse response = await stsClient.AssumeRoleAsync(assumeRoleReq);

            if (response == null)
            {
                return null;
            }

            // returns Credentials Object
            // which contains the AccessKey and AccessSecret
            return response.Credentials;
        }

       
    }
}
