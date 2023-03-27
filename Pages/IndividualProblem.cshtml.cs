using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
        public string output { get; set; }
        public string? error { get; set; }
        
        public void OnGet()
        {
        }

        public async Task OnPostAsync() 
        {
            // File Upload: save file into server directory with the naming convention of TEAMID_USERID_PROBLEMNUMBER_TIME.ext
            string saveFileName, saveFilePath;
            try
            {
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");
                //string savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);
                // !!!!!! Add Team name to file name
                // Add multiplelanguage support
                saveFileName = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}.py";
                saveFilePath = Path.Combine(PlatformConfig.SUBMISSION_OUTPUT_DIR, saveFileName);

                Console.WriteLine($"Saved file name: {saveFileName}");
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


            // Execute File and Read Output Back Out
            string outputFileName, outputFilePath;

            outputFileName = await CodeSubmit.Execute(saveFileName, User.Identity.Name);
            outputFilePath = Path.Combine(PlatformConfig.SUBMISSION_OUTPUT_DIR, outputFileName);

            using (StreamReader sr = new StreamReader(outputFilePath)) 
            {
                output = sr.ReadToEnd();
            }

            Console.WriteLine("\n\n\n\n");
        }

        public IActionResult OnGetProblemOnClick(int problemIndex)
        {
            return Page();
        }
    }
}
