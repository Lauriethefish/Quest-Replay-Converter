using System.IO;

// Utility class for storing XYZ coordinates
public struct Vector3 {
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void saveWithNewFormat(BinaryWriter writer) {
        writer.Write(x);
        writer.Write(y);
        writer.Write(z);
    }

    public static Vector3 loadFromString(StringReader reader) {
        return new Vector3(
            float.Parse(reader.ReadWord()),
            float.Parse(reader.ReadWord()),
            float.Parse(reader.ReadWord())
        );
    }
}