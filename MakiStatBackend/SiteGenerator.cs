using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MakiStatBackend;

public class WebsiteGenerationErrorException : Exception
{
    public Stack<string> TemplateStack { get; private set; }

    public WebsiteGenerationErrorException(string message, Stack<string> templateStack) : base(message)
    {
        TemplateStack = templateStack;
    }
}

//TODO Support JSON syntax errors
//BONUS add choosing to render part of the website
/// <summary>
/// Main component of the  Static Site Generator. It mediates between the UI and the template engine.
///
/// This is the framework which by default, requires making skeleton.html, which contain the HTML elements used in all pages,
/// multiple skeleton.[language code].json files, where language code means the language of data this file contains and two folders:
/// _global, _main and output. _global folder is meant to store data and partials meant to be used across multiple static pages.
/// _main folder is meant to store HTML templates representing output pages and its folder structure is perfectly mapped to the output
/// (e.g. if the HTML page is in _main/categories/animals.html, it will be in output/categories/animals.html).
/// Names of HTML templates representing output pages must not begin from underscore, since such files are recognised as partials.
/// Page HTML templates will have the same name as output page and will be stored in analogous folder relative to _main. Each of them must
/// have the JSON file of the same name and at least one page with compound extension .[lang code].json. They need to be in the same folder as
/// corresponding HTML file. However, not all pages need to have exactly the same set of languages it would be available. When the
/// language JSON is omitted, the webpage in defined language will not be generated.
/// 
/// One of the language codes must have value "default" and is recommended to be used as the main language of the website. HTML pages
/// of default lang code will be rendered in root output folder instead of one of subfolders which have the same name as the remaining
/// lang codes. Lang codes can be defined by user, although codes used for locales are recommended.
/// 
/// Type of the templates this class can handle is dependent on implementation of templateEngine field.
/// </summary>
public class SiteGenerator
{
    public ILogger Logger { get; private set; }
    public string MainPath { get; private set; } = "_main";
    public string GlobalPath { get; private set; } = "_global";
    public string OutputPath { get; private set; } = "output";
    public Stack<string> TemplateStack { get; private set; } = new Stack<string>();
    internal ModificationChecker ModificationChecker { get; private set; }

    private readonly ITemplateEngine templateEngine;

    public SiteGenerator() : this(new FileLogger()) { }
    public SiteGenerator(ILogger logger)
    {
        Logger = logger;
        ModificationChecker = new ModificationChecker() { OutputPath = OutputPath };
        templateEngine = new ScribanGenerationEngine(this);
    }

    public SiteGenerator(ITemplateEngine templateEngine) : this(new FileLogger())
    {
        this.templateEngine = templateEngine;
    }

