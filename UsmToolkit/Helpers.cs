﻿using System;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Text;
using VGMToolbox.format;

namespace UsmToolkit
{
    public static class Helpers
    {
        public static void ExecuteProcess(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();
        }

        public static string CreateFFmpegParameters(CriUsmStream usmStream, string pureFileName, string outputDir)
        {
            JoinConfig conf = JsonConvert.DeserializeObject<JoinConfig>(File.ReadAllText("config.json"));

            StringBuilder sb = new StringBuilder();
            sb.Append($"-i \"{Path.ChangeExtension(usmStream.VideoFilePath, usmStream.FileExtensionVideo)}\" ");

            sb.Append($"-c:v {conf.VideoParameter} ");

            sb.Append($"\"{Path.Combine(outputDir ?? string.Empty, $"{pureFileName}.{conf.OutputFormat}")}\"");
            
            return sb.ToString();
        }
    }
}
