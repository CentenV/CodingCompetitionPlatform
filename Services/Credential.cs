using System.ComponentModel.DataAnnotations;

namespace CodingCompetitionPlatform.Services
{
    public class Credential
    {
        public static readonly string COOKIE_NAME = "competitoruserauthentication";

        [Required]
        public string competitorId { get; set; }
        [Required]
        public string passphrase { get; set; }
    }
}
