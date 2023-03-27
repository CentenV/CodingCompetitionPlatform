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
            string saveFileName, saveFilePath;
            // File Upload
            try
            {
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");
                //string savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);
                // !!!!!! Add Team name to file name
                // Add multiplelanguage support
                saveFileName = $"{User.Identity.Name}_Problem{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}.py";
                Console.WriteLine($"Saved file name: {saveFileName}");
                saveFilePath = Path.Combine(PlatformConfig.SUBMISSION_OUTPUT_DIR, saveFileName);
                Console.WriteLine($"Saved file path: {saveFilePath}");

                using (FileStream fileStream = new FileStream(saveFilePath, FileMode.Create))
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
            CodeSubmit submissionInstance = new CodeSubmit(saveFileName);
            submissionInstance.Execute(User.Identity.Name);
        }

        public IActionResult OnGetProblemOnClick(int problemIndex)
        {
            return Page();
        }
    }
}
