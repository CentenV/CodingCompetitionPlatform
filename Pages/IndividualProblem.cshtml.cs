using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CodingCompetitionPlatform.Pages
{
    public class ProblemModel : PageModel
    {
        [BindProperty]
        public IFormFile? uploadedFile { get; set; }
        
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            // File Upload
            Console.WriteLine(uploadedFile.FileName);
            string saveFolder = Path.Combine(@"C:\Users\vincent\Documents\TEMP", uploadedFile.FileName);
            using (FileStream fileStream = new FileStream(saveFolder, FileMode.Create))
            {
                uploadedFile.CopyTo(fileStream);
            }
        }
    }
}
