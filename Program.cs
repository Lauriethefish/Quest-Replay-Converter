using System;
using System.IO;
using System.Collections.Generic;

namespace ReplayUpdator
{
    class Program
    {
        public Program(string[] args) {
            // Make the folder for the replays to be outputted in
            Console.WriteLine("Creating converted replays folder . . .");
            Directory.CreateDirectory("./converted");

            // Loop through each replay in the old replays folder, and update it
            Console.WriteLine("Updating replays . . .");

            string[] oldReplays = Directory.GetFiles("./replays");
            for(int i = 0; i < oldReplays.Length; i++) {
                updateReplay(oldReplays[i]);

                // Print a message to show progress
                Console.WriteLine("Updated replay {0}/{1} ({2})", i + 1, oldReplays.Length, Path.GetFileNameWithoutExtension(oldReplays[i]));
            }
        }

        private void updateReplay(string fileName) {
            // Read the contents of the replay file as text
            string replayContents = File.ReadAllText(fileName);

            // Load it using the old method
            Replay updatedReplay = Replay.loadOldReplay(replayContents);

            // Save the loaded replay using the binary format
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            FileStream saveFile = File.Open("converted/" + nameWithoutExtension + ".qplay", FileMode.Create);
            updatedReplay.saveWithNewFormat(new BinaryWriter(saveFile));
            saveFile.Close();
        }

        private static void Main(string[] args)
        {
            new Program(args);
        }
    }
}
