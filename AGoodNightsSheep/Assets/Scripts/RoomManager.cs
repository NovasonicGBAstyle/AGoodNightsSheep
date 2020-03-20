using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is just to take care of some stuff in the room.  Yay!
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("Pieces of the room to remove.")]
    [SerializeField]
    [Tooltip("A list of everything you want to make disappear at the start of the game.")]
    private List<GameObject> HideItems;

    [SerializeField]
    [Tooltip("Just the player XR rig so we can send things flying away from the player.")]
    private Transform playerTransform;

    [SerializeField]
    [Tooltip("How fast stuff should fly away from the player.")]
    private int flingAwayForce;

    [SerializeField]
    [Tooltip("Time in seconds before the object disappears")]
    private float disappearTime;

    //This is used to run coroutines that have arugments.  See the OpenRoom coroutine.
    private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        //Make sure we have stuff in the hide items list and a player transform.
        if(HideItems.Count > 0 && playerTransform != null)
        {
            //Great!  There are items!, start a coroutine to get rid of them!
            StartCoroutine("OpenRoom");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region "Coroutines"

    /// <summary>
    /// What this will do is open up the room letting the player enter the world!
    /// </summary>
    IEnumerator OpenRoom()
    {
        //So, just wait a second before we do anything.
        yield return new WaitForSeconds(1.0f);

        //We'll start by looping through the item sin the HideItems list.
        foreach(GameObject go in HideItems)
        {
            //Debug.Log("Hiding item: " + go.name);

            // Calculate Angle between game object and the player
            Vector3 dir = playerTransform.position - go.transform.position;

            // We then get the opposite (-Vector3) and normalize it
            dir = -dir.normalized;

            // And finally we add force in the direction of dir and multiply it by force. 
            // This will push back the player
            Rigidbody rb = go.GetComponent<Rigidbody>();
            rb.AddForce(dir * flingAwayForce);
            //rb.AddTorque(transform.up * flingAwayForce);

            //Now, start a coroutine to make the object disappear after a bit.
            coroutine = DisableObjectOverTime(go, disappearTime);
            StartCoroutine(coroutine);
        }
    }

    /// <summary>
    /// The purpose of this function is to have a game object disappear after a certain amount of time.
    /// </summary>
    /// <param name="go">Game object that is to disappear.</param>
    /// <param name="t">Time in seconds before the object disappears.</param>
    /// <returns></returns>
    IEnumerator DisableObjectOverTime(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        go.SetActive(false);
    }

    #endregion
}
