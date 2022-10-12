using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VGMToolbox.format;

namespace UsmToolkit
{
    [Command(Description = "Extract audio and video")]
    public class ExtractCommand
    {
        [Required]
        [FileOrDirectoryExists]
        [Argument(0, Description = "File or folder containing usm files")]
        public string InputPath { get; set; }
        
        [Required]
        [Option(CommandOptionType.SingleValue, Description = "Specify output directory", ShortName = "o", LongName = "output-dir")]
        public string OutputDir { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Specify number of threads to use (default : 1)",
            ShortName = "t", LongName = "threads")]
        public int MaxNumberOfThreads { get; set; } = 1;

        protected int OnExecute(CommandLineApplication app)
        {
            var attr = File.GetAttributes(InputPath);
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = MaxNumberOfThreads
            };
            if (attr.HasFlag(FileAttributes.Directory))
                Parallel.ForEach(Directory.GetFiles(InputPath, "*.usm"), parallelOptions ,Process);
            else
                Process(InputPath);

            return 0;
        }

        private void Process(string fileName)
        {
            Console.WriteLine($"File: {fileName}");
            var usmStream = new CriUsmStream(fileName);

            Console.WriteLine("Demuxing...");
            usmStream.DemultiplexStreams(new MpegStream.DemuxOptionsStruct()
            {
                AddHeader = false,
                AddPlaybackHacks = false,
                ExtractAudio = true,
                ExtractVideo = true,
                ExtractSubtitles = true,
                SplitAudioStreams = false,
                OutputDir = this.OutputDir
            });
            
            if (string.IsNullOrEmpty(usmStream.SubtitleFilePath)) return;
            Console.WriteLine("Converting Subs...");
            ConvertCommand.ConvertSubs(usmStream.SubtitleFilePath);
            File.Delete(usmStream.SubtitleFilePath);
        }
    }
    
    [Command(Description = "Convert according to the parameters in config.json")]
    public class ConvertCommand
    {
        public static readonly JoinConfig CONF = JsonConvert.DeserializeObject<JoinConfig>(File.ReadAllText(Program.AbsolutePath("config.json")));
        
        [Required]
        [FileOrDirectoryExists]
        [Argument(0, Description = "File or folder containing usm files")]
        public string InputPath { get; set; }

        [Required]
        [Option(CommandOptionType.SingleValue, Description = "Specify output directory", ShortName = "o", LongName = "output-dir")]
        public string OutputDir { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Remove temporary m2v and audio after converting", ShortName = "c", LongName = "clean")]
        public bool CleanTempFiles { get; set; }
        
        [Option(CommandOptionType.SingleValue, Description = "Specify number of threads to use (default : 1)",
            ShortName = "t", LongName = "threads")]
        public int MaxNumberOfThreads { get; set; } = 1;

        protected int OnExecute(CommandLineApplication app)
        {
            var conf = JsonConvert.DeserializeObject<JoinConfig>(File.ReadAllText(Program.AbsolutePath("config.json")));
            var attr = File.GetAttributes(InputPath);
            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = MaxNumberOfThreads
            };
            if (attr.HasFlag(FileAttributes.Directory))
                Parallel.ForEach(Directory.GetFiles(InputPath, "*.usm"), parallelOptions, Process);
            else
                Process(InputPath);

            return 0;
        }

        private void Process(string fileName)
        {
            Console.WriteLine($"File: {fileName}");
            var usmStream = new CriUsmStream(fileName);

            Console.WriteLine("Demuxing...");
            usmStream.DemultiplexStreams(new MpegStream.DemuxOptionsStruct()
            {
                AddHeader = false,
                AddPlaybackHacks = false,
                ExtractAudio = true,
                ExtractVideo = true,
                ExtractSubtitles = true,
                SplitAudioStreams = false,
                OutputDir = this.OutputDir
            });

            if (!string.IsNullOrEmpty(OutputDir) && !Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            ConvertOutputFile(usmStream);
            if (string.IsNullOrEmpty(usmStream.SubtitleFilePath)) return;
            Console.WriteLine("Converting Subs...");
            ConvertSubs(usmStream.SubtitleFilePath);
            File.Delete(usmStream.SubtitleFilePath);
        }

        public static void ConvertSubs(string file)
        {
            var path = file[..^4] + ".txt";
            List<string> lines = new List<string>();
            
            var framerate = 0;
            var inputSubBytes = File.ReadAllBytes(file);
            var offset = 0;
            
            while (offset + 32 < inputSubBytes.Length) // Should be good in theory
            {
                var framerateHex = new byte[4];
                Array.Copy(inputSubBytes, offset + 4, framerateHex, 0, 4);
                var startHex = new byte[4];
                Array.Copy(inputSubBytes, offset + 8, startHex, 0, 4);
                var durationHex = new byte[4];
                Array.Copy(inputSubBytes, offset + 12, durationHex, 0, 4);
                var subLengthHex = new byte[4];
                Array.Copy(inputSubBytes, offset + 16, subLengthHex, 0, 4);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(framerateHex);
                    Array.Reverse(startHex);
                    Array.Reverse(durationHex);
                    Array.Reverse(subLengthHex);
                }
                if (framerate == 0)
                {
                    framerate = BitConverter.ToInt32(framerateHex, 0);
                    lines.Add(framerate.ToString());
                }
                var start = BitConverter.ToInt32(startHex, 0);
                var duration = BitConverter.ToInt32(durationHex, 0); // We DO NOT ignore the 4 bytes after this
                var subLength = BitConverter.ToInt32(subLengthHex, 0);
                offset += 20;
                var text = new StringBuilder();
                byte[] text = new byte[0];
                for (int i = 1; i <= subLength; i++)
                {
					byte[] AddByteToArray(byte[] bArray, byte newByte)
					{
						byte[] newArray = new byte[bArray.Length + 1];
						bArray.CopyTo(newArray, 0);
						newArray[bArray.Length] = newByte;
						return newArray;
					}
					text = AddByteToArray(text, inputSubBytes[offset]);
					offset++;
                }
                string realText = Encoding.UTF8.GetString(text);
                lines.Add(start + ", " + (start + duration) + ", " + realText);
            }
            
            File.WriteAllLines(path, lines);
        }

        private void ConvertOutputFile(CriUsmStream usmStream)
        {
            if (!File.Exists(Program.AbsolutePath("config.json")))
            {
                Console.WriteLine("ERROR: config.json not found!");
                return;
            }

            var audioFormat = usmStream.FinalAudioExtension;
            var pureFileName = Path.GetFileNameWithoutExtension(usmStream.FilePath);

            if (audioFormat == ".adx")
            {
                //ffmpeg can not handle .adx from 0.2 for whatever reason
                //need vgmstream to format that to wav
                if (!Directory.Exists(Program.AbsolutePath("vgmstream")))
                {
                    Console.WriteLine("ERROR: vgmstream folder not found!");
                    return;
                }

                Console.WriteLine("adx audio detected, convert to wav...");
                foreach (var file in usmStream.CreatedFiles.Where(file => file.EndsWith(".adx")))
                {
                    Helpers.ExecuteProcess(Program.AbsolutePath("vgmstream/test"), $"\"{Path.ChangeExtension(file, usmStream.FinalAudioExtension)}\" -o \"{Path.ChangeExtension( Path.Combine(OutputDir, Path.GetFileName(file)), "wav")}\"");
                }
                
                usmStream.FinalAudioExtension = ".wav";
            }

            usmStream.HasAudio = false;

            Helpers.ExecuteProcess(Program.AbsolutePath("ffmpeg/ffmpeg"), Helpers.CreateFFmpegParameters(usmStream, pureFileName, OutputDir, CONF));

            if (!CleanTempFiles) return;
            Console.WriteLine($"Cleaning up temporary files from {pureFileName}");
            foreach (var file in usmStream.CreatedFiles)
            {
                File.Delete(Path.ChangeExtension(file, "adx"));
                File.Delete(Path.ChangeExtension(file, "hca"));
                File.Delete(Path.ChangeExtension(file, "m2v"));
            }
        }
    }
}
