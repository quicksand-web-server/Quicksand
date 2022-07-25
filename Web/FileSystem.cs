namespace Quicksand.Web
{
    public class FileSystem
    {
        private class File
        {
            private readonly string m_Path;
            private readonly string m_FileName;
            private readonly string m_Content;

            public File(string path, bool preLoad = false)
            {
                m_Path = path;
                m_FileName = Path.GetFileName(m_Path);
                if (preLoad)
                    m_Content = System.IO.File.ReadAllText(m_Path);
                else
                    m_Content = "";
            }

            public string GetFileName() { return m_FileName; }

            public bool Exist() { return m_Content.Length > 0 || System.IO.File.Exists(m_Path); }

            public string GetContent()
            {
                if (m_Content.Length > 0)
                    return m_Content;
                else if (System.IO.File.Exists(m_Path))
                    return System.IO.File.ReadAllText(m_Path);
                return "";
            }
        }

        private class Directory
        {
            private readonly Dictionary<string, Directory> m_Dirs = new();
            private readonly Dictionary<string, File> m_Files = new();

            public string? GetFileContent(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;
                string dirPath = (path[0] == '/') ? path[1..] : path;
                if (string.IsNullOrWhiteSpace(dirPath))
                    return null;
                int separatorPos = dirPath.IndexOf('/');
                if (separatorPos == -1)
                {
                    if (m_Files.TryGetValue(dirPath, out File? file))
                        return file.GetContent();
                    return null;
                }
                string dirName = dirPath[..separatorPos];
                if (!m_Dirs.ContainsKey(dirName))
                    return null;
                return m_Dirs[dirName].GetFileContent(dirPath[(separatorPos + 1)..]);
            }

            public void AddDir(string path, Directory dir)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    m_Dirs[path] = dir;
                    return;
                }
                string dirPath = (path[0] == '/') ? path[1..] : path;
                if (string.IsNullOrWhiteSpace(dirPath))
                {
                    m_Dirs[dirPath] = dir;
                    return;
                }
                int separatorPos = dirPath.IndexOf('/');
                string dirName = (separatorPos == -1) ? dirPath : dirPath[..separatorPos];
                string newPath = (separatorPos == -1) ? "" : dirPath[(separatorPos + 1)..];
                if (!m_Dirs.ContainsKey(dirName))
                    m_Dirs[dirName] = new();
                m_Dirs[dirName].AddDir(newPath, dir);
            }

            public void AddFile(string path, File file)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    m_Files[file.GetFileName()] = file;
                    return;
                }
                string dirPath = (path.Length > 0 && path[0] == '/') ? path[1..] : path;
                if (string.IsNullOrWhiteSpace(dirPath))
                {
                    m_Files[file.GetFileName()] = file;
                    return;
                }
                int separatorPos = dirPath.IndexOf('/');
                string dirName = (separatorPos == -1) ? dirPath : dirPath[..separatorPos];
                string newPath = (separatorPos == -1) ? "" : dirPath[(separatorPos + 1)..];
                if (!m_Dirs.ContainsKey(dirName))
                    m_Dirs[dirName] = new();
                m_Dirs[dirName].AddFile(newPath, file);
            }
        }

        private readonly Directory m_Root = new();

        public string? GetContent(string httpPath) { return m_Root.GetFileContent(httpPath); }

        public void AddPath(string httpPath, string path, bool preLoad = false)
        {
            if (System.IO.File.Exists(path))
                m_Root.AddFile(httpPath, new File(path, preLoad));
            else if (System.IO.Directory.Exists(path))
            {
                string[] entries = System.IO.Directory.GetFileSystemEntries(path);
                foreach (string entry in entries)
                {
                    if (System.IO.Directory.Exists(entry))
                        AddPath(string.Format("{0}/{1}", (httpPath.Length > 0 && httpPath[^1] == '/') ? httpPath[..^1] : httpPath, Path.GetFileName(entry).Replace(' ', '_').ToLowerInvariant()), entry, preLoad);
                    else
                        AddPath(httpPath, entry, preLoad);
                }
            }
        }
    }
}
