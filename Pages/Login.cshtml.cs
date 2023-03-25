using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;

namespace CodingCompetitionPlatform.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; }

        public void OnGet()
        {
            //HttpContext.Session.Set("username", Encoding.ASCII.GetBytes("TEST1234"));
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // MODIFY TO CHECK WITH DATABASE
            if (Credential.competitorId == "TEST1234" && Credential.passphrase == "t3st1ng")
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.NameIdentifier, "TEST1234"),
                };
                var identity = new ClaimsIdentity(claims, Credential.COOKIE_NAME);
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(Credential.COOKIE_NAME, claimsPrincipal);

                // INCLUDE IN REAL IMPLEMENTATION

                return RedirectToPage("/index");
            }

            return Page();
        }
    }
}
