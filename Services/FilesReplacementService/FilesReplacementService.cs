using FTPUpdaterAPI.Utilities;

namespace FTPUpdaterAPI.Services.FilesReplacementService
{
    internal class FilesReplacementService: IFilesReplacementService
    {
        private readonly string _tempDirectory;
        private string _rootDirectory;

        public FilesReplacementService()
        {
            _rootDirectory = DirectoriesUtility.GetRootDirectoryPath();
            _tempDirectory = DirectoriesUtility.GetDirectoryPath("UpdateTemp");
        }

        private void DeleteDirectoriesWithFiles(IEnumerable<string> entries)
        {
            foreach (var entry in entries)
            {

                string name = entry.Split("\\").Last().ToLower();

                /* some files like "web.config" in .NET applications or whole directories
                   should not be replaced, so you can make a rule for such files 
                   and use the bool variable below */
                bool canBeDeleted = true; 
                if (Directory.Exists(entry))
                {

                    if (canBeDeleted)
                    {
                        string[] dirEntries = Directory.GetFileSystemEntries(entry);
                        Directory.Delete(entry, true);
                    }
                }
                if (File.Exists(entry))
                {
                    canBeDeleted = (name != "web.config") && (name != "appsettings.json");

                    if (canBeDeleted)
                        File.Delete(entry);
                }
            }
        }

        private void CopyFilesIntoRoot(string tempDirPath, string rootDirPath = "")
        {
            if (string.IsNullOrEmpty(rootDirPath))
            {
                rootDirPath = _rootDirectory;
            }

            string rootSubDirectory = Path.Combine(rootDirPath, tempDirPath.Split("\\").Last());
            Directory.CreateDirectory(rootSubDirectory);

            string[] dirEntries = Directory.GetFileSystemEntries(tempDirPath);
            foreach (var entry in dirEntries)
            {
                //same logic as above in method DeleteDirectoriesWithFiles
                bool canBeReplaced = true;
                if (Directory.Exists(entry))
                {
                    if (canBeReplaced)
                    {
                        Directory.CreateDirectory(Path.Combine(rootSubDirectory, entry.Split("\\").Last()));
                        CopyFilesIntoRoot(entry, rootSubDirectory);
                    }
                }
                if (File.Exists(entry) && canBeReplaced)
                {
                    using (Stream sourceStream = File.OpenRead(entry))
                    {
                        using (Stream targetStream = File.Create(Path.Combine(rootSubDirectory, entry.Split("\\").Last())))
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
        }

        void IFilesReplacementService.ReplaceAllProjectFiles()
        {
            string[] directoriesWithUpdate = Directory.GetDirectories(_tempDirectory);

            foreach (var dir in directoriesWithUpdate)
            {
                // deleting folders with all content before updating
                string dirName = dir.Split("\\").Last();
                if (Directory.Exists(Path.Combine(_rootDirectory, dirName)))
                {
                    DeleteDirectoriesWithFiles(Directory.GetFileSystemEntries(Path.Combine(_rootDirectory, dirName)));
                }

                // copying the current folder from UpdateTemp directory into the root one
                CopyFilesIntoRoot(dir);
            }
        }

        void IFilesReplacementService.ClearTempDirectory()
        {
            string[] dirEntries = Directory.GetDirectories(_tempDirectory);
            DeleteDirectoriesWithFiles(dirEntries);

            File.Delete(Path.Combine(_tempDirectory, "config.xml"));
        }
    }
}
