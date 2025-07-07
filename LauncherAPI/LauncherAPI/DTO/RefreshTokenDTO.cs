using System.ComponentModel.DataAnnotations;

namespace JWTToken.Model
{
    public class RefreshTokenDTO
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
