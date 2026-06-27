using OfficeOpenXml;
using System.IO;
using System.Windows;
using TradeApp.Services;

namespace TradeApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string solutionFolder = FindSolutionFolder();
            if (string.IsNullOrEmpty(solutionFolder))
            {
                solutionFolder = AppDomain.CurrentDomain.BaseDirectory;
            }

            DataImporter.ImportAll(solutionFolder);
        }

        /// <summary>
        /// Ищет папку, содержащую файл решения (.sln), поднимаясь вверх от текущей папки.
        /// </summary>
        private string FindSolutionFolder()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo dir = new(currentDir);
            while (dir != null)
            {
                var slnFiles = dir.GetFiles("*.sln");
                if (slnFiles.Length > 0)
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }
    }
}