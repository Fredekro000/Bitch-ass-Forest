using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class showFPS : MonoBehaviour
{
    public TextMeshProUGUI fps;
    private float pollingTime = 1f;

    private float time;

    private int frameCount;
    

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if (time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fps.text = frameRate.ToString() + "FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}
