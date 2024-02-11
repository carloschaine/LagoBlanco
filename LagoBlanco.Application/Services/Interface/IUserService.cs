using LagoBlanco.Application.Common.DTO.Account;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Interface
{
    public interface IUserService
    {
        Task<RspnRegister> Register(RqstRegister registerDto);
        Task<RspnLogin> Login(RqstLogin loginDto);
        Task Logout();

        Task<IEnumerable<SelectListItem>> RolesForList(); 
    }
}
