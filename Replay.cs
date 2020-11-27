using System.IO;
using System.Collections.Generic;

// Stores a position and rotation in 3D space
public struct Location {
    public Vector3 position;
    public Vector3 rotation;

    public Location(Vector3 position, Vector3 rotation) {
        this.position = position;
        this.rotation = rotation;
    }

    public void saveWithNewFormat(BinaryWriter writer) {
        position.saveWithNewFormat(writer);
        rotation.saveWithNewFormat(writer);
    }
}

// Stores one "point" in a replay
public struct KeyFrame {
    public Location rightSaberLoc;
    public Location leftSaberLoc;

    public Location headLoc;

    public int score;
    public float percent;
    public int rank;
    public int combo;
    public float time;

    // Saves this KeyFrame to a binary writer, since replays are now stored in a binary format
    public void saveWithNewFormat(BinaryWriter writer) {
        rightSaberLoc.saveWithNewFormat(writer);
        leftSaberLoc.saveWithNewFormat(writer);
        headLoc.saveWithNewFormat(writer);

        writer.Write(score);
        writer.Write(percent);
        writer.Write(rank);
        writer.Write(combo);
        writer.Write(time);
    }

    // Used in older replay formats that don't store the current rank inside the replays
    private void getRankFromPercentage() {
        if(percent < 0.2) {
            rank = 0;
        }   else if(percent < 0.35) {
            rank = 1;
        }   else if(percent < 0.5) {
            rank = 2;
        }   else if(percent < 0.65) {
            rank = 3;
        }   else if(percent < 0.8) {
            rank = 4;
        }   else if(percent < 0.9){
            rank = 5;
        }   else    {
            rank = 6;
        }
    }

    // Loads all the values of this replay from the legacy replay format, this is janky AF
    public static KeyFrame loadFromStringNew(StringReader reader) {
        KeyFrame result = new KeyFrame();
        result.rightSaberLoc.position = Vector3.loadFromString(reader);
        result.rightSaberLoc.rotation = Vector3.loadFromString(reader);
        result.leftSaberLoc.position = Vector3.loadFromString(reader);
        result.leftSaberLoc.rotation = Vector3.loadFromString(reader);
        result.score = int.Parse(reader.ReadWord());
        result.time = float.Parse(reader.ReadWord());
        result.headLoc.position = Vector3.loadFromString(reader);
        result.combo = int.Parse(reader.ReadWord());
        result.percent = float.Parse(reader.ReadWord());
        result.headLoc.rotation = Vector3.loadFromString(reader);
        result.rank = int.Parse(reader.ReadWord());

        return result;
    }

    // Loads all the values of this replay from the legacy replay format, this is janky AF
    public static KeyFrame loadFromStringOld(StringReader reader) {
        KeyFrame result = new KeyFrame();
        result.rightSaberLoc.position = Vector3.loadFromString(reader);
        result.rightSaberLoc.rotation = Vector3.loadFromString(reader);
        result.leftSaberLoc.position = Vector3.loadFromString(reader);
        result.leftSaberLoc.rotation = Vector3.loadFromString(reader);
        result.score = int.Parse(reader.ReadWord());
        result.time = float.Parse(reader.ReadWord());
        float.Parse(reader.ReadWord()); // Energy level during this frame, now unused in the new format.
        result.headLoc.position = Vector3.loadFromString(reader);
        result.combo = int.Parse(reader.ReadWord());
        // Remove the percentage sign before parsing as a float, then divide by 100 since the percentage is stored from 0.0 to 1.0 in the new format.
        result.percent = float.Parse(reader.ReadWord().Replace("%", "")) / 100.0f;

        result.getRankFromPercentage(); // Find the rank from the percentage score
        return result;
    }
}

// This whole struct is kinda spammy, result of needing to write lots of individual booleans
public struct Modifiers {
    public bool instaFail;
    public bool batteryEnergy;
    public bool ghostNotes;
    public bool disappearingArrows;
    public bool fasterSong;
    public bool noFail;
    public bool noBombs;
    public bool noObstacles;
    public bool noArrows;
    public bool slowerSong;
    public bool leftHanded;

    public void saveWithNewFormat(BinaryWriter writer) {
        writer.Write(instaFail);
        writer.Write(batteryEnergy);
        writer.Write(ghostNotes);
        writer.Write(disappearingArrows);
        writer.Write(fasterSong);
        writer.Write(noFail);
        writer.Write(noBombs);
        writer.Write(noObstacles);
        writer.Write(noArrows);
        writer.Write(slowerSong);
        writer.Write(leftHanded);
    }

    public static Modifiers loadFromString(StringReader reader) {
        Modifiers result = new Modifiers();

        // Loop until all modifiers have been loaded
        while(true) {
            // If the next character is a digit, break since we have reached the actual beatmap data
            if(!char.IsLetter((char) reader.Peek())) {break;}

            // Read the next word, and check if it a modifier
            string nextModifier = reader.ReadWord();
            switch(nextModifier) {
                // For all modifiers, if they are found we update their value to true
                case "instaFail":
                    result.instaFail = true; continue;
                case "batteryEnergy":
                    result.batteryEnergy = true; continue;
                case "disappearingArrows":
                    result.disappearingArrows = true; continue;
                case "fasterSong":
                    result.fasterSong = true; continue;
                case "noFail":
                    result.noFail = true; continue;
                case "noBombs":
                    result.noBombs = true; continue;
                case "noObstacles":
                    result.noObstacles = true; continue;
                case "noArrows":
                    result.noArrows = true; continue;
                case "slowerSong":
                    result.slowerSong = true; continue;
                case "leftHanded":
                    result.leftHanded = true; continue;
            }
        }

        return result; // Return the fetched modifiers
    }
}

// Stores all the data associated with one replay
public class Replay {
    public KeyFrame[] Data {get; set;} // The status of everything at each point in time during a replay
    public Modifiers Modifiers {get; set;} // Which modifiers the user had enabled.

    private Replay(KeyFrame[] data, Modifiers modifiers) {
        this.Data = data;
        this.Modifiers = modifiers;
    }

    public void saveWithNewFormat(BinaryWriter writer) {
        Modifiers.saveWithNewFormat(writer); // Save the modifiers
        
        // Save each point of time during the replay
        foreach(KeyFrame keyFrame in Data) {
            keyFrame.saveWithNewFormat(writer);
        }
    }

    public static Replay loadOldReplay(string replayText) {
        // Find if the replay file uses the "old old" format, or the "new old" format.
        // The "old old" format stores the percentage score with a percent sign, so we can use that to check
        bool newImplementation = !replayText.Contains('%');

        StringReader reader = new StringReader(replayText);
        List<KeyFrame> keyFrames = new List<KeyFrame>();
        
        Modifiers modifiers = Modifiers.loadFromString(reader); // Load the gameplay modifiers

        // Read each keyFrame of the replay until we're at the end of the string
        while(reader.Peek() != -1) {
            // Load the KeyFrame using the correct implementation
            KeyFrame nextFrame;
            if(newImplementation) {
                nextFrame = KeyFrame.loadFromStringNew(reader);
            }   else    {
                nextFrame = KeyFrame.loadFromStringOld(reader);
            }

            keyFrames.Add(nextFrame);
        }
        
        return new Replay(keyFrames.ToArray(), new Modifiers());
    }
}