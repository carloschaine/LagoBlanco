using LagoBlanco.Application.Common.DTO.Account;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;


namespace LagoBlanco.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)  => _userService = userService;


        public async Task<IActionResult> Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            RqstRegister registerVM = new() {
                RedirectUrl = returnUrl,
                RoleList = await _userService.RolesForList()
            };
            return View(registerVM);
        }

        public IActionResult Login(string returnUrl=null)
        {
            returnUrl ??= Url.Content("~/"); //Si returnUrl=null no hace nada, sino le asigna Url.Content
            RqstLogin loginVM = new() { RedirectUrl = returnUrl};
            return View(loginVM);
        }


        public async Task<IActionResult> Logout()
        {
            //---
            await _userService.Logout();
            return RedirectToAction("Index", "Home");
            //---
        }

        public IActionResult AccessDenied()
        {
            return View();
        }




        [HttpPost]
        public async Task<IActionResult> Register(RqstRegister registerVM)
        {
            if (ModelState.IsValid) {
                //---
                RspnRegister result = await _userService.Register(registerVM); 
                //---                
                if (result.IsSuccess) {
                    if (!string.IsNullOrEmpty(registerVM.RedirectUrl)) 
                        return LocalRedirect(registerVM.RedirectUrl);                    
                    else 
                        return RedirectToAction("Index", "Home");                    
                }
                //No se pudo crear el usuario
                foreach (var error in result.Errors) {
                    ModelState.AddModelError("", error.Description);
                }
            }
            //ModelState invalido. 
            registerVM.RoleList =await _userService.RolesForList(); 
            //---
            return View(registerVM);
        }


        [HttpPost]
        public async Task<IActionResult> Login(RqstLogin loginDto)
        {
            if (ModelState.IsValid) {
                //---
                var result = await _userService.Login(loginDto);
                //---
                if (result.IsSuccess) {
                    if (result.Role == SD.Role_Admin) {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else if (!string.IsNullOrEmpty(loginDto.RedirectUrl)) {
                        return LocalRedirect(loginDto.RedirectUrl);
                    }
                    else {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            else {
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(loginDto);
        }
    }
}
