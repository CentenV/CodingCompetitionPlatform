using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Npgsql;
using System.Diagnostics;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using static System.Net.Mime.MediaTypeNames;

// CODE SUBMIT //
// Class for submitting the code
namespace CodingCompetitionPlatform.Services
{
    // Code Submission
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
        // returns [{ "actual outputted file info", "expected output file info" }, ... ]
        public static async Task<Dictionary<CompetitionFileIOInfo, CompetitionFileIOInfo>> ExecuteCases(List<CompetitionFileIOInfo> cases, string userId, Problem problem, CaseType caseType, ProgrammingLanguage programmingLanguage)
        {
            // Execute all files given in the cases list
            List<Task<CompetitionFileIOInfo>> executionTasks = new List<Task<CompetitionFileIOInfo>>();
            foreach (CompetitionFileIOInfo file in cases)
            {
                //executionTasks.Add(Task<CompetitionFileIOInfo>.Run(() => Execute(file, userId, programmingLanguage)));
                
                Execute(file, userId, programmingLanguage);
            }
            //await Task.WhenAll(executionTasks);

            Console.WriteLine(executionTasks.Count);

            // Return the files of the outputs of the cases
            var finalOutputs = new Dictionary<CompetitionFileIOInfo, CompetitionFileIOInfo>();
            if (executionTasks.Count != cases.Count) { throw new PlatformInternalException("Internal Error, Task count and cases count do not match in the ExecuteCases() function"); }

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


        // Determining Whether The Code Is Correct/Incorrect
        // returns true/false as a third 
        public static Dictionary<KeyValuePair<string, string>, bool> GetPassFailChallenge(Dictionary<string, string> actualexpectedOutput)
        {
            var gradedOutputs = new Dictionary<KeyValuePair<string, string>, bool>();

            foreach (var actualexpected in actualexpectedOutput)
            {
                if (actualexpected.Key == actualexpected.Value) 
                {
                    gradedOutputs.Add(actualexpected, true);
                }
                else
                {
                    gradedOutputs.Add(actualexpected, false);
                }
            }

            return gradedOutputs;
        }



        // Function for Executing Code in a Docker Container
        private static async Task<CompetitionFileIOInfo> Execute(CompetitionFileIOInfo inputFile, string userId, ProgrammingLanguage programmingLanguage)
        {
            // Output file name
            CompetitionFileIOInfo outputFile = new CompetitionFileIOInfo(inputFile.fileDirectory + $@"\_{inputFile.fileName}_OUTPUT.txt");
            CompetitionFileIOInfo compileOutput = new CompetitionFileIOInfo(inputFile.fileDirectory + $@"\_{inputFile.fileName}_COMPILEOUTPUT.txt");    // Only for compiled languages
            outputFile.identifier = inputFile.identifier;

            // Name of the Docker Image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = $"{SubmittedLanguage.GetLanguageName(programmingLanguage)}:{inputFile.fileName}";
            Console.WriteLine("\n" + dockerImageName);

            // Building Docker Images
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
            string compileError = "";
            if (SubmittedLanguage.IsCompiledLanguage(programmingLanguage))
            {
                if (CompileErrorOccurred(compileOutput))
                {
                    compileError = GetCompileError(compileOutput);
                }
            }

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

    // Enum to help with determining whether test or run case type
    public enum CaseType
    {
        Run,
        Test
    }


    // Code Type
    public enum ProgrammingLanguage
    {
        Python,
        Java,
        JavaScript,
        C,
        CPP,
        CSharp
    }
    public class SubmittedLanguage
    {
        public static string GetLanguageName(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.Python:
                    return "python";
                case ProgrammingLanguage.Java:
                    return "java";
                case ProgrammingLanguage.JavaScript:
                    return "javascript";
                case ProgrammingLanguage.C:
                    return "c";
                case ProgrammingLanguage.CPP:
                    return "cpp";
                case ProgrammingLanguage.CSharp:
                    return "csharp";
                default:
                    throw new CompetitionPlatformLanguageException("Invalid Language Used");
            }
        }
        public static string GetFileExtension(ProgrammingLanguage language) 
        {
            switch (language) 
            {
                case ProgrammingLanguage.Python:
                    return "py";
                case ProgrammingLanguage.Java:
                    return "java";
                case ProgrammingLanguage.JavaScript:
                    return "js";
                case ProgrammingLanguage.C:
                    return "c";
                case ProgrammingLanguage.CPP:
                    return "cpp";
                case ProgrammingLanguage.CSharp:
                    return "cs";
                default:
                    throw new CompetitionPlatformLanguageException("Invalid language used");
            }
        }
        public static bool IsCompiledLanguage(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.Java: case ProgrammingLanguage.C: case ProgrammingLanguage.CPP: case ProgrammingLanguage.CSharp:
                    return true;
                case ProgrammingLanguage.Python: case ProgrammingLanguage.JavaScript:
                    return false;
                default:
                    throw new CompetitionPlatformLanguageException("Unable to determine whether a language was compiled or interpreted");
            }
        }

        public static void HandleJavaClassName(CompetitionFileIOInfo file, string initialClassName)
        {
            /// Changes the class name to match the file name

            // Get file/java class name
            string initialFileClassName = "";
            if (initialClassName.Contains("."))
            {
                for (int i = 0; i < initialClassName.Length; i++)
                {
                    if (initialClassName[i] == '.')
                    {
                        initialFileClassName = initialClassName.Substring(0, i);
                    }
                }
            }
            else
            {
                initialFileClassName = initialClassName;
            }
            

            // Read file and replace the class name
            string uploadedJavaFileContent = File.ReadAllText(file.filePath);

            if (uploadedJavaFileContent == null || uploadedJavaFileContent.Length <= 7)
            {
                throw new Exception("Java File Error");
            }

            string initClassDef = $"class {initialFileClassName}";
            if (!uploadedJavaFileContent.Contains(initClassDef))
            {
                throw new Exception("No Class Found in Java File. Is the .java file named the same as the class?");
            }
            else
            {
                string newFileContent = uploadedJavaFileContent.Replace(initClassDef, $"class {file.identifier}");
                File.WriteAllText(file.filePath, newFileContent);
            }
        }
    }
    public class CompetitionPlatformLanguageException : Exception 
    {
        public CompetitionPlatformLanguageException(string message) : base(message) { }
    }
}
