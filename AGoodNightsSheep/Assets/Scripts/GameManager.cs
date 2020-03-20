using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager singleton;

    // This is a Singleton of the BoidSpawner.  Ther eis only one instance
    // of BoidSpawner, so we can store it in a static variable named "S".
    public static List<Sheep> sheepList;

    //These fields allow you to adjust the spawning behavior of the Boids
    [Header("Set in Inspector: Spawning")]
    [SerializeField]
    public GameObject sheepPrefab;
    [SerializeField]
    public Transform sheepAnchor;
    //This is a list of sheep spawn locations.  It picks them up automatically.
    private List<Transform> sheepSpawnLocations;
    [SerializeField]
    public int numSheep = 100;
    [SerializeField]
    public float minSpawnRadius = 40f;      //Minimum distance the sheep must spawn from.
    [SerializeField]
    public float spawnRadius = 100f;        //Maximum distance the sheep should spawn from.
    [SerializeField]
    public float spawnDelay = 0.1f;

    //These fields allow you to adjust the flocking behavior of the Boids
    [Header("Set in Inspector: Boids")]
    public float velocity = 30f;
    public float neighborDist = 30f;
    public float collDist = 4f;
    public float velMatching = 0.25f;
    public float flockCentering = 0.2f;
    public float collAvoid = 2f;
    public float attractPull = 2f;
    public float attractPush = 2f;
    public float attractPushDist = 5f;

    [Header("Set in Inspector: UI elements")]

    //public GameUI gameUI;
    public GameObject player;
    public int score;
    public int waveCountdown;
    public bool isGameOver;
    public GameObject gameOverPanel;

    [Header("Gun Clip Sizes")]
    //Just for fun, I'm thinking about adding a clip size to guns that enforces a reload.  Wouldn't that be interesting?
    public int rightMagazineSize = 15;
    public int leftMagazineSize = 15;

    [Header("Controller Haptic Feedback Values")]
    //Alright, so these next variables will be used for haptic feedback.  Sexy!
    public uint rightChannel = 0;
    public float rightAmplitude = 0.75f;
    public float rightDuration = 0.25f;
    public float rightAmplitudeEmpty = 0.1f;
    public float rightReloadAmplitude = 0.25f;
    public float rightDurationEmpty = 0.1f;
    public uint leftChannel = 0;
    public float leftAmplitude = 0.75f;
    public float leftDuration = 0.5f;
    public float leftAmplitudeEmpty = 0.1f;
    public float leftReloadAmplitude = 0.25f;
    public float leftDurationEmpty = 0.1f;

    //This basically ends up determining the health of the sheep each wave.
    private int timesSpawned = 0;

    //Oberservable Events.
    //Specifically, this is going to be used when a new round is starting.  Things will listen for this.
    public event Action NewRoundStart;

    // Use this for initialization
    void Start()
    {
        //Check basic singleton stuff.
        if (singleton == null)
        {
            singleton = this;
        }
        //Make sure no other instances are allowed to exist.
        else if (singleton != this)
        {
            Destroy(gameObject);
        }

        //I actually don't want this to persist throughout the game.
        //I actually want this to reload with every gameplay through.
        //DontDestroyOnLoad(gameObject);

        //Hide the game over menu in case that's showing.
        gameOverPanel.SetActive(false);

        GetSheepSpawnLocations();

        sheepList = new List<Sheep>();

        StartCoroutine("increaseScoreEachSecond");

        isGameOver = false;
        Time.timeScale = 1;

        StartCoroutine("UpdateWaveTimer");
        StartCoroutine("TestWave");
        SpawnSheep();

        //Let everything listening that a new round has started.
        NewRoundStart();
    }
    
    /// <summary>
    /// This runs frequently.
    /// </summary>
    private void Update()
    {
        //Here, we are going to see if the "escape" button has been pressed.
        if (Input.GetKey("escape"))
        {
            //The escape button has been pressed, so return to the main menu.
            LoadMainMenu();
        }
    }

    #region "UI Buttons"

    public void OnLoadMainMenuClick()
    {
        LoadMainMenu();
    }

    /// <summary>
    /// This runs when the game is over.
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;

        //I believe this stops time.  If it were 1, that's normal time.  Feel free to play with this later in the game for special effects.
        Time.timeScale = 0;
        //player.GetComponent<CharacterController>().enabled = false;

        //Show the game over screen.
        gameOverPanel.SetActive(true);
        Debug.Log("You have been put to bed.");
    }

    /// <summary>
    /// This will load the main menu scene.
    /// </summary>
    private void LoadMainMenu()
    {
        isGameOver = true;
        Debug.Log("Returning to main menu.");
        SceneManager.LoadScene(Constants.SceneMainMenu);
    }

    #endregion

    #region "Sheep Control"
    /// <summary>
    /// This will spawn the sheep.
    /// </summary>
    public void SpawnSheep()
    {
        //Increase the times spawned.
        timesSpawned++;

        //Alright, so we're just going to instantiate the sheep.
        GameObject go = Instantiate(sheepPrefab);

        //Get a reference to the Sheep script.
        Sheep s = go.GetComponent<Sheep>();

        //Set this sheep as a child of the sheep anchor.
        s.transform.SetParent(sheepAnchor);

        //Set sheep health.
        s.health += timesSpawned;

        //Add this sheep to the list of sheep.
        sheepList.Add(s);
        //Debug.Log("Sheep list: " + sheepList.Count.ToString());
        //Debug.Log("numSheep counter: " + numSheep.ToString());

        //Determine if we need to spawn more sheep.  I realize that if you start killing sheep before this finishes,
        //it may actually never stop producing sheep.  So I guess we should investigate that?
        if (sheepList.Count < numSheep)
        {
            //What this does is determine that it will spawn a sheep at a random interval between 0 seconds and whatever spawnDelay is set to.
            Invoke("SpawnSheep", UnityEngine.Random.Range(0, spawnDelay));
        }
        //gameUI.SetEnemyText(sheepLeft());
    }

    /// <summary>
    /// Remove a sheep from the game.
    /// </summary>
    /// <param name="s"></param>
    public static void RemoveSheep(Sheep s)
    {
        //Increase score.
        singleton.score += s.points;

        //Actually remove the sheep.
        //Debug.Log("Here we remove an enemy");
        //singleton.sheepLeft--;
        sheepList.Remove(s);

        //singleton.gameUI.SetEnemyText(singleton.sheepLeft);

        // Give player bonus for clearing the wave before timer is done
        if (sheepList.Count == 0)
        {
            singleton.score += 50;
            //singleton.gameUI.ShowWaveClearBonus();
        }
    }

    /// <summary>
    /// Simple function that shows how many sheep are left.
    /// </summary>
    /// <returns></returns>
    public int sheepLeft()
    {
        return sheepList.Count;
    }

    /// <summary>
    /// This is going ot pull out the sheep spawn locations.  It looks for things with the SheepSpawn
    /// tag, and adds them to the list.
    /// </summary>
    private void GetSheepSpawnLocations()
    {
        //Initialize the shepSpawnLocations list.
        sheepSpawnLocations = new List<Transform>();

        //Get all the spawn areas.
        GameObject[] tempSpawns = GameObject.FindGameObjectsWithTag("SheepSpawn");

        //Go through and put them in our spawn locations list.
        foreach(GameObject s in tempSpawns)
        {
            sheepSpawnLocations.Add(s.transform);
        }

        //Make sure there is at least one object in the table.
        if(sheepSpawnLocations.Count == 0)
        {
            sheepSpawnLocations.Add(gameObject.transform);
        }
    }

    /// <summary>
    /// This is going to return one of the spawn locations provided so that the sheep can spawn from different places.
    /// </summary>
    /// <returns></returns>
    public Transform GetRandomSheepSpawnLocation()
    {
        //Pick a random number between zero and the maximum items in the list.
        int r = UnityEngine.Random.Range(0,sheepSpawnLocations.Count);

        //Return the item at the random spot we selected.
        return sheepSpawnLocations[r];
    }

    #endregion

    #region CoRoutines

    /// <summary>
    /// Simple coroutine that will increase the player score for each second they are still playing.
    /// </summary>
    /// <returns></returns>
    IEnumerator increaseScoreEachSecond()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(1);
            score += 1;
            //gameUI.SetScoreText(score);
        }
    }

    /// <summary>
    /// This will start a new wave of sheep. It currenlty just runs wave after wave after wave.
    /// TODO:
    /// Have waves only run if a wave has been completed.  Currently, I give a bonus just for finishing
    /// before the next wave.  But this gets overwhelming very quickly.  Might leave it like that, might not.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateWaveTimer()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(1f);
            waveCountdown--;
            //gameUI.SetWaveText(waveCountdown);

            // Spawn next wave and restart count down.
            if (waveCountdown == 0)
            {
                //Increase the sheep count.
                numSheep = numSheep + 5;
                SpawnSheep();
                waveCountdown = 10;
                //gameUI.ShownewWaveText();
            }
        }
    }

    /// <summary>
    /// We have a coroutine that will just wait a moment.  It's needed
    /// to let some of the scene assests and such load.
    /// </summary>
    /// <returns></returns>
    IEnumerator waitAndLoad()
    {
        SceneManager.LoadScene(Constants.SceneBattle);

        yield return new WaitForSeconds(0.02f);

        //gameOverPanel.SetActive(true);
    }

    IEnumerator TestWave()
    {


        yield return new WaitForSeconds(5f);


        NewRoundStart();
    }

    #endregion

}
