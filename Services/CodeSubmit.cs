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
        private string filePath { get; set; }
        private ProcessStartInfo codeExecutionProcess { get; set; }
        public CodeSubmit(string pathToSavedFile)
        {
            filePath = pathToSavedFile;

            //"docker", $"build -t python-test /"
            codeExecutionProcess = new ProcessStartInfo("ipconfig");
            //codeExecutionProcess.UseShellExecute = false;
            //codeExecutionProcess.RedirectStandardOutput = true;
            //codeExecutionProcess.RedirectStandardError = true;
        }

        public bool execute()
        {
            Process.Start(codeExecutionProcess);
            return true;
        }
    }
}
