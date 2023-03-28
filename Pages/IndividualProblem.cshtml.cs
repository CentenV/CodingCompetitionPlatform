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
            output = "No Output";
        }

        public async Task OnPostAsync() 
        {
            // File Upload: save file into server directory with the naming convention of TEAMID_USERID_PROBLEMNUMBER_TIME.ext
            string identifier, saveFolderPath, savedFileName, fullSavedFilePath;
            try
            {
                output = "Uploading File...";
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");
                //string savePath = Path.Combine(@"C:\Users\Administrator\Documents\TEMP", uploadedFile.FileName);
                // !!!!!! Add Team name to file name
                // Add multiplelanguage support
                identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}";
                savedFileName = $"{identifier}.py";
                saveFolderPath = Path.Combine(PlatformConfig.SUBMISSION_OUTPUT_DIR, identifier);

                // Create Directory
                if (!Directory.Exists(saveFolderPath)) { Directory.CreateDirectory(saveFolderPath); }

                fullSavedFilePath = Path.Combine(saveFolderPath, savedFileName);

                Console.WriteLine($"Saved file name: {savedFileName}");
                Console.WriteLine($"Saved file path: {fullSavedFilePath}");

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

            outputFileName = await CodeSubmit.Execute(savedFileName, saveFolderPath, User.Identity.Name);
            outputFilePath = Path.Combine(saveFolderPath, outputFileName);

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
