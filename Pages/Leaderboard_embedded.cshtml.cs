using CodingCompetitionPlatform.Model;
using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingCompetitionPlatform.Pages
{
    public class Leaderboard_embeddedModel : PageModel
    {
        // Internal Properties
        private string teamId;
        public List<string> outputMessages = new List<string>();

        private readonly ILogger<IndexModel> _logger;
        private DatabaseContext _databaseContext;
        // Constructor
        public Leaderboard_embeddedModel (ILogger<IndexModel> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }


        public void OnGet()
        {
        }


        public List<TeamModel> GetTop5Teams()
        {
            return Leaderboard.GetTop(5, _databaseContext);
        }
    }
}
