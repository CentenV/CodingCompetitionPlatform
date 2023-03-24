using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace CodingCompetitionPlatform.Pages
{
    public class ProblemModel : PageModel
    {
        [BindProperty]
        public IFormFile? uploadedFile { get; set; }
        public string error { get; set; }
        
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
            catch (NullReferenceException nullex) 
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
