using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedBook.Core.Constants;
using Services.Abstraction;

namespace WebAPI.Controllers
{
    /// <summary>
    /// User Module
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userServices) : ControllerBase
    {
        private readonly IUserService _userServices = userServices;

        /// <summary>
        /// Permanantly delete user information (system admin user only)
        /// </summary>
        /// <param>Application Id or unique identifier which is the primary key of the application</param>
        [HttpPost]
        [Route("/api/Login")]
        public async Task<IActionResult> Login(LogIn userModel)
            => Ok(new { Response = await _userServices.Login(userModel) });

        /// <summary>
        /// Update own user information
        /// </summary>
        /// <param name="user">User details object<see cref="UserModel"/>Updated User Data</param>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(UserModel user)
        {
            var userData = await _userServices.UpdateInfoAsync(user);
            if (userData != null)
                return new OkObjectResult(new { Response = userData });
            else
                return new ConflictObjectResult(new { Response = CommonConstants.HttpResponseMessages.Exception });
        }

        /// <summary>
        /// Permanantly delete user information (system admin user only)
        /// </summary>
        /// <param>Application Id or unique identifier which is the primary key of the application</param>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            await _userServices.DeleteAccount();
            return Ok();
        }

        /// <summary>
        /// Reset own or user password (system admin user only)
        /// </summary>
        [HttpPut]
        [Authorize]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword()
        {
            await _userServices.ResetPassword();
            return Ok();
        }
    }
}
