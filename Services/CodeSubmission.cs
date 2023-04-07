using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Npgsql;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using static System.Net.Mime.MediaTypeNames;

namespace CodingCompetitionPlatform.Services
{
    // CODE SUBMIT //
    // Class for submitting the code
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

            // Create run case files
            for (int i = 0; i < problem.runCases; i++)
            {
                // Inject code into the user's uploaded code
                string runcaseFileName = $"{problem.problemIndex}_runcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{runcaseFileName}";
                string injectedCodeIdentifier = $"{userUploadedCode.identifier}_runcase{i}";
                CompetitionFileIOInfo combinedUserRuncaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{injectedCodeIdentifier}.{userUploadedCode.fileExtension}");
                combinedUserRuncaseCode.identifier = injectedCodeIdentifier;
                Console.WriteLine(runcaseFileName);
                InjectCaseCode(userUploadedCode, injectorFilePath, combinedUserRuncaseCode);

                combinedUserRuncaseCode.identifier = injectedCodeIdentifier;
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

            // Create test case files
            for (int i = 0; i < problem.testCases; i++)     // Test Cases
            {
                // Inject code into the user's uploaded code
                string testcaseFileName = $"{problem.problemIndex}_testcase{i}.{userUploadedCode.fileExtension}";
                string injectorFilePath = $@"{inputCasesPath.destinationPath}\{testcaseFileName}";
                string injectedCodeIdentifier = $"{userUploadedCode.identifier}_testcase{i}";
                CompetitionFileIOInfo combinedUserTestcaseCode = new CompetitionFileIOInfo(@$"{userUploadedCode.fileDirectory}\{injectedCodeIdentifier}.{userUploadedCode.fileExtension}");
                combinedUserTestcaseCode.identifier = injectedCodeIdentifier;
                Console.WriteLine(testcaseFileName);
                InjectCaseCode(userUploadedCode, injectorFilePath, combinedUserTestcaseCode);

                combinedUserTestcaseCode.identifier = injectedCodeIdentifier;
                codeReadyToBeExecuted.Add(combinedUserTestcaseCode);
            }

            Console.WriteLine("Number of test case files: " + codeReadyToBeExecuted.Count);
            return codeReadyToBeExecuted;
        }


        // Executing All Code Cases (running all testing and run cases), Returns a dictionary with file data in it
        public static async Task<List<ActualExpectedCompiler>> ExecuteCases(List<CompetitionFileIOInfo> cases, string userId, Problem problem, CaseType caseType, ProgrammingLanguage programmingLanguage)
        {
            // Execute all files given in the cases list
            List<Task<ExecuterCompiler>> executionTasks = new List<Task<ExecuterCompiler>>();
            foreach (CompetitionFileIOInfo file in cases)
            {
                executionTasks.Add(Task<CompetitionFileIOInfo>.Run(() => Execute(file, userId, programmingLanguage)));
                
                //Execute(file, userId, programmingLanguage);
            }
            await Task.WhenAll(executionTasks);

            Console.WriteLine(executionTasks.Count);

            // Return the files of the outputs of the cases
            var finalOutputs = new List<ActualExpectedCompiler>();
            if (executionTasks.Count != cases.Count) { throw new PlatformInternalException("Internal Error, Task count and cases count do not match in the ExecuteCases() function"); }

            if (caseType == CaseType.Run)
            {
                for (int i = 0; i < problem.runCases; i++)
                {
                    CompetitionFileIOInfo expectedOutput = new CompetitionFileIOInfo($@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}\{problem.problemIndex}_runcase{i}_EO.txt");

                    finalOutputs.Add(new ActualExpectedCompiler(executionTasks[i].Result.executionOutput, expectedOutput, executionTasks[i].Result.compilerErrors));
                }
            }
            else if (caseType == CaseType.Test) 
            {
                for (int i = 0; i < problem.testCases; i++)
                {
                    CompetitionFileIOInfo expectedOutput = new CompetitionFileIOInfo($@"{PlatformConfig.EXPECTEDOUTPUTS_DIR}\{problem.problemIndex}\{problem.problemIndex}_testcase{i}_EO.txt");

                    finalOutputs.Add(new ActualExpectedCompiler(executionTasks[i].Result.executionOutput, expectedOutput, executionTasks[i].Result.compilerErrors));
                }
            }

            return finalOutputs;
        }


