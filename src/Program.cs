using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace _7DaysToBackup
{
    class Program
    {
        static string desktopPath { get; set; }
        static string appDataPath { get; set; }
        static bool isRunning { get; set; }
        static Process[] SevenDaysProcess { get; set; }
        static int SevenDaysPID { get; set; }
        static string folderDate { get; set; }
        static string folderDateFixed { get; set; }
        static void Main(string[] args)
        {
            Console.Title = "7 Days to Die Gamesave Backup Tool by Empyreal96";
            if (args.Length != 0)
            {
                
                
                
                    Console.WriteLine("7 Days to Die Gamesave Backup Tool (App Info) by Empyreal96");
                    Console.WriteLine("\n");
                    Console.WriteLine("This tools will attempt to automatically backup your World Saves to your Desktop each time you exit the game!");
                    Console.WriteLine("\n");
                    Console.WriteLine("Notes:");
                    Console.WriteLine("- This tool will NOT delete any data from your Game Saves or Backups, this is down to the User to manage what old saves are still backed up.");
                    Console.WriteLine("- Data will be saved to \"Desktop\\7_DAYS_TO_DIE_BACKUPS\" in chronological order of Date and Time.");
                    Console.WriteLine("- Launch this program before loading the game (Recommended), or on Windows Startup for convenience (optional)");
                    Console.WriteLine("Press ENTER key to exit");
                    Console.ReadLine();
                    Environment.Exit(0);
                
            }
            else
            {
                Console.WriteLine("7 Days to Die Gamesave Backup Tool by Empyreal96");
                Console.WriteLine("\n");
                ApplicationStart();
              
            }
        }

        static void ApplicationStart()
        {
            try
            { 
           
            isRunning = false;
            Console.WriteLine("Waiting for \"7DaysToDie.exe\" to start.");
            // Prepare folder directories
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Debug.WriteLine(desktopPath);
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Debug.WriteLine(appDataPath);

            isApplicationRunning();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR!\n");

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
            }
        }
       
        public static void isApplicationRunning()
        {
            try
            {
                while (isRunning == false)
                {
                    SevenDaysProcess = Process.GetProcessesByName("7DaysToDie");
                    if (SevenDaysProcess.Length == 0)
                    {
                        isRunning = false;
                        Thread.Sleep(5000);
                        isApplicationRunning();
                    }
                    else
                    {
                        isRunning = true;
                        foreach (var id in SevenDaysProcess)
                        {
                            Console.WriteLine($"Detected \"7DaysToDie.exe\" running! PID: {id.Id}");
                            SevenDaysPID = id.Id;
                        }

                        isWaitingForExit();
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine("ERROR!\n");

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
            }
        }

        public static void isWaitingForExit()
        {
            try
            {
                Console.WriteLine($"Monitoring Process \"{SevenDaysPID}:7DaysToDie.exe\" for exit.");
                while (isRunning == true)
                {
                   
                    Process GameProcess = Process.GetProcessById(SevenDaysPID);
                    GameProcess.EnableRaisingEvents = true;
                    GameProcess.Exited += new EventHandler(GameProcess_Exited);
                    
                    GameProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR!\n");

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
            }
        }
        
        private static void GameProcess_Exited(object sender, EventArgs e)
        {
            try
            {
                isRunning = false;
            Console.WriteLine("Game Process Closed, Enumerating files to backup");

                /// TODO: Start backup of files.
                /// 
                // DateTime dt = DateTime.ParseExact(DateTime.Now.ToString(), "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                folderDate = DateTime.UtcNow.ToString();
                folderDateFixed = folderDate.Replace('/', '_').Replace(':', '-');
                string SevenDaySaveFilesDir = Path.Combine(appDataPath + "\\7DaysToDie");
                string OutputDirectory = Path.Combine(desktopPath + $"\\7DaysToDie_BACKUPS\\{folderDateFixed}");


                Console.WriteLine($"Copying: {SevenDaySaveFilesDir} > {OutputDirectory}");
                Console.WriteLine("Please Wait.");
                CopyDirectory(SevenDaySaveFilesDir, OutputDirectory, true);
                Console.Clear();
                Console.WriteLine("Backup saved to " + OutputDirectory + "\n");

                SevenDaysProcess = null;
                SevenDaysPID = 0;
                GC.Collect();
                ApplicationStart();

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR!\n");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
            }
        }



        /// <summary>
        /// Copy a Directory and it's contents
        /// Example class taken from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="recursive"></param>
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
           
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }
            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    if (subDir.Name != "GeneratedWorlds" && subDir.Name != "Screenshots")
                    {
                        string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                        CopyDirectory(subDir.FullName, newDestinationDir, true);
                    }
                }
            }

            dirs = null;
        }
    }
}
