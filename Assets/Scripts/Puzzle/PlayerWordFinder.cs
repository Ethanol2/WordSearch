// Script written by EthanASC
// https://www.fiverr.com/ethanasc

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWordFinder : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The colour the letters turn when highlighted")]
    public Color selectionColour = Color.blue;
    [Tooltip("The colour the letters turn when they are a part of a word and found")]
    public Color foundColour = Color.green;
    [Tooltip("The Words scritable object that hold the words in play. These words will be removed as they are found")]
    public Words activeWords;
    [Header("Debug")]
    public string word = "";                                    // The word the player is currently spelling
    public List<string> foundWords = new List<string>();        // List of found words
    public bool wordsAllFound = false;                          // Is true when the player has found all the words
    public bool hasContact = false;                             // Is true when the player has their finger on the screen
    public Vector3 firstContact = new Vector3();                // The first point at which the player touched
    public float distance;                                      // The distance from the first contact point and the current contact point
    public Vector3 direction;                                   // The direction vector from first contact to current contact
    List<Transform> colidingLetters = new List<Transform>();    // List of GameObject transforms that are being selected
    Transform firstContactObj;                                  // Object representing the first contact point in the editor
    Transform currentContactObj;                                // Object that tracks the current contact point of the player

    // Start is called before the first frame update
    void Start()
    {
        firstContactObj = this.transform.GetChild(1);
        currentContactObj = this.transform.GetChild(1);
    }

    // Update is called once per frame
    void Update()
    {
        currentContactObj.gameObject.SetActive(false);

        firstContactObj.gameObject.SetActive(false);
        firstContactObj.position = firstContact;

        // Check if the player has started a selection
        if (!hasContact && Input.GetMouseButton(0))
        {
            hasContact = true;
            firstContact = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        // Check if the player has finished their selection and check their word
        else if (hasContact && !Input.GetMouseButton(0))
        {
            hasContact = false;

            if (colidingLetters.Count >= 1)
            {
                int matchedWord = -1;
                string backwardsWord = "";
                for (int k = word.Length - 1; k >= 0; k--)
                {
                    backwardsWord += word[k];
                }
                for (int k = 0; k < activeWords.words.Count; k++)
                {
                    if (activeWords.words[k] == word || activeWords.words[k] == backwardsWord)
                    {
                        matchedWord = k;
                    }
                }
                if (matchedWord > -1)
                {
                    foundWords.Add(word);
                    activeWords.words.RemoveAt(matchedWord);
                    wordsAllFound = activeWords.words.Count <= 0 ? true : false;

                    foreach (Transform t in colidingLetters)
                    {
                        t.tag = "FoundLetter";
                        t.GetChild(1).GetComponent<SpriteRenderer>().color = foundColour;
                    }
                    colidingLetters.Clear();
                }
            }
        }
        word = "";

        // Clear the selection from last frame 
        foreach (Transform t in colidingLetters)
        {
            if (t.tag != "FoundLetter")
                t.GetChild(1).GetComponent<SpriteRenderer>().color = Color.white;
            else
                t.GetChild(1).GetComponent<SpriteRenderer>().color = foundColour;
        }
        colidingLetters.Clear();
        
        // If the player is currently performing a selection, check the selected and highlight letters
        if (hasContact)
        {
            this.transform.localPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentContactObj.gameObject.SetActive(true);
            firstContactObj.gameObject.SetActive(true);
            distance = Vector3.Distance(firstContact, this.transform.localPosition);
            direction = this.transform.localPosition - firstContact;
            RaycastHit2D[] hitLetters = Physics2D.RaycastAll(firstContact, direction, distance);

            foreach (RaycastHit2D hit in hitLetters)
            {
                if (hit.transform.tag == "RandomLetter" || hit.transform.tag == "WordLetter" || hit.transform.tag == "FoundLetter")
                {
                    word += hit.transform.name;
                    hit.transform.GetChild(1).GetComponent<SpriteRenderer>().color = selectionColour;
                    colidingLetters.Add(hit.transform);
                }
            }
        }
    }
}
