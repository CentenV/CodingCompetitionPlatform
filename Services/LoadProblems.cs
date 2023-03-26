using System.Text.Json;

namespace CodingCompetitionPlatform.Services
{
    public class LoadProblems
    {
        public static bool initialized = false;
        private static string path = @"wwwroot/sampleproblems.json";
        public static Problem[]? PROBLEMS { get; set; }
        public static int? NUMBEROFPROBLEMS { get; set; }

        public static void initialize()
        {
            if (!LoadProblems.initialized)
            {
                try
                {
                    string fileContent = File.ReadAllText(path);
                    PROBLEMS = JsonSerializer.Deserialize<Problem[]>(fileContent);
                    NUMBEROFPROBLEMS = PROBLEMS.Length;

                    Console.WriteLine("\nProblems imported from " + path + "\nNumber of Problems: " + NUMBEROFPROBLEMS + "\n");
                    initialized = true;
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.ToString());
                    initialized = false;
                }
            }
            else
            {
                initialized = false;
            }
        }

        public static string convertTitle(string initialTitle)
        {
            // Capitalizes the first letter of the difficulty
            if (initialTitle == "easy") 
            {
                return "Easy";
            }
            else if (initialTitle == "medium")
            {
                return "Medium";
            }
            else if (initialTitle == "hard")
            { 
                return "Hard"; 
            }
            else
            {
                return "Unknown Difficulty";
            }
        }
    }

    public class Problem
    {
        public int problemIndex { get; set; }
        public string difficulty { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int points { get; set; }
    }
}
