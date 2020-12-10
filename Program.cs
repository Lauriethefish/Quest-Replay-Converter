using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace ReplayUpdator
{
    class Program
    {
        // I'm not kidding, this is the new file extension for replays
        const string replayFileExtension = "questReplayFileForQuestDontTryOnPcAlsoPinkEraAndLillieAreCuteBtwWilliamGay";
        const string oldReplaysDirectory = "replays";
        const string resultDirectory = "converted";

        public Program(string[] args) {
            // Make the folder for the replays to be outputted in
            Console.WriteLine("Creating converted replays folder . . .");
            Directory.CreateDirectory(resultDirectory);

            // Loop through each replay in the old replays folder, and update it
            Console.WriteLine("Updating replays . . .");

            // Check to see if the replays directory exists

            Console.WriteLine("Do you want to use adb to pull old replays? (y/n)");
            String selection = Console.ReadLine();
            if (selection.StartsWith("y")) PullReplays();


            recursivefoldercheck(AppDomain.CurrentDomain.BaseDirectory);
            if (!Directory.Exists(oldReplaysDirectory)) {
                Console.Error.WriteLine("Replays directory \"{0}\" not found. Please put your replays in this directory!", oldReplaysDirectory);
                Console.ReadLine();
                return;
            }

            string[] oldReplays = Directory.GetFiles(oldReplaysDirectory);
            for(int i = 0; i < oldReplays.Length; i++) {

                // Print a message to show progress after updating the replay
                if(updateReplay(oldReplays[i], i + 1))
                {
                    Console.WriteLine("Updated replay {0}/{1} ({2})", i + 1, oldReplays.Length, Path.GetFileNameWithoutExtension(oldReplays[i]));
                }
            }

            Console.WriteLine("All replays converted!");
            Console.ReadLine();
        }

        private void PullReplays()
        {
            // Console.WriteLine("Pulling Replays");

            // Pull all replays
            adb("pull /sdcard/Android/data/com.beatgames.beatsaber/files/replays \"" + AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1) + "\"");
        }

        private Boolean adb(String Argument)
        {
            ArrayList ADBPaths = new ArrayList();
            // Add all paths to ADB (with file extension) here
            ADBPaths.Add("adb.exe");
            ADBPaths.Add("User\\Android\\platform-tools_r29.0.4-windows\\platform-tools\\adb.exe");
            ADBPaths.Add("User\\AppData\\Roaming\\SideQuest\\platform-tools\\adb.exe");

            // Get user env variable for Windows
            String User = System.Environment.GetEnvironmentVariable("USERPROFILE"); //This May not work on Ubuntu and MacOS. Please check that.

            foreach (String ADB in ADBPaths)
            {
                ProcessStartInfo s = new ProcessStartInfo();
                s.CreateNoWindow = false;
                s.UseShellExecute = false;
                s.FileName = ADB.Replace("User", User);
                s.WindowStyle = ProcessWindowStyle.Minimized;
                s.Arguments = Argument;
                s.RedirectStandardOutput = false;
                // To check if a device is found set redirectstandartoutput to true
                try
                {
                    // Start Process
                    using (Process exeProcess = Process.Start(s))
                    {
                        //String IPS = exeProcess.StandardOutput.ReadToEnd();
                        exeProcess.WaitForExit();
                        /*
                        if (IPS.Contains("no devices/emulators found"))
                        {
                            Console.WriteLine("Couldn't find your Quest. Check following:\n- Your Quest is connected, Developer Mode enabled and USB Debugging enabled.");
                            return false;
                        }
                        */

                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            // Nothing worked
            Console.WriteLine("Couldn't find ADB.");
            return false;
        }

        private void recursivefoldercheck(String folder)
        {
            // Move all files that end with .txt in the programs folder into the replays folder
            foreach (String CurrentFolder in Directory.GetDirectories(folder))
            {
                recursivefoldercheck(CurrentFolder);
                String FolderName = new DirectoryInfo(CurrentFolder).Name;
                if (!FolderName.EndsWith(oldReplaysDirectory))
                {
                    foreach (String file in Directory.GetFiles(CurrentFolder))
                    {
                        if (file.EndsWith(".txt"))
                        {
                            File.Move(file, oldReplaysDirectory + "\\" + Path.GetFileName(file));
                            Console.WriteLine("Moved old replay to right directory (" + Path.GetFileNameWithoutExtension(file) + ")");
                        }
                    }
                }
            }
            if(folder == AppDomain.CurrentDomain.BaseDirectory)
            {
                foreach (String file in Directory.GetFiles(folder))
                {
                    if (file.EndsWith(".txt"))
                    {
                        File.Move(file, oldReplaysDirectory + "\\" + Path.GetFileName(file));
                        Console.WriteLine("Moved old replay to right directory (" + Path.GetFileNameWithoutExtension(file) + ")");
                    }
                }
            }
        }

        private Boolean updateReplay(string fileName, int CurrentFile) {
            // Check if the file is a replay
            if(!fileName.EndsWith(".txt"))
            {
                if(fileName.EndsWith("." + replayFileExtension))
                {
                    Console.WriteLine("File " + CurrentFile + "/" + Directory.GetFiles(oldReplaysDirectory).Length + " (" + Path.GetFileNameWithoutExtension(fileName) + ") is a new Replay. Skipping.");
                } else
                {
                    Console.WriteLine("File " + CurrentFile + "/" + Directory.GetFiles(oldReplaysDirectory).Length + " (" + Path.GetFileNameWithoutExtension(fileName) + ") is no old Replay. Skipping.");
                }
                return false;
            }
            // Read the contents of the replay file as text
            string replayContents = File.ReadAllText(fileName);

            // Display an error if the replay failed to avoid crashing the program
            Replay updatedReplay;
            try {
                // Load it using the old method
                updatedReplay = Replay.loadOldReplay(replayContents);
            }   catch(Exception ex) {
                Console.Error.WriteLine("An error occured while processing replay {0}: {1}", fileName, ex.Message);
                return false;
            }

            // Save the loaded replay using the binary format
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            FileStream saveFile = File.Open(resultDirectory + "/" + nameWithoutExtension + "." + replayFileExtension, FileMode.Create);
            updatedReplay.saveWithNewFormat(new BinaryWriter(saveFile));
            saveFile.Close();
            return true;
        }

        private static void Main(string[] args)
        {
            new Program(args);
        }
    }
}
