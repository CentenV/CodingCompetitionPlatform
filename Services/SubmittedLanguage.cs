﻿namespace CodingCompetitionPlatform.Services
{
    // Submitted Code Type
    public enum ProgrammingLanguage
    {
        PYTHON,
        JAVA,
        JAVASCRIPT,
        C,
        CPP,
        CSHARP
    }
    public class SubmittedLanguage
    {
        public static string GetLanguageName(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.PYTHON:
                    return "python";
                case ProgrammingLanguage.JAVA:
                    return "java";
                case ProgrammingLanguage.JAVASCRIPT:
                    return "javascript";
                case ProgrammingLanguage.C:
                    return "c";
                case ProgrammingLanguage.CPP:
                    return "cpp";
                case ProgrammingLanguage.CSHARP:
                    return "csharp";
                default:
                    throw new CompetitionPlatformLanguageException("Invalid Language Used");
            }
        }
        public static string GetFileExtension(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.PYTHON:
                    return "py";
                case ProgrammingLanguage.JAVA:
                    return "java";
                case ProgrammingLanguage.JAVASCRIPT:
                    return "js";
                case ProgrammingLanguage.C:
                    return "c";
                case ProgrammingLanguage.CPP:
                    return "cpp";
                case ProgrammingLanguage.CSHARP:
                    return "cs";
                default:
                    throw new CompetitionPlatformLanguageException("Invalid language used");
            }
        }
        public static bool IsCompiledLanguage(ProgrammingLanguage language)
        {
            switch (language)
            {
                case ProgrammingLanguage.JAVA:
                case ProgrammingLanguage.C:
                case ProgrammingLanguage.CPP:
                case ProgrammingLanguage.CSHARP:
                    return true;
                case ProgrammingLanguage.PYTHON:
                case ProgrammingLanguage.JAVASCRIPT:
                    return false;
                default:
                    throw new CompetitionPlatformLanguageException("Unable to determine whether a language was compiled or interpreted");
            }
        }

        public static void HandleJavaFileClassName(CompetitionFileIOInfo file, string initialClassName)
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
