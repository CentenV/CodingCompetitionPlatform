using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodingCompetitionPlatform.Pages
{
    [Authorize]
    public class ProblemsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
