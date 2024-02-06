using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LagoBlanco.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(IUnitOfWork repo,
                                 UserManager<ApplicationUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _repo = repo; 
            _userManager = userManager; _roleManager = roleManager; _signInManager = signInManager;
        }


        public IActionResult Login(string returnUrl=null)
        {
            returnUrl ??= Url.Content("~/"); //Si returnUrl=null no hace nada, sino le asigna Url.Content
            LoginVM loginVM = new() {
                RedirectUrl = returnUrl
            };

            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            //---
            await _signInManager.SignOutAsync();
            //---
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }




        public async Task<IActionResult> Register(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult()) {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }

            returnUrl ??= Url.Content("~/");
            RegisterVM registerVM = new() {
                 RedirectUrl = returnUrl, 
                 RoleList = _roleManager.Roles
                               .Select(r => new SelectListItem {Text=r.Name, Value=r.Name })
            };
            return View(registerVM);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (ModelState.IsValid) {
                ApplicationUser user = new() {
                    Email = registerVM.Email,
                    NormalizedEmail = registerVM.Email.ToUpper(),
                    UserName = registerVM.Email,
                    EmailConfirmed = true,
                    PhoneNumber = registerVM.PhoneNumber,
                    Name = registerVM.Name,
                    CreatedAt = DateTime.Now
                };

                //--- Insert en AspNetUsers
                var result = await _userManager.CreateAsync(user, registerVM.Password);
                //---
                if (result.Succeeded) {
                    //Insert en AspNetUserRoles. 
                    string userRole = string.IsNullOrEmpty(registerVM.Role) ? SD.Role_Customer : registerVM.Role;
                    await _userManager.AddToRoleAsync(user, userRole);                    

                    //--- Me Logeo. No paso Password, pq esta OK para entrar ya. 
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    //---
                    if (string.IsNullOrEmpty(registerVM.RedirectUrl)) 
                        return RedirectToAction("Index", "Home");                    
                    else 
                        return LocalRedirect(registerVM.RedirectUrl);                    
                }

                //No se pudo crear el usuario
                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                }
            }
            //ModelState invalido. 
            registerVM.RoleList = _roleManager.Roles.Select(x => new SelectListItem {Text=x.Name, Value=x.Name});
            return View(registerVM);
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (ModelState.IsValid) {
                //---
                var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, 
                                                                      loginVM.RememberMe, false);
                //---
                if (result.Succeeded) {
                    var user = await _userManager.FindByEmailAsync(loginVM.Email);
                    if (await _userManager.IsInRoleAsync(user, SD.Role_Admin)) {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else{   
                        if (string.IsNullOrEmpty(loginVM.RedirectUrl)) 
                            return RedirectToAction("Index", "Home");
                        return LocalRedirect(loginVM.RedirectUrl);                        
                    }
                }
                else {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(loginVM);
        }


    }
}
