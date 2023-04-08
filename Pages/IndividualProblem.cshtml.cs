using CodingCompetitionPlatform.Model;
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
        public string status { get; set; }
        public string? error { get; set; }
        public bool displayCasesStatus { get; set; }

        // Displaying the Run/Test Cases and the Actual/Expected Output
        public List<AECContentPassFail> runCasesAllOutput { get; set; }
        public List<AECContentPassFail> testCasesAllOutput { get; set; }

        // Internal Properties
        private string teamId;
        public List<string> outputMessages = new List<string>();

        private readonly ILogger<IndexModel> _logger;
        private DatabaseContext _databaseContext;
        // Constructor
        public ProblemModel(ILogger<IndexModel> logger, DatabaseContext context)
        {
            _logger = logger;
            _databaseContext = context;
        }


        public void OnGet()
        {
            status = "No file submitted.";
            displayCasesStatus = false;
            teamId = User.FindFirst(ClaimTypes.GroupSid).Value;
        }

        public async Task OnPostAsync()
        {
            teamId = User.FindFirst(ClaimTypes.GroupSid).Value;
            LoadProblems.Initialize();
            var currentProblem = LoadProblems.PROBLEMS[problemIndex - 1];

            // File Upload: save file into server directory with the naming convention of TEAMID_USERID_PROBLEMNUMBER_TIME.ext
            // identifier (i.e. TEAM1234_TEST1234_1_10_16_07PM)
            string identifier = $"{@User.FindFirst(ClaimTypes.GroupSid).Value}_{User.Identity.Name}_{problemIndex}_{DateTime.Now.ToLongTimeString().Replace(":", "_").Replace(" ", "")}";

            // SAMPLE, CHANGE
            ProgrammingLanguage submittedLanguage = ProgrammingLanguage.JavaScript;

            // Read/Create file that user submits
            CompetitionFileIOInfo workingSaveFile, destinationFolderPath;
            try
            {
                status = "Uploading File... ⏳";
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
                status = "File Uploaded. ✔️";
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

            // Create and inject all the run and test cases, get the list of all the paths to those files
            status += "\nPreparing Run/Test Cases... ⏳";
            List<CompetitionFileIOInfo> runcaseCodeReady = CodeSubmission.RunCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);
            List<CompetitionFileIOInfo> testcaseCodeReady = CodeSubmission.TestCaseFiles(currentProblem, workingSaveFile, User.Identity.Name);
            status += "\nRun/Test Cases Created. ✔️";

            // Run the run/test cases
            status += "\nExecuting Code... ⏳";
            Task<List<ActualExpectedCompiler>> runcaseOutputTask = CodeSubmission.ExecuteCases(runcaseCodeReady, User.Identity.Name, currentProblem, CaseType.Run, submittedLanguage);
            Task<List<ActualExpectedCompiler>> testcaseOutputTask = CodeSubmission.ExecuteCases(testcaseCodeReady, User.Identity.Name, currentProblem, CaseType.Test, submittedLanguage);
            await Task.WhenAll(runcaseOutputTask, testcaseOutputTask);
            status += "\nCode Executed. ✔️";

            status += "\nGrading Run/Test Cases... ⏳";
            List<AECContent> runCasesActualExpected = CodeSubmission.GetActualExpectedOutput(runcaseOutputTask.Result);
            List<AECContent> testCasesActualExpected = CodeSubmission.GetActualExpectedOutput(testcaseOutputTask.Result);
            status += "\nRun/Test Cases Graded. ✔️";

            // Pass/Fail cases
            runCasesAllOutput = CodeSubmission.GetPassFailChallenge(runCasesActualExpected);
            testCasesAllOutput = CodeSubmission.GetPassFailChallenge(testCasesActualExpected);

            RewardPoints(currentProblem, runCasesAllOutput, testCasesAllOutput);

            displayCasesStatus = true;      // Enable display output to the user
            Console.WriteLine("\n\n\n\n");
        }

        // Get Problem
        //public IActionResult OnGetProblemOnClick(int problemIndex)
        //{
        //    // Redirect to /index If Problem Solved
        //    var problemInDb = (from p in _databaseContext.ProblemStatuses where p.teamid == teamId && p.problemid == problemIndex select p).FirstOrDefault();
        //    if (problemInDb.problemcompleted) { return RedirectToPage("/index"); }

        //    return Page();
        //}
        // Managing Problem Status
        //public string GetProblemStatus()
        //{
        //    string combinedOutput = "";
        //    foreach ()
        //}



        // CODE SUBMISSION OPERATIONS //

        // Reward points if problem was correctly solved with all test cases
        private void RewardPoints(Problem problem, List<AECContentPassFail> runcasesResult, List<AECContentPassFail> testcasesResult)
        {
            // Get team and update points
            TeamModel? teamInDb = (from t in _databaseContext.Teams where t.teamid == teamId select t).FirstOrDefault();
            ProblemStatusModel? problemInDb = (from p in _databaseContext.ProblemStatuses where p.teamid == teamInDb.teamid && p.problemid == problem.problemIndex select p).FirstOrDefault();
            CodeSubmission.Reward(problem, runcasesResult, testcasesResult, teamInDb, problemInDb);

            _databaseContext.SaveChanges();
        }

        public bool GetCompletionStatus()
        {
            ProblemStatusModel? problemInDb = (from p in _databaseContext.ProblemStatuses where p.teamid == teamId && p.problemid == problemIndex select p).FirstOrDefault();
            if (problemInDb.problemcompleted) { status = "Problem Completed ✔️"; }
            return problemInDb.problemcompleted;
        }
    }
}
