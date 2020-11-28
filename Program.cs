using System;
using System.IO;
using System.Collections.Generic;

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
            if(!Directory.Exists(oldReplaysDirectory)) {
                Console.Error.WriteLine("Replays directory \"{0}\" not found. Please put your replays in this directory!", oldReplaysDirectory);
                return;
            }

            string[] oldReplays = Directory.GetFiles(oldReplaysDirectory);
            for(int i = 0; i < oldReplays.Length; i++) {
                updateReplay(oldReplays[i]);

                // Print a message to show progress
                Console.WriteLine("Updated replay {0}/{1} ({2})", i + 1, oldReplays.Length, Path.GetFileNameWithoutExtension(oldReplays[i]));
            }

            Console.WriteLine("All replays converted!");
        }

        private void updateReplay(string fileName) {
            // Read the contents of the replay file as text
            string replayContents = File.ReadAllText(fileName);

            // Display an error if the replay failed to avoid crashing the program
            Replay updatedReplay;
            try {
                // Load it using the old method
                updatedReplay = Replay.loadOldReplay(replayContents);
            }   catch(Exception ex) {
                Console.Error.WriteLine("An error occured while processing replay {0}: {1}", fileName, ex.Message);
                return;
            }

            // Save the loaded replay using the binary format
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            FileStream saveFile = File.Open(resultDirectory + "/" + nameWithoutExtension + "." + replayFileExtension, FileMode.Create);
            updatedReplay.saveWithNewFormat(new BinaryWriter(saveFile));
            saveFile.Close();
        }

        private static void Main(string[] args)
        {
            new Program(args);
        }
    }
}
