using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CodingCompetitionPlatform.Pages
{
    public class ProblemModel : PageModel
    {
        [FromQuery(Name = "problemIndex")]      // Query string!!!!!!
        public int problemIndex { get; set; }
        [BindProperty]
        public IFormFile? uploadedFile { get; set; }
        public string? error { get; set; }
        
        public void OnGet()
        {

        }

        public void OnPost() 
        {
            // File Upload
            try
            {
                string saveFolder = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);

                using (FileStream fileStream = new FileStream(saveFolder, FileMode.Create))
                {
                    uploadedFile.CopyTo(fileStream);
                }
            }
            catch (NullReferenceException) 
            {
                error = "No File Uploaded.";
                return;
            }
            catch (Exception ex)
            {
                error = ex.GetType().ToString();
                return;
            }


        }
    }
}
