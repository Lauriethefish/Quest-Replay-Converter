using System.IO;

public static class StringReaderExtensions {
    public static string ReadWord(this StringReader reader) {
        string result = "";

        // Read characters until we find a space
        while(true) {
            char nextChar = (char) reader.Read();
            if(nextChar == ' ') {break;}

            result += nextChar;
        }

        return result; // Return the characters without the space
    }
}