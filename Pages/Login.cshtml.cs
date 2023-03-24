using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace CodingCompetitionPlatform.Pages
{
    public class LoginModel : PageModel
    {
        private string username;
        private string passphrase;

        public void OnGet()
        {
            //HttpContext.Session.Set("username", Encoding.ASCII.GetBytes("TEST1234"));
        }
    }
}
