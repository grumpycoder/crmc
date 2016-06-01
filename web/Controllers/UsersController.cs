using crmc.domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using web.ViewModels;

namespace web.Controllers
{
    [RoutePrefix("api/users")]
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

        public IHttpActionResult Get(string searchTerm)
        {
            var users = UserManager.Users.Where(x => x.UserName.Contains(searchTerm) || x.FullName.Contains(searchTerm)).ToList().Select(u => new UserViewModel()
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
                FullName = vm.FullName
            };

            IdentityResult addUserResult = await UserManager.CreateAsync(user, vm.Password);

            if (!addUserResult.Succeeded)
            {
                return await GetErrorResult(addUserResult);
            }

            IdentityResult addResult = await UserManager.AddToRolesAsync(user.Id, vm.Roles);
            //TODO: Need automapping
            vm.Id = user.Id;
            return Ok(vm);
        }

        public async Task<IHttpActionResult> Put([FromBody]UserViewModel vm)
        {
            //TODO: Automapper model to viewmodel
            var user = await UserManager.FindByIdAsync(vm.Id);

            var currentRoles = await UserManager.GetRolesAsync(vm.Id);
            var rolesNotExists = vm.Roles.Except(RoleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Any())
            {
                ModelState.AddModelError("", $"Roles '{string.Join(",", rolesNotExists)}' does not exixts in the system");
                return BadRequest(ModelState);
            }
            var removeResult = await UserManager.RemoveFromRolesAsync(vm.Id, currentRoles.ToArray());

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest(ModelState);
            }
            var addResult = await UserManager.AddToRolesAsync(vm.Id, vm.Roles);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return BadRequest(ModelState);
            }

            user.FullName = vm.FullName;
            user.Email = vm.Email;
            await UserManager.UpdateAsync(user);

            return Ok(vm);
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

        [HttpPost, Route("UploadAvatar/{id}")]
        public async Task<IHttpActionResult> UploadAvatar(string id)
        {
            var request = HttpContext.Current.Request;

            if (request.Files.Count == 0)
            {
                return BadRequest();
            }

            var postedFile = request.Files[0];

            var filename = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\") + 1);

            try
            {
                var filePath = HttpContext.Current.Server.MapPath(@"~\app_data\" + filename);
                postedFile.SaveAs(filePath);
                var file = new FileInfo(filePath);
                var destPath = HttpContext.Current.Server.MapPath(@"~\images\users\" + id + file.Extension);
                file.MoveTo(destPath);
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok("Successfully saved avatar");
        }

        [HttpGet]
        [Route("roles")]
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