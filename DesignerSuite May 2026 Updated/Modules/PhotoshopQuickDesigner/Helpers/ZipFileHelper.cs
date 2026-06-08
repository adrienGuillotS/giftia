using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PSQuickDesigner
{
    internal static class ZipFileHelper
    {
        public static string[] GetZipFiles(string directoryPath, string processedZipFilesPath)
        {
            Directory.CreateDirectory(processedZipFilesPath);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return directory.GetFiles("*.zip").OrderBy(p => p.CreationTime).Select(f => f.FullName).ToArray();
        }

        public static void MoveZipFiles(List<string> zipFiles, string destinationFolder)
        {
            foreach (string zipFile in zipFiles)
            {
                try
                {
                    string file = Path.Combine(destinationFolder, Path.GetFileName(zipFile));
                    if (!File.Exists(file))
                        File.Move(zipFile, file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error moving {zipFile}: {ex.Message}");
                }
            }
            zipFiles.RemoveAll(s => true);
        }

        public static List<string> GetXmlFilesFromZip(string zipFilePath, string extractPath)
        {
            List<string> XmlDataList = new List<string>();
            Directory.CreateDirectory(extractPath);
            EmptyTempDirectory(extractPath);


            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(extractPath, entry.FullName));
                    entry.ExtractToFile(destinationPath);

                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            XmlDataList.Add(destinationPath);
                        }
                    }
                }
            }
            return XmlDataList;
        }


        public static void EmptyTempDirectory(string destinationFolderPath)
        {
            if (!Directory.Exists(destinationFolderPath)) return;
            string tempDirectoryPath = destinationFolderPath /*+ @"\temp\"*/;
            DirectoryInfo di = new DirectoryInfo(tempDirectoryPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
