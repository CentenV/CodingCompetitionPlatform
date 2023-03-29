using CodingCompetitionPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
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
            output = "No file submitted.";
        }

        public async Task OnPostAsync()
        {
            var currentProblem = LoadProblems.PROBLEMS[problemIndex - 1];

            // File Upload: save file into server directory with the naming convention of TEAMID_USERID_PROBLEMNUMBER_TIME.ext
            // identifier (i.e. TEAM1234_TEST1234_1_10-16-07PM)
            string identifier = identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}";
            string destinationFolderPath, savedFileName, fullSavedFilePath;
            try
            {
                output = "Uploading File...";
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");
                //string savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);
                // !!!!!! Add Team name to file name
                // Add multiplelanguage support
                savedFileName = $"{identifier}.py";
                destinationFolderPath = Path.Combine(PlatformConfig.SUBMISSION_OUTPUT_DIR, identifier);

                // Create Directory
                if (!Directory.Exists(destinationFolderPath)) { Directory.CreateDirectory(destinationFolderPath); }

                fullSavedFilePath = Path.Combine(destinationFolderPath, savedFileName);

                Console.WriteLine($"Saved file name: {savedFileName}");
                Console.WriteLine($"Saved file full path: {fullSavedFilePath}");

                using (FileStream fileStream = new FileStream(fullSavedFilePath, FileMode.Create))
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
            output += "\nExecuting code...";
            string outputFileName, outputFilePath;

            // Assemble List of Input Test Case File Names and Expected Outputs
            
            CodeSubmit.Submit(currentProblem, identifier, savedFileName, fullSavedFilePath, destinationFolderPath, User.Identity.Name);

            //////////////////
            //outputFileName = await CodeSubmit.Execute(savedFileName, saveFolderPath, User.Identity.Name);
            //outputFilePath = Path.Combine(saveFolderPath, outputFileName);

            //output = LoadProblems.ReadFile(outputFilePath);
            /////////////////
            Console.WriteLine("\n\n\n\n");
        }

        public IActionResult OnGetProblemOnClick(int problemIndex)
        {
            return Page();
        }
    }
}
