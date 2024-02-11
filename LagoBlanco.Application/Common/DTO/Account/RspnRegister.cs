using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.DTO.Account
{
    public class RspnRegister
    {
        public bool IsSuccess  { get; set; }=false;
        public IEnumerable<IdentityError> Errors { get; set; }
    }
}
