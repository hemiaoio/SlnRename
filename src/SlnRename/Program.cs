using System;
using System.IO;
using System.Linq;

namespace SlnRename {
    class Program {
        static void Main(string[] args) {
            RepeatRun();
        }

        private static void RepeatRun() {



            try
            {
                Console.WriteLine("请输入解决方案文件夹目录:(" + Directory.GetCurrentDirectory() + ")");
                string currentDirPath = Console.ReadLine();
                if(string.IsNullOrWhiteSpace(currentDirPath)) {
                    currentDirPath = Directory.GetCurrentDirectory();
                }
                string slnFullName = Directory.GetFiles(currentDirPath, "*.sln").FirstOrDefault();
                string slnName = string.Empty;
                if(!string.IsNullOrWhiteSpace(slnFullName)) {
                    slnName = slnFullName.Remove(0, slnFullName.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                }

                string companyName = string.Empty;
                if(!string.IsNullOrWhiteSpace(slnName)) {
                    companyName = slnName.Substring(0, slnName.IndexOf(".", StringComparison.Ordinal));
                }
                Console.WriteLine("请输入解决方案公司名称:(" + companyName + ")");
                string tempCompanyName = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(tempCompanyName)) {
                    companyName = tempCompanyName;
                }

                string projectName = string.Empty;
                if(!string.IsNullOrWhiteSpace(slnName) && slnName.Count(c => c.Equals('.')) > 1) {
                    projectName = slnName.Remove(0, slnName.IndexOf(".", StringComparison.Ordinal) + 1);
                    projectName = projectName.Substring(0, projectName.IndexOf(".", StringComparison.Ordinal));
                }
                Console.WriteLine("请输入解决方案项目名称:(" + projectName + ")");
                string tempProjectName = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(tempProjectName)) {
                    projectName = tempProjectName;
                }

                string targetCompanyName = ReadLineTargetCompanyName();
                string targetProjectName = ReadLineTargetProjectName();

                SolutionRenamer solutionRenamer = new SolutionRenamer(currentDirPath, companyName, projectName,
                    targetCompanyName, targetProjectName) {CreateBackup = false};
                solutionRenamer.Run();
            } catch(Exception e) {
                Console.WriteLine(e);
                Console.Write("出现异常,按回车键重新来过,否则退出:");
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if(keyInfo.Key == ConsoleKey.Enter) {
                    RepeatRun();
                }
            }
        }

        private static string ReadLineTargetCompanyName() {
            Console.WriteLine("请输入目标解决方案公司名称:");
            var targetCompanyName = Console.ReadLine();
            return targetCompanyName;
        }

        private static string ReadLineTargetProjectName()
        {
            Console.WriteLine("请输入目标解决方案项目名称:");
            var targetProjectName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(targetProjectName))
            {
                return ReadLineTargetProjectName();
            }
            return targetProjectName;
        }
    }
}