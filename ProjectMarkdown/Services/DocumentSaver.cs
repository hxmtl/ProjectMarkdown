﻿using System;
using System.IO;
using System.IO.Compression;
using IOUtils;
using LogUtils;
using Microsoft.Win32;
using ProjectMarkdown.ExtensionMethods;
using ProjectMarkdown.MarkdownLibrary;
using ProjectMarkdown.Model;
using ProjectMarkdown.Statics;

namespace ProjectMarkdown.Services
{
    public static class DocumentSaver
    {
        public static SaveResult SaveAs(DocumentModel document, string style)
        {
            Logger.GetInstance().Debug("SaveAs() >>");

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    CreatePrompt = true,
                    OverwritePrompt = true,
                    Title = "Save a PMD file",
                    Filter = "Project Markdown File | *.pmd"
                };

                var result = saveDialog.ShowDialog();

                if (result != null)
                {
                    if (result == true)
                    {
                        var tempFolder = FolderPaths.TempFolderPath + "\\" + saveDialog.SafeFileName + "_temp";

                        if (Directory.Exists(tempFolder))
                        {
                            Directory.Delete(tempFolder, true);
                        }

                        var parentFolder = Directory.CreateDirectory(tempFolder).FullName;

                        var mp = new MarkdownParser();
                        // Generate HTML
                        var html = mp.Parse(document.Markdown, style);

                        var markdownFileName = saveDialog.SafeFileName + ".md";
                        var markdownFilePath = parentFolder + "\\" + markdownFileName;
                        var htmlFileName = saveDialog.SafeFileName + ".html";
                        var htmlFilePath = parentFolder + "\\" + htmlFileName;
                        var xmlFileName = saveDialog.SafeFileName + ".xml";
                        var metadataFilePath = parentFolder + "\\" + xmlFileName;
                        // Generate MD file
                        using (var sw = new StreamWriter(markdownFilePath))
                        {
                            sw.Write(document.Markdown);
                        }
                        // Generate HTML file
                        using (var sw = new StreamWriter(htmlFilePath))
                        {
                            sw.Write(html);
                        }

                        document.FilePath = saveDialog.FileName;

                        // Generate XML file
                        document.Metadata.FileName = saveDialog.SafeFileName;
                        document.Metadata.IsNew = false;
                        var gxs = new GenericXmlSerializer<DocumentMetadata>();
                        gxs.Serialize(document.Metadata, metadataFilePath);
                        // Generate the package
                        if (File.Exists(document.FilePath))
                        {
                            File.Delete(document.FilePath);
                        }
                        ZipFile.CreateFromDirectory(parentFolder, saveDialog.FileName);
                        // Update the view
                        var saveResult = new SaveResult
                        {
                            FileName = saveDialog.SafeFileName,
                            Source = htmlFilePath.ToUri(),
                            TempFile = tempFolder
                        };

                        Logger.GetInstance().Debug("<< SaveAs()");
                        return saveResult;
                    }
                }

                Logger.GetInstance().Debug("<< SaveAs()");
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static SaveResult Save(DocumentModel document, string style)
        {
            Logger.GetInstance().Debug("Save() >>");

            try
            {
                var tempFolder = FolderPaths.TempFolderPath + "\\" + document.Metadata.FileName + "_temp";

                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }

                var parentFolder = Directory.CreateDirectory(tempFolder).FullName;

                var mp = new MarkdownParser();
                // Generate HTML
                var html = mp.Parse(document.Markdown, style);

                var markdownFileName = document.Metadata.FileName + ".md";
                var markdownFilePath = parentFolder + "\\" + markdownFileName;
                var htmlFileName = document.Metadata.FileName + ".html";
                var htmlFilePath = parentFolder + "\\" + htmlFileName;
                var xmlFileName = document.Metadata.FileName + ".xml";
                var metadataFilePath = parentFolder + "\\" + xmlFileName;
                // Generate MD file
                using (var sw = new StreamWriter(markdownFilePath))
                {
                    sw.Write(document.Markdown);
                }
                // Generate HTML file
                using (var sw = new StreamWriter(htmlFilePath))
                {
                    sw.Write(html);
                }

                document.FilePath = document.FilePath;

                // Generate XML file
                document.Metadata.FileName = document.Metadata.FileName;
                document.Metadata.IsNew = false;
                var gxs = new GenericXmlSerializer<DocumentMetadata>();
                gxs.Serialize(document.Metadata, metadataFilePath);

                // Generate the package
                if (File.Exists(document.FilePath))
                {
                    File.Delete(document.FilePath);
                }
                ZipFile.CreateFromDirectory(parentFolder, document.FilePath);
                // Update the view
                var saveResult = new SaveResult
                {
                    FileName = document.Metadata.FileName,
                    Source = htmlFilePath.ToUri(),
                    TempFile = tempFolder
                };

                Logger.GetInstance().Debug("<< Save()");
                return saveResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
