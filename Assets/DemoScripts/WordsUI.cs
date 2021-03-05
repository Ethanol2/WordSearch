using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WordsUI : MonoBehaviour
{
    public Words fileImportWords;
    public Words activeWords;
    public TMP_Text catText;
    public TMP_Text activeWordsText;
    public TMP_Text errorText;
    Scene activeScene;
    float restartTime = 4f;


    // Start is called before the first frame update
    void Start()
    {
        activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "GameDemo" || activeScene.name == "FileImportDemo_Game")
        {
            catText.text = activeWords.name;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (activeScene.name == "GameDemo" || activeScene.name == "FileImportDemo_Game")
        {
            activeWordsText.text = "";
            foreach (string word in activeWords.words)
            {
                activeWordsText.text += " " + word;
            }

            restartTime -= Time.deltaTime;
            if (!GameObject.Find("GameCursor").GetComponent<PlayerWordFinder>().wordsAllFound)
            {
                restartTime = 4f;
            }
            if (restartTime <= 0f)
            {
                SceneManager.LoadScene(activeScene.name);
            }
            if (Input.GetKeyDown("r"))
            {
                SceneManager.LoadScene(activeScene.name);
            }
        }
        else
        {
            if (GameObject.Find("FileImport").GetComponent<FileImport>().fileLoaded)
            {
                activeWordsText.text = "";
                foreach (string word in fileImportWords.words)
                {
                    activeWordsText.text += " " + word;
                }
            }
            errorText.text = GameObject.Find("FileImport").GetComponent<FileImport>().statusMessage;
        }
    }

    public void toggleBackwards()
    {
        GameObject.Find("WordSearch").GetComponent<GridGenerate>().allowBackwards =
        !GameObject.Find("WordSearch").GetComponent<GridGenerate>().allowBackwards;
    }

    public void startGame()
    {
        if (GameObject.Find("FileImport").GetComponent<FileImport>().fileLoaded)
        {
            SceneManager.LoadScene("FileImportDemo_Game");
        }
        else
        {
            SceneManager.LoadScene("GameDemo");
        }
    }
}
