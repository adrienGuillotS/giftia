using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BirthdayCertificateDesigner2023
{
    public static class ZipFileHelper
    {
        public static void ReplaceSourceFileInsideZip(string zipFilePath,
           string fileNameToReplace, string replacementFileContent,
           string tempZipFilePath)
        {
            // Open the original ZIP file and create a temporary copy
            using (FileStream originalFileStream = new FileStream(zipFilePath, FileMode.Open))
            using (FileStream tempFileStream = new FileStream(tempZipFilePath, FileMode.Create))
            {
                // Copy the contents of the original ZIP file to the temporary file
                originalFileStream.CopyTo(tempFileStream);
            }

            // Open the temporary ZIP file for modification
            using (ZipArchive archive = ZipFile.Open(tempZipFilePath, ZipArchiveMode.Update))
            {
                // Locate the entry (file) inside the ZIP archive
                ZipArchiveEntry? entry = archive.GetEntry(fileNameToReplace);

                // Check if the entry exists
                if (entry != null)
                {
                    // Delete the existing entry
                    entry.Delete();
                }

                // Create a new entry with the same name and write the replacement content
                entry = archive.CreateEntry(fileNameToReplace);
                using (StreamWriter writer = new StreamWriter(entry.Open()))
                {
                    writer.Write(replacementFileContent);
                }
            }

            // Replace the original ZIP file with the modified one
            File.Copy(tempZipFilePath, zipFilePath, true);

            // Delete the temporary ZIP file
            File.Delete(tempZipFilePath);

            Console.WriteLine("File content replaced in the ZIP archive successfully.");
        }
    }
}
