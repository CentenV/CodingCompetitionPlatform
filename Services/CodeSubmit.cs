using System.Diagnostics;

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
        // Function for Executing the Submitted Code in a Docker Container
        public static async Task<string> Execute(string fileName, string userid)
        {
            string outputFileName = $"{fileName}_OUTPUT.txt";

            // Name of the Docker Image (language:userid). Docker image name cannot contain numbers and can only be lowercase
            string dockerImageName = "python:" + userid;
            Console.WriteLine("\n" + dockerImageName);

            string buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f C:\Users\Administrator\Documents\CODEREPO\playgrounds\docker\python\python.dockerfile --build-arg input_file_name={fileName}";
            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {outputFileName}";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            executeCommand(buildDockerImageCmd);
            executeCommand(runDockerImageCmd);
            executeCommand(cleanupDockerImageCmd);

            Console.WriteLine($"\nExecuted {fileName}");

            return outputFileName;
        }



        // Internal Methods
        private static void executeCommand(string command)
        {
            ProcessStartInfo ps = new ProcessStartInfo(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "/c " + command);
            ps.WorkingDirectory = PlatformConfig.SUBMISSION_OUTPUT_DIR;
            ps.UseShellExecute = false;
            Console.WriteLine(command);
            
            Process.Start(ps).WaitForExit();
        }
    }
}
