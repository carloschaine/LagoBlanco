using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.DTO.Account
{
    public class RspnLogin
    {
        public bool IsSuccess  { get; set; }=false;
        public string Role { get; set; } = string.Empty;
}
}
