using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlnRename
{
    /// <summary>
    /// Used to rename a solution
    /// </summary>
    public class SolutionRenamer
    {
        public static string[] IgnoredDirectoryNames = { ".nuget",".git", ".svn", ".vs" };//packages, "obj","bin"
        /// <summary>
        /// Create a backup of the solution before renaming?
        /// Default: true.
        /// </summary>
        public bool CreateBackup { get; set; }

        private readonly string _folder;

        private readonly string _oldCompanyName;
        private readonly string _oldProjectName;

        private readonly string _companyName;
        private readonly string _projectName;

        /// <summary>
        /// Creates a new <see cref="SolutionRenamer"/>.
        /// </summary>
        /// <param name="folder">Solution folder (which includes .sln file)</param>
        /// <param name="companyNamePlaceHolder">Company name place holder (can be null if there is not a company name place holder)</param>
        /// <param name="projectNamePlaceHolder">Project name place holder</param>
        /// <param name="companyName">Company name. Can be null if new solution will not have a company name prefix. Should be null if <see cref="companyNamePlaceHolder"/> is null</param>
        /// <param name="projectName">Project name</param>
        public SolutionRenamer(string folder, string companyNamePlaceHolder, string projectNamePlaceHolder, string companyName, string projectName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
            {
                companyName = null;
            }

            if (!Directory.Exists(folder))
            {
                throw new Exception("There is no folder: " + folder);
            }

            folder = folder.Trim('\\');

            if (companyNamePlaceHolder == null && companyName != null)
            {
                throw new Exception("Can not set companyName if companyNamePlaceHolder is null.");
            }

            _folder = folder;

            _oldCompanyName = companyNamePlaceHolder;
            _oldProjectName = projectNamePlaceHolder ?? throw new ArgumentNullException(nameof(projectNamePlaceHolder));

            _companyName = companyName;
            _projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));

            CreateBackup = true;
        }

        public void Run()
        {
            if (CreateBackup)
            {
                Backup();
            }
            if (_oldCompanyName != null)
            {
                if (_companyName != null)
                {
                    RenameDirectoryRecursively(_folder, _oldCompanyName, _companyName);
                    RenameAllFiles(_folder, _oldCompanyName, _companyName);
                    ReplaceContent(_folder, _oldCompanyName, _companyName);
                }
                else
                {
                    RenameDirectoryRecursively(_folder, _oldCompanyName + "." + _oldProjectName, _oldProjectName);
                    RenameAllFiles(_folder, _oldCompanyName + "." + _oldProjectName, _oldProjectName);
                    ReplaceContent(_folder, _oldCompanyName + "." + _oldProjectName, _oldProjectName);
                }
            }

            RenameDirectoryRecursively(_folder, _oldProjectName, _projectName);
            RenameAllFiles(_folder, _oldProjectName, _projectName);
            ReplaceContent(_folder, _oldProjectName, _projectName);
        }

        private void Backup()
        {
            var normalBackupFolder = _folder + "-BACKUP";
            var backupFolder = normalBackupFolder;

            int backupNo = 1;
            while (Directory.Exists(backupFolder))
            {
                backupFolder = normalBackupFolder + "-" + backupNo;
                ++backupNo;
            }

            DirectoryCopy(_folder, backupFolder, true);
        }

        private static void RenameDirectoryRecursively(string directoryPath, string placeHolder, string name)
        {
            var subDirectories = Directory.GetDirectories(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var subDirectory in subDirectories)
            {
                if (IgnoredDirectoryNames.Any(t => subDirectory.Contains(t)))
                {
                    continue;
                }
                var newDir = subDirectory;
                if (subDirectory.Contains(placeHolder))
                {
                    newDir = subDirectory.Replace(placeHolder, name);
                    if (newDir == subDirectory)
                        continue;
                    Console.WriteLine("Rename Directory:" + subDirectory + ">>>>>" + newDir);
                    Directory.Move(subDirectory, newDir);
                }

                RenameDirectoryRecursively(newDir, placeHolder, name);
            }
        }

        private static void RenameAllFiles(string directory, string placeHolder, string name)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (IgnoredDirectoryNames.Any(t => file.Contains(t)))
                {
                    continue;
                }
                if (file.Contains(placeHolder))
                {
                    string newFile = file.Replace(placeHolder, name);
                    File.Move(file, newFile);

                    Console.WriteLine("Rename File:" + file + ">>>>>" + newFile);
                }
            }
        }

        private static void ReplaceContent(string rootPath, string placeHolder, string name)
        {
            var skipExtensions = new[] { ".exe", ".gif", ".dll", ".bin", ".suo", ".png", ".jpg", ".jpeg", ".pdb", ".obj" };

            var files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
            var tasks = files.ToList().Select(async file =>
            {
                await Task.Run(() =>
                {
                    if (IgnoredDirectoryNames.Any(file.Contains))
                    {
                        return;
                    }

                    if (skipExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    {
                        return;
                    }

                    var fileSize = GetFileSize(file);
                    if (fileSize < placeHolder.Length)
                    {
                        return;
                    }

                    var encoding = GetEncoding(file);

                    var content = File.ReadAllText(file, encoding);
                    var newContent = content.Replace(placeHolder, name);
                    if (newContent != content)
                    {
                        var trycount = 0;
                        while (true)
                        {
                            try
                            {
                                File.WriteAllText(file, newContent, encoding);
                                break;
                            }
                            catch (Exception e)
                            {
                                trycount++;
                                Console.WriteLine("Write Error in " + file.Replace(rootPath, "") + "\r\n" + e.Message);

                                Thread.Sleep(5000* trycount);
                                 if (trycount>3)
                                {
                                    throw;
                                }
                            }
                        }
                       
                    }

                    Console.WriteLine("Replace Content:" + file.Replace(rootPath,""));
                });

            });

            Task.WhenAll(tasks).Wait();
        }

        private static long GetFileSize(string file)
        {
            return new FileInfo(file).Length;
        }
        /// <summary>
        /// 获取编码
        /// </summary>
        private static Encoding GetEncoding(string filename)
        {
            //原方法并不能正确识别Utf-8 常常被识别出来是 ASCII;
            return EncodingType.GetType(filename);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}