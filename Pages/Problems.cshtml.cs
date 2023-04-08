using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodingCompetitionPlatform.Pages
{
    [Authorize]
    public class ProblemsModel : PageModel
    {
        // Internal Properties
        private string teamId;
        private readonly ILogger<IndexModel> _logger;
        private DatabaseContext _databaseContext;
        // Model Constructor
        public ProblemsModel(ILogger<IndexModel> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }

        public void OnGet()
        {
            teamId = User.FindFirst(ClaimTypes.GroupSid).Value;
        }

        public bool GetCompletionStatus(Problem problem)
        {
            var problemInDb = (from p in _databaseContext.ProblemStatuses where p.teamid == teamId && p.problemid == problem.problemIndex select p).Include(p => p.team).FirstOrDefault();
            return problemInDb.problemcompleted;
        }
    }
}
