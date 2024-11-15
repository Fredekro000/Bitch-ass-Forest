using System.IO;
using UnityEngine;

public class PositionLogger : MonoBehaviour
{
    private string filePath;
    private Transform playerTransform;
    private float timer = 0f; // Timer to track elapsed time
    private float logInterval = 2f; // Interval in seconds

    void Start()
    {
        // Define the file path for logging
        filePath = Application.persistentDataPath + "/player_positions.csv";

        // Create the file and write the header
        File.WriteAllText(filePath, "Time,PositionX,PositionZ\n");

        // Reference the player's transform (camera or XR rig)
        playerTransform = Camera.main.transform; // Adjust if you're using a custom player object
    }

    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if the interval has elapsed
        if (timer >= logInterval)
        {
            // Reset the timer
            timer = 0f;

            // Log the player's position
            Vector3 position = playerTransform.position;
            string log = $"{Time.time},{position.x},{position.z}\n";

            // Write the log to the file
            File.AppendAllText(filePath, log);

            // Optional: Log to the console for debugging
            Debug.Log($"Logged position: {log}");
        }
    }
}