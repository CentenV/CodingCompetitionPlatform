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
    public class CodeSubmit
    {
        // Submission (running all testing and run cases), Returns the dictionary which contains {"path to actual output file", "path to expected output file"}
        // (problem context, name of inputted user file, full path to inputted user file, directory of the submitted file folder, userid)
        public static void Submit(Problem problem, string identifier, string fileName, string fileFullPath, string workingDirectory, string userId)
        {
            // All needed paths and the directory with the challenge resources
            string inputCasesPath = @$"{PlatformConfig.INPUTCASES_DIR}\{problem.problemIndex}";
            string expectedOutputsPath = $@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}";
            string extension = "py";

            // List of all the code ready to be executed
            List<string> readyCodeToBeExecuted = new List<string>();

            // Getting a list of paths to the run cases and test cases files and injecting the run and test case into the user submitted code
            string[] inputRunCases = new string[problem.runCases];
            for (int i = 0; i < problem.runCases; i++)
            {
                // Compile the list of paths for all run cases
                string caseFileName = $"{problem.problemIndex}_runcase{i}.{extension}";
                inputRunCases[i] = @$"{inputCasesPath}\{caseFileName}";
                Console.WriteLine(caseFileName);

                // Inject code into the user's uploaded code
                string injectorFilePath = $@"{inputCasesPath}\{caseFileName}";
                string combinedCodeOutputPath = @$"{workingDirectory}\{identifier}_runcase{i}.{extension}";    // Full path and output file name
                readyCodeToBeExecuted.Add(combinedCodeOutputPath);
                InjectCaseCode(fileFullPath, injectorFilePath, combinedCodeOutputPath);
            }
            string[] inputTestCases = new string[problem.testCases];
            for (int i = 0; i < problem.testCases; i++)
            {
                // Compile the list of paths of for all test cases
                string caseFileName = $"{problem.problemIndex}_testcase{i}.{extension}";
                inputTestCases[i] = @$"{inputCasesPath}\{caseFileName}";
                Console.WriteLine(caseFileName);

                // Inject code into the user's uploaded code
                string injectorFilePath = $@"{inputCasesPath}\{caseFileName}";
                string combinedCodeOutputPath = @$"{workingDirectory}\{identifier}_testcase{i}.{extension}";    // Full path and output file name
                readyCodeToBeExecuted.Add(combinedCodeOutputPath);
                InjectCaseCode(fileFullPath, injectorFilePath, combinedCodeOutputPath);
            }

            Console.WriteLine(readyCodeToBeExecuted.Count);
        }

        // Function for Executing the Submitted Code in a Docker Container
        public static async Task<string> Execute(string fileName, string fileDirectory, string userId)
        {
            // Output File Name
            string outputFileName = $"{fileName}_OUTPUT.txt";

            // Name of the Docker Image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = "python:" + userId;
            Console.WriteLine("\n" + dockerImageName);

            string buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\python.dockerfile --build-arg input_file_name={fileName}";
            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {outputFileName} 2>&1";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            ExecuteCommand(buildDockerImageCmd, fileDirectory);
            ExecuteCommand(runDockerImageCmd, fileDirectory);
            ExecuteCommand(cleanupDockerImageCmd, fileDirectory);

            Console.WriteLine($"\nExecuted {fileName}");

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
