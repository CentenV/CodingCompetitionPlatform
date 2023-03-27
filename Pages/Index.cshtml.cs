using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace CodingCompetitionPlatform.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // TEMP CODE ********************!!!!!!!!!!!!!!!
            //HttpContext.Session.Set("username", Encoding.ASCII.GetBytes("TEST1234"));
            //Console.WriteLine(HttpContext.Session.IsAvailable);
            //byte[] userSession = HttpContext.Session.Get("username");
            //name = System.Text.Encoding.Default.GetString((userSession == null) ? new byte[0] : userSession);
            //***********************************
        }
    }
}