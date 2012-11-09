using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Threading;

namespace AWCLP
{

    /// <summary>
    /// Generates separate text files from an ActiveWorlds chat log
    /// </summary>
    class AWCLP
    {
        static Regex RgxSession =
            new Regex(@"^\* ActiveWorlds chat session: (.+?) \*$", RegexOptions.IgnoreCase);

        const string TERMINATOR = "*****************************************************";

        static string myPath;
        static StreamReader file;
        static string line;
        static string prevLine;
        static StreamWriter currentSection;

        static void WARN(string msg, params string[] parts)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg, parts);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void ERROR(string msg, params string[] parts)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg, parts);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                myPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                file = new StreamReader(args[0]);

                while (true)
                {
                    // Get the next line or finish conversion
                    line = file.ReadLine();
                    if (line == null) goto end;
                    else line = line.Trim();

                    // Discard empty and immediate duplicate lines
                    if (line == prevLine) continue;
                    else if (line == "") continue;

                    // New section hit
                    if (line == TERMINATOR)
                    {
                        // If returns false, we've hit the end of the file prematurely
                        if (!ProcessSection()) goto end;
                    }

                    // Write to current section
                    if (currentSection != null) currentSection.WriteLine(line);
                    prevLine = line;
                }
            }
            catch (Exception e)
            {
                ERROR("Exception hit: {0}", e.Message);
                Console.WriteLine(e.StackTrace);
                goto end;
            }


        end:
            CloseSection();

            if (file != null)
            {
                file.Dispose();
                file.Close();
            }
            Console.WriteLine("Finished. Press any key to quit.");
            Console.ReadKey(true);
        }

        static bool ProcessSection()
        {
            var nextLine = file.ReadLine();
            if (nextLine == null) return false;

            var sectionDate = RgxSession.Match(nextLine);
            if (!sectionDate.Success)
            {
                WARN("Could not match a date and time after terminator; continuing parsing section");
                return true;
            }

            // Get date
            var date = sectionDate.Groups[1].Value.Replace(':', '.');
            Console.WriteLine("\tProcessing new log for {0}", date);
            NewSection(date);

            // Skip next terminator
            file.ReadLine();
            return true;
        }

        static void NewSection(string name)
        {
            CloseSection();
            currentSection = File.CreateText(Path.Combine(myPath, name + ".txt"));
        }

        static void CloseSection()
        {
            if (currentSection == null) return;

            Console.WriteLine("\tFinished log section, closing file");
            currentSection.Flush();
            currentSection.Dispose();
            currentSection.Close();
            currentSection = null;
        }
    }
}
