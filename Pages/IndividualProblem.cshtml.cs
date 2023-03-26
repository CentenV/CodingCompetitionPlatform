using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CodingCompetitionPlatform.Pages
{
    [Authorize]
    public class ProblemModel : PageModel
    {
        // Query String
        [FromQuery(Name = "problemIndex")]
        public int problemIndex { get; set; }

        [BindProperty]
        public IFormFile? uploadedFile { get; set; }
        public string? error { get; set; }
        
        public void OnGet()
        {
        }

        public void OnPost() 
        {
            string savePath;
            // File Upload
            try
            {
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");
                //string savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);
                // !!!!!! Add Team name to file name
                savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", $"{User.Identity.Name}_Problem{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}.py");
                Console.WriteLine($"Saved file: {savePath}");

                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
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
                Console.WriteLine(ex);
                error = ex.GetType().ToString();
                return;
            }

            // Execute File
            CodeSubmit submissionInstance = new CodeSubmit(savePath);
            submissionInstance.execute();
        }

        public IActionResult OnGetProblemOnClick(int problemIndex)
        {
            return Page();
        }
    }
}
