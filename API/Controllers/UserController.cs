using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Data.Repository.IRepository;
using Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Utility;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppSettings _appSettings;
        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager,
            IOptions<AppSettings> appSettings)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _appSettings = appSettings.Value;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new { data = _unitOfWork.ApplicationUser.GetAll() });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(100);
            }
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation successful." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]InputModel Input)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                PhoneNumber = Input.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!await _roleManager.RoleExistsAsync(SD.ManagerRole))
            {
                _roleManager.CreateAsync(new IdentityRole(SD.ManagerRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.FontDeskRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.KitchenRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.CustomerRole)).GetAwaiter().GetResult();
            }
            if (result.Succeeded)
            {
                if (Input.Role == SD.KitchenRole)
                {
                    await _userManager.AddToRoleAsync(user, SD.KitchenRole);
                }
                else
                {
                    if (Input.Role == SD.ManagerRole)
                    {
                        await _userManager.AddToRoleAsync(user, SD.ManagerRole);
                    }
                    else
                    {
                        if (Input.Role == SD.FontDeskRole)
                        {
                            await _userManager.AddToRoleAsync(user, SD.FontDeskRole);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, SD.CustomerRole);
                        }
                    }
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name,user.Id.ToString())
                }),
                    Expires = DateTime.UtcNow.AddDays(3),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.PhoneNumber,
                    user.Email,
                    token = tokenString
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    Message = result.Errors.ToArray()[0].Description
                });
            }

        }
    }
}