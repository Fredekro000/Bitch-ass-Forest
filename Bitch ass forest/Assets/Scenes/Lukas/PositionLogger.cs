using System.IO;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PositionLogger : MonoBehaviour
{
    private string filePath;
    private Transform playerTransform;

    void Start()
    {
        // Define the file path for logging
        filePath = "C:\\Users\\lukas\\OneDrive - Aalborg Universitet\\Dokumenter\\player_positions.csv";


        // Create the file and write the header
        File.WriteAllText(filePath, "Time,PositionX,PositionZ\n");

        // Reference the main camera or player's transform
        playerTransform = Camera.main.transform; // Adjust if you're using a custom player object
    }

    void Update()
    {
        // Get the current x and z positions
        Vector3 position = playerTransform.position;
        string log = $"{Time.time},{position.x},{position.z}\n";

        // Append the log to the file
        File.AppendAllText(filePath, log);
    }
}
