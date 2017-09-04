using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostBuildLog : ScriptableObject
{
    [PostProcessBuild]
    private static void OnPostProcessBuildPlayer(BuildTarget target, string buildPath)
    {
        WriteBuildLog(buildPath, target.ToString());
    }

    public static void WriteBuildLog(string buildPath, string target = "")
    {
        if (string.IsNullOrEmpty(buildPath))
            return;
        string editorLogFilePath = null;
        string[] pieces = null;

        var winEditor = Application.platform == RuntimePlatform.WindowsEditor;

        if (winEditor)
        {
            editorLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            pieces = new[] {"Unity", "Editor", "Editor.log"};
        }
        else
        {
            editorLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            pieces = new[] {"Library", "Logs", "Unity", "Editor.log"};
        }

        foreach (var e in pieces)
        {
            editorLogFilePath = Path.Combine(editorLogFilePath, e);
        }

        if (!File.Exists(editorLogFilePath))
        {
            Debug.LogWarning("Editor log file could not be found at: " + editorLogFilePath);
            return;
        }

        var report = new StringBuilder();

        using (
            var reader =
                new StreamReader(File.Open(editorLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                if (line.StartsWith("Mono dependencies included in the build"))
                {
                    report.Length = 0;
                    report.AppendFormat("Build Report @ {0}\n\n", DateTime.Now.ToString("u"));
                }
                report.AppendLine(line);
            }
        }

        if (report.Length == 0)
        {
            return; // No builds have been run.
        }

        string outputPath = null;

        outputPath = Path.Combine(Path.GetDirectoryName(buildPath), "build.log");

        var buildLogFile = new FileInfo(outputPath);

        try
        {
            using (var writer = buildLogFile.CreateText())
            {
                writer.Write(report.ToString());
            }
        }
        catch
        {
            Debug.LogError("Build log file could not be created for writing at: " + buildLogFile.FullName);
        }
    }
}