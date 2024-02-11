using System.ComponentModel.DataAnnotations;

namespace LagoBlanco.Application.Common.DTO.Account
{
    public class RqstLogin
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
