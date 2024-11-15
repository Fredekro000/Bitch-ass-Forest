using UnityEngine;
using System.Collections;
using System.IO;
using System.Globalization;

public class PositionLogger : MonoBehaviour
{
    // Use persistent data path for file storage
    private string filePath;

    private float logInterval = 2f;  // Time in seconds between each log
    private float timeSinceLastLog = 0f;

    void Start()
    {
        // Ensure the file path is constructed correctly
        filePath = Path.Combine(Application.persistentDataPath, "player_positions.csv");

        // Log the file path to debug
        Debug.Log("File Path: " + filePath);

        // Check if the file exists and create a new unique file if needed
        filePath = GetUniqueFilePath(filePath);

        // Write the header only once at the start
        WriteToCSV("PositionX,PositionZ");
    }

    void Update()
    {
        timeSinceLastLog += Time.deltaTime;

        if (timeSinceLastLog >= logInterval)
        {
            // Log the player's position (X and Z)
            float playerX = transform.position.x;
            float playerZ = transform.position.z;

            // Prepare the data as a string with a dot as the decimal separator
            string logEntry = string.Format(CultureInfo.InvariantCulture, "{0:F6},{1:F6}", playerX, playerZ);

            // Write the data to the CSV file
            WriteToCSV(logEntry);

            // Reset the timer
            timeSinceLastLog = 0f;
        }
    }

    void WriteToCSV(string data)
    {
        // Log the file path to check if it's empty or incorrect
        Debug.Log("Writing to: " + filePath);

        // Check if the file exists. If not, create it
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, data + "\n");  // Write the header line
        }
        else
        {
            File.AppendAllText(filePath, data + "\n");  // Append data
        }
    }

    string GetUniqueFilePath(string baseFilePath)
    {
        // If file exists, generate a new file name with timestamp
        if (File.Exists(baseFilePath))
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string directory = Path.GetDirectoryName(baseFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            string extension = Path.GetExtension(baseFilePath);

            // Create a new file path with timestamp
            baseFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{timestamp}{extension}");
        }

        return baseFilePath;
    }
}