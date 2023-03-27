using CodingCompetitionPlatform.Services;
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
        private string fileName { get; set; }

        // Public Methods
        public CodeSubmit(string savedFileName)
        {
            fileName = savedFileName;
        }

        public void Execute(string userid)
        {
            string dockerImageName = "python:" + userid;
            Console.WriteLine("\n" + dockerImageName);

            string buildDockerImageCmd = $@"docker build .\ -t {dockerImageName} -f C:\Users\Administrator\Documents\CODEREPO\playgrounds\docker\python\python.dockerfile --build-arg input_file_name={fileName}";
            string runDockerImageCmd = $"docker run --rm {dockerImageName} > {fileName}_OUTPUT.txt";
            string cleanupDockerImageCmd = $"docker rmi -f {dockerImageName}";

            executeCommand(buildDockerImageCmd);
            executeCommand(runDockerImageCmd);
            executeCommand(cleanupDockerImageCmd);

            Console.WriteLine($"\nExecuted {fileName}\n\n\n\n");
        }


        // Internal Methods
        private void executeCommand(string command)
        {
            ProcessStartInfo ps = new ProcessStartInfo(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", "/c " + command);
            ps.WorkingDirectory = PlatformConfig.SUBMISSION_OUTPUT_DIR;
            ps.UseShellExecute = false;
            Console.WriteLine(command);
            
            Process.Start(ps).WaitForExit();
        }
    }
}
