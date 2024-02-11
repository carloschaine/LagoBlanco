using LagoBlanco.Application.Common.DTO.Account;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public UserService(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager; _roleManager = roleManager; _signInManager = signInManager;
        }

        public async Task<RspnRegister> Register(RqstRegister rqstRegister)
        {
           

            //--- INSERT User
            ApplicationUser user = new() {
                Email = rqstRegister.Email,
                NormalizedEmail = rqstRegister.Email.ToUpper(),
                UserName = rqstRegister.Email,
                EmailConfirmed = true,
                PhoneNumber = rqstRegister.PhoneNumber,
                Name = rqstRegister.Name,
                CreatedAt = DateTime.Now
            };

            //--- Insert en AspNetUsers
            var result = await _userManager.CreateAsync(user, rqstRegister.Password);
            //---
            if (result.Succeeded) {
                //Insert en AspNetUserRoles. 
                string userRole = string.IsNullOrEmpty(rqstRegister.Role) ?
                                  SD.Role_Customer : rqstRegister.Role;
                await _userManager.AddToRoleAsync(user, userRole);

                //--- Me Logeo. No paso Password, pq esta OK para entrar ya. 
                await _signInManager.SignInAsync(user, isPersistent: false);
                //---
                return new RspnRegister() { IsSuccess = true };
            }
            return new RspnRegister() { IsSuccess = false, Errors = result.Errors };
        }

        public async Task<RspnLogin> Login(RqstLogin loginDto)
        {
            //---
            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password,
                                                                   loginDto.RememberMe, false);
            //---
            if (result.Succeeded) {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                //---
                return new RspnLogin() {
                    IsSuccess = true,
                    Role = (await _userManager.IsInRoleAsync(user, SD.Role_Admin)) ?
                            SD.Role_Admin : SD.Role_Customer
                };
            }
            return new RspnLogin();
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }        


        public async Task<IEnumerable<SelectListItem>> RolesForList()
        {  
           //---Compruebo que esten cargados los Roles (SEED). 
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult()) {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }
            //---Text-Value
            return _roleManager.Roles.Select(x => new SelectListItem 
                                                      { Text = x.Name, Value = x.Name }
                                            );
        }
    }
}
