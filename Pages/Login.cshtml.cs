using CodingCompetitionPlatform.Model;
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


        private readonly ILogger<IndexModel> _logger;
        private DatabaseContext _databaseContext;
        public LoginModel(ILogger<IndexModel> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }

        public void OnGet()
        {
            
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }

            Competitor c = GetCompetitor(Credential.competitorId, Credential.passphrase);
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // MODIFY TO CHECK WITH DATABASE
            //if (Credential.competitorId == "TEST1234" && Credential.passphrase == "t3st1ng")
            if (c != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, c.competitorID),
                    new Claim(ClaimTypes.GroupSid, c.team.teamId),
                };
                var identity = new ClaimsIdentity(claims, Credential.COOKIE_NAME);
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(Credential.COOKIE_NAME, claimsPrincipal);

                // INCLUDE IN REAL IMPLEMENTATION

                return RedirectToPage("/index");
            }

            return Page();
        }

        private Competitor GetCompetitor(string competitorId, string passphrase)
        {
            //var foundCompetitor = _databaseContext.competitors.Where(c => c.co
            //mpetitorID == competitorId && c.team.passphrase == passphrase).FirstOrDefault();
            
            var team = _databaseContext.teams.Where(t => t.passphrase == passphrase).FirstOrDefault();

            Competitor foundCompetitor = null;
            if (team != null)
            {
                foundCompetitor = _databaseContext.competitors.Where(c => c.competitorID == competitorId && c.teamID == team.teamId).FirstOrDefault();
                if (foundCompetitor != null)
                {
                    foundCompetitor.team = team;
                }
            }

            return foundCompetitor;
        }
    }
}
