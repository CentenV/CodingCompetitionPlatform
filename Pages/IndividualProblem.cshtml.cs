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
            // identifier (i.e. TEAM1234_TEST1234_1_10_16_07PM)
            string identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "_").Replace(" ", "")}";

            // SAMPLE, CHANGE
            ProgrammingLanguage submittedLanguage = ProgrammingLanguage.Java;

            CompetitionFileIOInfo workingSaveFile, destinationFolderPath;
            try
            {
                status = "Uploading File... ";
                Console.WriteLine($"Uploaded file: {uploadedFile?.FileName}");

                // Configure output file name and folder in the server save directory
                string fileExtension = SubmittedLanguage.GetFileExtension(submittedLanguage);
                destinationFolderPath = new CompetitionFileIOInfo($@"{PlatformConfig.SUBMISSION_OUTPUT_DIR}\{identifier}", folder: true);
                workingSaveFile = new CompetitionFileIOInfo($@"{destinationFolderPath.destinationPath}\{identifier}.{fileExtension}");
                workingSaveFile.identifier = identifier;

                // Create directory
                if (!Directory.Exists(destinationFolderPath.destinationPath)) { Directory.CreateDirectory(destinationFolderPath.destinationPath); }

                Console.WriteLine($"Saved file name: {workingSaveFile.fileName}");
                Console.WriteLine($"Saved file full path: {workingSaveFile.filePath}");

                // Save file to server
                using (FileStream fileStream = new FileStream(workingSaveFile.filePath, FileMode.Create))
                {
                    uploadedFile.CopyTo(fileStream);
                }

                // Handle Java upload
                if (submittedLanguage == ProgrammingLanguage.Java)
                {
                    SubmittedLanguage.HandleJavaClassName(workingSaveFile, uploadedFile.FileName);
                }
            }
            catch (NullReferenceException)
            {
                status = "No File Uploaded. ❌";

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                error = ex.GetType().ToString();
                status += "\nFile Upload Failed. ❌";
                return;
            }


            // Execute File and Read Output Back Out
            status += "\nExecuting Code... ";
            
            // Create and inject all the run and test cases, get the list of all the paths to those files
            List<CompetitionFileIOInfo> runcaseCodeReady = CodeSubmission.RunCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);
            List<CompetitionFileIOInfo> testcaseCodeReady = CodeSubmission.TestCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);

            // Run the run/test cases
            var runcaseOutput = CodeSubmission.ExecuteCases(runcaseCodeReady, User.Identity.Name, currentProblem, CaseType.Run, submittedLanguage);
            var testcaseOutput = CodeSubmission.ExecuteCases(testcaseCodeReady, User.Identity.Name, currentProblem, CaseType.Test, submittedLanguage);
            await Task.WhenAll(runcaseOutput, testcaseOutput);

            var runCasesActualExpected = CodeSubmission.GetActualExpectedOutput(runcaseOutput.Result);
            var testCasesActualExpected = CodeSubmission.GetActualExpectedOutput(testcaseOutput.Result);

            runCasesAllOutput = CodeSubmission.GetPassFailChallenge(runCasesActualExpected);
            testCasesAllOutput = CodeSubmission.GetPassFailChallenge(testCasesActualExpected);

            displayCasesStatus = true;      // Enable display output to the user
            Console.WriteLine("\n\n\n\n");
        }

        public IActionResult OnGetProblemOnClick(int problemIndex)
        {
            return Page();
        }
    }
}
