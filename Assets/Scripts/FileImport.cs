// Script written by EthanASC
// https://www.fiverr.com/ethanasc

// File picker package found at
// https://assetstore.unity.com/packages/tools/integration/native-file-picker-for-android-ios-173238#description
// https://github.com/yasirkula/UnityNativeFilePicker

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class FileImport : MonoBehaviour
{
    [Tooltip("The file path when running the game in the editor")]
    public string windowsFilePath = "";
    [Tooltip("The Words scriptable object in to which the txt file will be imported to")]
    public Words importedWords;
    [Tooltip("Toggles to true when a file has been successfully imported")]
    public bool fileLoaded = false;
    [Tooltip("Status messages should appear here. Check if something fails on import")]
    public string statusMessage = "";
    string importedFile = "";                       // The string the file is imported to before parsing
    List<string> parsedFile = new List<string>();   // The list in which the individual words are places before being put in the Words object
    string txtFileType;                             // A string required for the android file import package

    // Start is called before the first frame update
    void Start()
    {
        txtFileType = NativeFilePicker.ConvertExtensionToFileType("txt");

        // Confirming write and read permissions on Android
#if PLATFORM_ANDROID
        Permission.RequestUserPermission(Permission.ExternalStorageRead);
#endif

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void importFile()
    {
#if UNITY_EDITOR
        // Basic windows file import for editor debugging
        StreamReader reader;
        reader = new StreamReader(windowsFilePath);

        while (!reader.EndOfStream)
        {
            parsedFile.Add(reader.ReadLine());
        }
        reader.Close();

        // Load the file contents in to the Words scriptable object
        loadWordsInToObject(windowsFilePath);
        fileLoaded = true;
#else
        // Using the native file picker package. Code adapted from their github page 
        if (NativeFilePicker.IsFilePickerBusy())
        {
            Debug.Log("File picker is busy");
            statusMessage = "File picker is busy";
            return;
        }
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((filePath) => {
            if (filePath == null)
            {
                Debug.Log("Operation Cancelled");
                statusMessage = "Operation Cancelled";
            }
            else
            {
                Debug.Log("Picked file: " + filePath);
                statusMessage = "Picked file: " + filePath;

                // Run the file import function using the aquired file path
                StartCoroutine(importFileAndroid("file://" + filePath));
            }
        }
        , new string[] { txtFileType });
        fileLoaded = true;
#endif
        return;
    }

    // This function imports a text file on android. Android is special and requires the use of
    // UnityWebRequest to aqquire and import a file. A file path requires file:/// at the front
    // for this function to work.
    IEnumerator importFileAndroid(string path)
    {
        UnityWebRequest www = UnityWebRequest.Get(path);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
            statusMessage = www.error;
        }
        else
        {
            importedFile = ((DownloadHandler)www.downloadHandler).text;
            StringReader reader = new StringReader(importedFile);
            while (true)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    parsedFile.Add(line);
                }
                else
                {
                    break;
                }
            }
            // Load the file contents in to the Words scriptable object
            loadWordsInToObject(path);
        }
    }

    // This function loads the contents of parsedFile in to the Words scriptable object
    public void loadWordsInToObject(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        importedWords.name = fileName;
        importedWords.words.Clear();
        foreach (string word in parsedFile)
        {
            importedWords.words.Add(word);
        }
        return;
    }
}


