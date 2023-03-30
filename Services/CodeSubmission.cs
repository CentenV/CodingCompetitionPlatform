using System.Diagnostics;

// CODE SUBMIT //
// Class for submitting the code
namespace CodingCompetitionPlatform.Services
{
    public enum ProgrammingLanguage
    {
        Python,
        Java,
        JavaScript,
        C,
        CPP,
    }
    public class CodeSubmission
    {
        // Creating Case Files (injecting all testing and run cases), Returns the list of all the injected files
        // (problem context, inputted user file and its respective data, userid)
        public static List<CompetitionFileIOInfo> CreateCaseFiles(Problem problem, CompetitionFileIOInfo userUploadedCode, string userId)
        {
            // All needed paths and the directory with the challenge resources
            CompetitionFileIOInfo inputCasesPath = new CompetitionFileIOInfo(@$"{PlatformConfig.INPUTCASES_DIR}\{problem.problemIndex}", folder: true);
            string expectedOutputsPath = $@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}";

            // List of all the code ready to be executed
            List<CompetitionFileIOInfo> codeReadyToBeExecuted = new List<CompetitionFileIOInfo>();

            // Getting a list of paths to the run cases and test cases files and injecting the run and test case into the user submitted code
            for (int i = 0; i < problem.runCases; i++)      // Run Cases
            {
                // Inject code into the user's uploaded code
                string runcaseFileName = $"{problem.problemIndex}_runcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{runcaseFileName}";
                CompetitionFileIOInfo combinedUserRuncaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{userUploadedCode.identifier}_runcase{i}.{userUploadedCode.fileExtension}");
                Console.WriteLine(runcaseFileName);
                InjectCaseCode(userUploadedCode.filePath, injectorFilePath, combinedUserRuncaseCode.filePath);
                codeReadyToBeExecuted.Add(combinedUserRuncaseCode);
            }
            for (int i = 0; i < problem.testCases; i++)     // Test Cases
            {
                // Inject code into the user's uploaded code
                string testcaseFileName = $"{problem.problemIndex}_testcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{testcaseFileName}";
                CompetitionFileIOInfo combinedUserTestcaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{userUploadedCode.identifier}_testcase{i}.{userUploadedCode.fileExtension}");
                Console.WriteLine(testcaseFileName);
                InjectCaseCode(userUploadedCode.filePath, injectorFilePath, combinedUserTestcaseCode.filePath);
                codeReadyToBeExecuted.Add(combinedUserTestcaseCode);
            }

            Console.WriteLine(codeReadyToBeExecuted.Count);
            return codeReadyToBeExecuted;
        }

        // Executing All Code Cases (running all testing and run cases), Returns 
        public static async Task ExecuteAllCases(List<CompetitionFileIOInfo> cases, string userId)
        {
            List<Task<string>> executionTasks = new List<Task<string>>();
            foreach (CompetitionFileIOInfo file in cases)
            {
                executionTasks.Add(Execute(file, userId));
            }

            await Task.WhenAll(executionTasks);

            Console.WriteLine(executionTasks.Count);
        }



        // Function for Executing Code in a Docker Container
        private static async Task<string> Execute(CompetitionFileIOInfo inputFile, string userId)
        {
            // Output File Name
            string outputFileName = $"_{inputFile.fileName}_OUTPUT.txt";

            // Name of the Docker Image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = "python:" + "test";
            Console.WriteLine("\n" + dockerImageName);

            string buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\python.dockerfile --build-arg input_file_name={inputFile.fileName}";
            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {outputFileName} 2>&1";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            ExecuteCommand(buildDockerImageCmd, inputFile.fileDirectory);
            ExecuteCommand(runDockerImageCmd, inputFile.fileDirectory);
            ExecuteCommand(cleanupDockerImageCmd, inputFile.fileDirectory);

            Console.WriteLine($"\nExecuted {inputFile.fileName}");

            return outputFileName;
        }


        // Internal Methods
        private static void ExecuteCommand(string command, string workingDir)
        {
            ProcessStartInfo ps = new ProcessStartInfo(@"C:\Windows\System32\cmd.exe", "/c " + command);
            ps.WorkingDirectory = workingDir;
            ps.UseShellExecute = false;
            Console.WriteLine(command);
            
            Process.Start(ps).WaitForExit();
        }
        private static void InjectCaseCode(string receivingFilePath, string injectFilePath, string destinationFile)
        {
            // receivingFilePath: (Uploaded File) Full path and name of the file where code is going to be injected into
            // injectFilePath: Full path and name of the file where the code for injection is
            // destinationFile: Full path and name of the file where the outputted, combined code is

            // Reading Files
            string receivingFile, injectFile;   // Files Content
            using (StreamReader  sr = new StreamReader(receivingFilePath)) 
            {
                receivingFile = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(injectFilePath))
            {
                injectFile = sr.ReadToEnd();
            }

            // Output File
            using (StreamWriter sw = new StreamWriter(destinationFile))
            {
                sw.WriteLine(receivingFile + "\n\n\n\n" + injectFile);
            }
        }
    }
}
