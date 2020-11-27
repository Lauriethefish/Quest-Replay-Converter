using System.IO;
using System.Collections.Generic;

// Stores a position and rotation in 3D space
public struct Location {
    public Vector3 position;
    public Vector3 rotation;

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
}

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

    // I know that this code is awful, but it's pretty much just adapted from the old replay mod, so . . . don't blame me ¯\_(ツ)_/¯
    public static Replay loadOldReplay(string str) {
        const int amountPerLine = 23;

        string[] words = str.Split(" ");

        Modifiers modifiers = new Modifiers();
        List<KeyFrame> keyFrames = new List<KeyFrame>();


        KeyFrame nextFrame = new KeyFrame();

        int timesThrough = 0;    
        
    
        /* Running loop till the end of the stream */
        float floatFound = -1.0f;
        int intFound;
        Vector3 tempVector = new Vector3();
        foreach(string temp in words) {    
            /* Checking the given word is integer or not */
            if (temp.Contains(".") && float.TryParse(temp, out floatFound)) {
                if(timesThrough%amountPerLine == 0 || timesThrough%amountPerLine == 3 || timesThrough%amountPerLine == 6 || timesThrough%amountPerLine == 9 || timesThrough%amountPerLine == 14 || timesThrough%amountPerLine == 19) {
                    tempVector.x = floatFound;
                } else if(timesThrough%amountPerLine == 1 || timesThrough%amountPerLine == 4 || timesThrough%amountPerLine == 7 || timesThrough%amountPerLine == 10 || timesThrough%amountPerLine == 15 || timesThrough%amountPerLine == 20) {
                    tempVector.y = floatFound;
                } else if(timesThrough%amountPerLine == 2 || timesThrough%amountPerLine == 5 || timesThrough%amountPerLine == 8 || timesThrough%amountPerLine == 11 || timesThrough%amountPerLine == 16 || timesThrough%amountPerLine == 21) {
                    tempVector.z = floatFound;
                    if(timesThrough%amountPerLine == 2) {
                        nextFrame.rightSaberLoc.position = tempVector;
                    } else if(timesThrough%amountPerLine == 5) {
                        nextFrame.rightSaberLoc.rotation = tempVector;
                    } else if(timesThrough%amountPerLine == 8) {
                        nextFrame.leftSaberLoc.position = tempVector;
                    } else if(timesThrough%amountPerLine == 11) {
                        nextFrame.leftSaberLoc.rotation = tempVector;
                    } else if(timesThrough%amountPerLine == 16) {
                        nextFrame.headLoc.position = tempVector;
                    } else if(timesThrough%amountPerLine == 21) {
                        nextFrame.headLoc.rotation = tempVector;
                    }
                } else if(timesThrough%amountPerLine == 13) {
                    nextFrame.time = floatFound;
                }
            }
            if(timesThrough%amountPerLine == 18) {
                nextFrame.percent = floatFound;
            }
            if(int.TryParse(temp, out intFound)) {
                if(timesThrough%amountPerLine == 12) {
                    nextFrame.score = intFound;
                } else if(timesThrough%amountPerLine == 17) {
                    nextFrame.combo = intFound;
                } else if(timesThrough%amountPerLine == 22) {
                    nextFrame.rank = intFound;

                    keyFrames.Add(nextFrame);
                    nextFrame = new KeyFrame();
                }
                timesThrough++;
            }

            if(temp == "batteryEnergy") modifiers.batteryEnergy = true;
            if(temp == "disappearingArrows") modifiers.disappearingArrows = true;
            if(temp == "noObstacles") modifiers.noObstacles = true;
            if(temp == "noBombs") modifiers.noBombs = true;
            if(temp == "noArrows") modifiers.noArrows = true;
            if(temp == "slowerSong") modifiers.slowerSong = true;
            if(temp == "noFail") modifiers.noFail = true;
            if(temp == "instafail") modifiers.instaFail = true;
            if(temp == "ghostNotes") modifiers.ghostNotes = true;
            if(temp == "fasterSong") modifiers.fasterSong = true;
            if(temp == "slowerSong") modifiers.slowerSong = true;
            if(temp == "leftHanded") modifiers.leftHanded = true;
        }

        return new Replay(keyFrames.ToArray(), modifiers);
    }
}