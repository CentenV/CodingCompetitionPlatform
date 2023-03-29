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
            string identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}";
            CompetitionFileIOInfo workingSaveFile, destinationFolderPath;
            try
            {
                output = "Uploading File...";
                Console.WriteLine($"Uploaded file: {uploadedFile.FileName}");

                // !!!!!! Add Team name to file name
                // Add multiplelanguage support
                string fileExtension = "py";
                // TEMP
                destinationFolderPath = new CompetitionFileIOInfo($@"{PlatformConfig.SUBMISSION_OUTPUT_DIR}\{identifier}", folder: true);
                workingSaveFile = new CompetitionFileIOInfo($@"{destinationFolderPath.destinationPath}\{identifier}.{fileExtension}");
                workingSaveFile.identifier = identifier;

                // Create Directory
                if (!Directory.Exists(destinationFolderPath.destinationPath)) { Directory.CreateDirectory(destinationFolderPath.destinationPath); }

                Console.WriteLine($"Saved file name: {workingSaveFile.fileName}");
                Console.WriteLine($"Saved file full path: {workingSaveFile.filePath}");

                using (FileStream fileStream = new FileStream(workingSaveFile.filePath, FileMode.Create))
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
            
            List<CompetitionFileIOInfo> caseCodeReady = CodeSubmission.CreateCaseFiles(currentProblem, workingSaveFile, destinationFolderPath, User.Identity.Name);
            Console.WriteLine("test");

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
