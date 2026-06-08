using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

namespace KeychainQuickDesigner.Module.Helpers
{

    public static class PdfOperations
    {
        public static void MergePdfFiles(List<string> sourceFiles, string destinationPath, IProgress<int> progress)
        {
            if (sourceFiles == null || sourceFiles.Count == 0)
                throw new ArgumentException("No PDF files provided to merge.");
            if (!Path.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            string destinationFile = Path.Combine(destinationPath, $"{sourceFiles.Count}_{Guid.NewGuid()}.pdf");

            // Create the output document
            PdfDocument outputDocument = new();

            for (int i = 0; i < sourceFiles.Count; i++)
            {
                string? file = sourceFiles[i];
                // Open the file
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                // Iterate pages
                for (int idx = 0; idx < inputDocument.PageCount; idx++)
                {
                    // Get the page from the external document...
                    PdfPage page = inputDocument.Pages[idx];
                    // ...and add it to the output document.
                    outputDocument.AddPage(page);
                }
                progress.Report((i + 1) * 100 / sourceFiles.Count);
            }

            // Save the document to file
            outputDocument.Save(destinationFile);
        }
    }
}
