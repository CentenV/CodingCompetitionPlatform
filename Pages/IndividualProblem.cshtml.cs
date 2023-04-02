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
        public string status { get; set; }
        public string? error { get; set; }
        public bool displayCasesStatus { get; set; }

        // Displaying the Run/Test Cases and the Actual/Expected Output
        public Dictionary<KeyValuePair<string, string>, bool> runCasesAllOutput { get; set; }
        public Dictionary<KeyValuePair<string, string>, bool> testCasesAllOutput { get; set; }


        public void OnGet()
        {
            status = "No file submitted.";
            displayCasesStatus = false;
        }

        public async Task OnPostAsync()
        {
            LoadProblems.Initialize();
            var currentProblem = LoadProblems.PROBLEMS[problemIndex - 1];

            // File Upload: save file into server directory with the naming convention of TEAMID_USERID_PROBLEMNUMBER_TIME.ext
            // identifier (i.e. TEAM1234_TEST1234_1_10-16-07PM)
            string identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "-").Replace(" ", "")}";
            CompetitionFileIOInfo workingSaveFile, destinationFolderPath;
            try
            {
                status = "Uploading File... ";
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
                status = "\nNo File Uploaded. ❌";

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                error = ex.GetType().ToString();
                status += "File Upload Failed. ❌";
                return;
            }


            // Execute File and Read Output Back Out
            status += "\nExecuting code... ";
            
            // Create and inject all the run and test cases, get the list of all the paths to those files
            List<CompetitionFileIOInfo> runcaseCodeReady = CodeSubmission.RunCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);
            List<CompetitionFileIOInfo> testcaseCodeReady = CodeSubmission.TestCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);
            var runcaseOutput = await CodeSubmission.ExecuteCases(runcaseCodeReady, currentProblem, CaseType.Run, User.Identity.Name);
            var testcaseOutput = await CodeSubmission.ExecuteCases(testcaseCodeReady, currentProblem, CaseType.Test, User.Identity.Name);

            var runCasesActualExpected = CodeSubmission.GetActualExpectedOutput(runcaseOutput);
            var testCasesActualExpected = CodeSubmission.GetActualExpectedOutput(testcaseOutput);

            runCasesAllOutput = CodeSubmission.GetPassFailChallenge(runCasesActualExpected);
            testCasesAllOutput = CodeSubmission.GetPassFailChallenge(testCasesActualExpected);

            displayCasesStatus = true;


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