    /// <summary>
    /// Extracts language code from JSON file name.
    /// </summary>
    /// <param name="jsonFilePath">Path to the JSON file name which contains language code for extraction.</param>
    /// <returns>Extracted language code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ExtractLangCode(string jsonFilePath)
    {
        string filename = Path.GetFileName(jsonFilePath);
        string[] fragments = filename.Split('.');
        //if there are two fragments, return lang code
        return fragments[^2];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GenerateLanguageDirPath(string languageCode) => languageCode != "default" ? "/" + languageCode : null;

    //TODO switch your logger to built-in logger
    /// <summary>
    /// Generates the static multilingual website.
    /// </summary>
    /// <param name="skeletonPath">Path to the skeleton HTML template.</param>
    /// <param name="progressReporter">It passes the progress data to the UI.</param>
    /// <exception cref="FileNotFoundException">It is thrown when the skeleton HTML file does not exists.</exception>
    /// <remarks>Calling this method sets skeletonPath parameter as current directory.</remarks>
    public void GenerateSite(string skeletonPath, IWebsiteGenerationProgressReporter progressReporter)
    {
        if (skeletonPath == null || skeletonPath == string.Empty)
            throw new FileNotFoundException($"You did not enter the path of the skeleton.{Environment.NewLine}Please give the path to the existing skeleton of the page.");

        Environment.CurrentDirectory = Path.GetDirectoryName(skeletonPath);
        Logger.Open();
        TemplateStack.Clear();
        string[] filesinMainFolder = Directory.GetFiles(MainPath, "*.*", new EnumerationOptions() { RecurseSubdirectories = true });
        string outputDirectory = "output";
        Directory.CreateDirectory(outputDirectory);

        string skeletonFileName = Path.GetFileNameWithoutExtension(skeletonPath);
        string skeletonHtml = File.ReadAllText($"{skeletonFileName}.html");
        ModificationChecker.LoadModificationData(skeletonHtml);

        string[] jsonLanguageFilePaths = Directory.GetFiles(Environment.CurrentDirectory, $"{skeletonFileName}*.json", SearchOption.TopDirectoryOnly);

        float maxPageNumber = filesinMainFolder.Length * jsonLanguageFilePaths.Length;
        float pageNumber = 0;

        string[] langCodes = jsonLanguageFilePaths.Select(lc => ExtractLangCode(lc)).ToArray();
        using (Logger)
        {
            foreach (string jsonFilePath in jsonLanguageFilePaths)
            {
                string currentLangCode = ExtractLangCode(jsonFilePath);
                Dictionary<string, object> globalData = JsonProcessor.ReadJSONModelFromJSONFile(jsonFilePath);

                foreach (string path in filesinMainFolder)
                {
                    pageNumber++;
                    float progressPercent = (pageNumber / maxPageNumber) * 100;
                    progressReporter.ReportProgress(Convert.ToInt32(progressPercent), path);

                    string ext = Path.GetExtension(path);
                    string pageFileName = Path.GetFileName(path);
                    string relativeFilePath = Path.GetRelativePath(MainPath, path);
                    if (ext is ".html" or ".sbn-html")
                    {
                        if (!pageFileName.StartsWith('_')) //It is not a partial
                        {
                            try
                            {
                                string[] availableLangCodes = langCodes.Where(lc =>
                                    File.Exists($"{MainPath}/{Path.GetDirectoryName(relativeFilePath)}/{Path.GetFileNameWithoutExtension(pageFileName)}.{lc}.json"))
                                    .ToArray();

                                string langFolder = currentLangCode == "default" ? string.Empty : $"{currentLangCode}";
                                string rootDir = Path.Combine(outputDirectory, langFolder);
                                string destDir = Path.Combine(rootDir, Path.GetDirectoryName(relativeFilePath));
                                string fileDest = Path.Combine(destDir, $"{Path.GetFileNameWithoutExtension(relativeFilePath)}.html");
                                string pathForMc = Path.Combine(langFolder, Path.GetDirectoryName(relativeFilePath), $"{Path.GetFileNameWithoutExtension(relativeFilePath)}.html");

                                if (ModificationChecker.CheckIfPageIsNotModified(pathForMc, fileDest))
                                {
                                    TemplateStack.Push(fileDest);
                                    string generatedPage = templateEngine.GeneratePage(path, skeletonHtml, globalData, currentLangCode, availableLangCodes);
                                    TemplateStack.Pop();

                                    ModificationChecker.AddResourceToModificationChecking(pathForMc, jsonFilePath, File.ReadAllText(jsonFilePath));
                                    //I have place above statement here, since it will not be reached when exception is thrown
                                    _ = Directory.CreateDirectory(destDir);
                                    File.WriteAllText(fileDest, generatedPage);
                                }
                                ModificationChecker.RegisterPageForChecking(pathForMc);
                            }
                            catch (FileNotFoundException ex)
                            {
                                Logger.Warning(TemplateStack, ex.Message);
                                TemplateStack.Pop();
                            }
                        }
                    }
                    else if (ext is not ".json" and not ".sbn")
                    {
                        //copy file to equivalent folder
                        string destDir = Path.GetDirectoryName(relativeFilePath);
                        Directory.CreateDirectory(destDir);
                        File.Copy($"{MainPath}/{relativeFilePath}", $"{outputDirectory}/{relativeFilePath}", true);
                    }
                }
            }// end foreach
            ModificationChecker.SaveLastWebsiteStateIfNeeded();
        }
    }
}
