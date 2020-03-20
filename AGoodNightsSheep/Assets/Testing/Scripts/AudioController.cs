using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AudioController : MonoBehaviour
{
    [Header("Audio Stuff")]
    //This is an audio source.  We'll use it to build audio.
    private AudioSource audioSource;

    //This is going to be the clip we will actually play.
    private AudioClip audioClip;

    //This is the path to the sound file we will play.
    private string soundPath;

    //This is the name of the audio file we will be playing.
    private string audioName = "";

    /// <summary>
    /// Basic awake create stuff.  No biggie.
    /// </summary>
    private void Awake()
    {
        //Gotta have one of these.  If none exists, create one.
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        //Create the sound path.  It's going to use the steamingAssets folder, and the Sounds folder in there.
        soundPath = "file://" + Application.streamingAssetsPath + "/Sounds/";
        //Debug.Log("Sound path created as: " + soundPath);

        //Example of how we would play a sound.
        PlayAudioFromByFileName("QuestionSound_1000.wav");
    }

    /// <summary>
    /// So, this is the coroutine that will play the sound.  I don't know that it has to be done as a coroutine,
    /// but I guess the logic is that the sound loading thing might be downloading from the internet (as it can take
    /// a web url as the first argument), and that might take a while so the whole program shouldn't be waiting
    /// on this.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetAudioClip()
    {
        //Using our sound path and whatever the audio name is, we'll create a single string of the file to play.
        string audioToLoad = string.Format(soundPath + audioName);

        //Now, we'll actually create the mutlimedia request, and see what happens with it.
        //Side note, apparently you can't do this with .mp3 files, so people use conversion software
        //to make it work.  I'm not really doing that here though.
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioToLoad, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            //Check for error.
            if (www.isNetworkError)
            {
                //Log error.
                Debug.Log(www.error);
            }
            else
            {
                //Success!  So now we finally have the sound file as an audio clip that we can use!
                audioClip = DownloadHandlerAudioClip.GetContent(www);

                //Call the function that plays audio.
                PlayAudioFile();
            }
        }
    }

    /// <summary>
    /// Play the audio clip in the audioClip variable.
    /// </summary>
    private void PlayAudioFile()
    {
        //Just playe the clip once.
        audioSource.PlayOneShot(audioClip);

        //I left the stuff in below just in case you wanted to loop a thing or something.
        //Just make sure the audio clip variable is loaded into the audioSource.
        //audioSource.clip = audioClip;
        //audioSource.Play();
        //audioSource.loop = false;
    }

    /// <summary>
    /// This will allow us to set the audio source that will play a sound
    /// we load from a file.
    /// </summary>
    /// <param name="audio">Audio source that we want to cause to play sounds.</param>
    public void setAudioSource(AudioSource audio)
    {
        audioSource = audio;
    }

    /// <summary>
    /// This is how to tell the audio controller to play a sound clip.  Give it a file name.
    /// PLEASE NOTE: the file must exist in the /StreamingAssets/Sounds folder.
    /// </summary>
    /// <param name="audioFileName">Name of the file in the /StreamingAssets/Sounds folder to be played.</param>
    public void PlayAudioFromByFileName(string audioFileName)
    {
        //Set the name of the audio file.
        audioName = audioFileName;

        //Start the coroutine to load and play the sound clip.
        StartCoroutine("GetAudioClip");
    }
}
