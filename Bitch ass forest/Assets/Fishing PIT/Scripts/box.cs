using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class box : MonoBehaviour
{
    // Start is called before the first frame update
    public Canvas PaperCanvas;
    public Image checkmarkSardine;
    public Sprite checkmark;
    private int counterSardine = 0;
    public Image checkmarkTuna;
    private int counterTuna = 0;
    
    private TextMeshProUGUI textForSardine;
    private TextMeshProUGUI textForTuna;

    private void Start()
    {
        textForSardine = CreateTMPAboveImage(checkmarkSardine, "Caught: " + counterSardine);
        textForTuna = CreateTMPAboveImage(checkmarkTuna, "Caught: " + counterTuna);
        
        if (checkmarkSardine == null)
        {
            GameObject imageObject = GameObject.Find("SardineCheck");
            if (imageObject != null)
            {
                checkmarkSardine = imageObject.GetComponent<Image>();
            }
            else
            {
                Debug.LogError("SardineCheck image not found!");
            }
        }
    }

    private HashSet<GameObject> processedObjects = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (processedObjects.Contains(other.gameObject)) return; // Skip if already processed

        if (other.CompareTag("Sardine"))
        {
            processedObjects.Add(other.gameObject);
            Destroy(other.gameObject);
            checkmarkSardine.sprite = checkmark;
            checkmarkSardine.color = Color.green;
            counterSardine += 1;
            textForSardine.text = "Caught: " + counterSardine;
        }
        else if (other.CompareTag("Tuna"))
        {
            processedObjects.Add(other.gameObject);
            Destroy(other.gameObject);
            checkmarkTuna.sprite = checkmark;
            checkmarkTuna.color = Color.green;
            counterTuna += 1;
            textForTuna.text = "Caught: " + counterTuna;
        }
    }

    private TextMeshProUGUI CreateTMPAboveImage(Image image, string textContent)
    {
        // Create a new TMP text object
        GameObject tmpGameObject = new GameObject("TMPAboveImage");
        tmpGameObject.transform.SetParent(PaperCanvas.transform, false);

        // Add TextMeshProUGUI component
        TextMeshProUGUI tmpText = tmpGameObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = textContent;
        tmpText.fontSize = 0.01f;
        tmpText.color = Color.gray;
        tmpText.alignment = TextAlignmentOptions.Center;

        // Position the TMP text above the image
        RectTransform tmpRect = tmpGameObject.GetComponent<RectTransform>();
        RectTransform imageRect = image.GetComponent<RectTransform>();
        tmpRect.sizeDelta = new Vector2(0.1f, 0.01f); // Match image width, set height for text
        tmpRect.anchoredPosition = imageRect.anchoredPosition + new Vector2(0, imageRect.rect.height + 0.01f); // Place above the image

        return tmpText;
    }
}
