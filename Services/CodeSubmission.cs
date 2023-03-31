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

    // Enum to help with determining whether test or run case type
    public enum CaseType
    {
        Run,
        Test
    }

    public class CodeSubmission
    {
        // Creating Case Files (injecting all testing and run cases), Returns the list of all the injected files
        // (problem context, inputted user file and its respective data, userid)
        public static List<CompetitionFileIOInfo> RunCaseFiles(Problem problem, CompetitionFileIOInfo userUploadedCode, string userId)
        {
            // All needed paths and the directory with the challenge resources
            CompetitionFileIOInfo inputCasesPath = new CompetitionFileIOInfo(@$"{PlatformConfig.INPUTCASES_DIR}\{problem.problemIndex}", folder: true);

            // List of all the code ready to be executed
            List<CompetitionFileIOInfo> codeReadyToBeExecuted = new List<CompetitionFileIOInfo>();

            // Run Case Files
            for (int i = 0; i < problem.runCases; i++)
            {
                // Inject code into the user's uploaded code
                string runcaseFileName = $"{problem.problemIndex}_runcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{runcaseFileName}";
                CompetitionFileIOInfo combinedUserRuncaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{userUploadedCode.identifier}_runcase{i}.{userUploadedCode.fileExtension}");
                Console.WriteLine(runcaseFileName);
                InjectCaseCode(userUploadedCode.filePath, injectorFilePath, combinedUserRuncaseCode.filePath);

                combinedUserRuncaseCode.identifier = userUploadedCode.identifier;
                codeReadyToBeExecuted.Add(combinedUserRuncaseCode);
            }

            Console.WriteLine("Number of run case files: " + codeReadyToBeExecuted.Count);
            return codeReadyToBeExecuted;
        }
        public static List<CompetitionFileIOInfo> TestCaseFiles(Problem problem, CompetitionFileIOInfo userUploadedCode, string userId)
        {
            // All needed paths and the directory with the challenge resources
            CompetitionFileIOInfo inputCasesPath = new CompetitionFileIOInfo(@$"{PlatformConfig.INPUTCASES_DIR}\{problem.problemIndex}", folder: true);

            // List of all the code ready to be executed
            List<CompetitionFileIOInfo> codeReadyToBeExecuted = new List<CompetitionFileIOInfo>();

            // Test Case Files
            for (int i = 0; i < problem.testCases; i++)     // Test Cases
            {
                // Inject code into the user's uploaded code
                string testcaseFileName = $"{problem.problemIndex}_testcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{testcaseFileName}";
                CompetitionFileIOInfo combinedUserTestcaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{userUploadedCode.identifier}_testcase{i}.{userUploadedCode.fileExtension}");
                Console.WriteLine(testcaseFileName);
                InjectCaseCode(userUploadedCode.filePath, injectorFilePath, combinedUserTestcaseCode.filePath);

                combinedUserTestcaseCode.identifier = userUploadedCode.identifier;
                codeReadyToBeExecuted.Add(combinedUserTestcaseCode);
            }

            Console.WriteLine("Number of test case files: " + codeReadyToBeExecuted.Count);
            return codeReadyToBeExecuted;
        }


        // Executing All Code Cases (running all testing and run cases), Returns a dictionary with file data in it
        // returns [{ "actual outputted file info", "expected output file info" }, ... ]
        public static async Task<Dictionary<CompetitionFileIOInfo, CompetitionFileIOInfo>> ExecuteCases(List<CompetitionFileIOInfo> cases, Problem problem, CaseType caseType, string userId)
        {
            // Execute all files in the cases list
            List<Task<CompetitionFileIOInfo>> executionTasks = new List<Task<CompetitionFileIOInfo>>();
            foreach (CompetitionFileIOInfo file in cases)
            {
                executionTasks.Add(Task<CompetitionFileIOInfo>.Run(() => Execute(file, userId)));
                //Execute(file, userId);
            }
            await Task.WhenAll(executionTasks);

            Console.WriteLine(executionTasks.Count);

            // Return the files of the outputs of the cases
            var finalOutputs = new Dictionary<CompetitionFileIOInfo, CompetitionFileIOInfo>();
            if (executionTasks.Count != cases.Count) { throw new PlatformInternalException("Internal Error, Task count and cases count do not match in the ExecuteCases() function"); }

            int numberOfCases;
            if (caseType == CaseType.Run)
            {
                for (int i = 0; i < problem.runCases; i++)
                {
                    CompetitionFileIOInfo expectedOutput = new CompetitionFileIOInfo($@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}\{problem.problemIndex}_runcase{i}_EO.txt");

                    finalOutputs.Add(executionTasks[i].Result, expectedOutput);
                }
            }
            else if (caseType == CaseType.Test) 
            {
                for (int i = 0; i < problem.testCases; i++)
                {
                    CompetitionFileIOInfo expectedOutput = new CompetitionFileIOInfo($@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}\{problem.problemIndex}_testcase{i}_EO.txt");

                    finalOutputs.Add(executionTasks[i].Result, expectedOutput);
                }
            }

            return finalOutputs;
        }


        // Displaying All The Executed Code to the Page, Returns a dictionary containing the output
        // returns [{ "outputted file content", "expected output file content" }]
        public static Dictionary<string, string> GetActualExpectedOutput(Dictionary<CompetitionFileIOInfo, CompetitionFileIOInfo> actualexpectedFiles)
        {
            var content = new Dictionary<string, string>();

            foreach (var file in actualexpectedFiles) 
            {
                string actualOutputContent = ReadFile(file.Key.filePath);
                string expectedOutputContent = ReadFile(file.Value.filePath);
                content.Add(actualOutputContent, expectedOutputContent);
            }

            return content;
        }





        // Function for Executing Code in a Docker Container
        private static async Task<CompetitionFileIOInfo> Execute(CompetitionFileIOInfo inputFile, string userId)
        {
            // Output File Name
            CompetitionFileIOInfo outputFile = new CompetitionFileIOInfo(inputFile.fileDirectory + $@"\_{inputFile.fileName}_OUTPUT.txt");
            outputFile.identifier = inputFile.identifier;

            // Name of the Docker Image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = $"python:{inputFile.fileName}";
            Console.WriteLine("\n" + dockerImageName);

            string buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\python.dockerfile --build-arg input_file_name={inputFile.fileName}";
            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {outputFile.fileName} 2>&1";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            ExecuteCommand(buildDockerImageCmd, inputFile.fileDirectory);
            ExecuteCommand(runDockerImageCmd, inputFile.fileDirectory);
            ExecuteCommand(cleanupDockerImageCmd, inputFile.fileDirectory);

            Console.WriteLine($"\nExecuted {inputFile.fileName}");

            return outputFile;
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

        private static string ReadFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath)) 
            {
                return sr.ReadToEnd();
            }
        }
    }
}
