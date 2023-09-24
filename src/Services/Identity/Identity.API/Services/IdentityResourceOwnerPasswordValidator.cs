using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.API.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Services
{
    public class IdentityResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByEmailAsync(context.UserName);

            if (user is null)
            {
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email or password is wrong" });
                context.Result.CustomResponse = errors;

                return;
            }
            
            var passwordCheck = await _userManager.CheckPasswordAsync(user, context.Password);
            
            if (!passwordCheck)
            {
                var errors = new Dictionary<string, object>();
                errors.Add("errors", new List<string> { "Email or password is wrong" });
                context.Result.CustomResponse = errors;

                return;
            }
            
            context.Result = new GrantValidationResult(user.Id.ToString(), "password");
        }
    }
}