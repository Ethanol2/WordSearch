using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomStart : MonoBehaviour
{
    public List<Words> allTheWords = new List<Words>();
    public GridGenerate gridGenerate;
    public TMP_Text category;

    static bool backwards = false;

    // Start is called before the first frame update
    void Start()
    {
        int randomWords = Random.Range(0, allTheWords.Count);
        gridGenerate.wordsToUse = allTheWords[randomWords];
        category.text = allTheWords[randomWords].name;

        GameObject.Find("WordSearch").GetComponent<GridGenerate>().allowBackwards = backwards;
    }

    // Update is called once per frame
    void Update()
    {
        backwards = GameObject.Find("WordSearch").GetComponent<GridGenerate>().allowBackwards;
    }
}
