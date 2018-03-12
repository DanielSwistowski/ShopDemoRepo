using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Service_Layer.Services
{
    public interface ILogFileManagementService
    {
        IEnumerable<string> GetLogFilesNames();
        Task<string> ReadFileAsync(string fileName);
        bool FileExists(string fileName);
        void DeleteFile(string fileName);
    }

    public class LogFileManagementService : ILogFileManagementService
    {
        private const string FilesDirectoryPath = "~/App_Data/Logs/";
        private readonly IPathProvider pathProvider;
        public LogFileManagementService(IPathProvider pathProvider)
        {
            this.pathProvider = pathProvider;
        }

        public IEnumerable<string> GetLogFilesNames()
        {
            string filesDirectoryFullPath = pathProvider.MapPath(FilesDirectoryPath);
            return Directory.EnumerateFiles(filesDirectoryFullPath).Select(f => Path.GetFileName(f)).Reverse();
        }

        public async Task<string> ReadFileAsync(string fileName)
        {
            string logFilePath = pathProvider.MapPath(FilesDirectoryPath + fileName);

            string jsonString = string.Empty;
            string fileString = string.Empty;

            using (StreamReader reader = new StreamReader(logFilePath))
            {
                fileString = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrEmpty(fileString))
            {
                fileString = fileString.Remove(fileString.Length - 1); //remove last ","
                jsonString = "[" + fileString + "]";
            }
            return jsonString;
        }

        public bool FileExists(string fileName)
        {
            string logFilePath = pathProvider.MapPath(FilesDirectoryPath + fileName);
            return File.Exists(logFilePath);
        }

        public void DeleteFile(string fileName)
        {
            string logFilePath = pathProvider.MapPath(FilesDirectoryPath + fileName);
            File.Delete(logFilePath);
        }
    }
}