        // Displaying All The Executed Code to the Page, Returns a dictionary containing the output
        public static List<AECContent> GetActualExpectedOutput(List<ActualExpectedCompiler> actualexpectedFiles)
        {
            var content = new List<AECContent>();

            foreach (var entry in actualexpectedFiles) 
            {
                string actualOutputContent = ReadFile(entry.actualOutput.filePath);
                string expectedOutputContent = ReadFile(entry.executionOutput.filePath);
                content.Add(new AECContent(actualOutputContent, expectedOutputContent, entry.compilerErrors));
            }

            return content;
        }


        // Determining Whether The Code Is Correct/Incorrect
        public static List<AECContentPassFail> GetPassFailChallenge(List<AECContent> actualexpectedOutput)
        {
            var gradedOutputs = new List<AECContentPassFail>();

            foreach (var actualexpectedcompiler in actualexpectedOutput)
            {
                if (actualexpectedcompiler.actualOutputContent == actualexpectedcompiler.expectedOutputContent) 
                {
                    gradedOutputs.Add(new AECContentPassFail(actualexpectedcompiler, true));
                }
                else
                {
                    gradedOutputs.Add(new AECContentPassFail(actualexpectedcompiler, false));
                }
            }

            return gradedOutputs;
        }



        // Function for Executing Code in a Docker Container
        private static async Task<ExecuterCompiler> Execute(CompetitionFileIOInfo inputFile, string userId, ProgrammingLanguage programmingLanguage)
        {
            // Executed and compiled output file name
            CompetitionFileIOInfo outputFile = new CompetitionFileIOInfo(inputFile.fileDirectory + $@"\_{inputFile.fileName}_OUTPUT.txt");
            CompetitionFileIOInfo compileOutput = new CompetitionFileIOInfo(inputFile.fileDirectory + $@"\_{inputFile.fileName}_COMPILEOUTPUT.txt");    // Only for compiled languages
            outputFile.identifier = inputFile.identifier;

            // Name of the Docker image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = $"{SubmittedLanguage.GetLanguageName(programmingLanguage)}:{inputFile.fileName}";
            Console.WriteLine("\n" + dockerImageName);

            // Docker images build command
            string buildDockerImageCmd;
            // Compiled languages
            if (SubmittedLanguage.IsCompiledLanguage(programmingLanguage)) 
            {
                if (programmingLanguage == ProgrammingLanguage.Java) 
                {
                    // Special run case for Java
                    buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\{SubmittedLanguage.GetLanguageName(programmingLanguage)}.dockerfile --build-arg input_file_name={inputFile.fileName} --build-arg class_name={inputFile.identifier} > {compileOutput.fileName}";
                }
                else
                {
                    buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\{SubmittedLanguage.GetLanguageName(programmingLanguage)}.dockerfile --build-arg input_file_name={inputFile.fileName} > {compileOutput.fileName}";
                }
            }
            // Interpreted languages
            else
            {
                buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f {PlatformConfig.DOCKERFILES_DIR}\{SubmittedLanguage.GetLanguageName(programmingLanguage)}.dockerfile --build-arg input_file_name={inputFile.fileName}";
            }

            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {outputFile.fileName} 2>&1";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            ExecuteCommand(buildDockerImageCmd, inputFile.fileDirectory);

            // Get compile error if it occurred
            string compileErrorMessage = "";
            if (SubmittedLanguage.IsCompiledLanguage(programmingLanguage))
            {
                if (CompileErrorOccurred(compileOutput))
                {
                    compileErrorMessage = GetCompileError(compileOutput);
                }
            }

            ExecuteCommand(runDockerImageCmd, inputFile.fileDirectory);
            ExecuteCommand(cleanupDockerImageCmd, inputFile.fileDirectory);

            Console.WriteLine($"\nExecuted {inputFile.fileName}");

            // Output file and compile output return object
            ExecuterCompiler outputs = new ExecuterCompiler(outputFile, compileErrorMessage);

            return outputs;
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
        private static void InjectCaseCode(CompetitionFileIOInfo receivingFilePath, string injectFilePath, CompetitionFileIOInfo destinationFile)
        {
            // receivingFilePath: (Uploaded File) Full path and name of the file where code is going to be injected into
            // injectFilePath: Full path and name of the file where the code for injection is
            // destinationFile: Full path and name of the file where the outputted, combined code is

            // Reading Files
            string receivingCode, injectCode;   // Files Content
            using (StreamReader  sr = new StreamReader(receivingFilePath.filePath)) 
            {
                receivingCode = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(injectFilePath))
            {
                injectCode = sr.ReadToEnd();
            }

            // Output File
            string outputInjected;
            if (receivingFilePath.fileExtension == "java")      // Custom Java injection process
            {
                outputInjected = receivingCode.Substring(0, receivingCode.LastIndexOf("}")) + $"\n\n\n\t{injectCode.Replace("\n", "\n\t")}\n\n\n" + receivingCode.Substring(receivingCode.LastIndexOf("}"));
            }
            else 
            {
                outputInjected = receivingCode + "\n\n\n\n" + injectCode;
            }

            using (StreamWriter sw = new StreamWriter(destinationFile.filePath))
            {
                sw.WriteLine(outputInjected);
            }

            if (receivingFilePath.fileExtension == "java")
            {
                SubmittedLanguage.HandleJavaClassName(destinationFile, receivingFilePath.identifier);
            }
        }

        private static bool CompileErrorOccurred(CompetitionFileIOInfo errorFile)
        {
            string errorFileContents = ReadFile(errorFile.filePath);
            return errorFileContents.Contains("------");
        }
        private static string GetCompileError(CompetitionFileIOInfo errorFile)
        {
            // Used only if CompileErrorOccurred is true
            string errorFileContents = ReadFile(errorFile.filePath);
            return errorFileContents.Substring(errorFileContents.IndexOf("------"));
        }

        private static string ReadFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath)) 
            {
                return sr.ReadToEnd();
            }
        }
    }

    // Class that stores the output file info object, compiler error message (if there is one)
    // returned by the CodeSubmission.Execute() function
    public class ExecuterCompiler
    {
        public CompetitionFileIOInfo executionOutput { get; set; }
        public string compilerErrors { get; set; }

        public ExecuterCompiler(CompetitionFileIOInfo executionOutput, string compilerErrors)
        {
            this.executionOutput = executionOutput;
            this.compilerErrors = compilerErrors;
        }
    }
    // Class that stores the actual output file info object, expected output file info object, compiler error message (if there is one)
    public class ActualExpectedCompiler : ExecuterCompiler
    {
        public CompetitionFileIOInfo actualOutput { get; set; }

        public ActualExpectedCompiler(CompetitionFileIOInfo actualOutput, CompetitionFileIOInfo executionOutput, string compilerErrors) : base(executionOutput, compilerErrors)
        {
            this.actualOutput = actualOutput;
        }
    }

    // Class that stores the content of the actual output file, expected output file, and compiler error message (if there is one)
    public class AECContent
    {
        // AEC: Actual Expected Compiler
        public string actualOutputContent { get; set; }
        public string expectedOutputContent { get; set; }
        public string compilerError { get; set; }

        public AECContent(string actualOutputContent, string expectedOutputContent, string compilerError)
        {
            this.actualOutputContent = actualOutputContent;
            this.expectedOutputContent = expectedOutputContent;
            this.compilerError = compilerError;
        }
    }
    // Class that stores the actual, expected, compiler file contents and whether they passed/failed the problem
    public class AECContentPassFail : AECContent
    {
        public bool passChallenge { get; set; }

        public AECContentPassFail(AECContent actualExpectedCompiler, bool passfailChallenge) : base(actualExpectedCompiler.actualOutputContent, actualExpectedCompiler.expectedOutputContent, actualExpectedCompiler.compilerError)
        {
            passChallenge = passfailChallenge;
        }
    }


    // Enum to help with determining whether test or run case type
    public enum CaseType
    {
        Run,
        Test
    }
}
