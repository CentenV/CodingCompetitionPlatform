using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingCompetitionPlatform.Pages
{
    public class testpageModel : PageModel
    {
        public string name { get; set; }
        public void OnGet()
        {
            Console.WriteLine(HttpContext.Session.IsAvailable);
            byte[] userSession = HttpContext.Session.Get("username");
            name = System.Text.Encoding.Default.GetString((userSession == null) ? new byte[0] : userSession);
        }
    }
}
