namespace FTPUpdaterAPI.Utilities
{
    internal static class DirectoriesUtility
    {
        private static string? _rootDirectoryPath;
        private static string? _baseName;
        internal static string GetDirectoryPath(string subDirectory)
        {
            string path = "";
            string? parent = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName;
            bool dirExists = false;
            while (!dirExists)
            {
                dirExists = Directory.Exists(Path.Combine(parent ?? "", subDirectory));
                if (!dirExists)
                    parent = Directory.GetParent(parent ?? "")?.FullName;
                else
                    path = Path.Combine(parent ?? "", subDirectory);
            }

            return path;
        }


        internal static string GetFilePath(string fileName)
        {
            string path = "";
            string? parent = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName;
            bool fileExists = false;
            while (!fileExists)
            {
                fileExists = File.Exists(Path.Combine(parent ?? "", fileName));
                if (!fileExists)
                    parent = Directory.GetParent(parent ?? "")?.FullName;
                else
                    path = Path.Combine(parent ?? "", fileName);
            }

            return path;
        }
        internal static string GetRootDirectoryPath()
        {
            if (String.IsNullOrEmpty(_rootDirectoryPath))
            {
                string? path = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName;
                for (int x = 0; x < 4; x++)
                {
                    path = Directory.GetParent(path ?? "")?.FullName;
                }

                _rootDirectoryPath = path;
            }

            return _rootDirectoryPath ?? "";
        }

        internal static string GetBaseName()
        {
            if (String.IsNullOrEmpty(_baseName))
            {
                _baseName = GetRootDirectoryPath().Split('\\').Last();
            }

            return _baseName;
        }
    }
}
