using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: REMOVE THIS TEST CODE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnClick_Start();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClick_Quit();
        }

    }

    #region "Button Clicks"

    /// <summary>
    /// This will handle but start button click.
    /// </summary>
    public void OnClick_Start()
    {
        //Debug.Log("Start button pressed.");

        //Load the level scene.
        //Debug.Log("sceneName to load: " + Constants.SceneBattle);
        SceneManager.LoadScene(Constants.SceneBattle);
    }

    /// <summary>
    /// This will handle quit button click.
    /// </summary>
    public void OnClick_Quit()
    {
        //Debug.Log("Quit button pressed.");

        //Just quit the application.  Fun fact, this only works in a built game.  Does not stop the editor.
        Application.Quit();
    }

    #endregion
}
