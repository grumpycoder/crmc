﻿using crmc.data;
using crmc.domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using web.ViewModels;

namespace web.Controllers
{
    public class UsersController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public UsersController()
        {
        }

        public UsersController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        protected ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
            private set { _roleManager = value; }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.Current.GetOwinContext().Authentication; }
        }

        public IHttpActionResult Get()
        {
            var users = UserManager.Users.ToList().Select(u => new UserViewModel()
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Roles = UserManager.GetRolesAsync(u.Id).Result.ToArray()
            });
            return Ok(users);
        }

        public async Task<IHttpActionResult> Post(UserViewModel vm)
        {
            var existing = UserManager.FindByName(vm.UserName);

            if (existing != null)
            {
                return BadRequest("Username already exists");
            }

            var user = new ApplicationUser()
            {
                UserName = vm.UserName,
                Email = vm.Email,
            };

            IdentityResult addUserResult = await UserManager.CreateAsync(user, vm.Password);

            if (!addUserResult.Succeeded)
            {
                return await GetErrorResult(addUserResult);
            }

            IdentityResult addResult = await UserManager.AddToRolesAsync(user.Id, vm.Roles);

            return Ok(vm);
            //context.Persons.AddOrUpdate(person);
            //context.SaveChanges();
            //return Ok(person);
        }

        public async Task<IHttpActionResult> Delete(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                await UserManager.DeleteAsync(user);
            }

            return Ok("User deleted");
        }

        [HttpGet]
        [Route("api/roles")]
        //public async Task<IHttpActionResult> Roles()
        public IHttpActionResult Roles()
        {
            var roles = RoleManager.Roles.ToArray();
            return Ok(roles);
        }

        protected async Task<IHttpActionResult> GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return BadRequest();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }
                return BadRequest();
            }

            return null;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}