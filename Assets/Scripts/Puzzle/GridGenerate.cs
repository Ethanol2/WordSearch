// Script written by EthanASC
// https://www.fiverr.com/ethanasc

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridGenerate : MonoBehaviour
{
    [Tooltip("The size of the grid")]
    public int gridSize = 10;
    [Tooltip("Should words appear backwards in the puzzle?")]
    public bool allowBackwards = true;
    [Tooltip("List of words to fit in to the puzzle")]
    public Words wordsToUse;
    [Tooltip("List of words actually used (in case a word doesn't fit)")]
    public Words activeWords;
    [Tooltip("The prefab that will be used to generate the letters")]
    public GameObject letterTemplate;
    char[,] grid;                                       // The 2D array holding the string info for the current grid
    bool visibleWords = false;                          // Is true if the words are visible in the grid
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";     // The alphabet. Easiest way to randomly select a char to fill in the rest of the grid
    List<string> containedWords = new List<string>();   // List containing the words that make it into the word search

    // Start is called before the first frame update
    void Start()
    {
        // Sets all the words to be uppercase letters
        wordsToUpper(ref wordsToUse);
        // Generates the grid
        generateGrid();
        // Saves the words actually used to the activeWords Words scriptable object
        SaveContainedWords();
    }

    // Update is called once per frame
    void Update()
    {
        // Makes the words in the grid visible
        if (Input.GetKeyDown("d"))
        {
            visibleWords = !visibleWords;
            toggleWordDebug(visibleWords);
        }
    }

    void SaveContainedWords()
    {
        activeWords.words.Clear();
        for (int k = 0; k < containedWords.Count; k++)
        {
            activeWords.words.Add(containedWords[k]);
        }
    }

    void wordsToUpper(ref Words words)
    {
        for (int k = 0; k < words.words.Count; k++)
        {
            words.words[k] = words.words[k].ToUpper();
        }
    }

    void ClearGrid()
    {
        foreach (Transform t in this.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    void toggleWordDebug(bool enable)
    {
        foreach (Transform t in this.transform)
        {
            if (t.tag == "WordLetter")
            {
                t.GetChild(1).GetComponent<SpriteRenderer>().color = enable ? Color.red : Color.white;
            }
        }
    }

    void generateGrid()
    {
        // Orders the words from longest to shortest
        List<string> words = wordsToUse.words;
        words.Sort((x, y) => x.Length.CompareTo(y.Length));
        words.Reverse();

        // Creates the grid and sets the letter template to active
        grid = new char[gridSize, gridSize];
        letterTemplate.SetActive(true);

        foreach (string word in words)
        {
            // Culls words that won't fit in to the current grid size
            if (word.Length > gridSize) { Debug.Log("Word " + word + " is too long for the grid"); continue; }
            bool backwards = Random.Range(0, 2) == 0 && allowBackwards ? true : false;

            // Randomly picks a direction to start 0 = Vertical | 1 = Horizontal | 2 = Diagonal
            int direction = Random.Range(0, 3);
            // If the word is diagonal, this determines in what orientation
            bool diaDown = Random.Range(0, 2) == 0 ? true : false;

            int startRange = gridSize - word.Length;
            Vector2Int startPos = new Vector2Int();

            // Loop that tries to fit the words in to the grid. If a certain orientation fails the 
            // function will try each other orientation in turn. If no placement is found, the word is skipped
            bool success = false;
            int directionAttempts = 0;
            do
            {
                startPos = new Vector2Int();

                // Vertical
                if (direction == 0)
                {
                    success = fitWordVertical(word, startRange, backwards, ref startPos);
                }
                // Horizontal
                else if (direction == 1)
                {
                    success = fitWordHorizontal(word, startRange, backwards, ref startPos);
                }
                // Diagonal
                else if (direction == 2)
                {
                    success = fitWordDiagonal(word, ref diaDown, backwards, ref startPos);
                }

                if (!success)
                {
                    direction++;
                    if (direction > 2)
                    {
                        direction = 0;
                    }
                    directionAttempts++;
                }

            } while (!success && directionAttempts < 3);

            if (directionAttempts >= 2)
            {
                Debug.Log("Word " + word + " was unable to be fit in to the grid");
                continue;
            }

            // Apply Vertical
            if (direction == 0)
            {
                if (backwards)
                {
                    for (int k = 0; k < word.Length; k++)
                    {
                        grid[startPos.x, startPos.y + k] = word[word.Length - 1 - k];
                    }
                }
                else
                {
                    for (int k = 0; k < word.Length; k++)
                    {
                        grid[startPos.x, startPos.y + k] = word[k];
                    }
                }
            }
            // Apply Horizontal
            else if (direction == 1)
            {
                if (backwards)
                {
                    for (int k = 0; k < word.Length; k++)
                    {
                        grid[startPos.x + k, startPos.y] = word[word.Length - 1 - k];
                    }
                }
                else
                {
                    for (int k = 0; k < word.Length; k++)
                    {
                        grid[startPos.x + k, startPos.y] = word[k];
                    }
                }
            }
            // Apply Diagonal
            else if (direction == 2)
            {
                if (diaDown)
                {
                    if (backwards)
                    {
                        for (int k = 0; k < word.Length; k++)
                        {
                            grid[startPos.x + k, startPos.y + k] = word[word.Length - 1 - k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < word.Length; k++)
                        {
                            grid[startPos.x + k, startPos.y + k] = word[k];
                        }
                    }
                }
                else
                {
                    if (backwards)
                    {
                        for (int k = 0; k < word.Length; k++)
                        {
                            grid[startPos.x + k, startPos.y - k] = word[word.Length - 1 - k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < word.Length; k++)
                        {
                            grid[startPos.x + k, startPos.y - k] = word[k];
                        }
                    }
                }
            }
            containedWords.Add(word);
        }

        // This loop fills in the grid with random letters and creates the GameObjects for each letter
        for (int k = 0; k < gridSize; k++)
        {
            for (int l = 0; l < gridSize; l++)
            {
                string tag = "RandomLetter";
                string character = "";
                if (grid[k, l] == new char())
                {
                    char a = alphabet[Random.Range(0, alphabet.Length)];
                    grid[k, l] = a;
                    character += a;
                }
                else
                {
                    tag = "WordLetter";
                    character += grid[k, l];
                }

                Vector3 offset = (new Vector3(-gridSize, gridSize, 0f) / 2f) + new Vector3(0.5f, -0.5f, 0f);

                GameObject letter = Instantiate(letterTemplate, this.transform);
                letter.transform.localPosition = new Vector3(k, -l, 0f) + offset;
                letter.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "" + grid[k, l];
                letter.tag = tag;
                letter.name = character;
            }
        }

        // Disables the template
        letterTemplate.SetActive(false);
        activeWords.name = wordsToUse.name;
    }

    bool checkFilled(Vector2Int pos)
    {
        if (grid[pos.x, pos.y] == '\0') return false;
        return true;
    }

    bool fitWordVertical(string word, int startRange, bool backwards, ref Vector2Int startPos)
    {
        int count = 0;
        bool safe;
        do
        {
            count++;
            safe = true;
            startPos.x = Random.Range(0, gridSize);
            startPos.y = Random.Range(0, startRange);

            for (int k = 0; k < word.Length; k++)
            {
                if (checkFilled(new Vector2Int(startPos.x, startPos.y + k)))
                {
                    if (grid[startPos.x, startPos.y + k] != word[k] && !backwards)
                    {
                        safe = false;
                    }
                    else if (grid[startPos.x, startPos.y + k] != word[word.Length - 1 - k] && backwards)
                    {
                        safe = false;
                    }
                }
            }
        }
        while (!safe && count <= gridSize * startRange);

        if (count >= gridSize * startRange)
        {
            return false;
        }
        return true;
    }

    bool fitWordHorizontal(string word, int startRange, bool backwards, ref Vector2Int startPos)
    {
        int count = 0;
        bool safe;
        do
        {
            count++;
            safe = true;
            startPos.x = Random.Range(0, startRange);
            startPos.y = Random.Range(0, gridSize);

            for (int k = 0; k < word.Length; k++)
            {
                if (checkFilled(new Vector2Int(startPos.x + k, startPos.y)))
                {
                    if (grid[startPos.x + k, startPos.y] != word[k] && !backwards)
                    {
                        safe = false;
                    }
                    else if (grid[startPos.x + k, startPos.y] != word[word.Length - 1 - k] && backwards)
                    {
                        safe = false;
                    }
                }
            }
        }
        while (!safe && count <= gridSize * startRange);

        if (count >= gridSize * startRange)
        {
            return false;
        }
        return true;
    }

    bool fitWordDiagonal(string word, ref bool diaDown, bool backwards, ref Vector2Int startPos)
    {
        int startRange = gridSize - word.Length;
        int dirCount = 0;
        int startCount = 0;
        bool safe;

        do
        {
            safe = true;

            // Diagonal Down
            if (diaDown)
            {
                do
                {
                    safe = true;
                    startCount++;
                    startPos.x = Random.Range(0, startRange);
                    startPos.y = Random.Range(0, startRange);
                    for (int k = 0; k < word.Length; k++)
                    {
                        if (checkFilled(new Vector2Int(startPos.x + k, startPos.y + k)))
                        {
                            if (grid[startPos.x + k, startPos.y + k] != word[k] && !backwards)
                            {
                                safe = false;
                            }
                            else if (grid[startPos.x + k, startPos.y + k] != word[word.Length - 1 - k] && backwards)
                            {
                                safe = false;
                            }
                        }
                    }
                }
                while (!safe && startCount <= startRange * startRange);

                if (startCount >= startRange * startRange && !safe)
                {
                    dirCount++;
                    diaDown = !diaDown;
                    startCount = 0;
                }
            }
            // Diagonal Up
            else if (!diaDown)
            {
                do
                {
                    safe = true;
                    startCount++;
                    startPos.x = Random.Range(0, startRange);
                    startPos.y = Random.Range(gridSize - startRange, gridSize);
                    for (int k = 0; k < word.Length; k++)
                    {
                        if (checkFilled(new Vector2Int(startPos.x + k, startPos.y - k)))
                        {
                            if (grid[startPos.x + k, startPos.y - k] != word[k] && !backwards)
                            {
                                safe = false;
                            }
                            else if (grid[startPos.x + k, startPos.y - k] != word[word.Length - 1 - k] && backwards)
                            {
                                safe = false;
                            }
                        }
                    }
                }
                while (!safe && startCount <= startRange * startRange && !safe);

                if (startCount >= startRange * startRange && !safe)
                {
                    dirCount++;
                    diaDown = !diaDown;
                    startCount = 0;
                }
            }
        } while (!safe && dirCount <= 1);

        if (dirCount >= 2)
        {
            return false;
        }
        return true;
    }
}
