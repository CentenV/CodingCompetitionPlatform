using System.Security.Cryptography.X509Certificates;

namespace CodingCompetitionPlatform.Services
{
    /// <summary>
    /// The class that represents a file path, folder path, and/or identifer
    /// </summary>
    public class CompetitionFileIOInfo
    {
        // Files
        public string? filePath { get; }
        public string? fileName { get; }
        public string? fileDirectory { get; }
        public string? fileExtension { get; }

        // Directories
        public string? destinationPath { get; }
        public string? destinationName { get; }

        // Miscellaneous
        public string? identifier { get; set; }


        // DIFFERENT PERMUTATIONS OF THE CONSTRUCTOR //
        // Storing file info based off of full file path
        public CompetitionFileIOInfo(string fullFilePath)
        {
            this.filePath = fullFilePath;
            this.fileName = (fileName == null) ? getFinalSubpath(fullFilePath) : fileName;
            this.fileExtension = getFileExtension(fullFilePath);
            this.fileDirectory = getDirectory(fullFilePath);

            // Checking whether the specfied file name is also specfied in the path
            if (!fileName.Equals(getFinalSubpath(fullFilePath)))
                throw new PlatformFileIOException($"{fileName} does match the same file name in the specified full path of the file {fullFilePath}");
        }
        // Storing file info based off of full file path and file name
        public CompetitionFileIOInfo(string fullFilePath, string fileName)
        {
            this.filePath = fullFilePath;
            this.fileName = (fileName == null) ? getFinalSubpath(fullFilePath) : fileName;
            this.fileExtension = getFileExtension(fullFilePath);
            this.fileDirectory = getDirectory(fullFilePath);

            // Checking whether the specfied file name is also specfied in the path
            if (!fileName.Equals(getFinalSubpath(fullFilePath)))
                throw new PlatformFileIOException($"{fileName} does match the same file name in the specified full path of the file {fullFilePath}");
        }
        // Storing directory path and name only
        public CompetitionFileIOInfo(string fullOutputFolderPath, bool folder = true)
        {
            if (folder == false) throw new PlatformFileIOException("Invalid parameter. The parameter that should be used is folder = true");

            this.destinationPath = fullOutputFolderPath;
            this.destinationName = getFinalSubpath(fullOutputFolderPath);
        }
        // Current file and output after operations complete
        public CompetitionFileIOInfo(string fullFilePath, string fileName, string fullOutputFolderPath, string outputFolderName)
        {
            this.fileName = fileName;
            this.filePath = fullFilePath;
            this.fileExtension = getFileExtension();
            this.fileDirectory = getDirectory(fullFilePath);
            this.destinationName = outputFolderName;
            this.destinationPath = fullOutputFolderPath;

            // Checking whether the specfied file name is also specfied in the path
            if (!fileName.Equals(getFinalSubpath(fullFilePath)))
                throw new PlatformFileIOException($"{fileName} does match the same file name in the specified full path of the file {fullFilePath}");
            // Checking whether the specfied folder name is also specfied in the path
            if (!outputFolderName.Equals(getFinalSubpath(fullOutputFolderPath)))
                throw new PlatformFileIOException($"{outputFolderName} does match the same file name in the specified full path of the file {fullOutputFolderPath}");
        }
        public CompetitionFileIOInfo(string fullFilePath, string fullOutputFolderPath, bool purePaths = true)
        {
            // purePaths: always true and signifies that the first and second parameters are full paths of the input file and output folder
            // File name and output folder name are automatically handled
            this.filePath = fullFilePath;
            this.fileName = getFinalSubpath(filePath);
            this.fileDirectory = getDirectory(filePath);
            this.fileExtension = getFileExtension(fullFilePath);

            this.destinationPath = fullOutputFolderPath;
            this.destinationName = getFinalSubpath(fullOutputFolderPath);
        }


        // INTERNAL METHODS //

        // Gets the last section (subpath) of the path
        private string getFinalSubpath(string path)
        {
            // i.e. C:\User\Admin\test.txt => test.txt
            string lastSubpath = "";
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '\\')
                {
                    break;
                }
                else
                {
                    lastSubpath = path.Substring(i);
                }
            }
            return lastSubpath;
        }
        // Gets directory in which a file is contained in
        private string getDirectory(string path)
        {
            string directoryPath = "";
            for (int i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == '\\')
                {
                    break;
                }
                else
                {
                    directoryPath = path.Substring(0, i-1);
                }
            }
            return directoryPath;
        }
        // Gets file/language extension (i.e. py, java)
        private string getFileExtension()        // Using already initialized property of full path to the file
        {
            string subpath = getFinalSubpath(filePath);
            if (!subpath.Contains("."))
            {
                throw new PlatformFileIOException($"{subpath} in {filePath} is not a valid file to obtain a file extension from");
            }
            else
            {
                return getFileExtension(subpath);
            }
        }
        private string getFileExtension(string fileName)     // Given name only
        {
            string extension = "";

            if (!fileName.Contains(".")) { throw new PlatformFileIOException($"{fileName} is not a valid value to obtain a file extension from"); }

            for (int i = fileName.Length - 1; i >= 0; i--)
            {
                if (fileName[i] == '.')
                {
                    break;
                }
                else
                {
                    extension = fileName.Substring(i);
                }
            }

            return extension;
        }
    }

    public class PlatformFileIOException : Exception
    {
        public PlatformFileIOException(string message) : base(message) { }
    }
}
