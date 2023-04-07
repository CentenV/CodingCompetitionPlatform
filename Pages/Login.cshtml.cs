using CodingCompetitionPlatform.Model;
using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace CodingCompetitionPlatform.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential { get; set; }


        private readonly ILogger<IndexModel> _logger;
        private DatabaseContext _databaseContext;
        public LoginModel(ILogger<IndexModel> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //if (ClaimsPrincipal.Current?.Identities?.FirstOrDefault()?.Name != null)
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/index");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            CompetitorModel c = GetCompetitor(Credential.competitorId, Credential.passphrase);
            if (c != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, c.competitorID),
                    new Claim(ClaimTypes.GroupSid, c.teamid),
                };
                var identity = new ClaimsIdentity(claims, Credential.COOKIE_NAME);
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(Credential.COOKIE_NAME, claimsPrincipal);

                return RedirectToPage("/index");
            }
            else
            {
                return Page();
            }
        }

        private CompetitorModel GetCompetitor(string competitorId, string passphrase)
        {
            var foundCompetitor = (from c in _databaseContext.Competitors join t in _databaseContext.Teams on c.teamid equals t.teamid where c.competitorID == competitorId && t.passphrase == passphrase select c).Include(c => c.team).FirstOrDefault();

            return foundCompetitor;
        }
    }
}
