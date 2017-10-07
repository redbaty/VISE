using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Vise.Config
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Colorful.Console.WriteAscii("Vise", Color.DodgerBlue);

            var vlcPath = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                vlcPath = Environment.GetEnvironmentVariable("AppData") + @"\vlc\vlcrc";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                vlcPath = "$(HOME)/.config/vlc/vlcrc";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                vlcPath = "HOME/Library/Preferences/org.videolan.vlc";
            }

            if (!File.Exists(vlcPath))
            {
                ConsoleUtils.Report("VLC Settings file not found", ReportType.ERROR);
                return;
            }

            ConsoleUtils.Report("Found VLC Settings file", ReportType.INFO);

            var fileContent = File.ReadAllLines(vlcPath);
            var extraintf = new KeyValue(fileContent.First(i => i.StartsWith("extraintf")));
            var httpPassword = new KeyValue(fileContent.First(i => i.StartsWith("http-password")));
            var rewriteFile = false;

            if (httpPassword.Value.Count == 0)
            {
                ConsoleUtils.Report("Type in a HTTP password", ReportType.INFO);
                Colorful.Console.Write("> ");
                var password = Console.ReadLine();
                httpPassword.Value.Add(password);

                for (var i = 0; i < fileContent.Length; i++)
                    if (fileContent[i].StartsWith("http-password"))
                        fileContent[i] = httpPassword.ToString();

                rewriteFile = true;
            }

            if (extraintf.Value.All(i => i != "http"))
            {
                ConsoleUtils.Report("HTTP interface not found, adding it.", ReportType.WARN);

                extraintf.AppendValue("http");

                for (var i = 0; i < fileContent.Length; i++)
                    if (fileContent[i].StartsWith("extraintf"))
                        fileContent[i] = extraintf.ToString();

                rewriteFile = true;
            }

            if (rewriteFile)
            {
                File.WriteAllLines(vlcPath, fileContent);
                ConsoleUtils.Report("Sucessfully wrote the settings file");
            }

            ConsoleUtils.Report("Press any key to close");
            Console.ReadKey();
        }
    }
}