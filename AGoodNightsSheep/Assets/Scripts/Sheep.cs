using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour {

    [Header("Set in Inspector: May be modified by code")]
    public int points;
    public int health;
    public int damage;

    [Header("Set Dynamically")]
    public Rigidbody rigid;

    private Flock flock;
    private bool isDead;
    public GameManager gameManager;

    // Use this for initialization
    void Start () {
        isDead = false;
    }


    //Use this for initialization
    private void Awake()
    {
        //Get a reference to the game manager.
        gameManager = GameManager.singleton;

        isDead = false;
        flock = GetComponent<Flock>();
        rigid = GetComponent<Rigidbody>();

        //So, we are going to generate a random place for the sheep to spawn.  It uses this old thing called trigonometry.  You've probably never heard of it.
        //So, this likes to do stuff in radians.  So, we'll randomly generate an angle between 0 and 360, then convert it to radians.
        //float degrees = Random.Range(0, 360) * Mathf.Deg2Rad;

        //Now, we'll get our ratios.  On a unit circle, the sin and cos functions give you that actual x and y values.  But we have a differnt range we are going to 
        //use, so we will get the ratios and use them in multiplication later.  Cosine is x, and sine is y.
        //float x = Mathf.Sin(degrees);
        //float y = Mathf.Cos(degrees);

        //Set a random initial position.  We'll manually set the height. However, the x and z are going to be some distance multipled by the x and y we generated earlier.  This will cause the
        //sheep to spawn in a random band around the player.  Genius.  Pure genius.
        //pos = new Vector3(Random.Range(gameManager.minSpawnRadius, gameManager.spawnRadius) * x, 20, Random.Range(gameManager.minSpawnRadius, gameManager.spawnRadius) * y);
        //pos = new Vector3(Random.Range(-spn.spawnRadius, spn.spawnRadius), 40, Random.Range(-spn.spawnRadius, spn.spawnRadius));

        //Debug.Log("New Sheep Vector is: " + pos);

        //New Spawn Stuff.  I just want to the sheep to spawn a specific points.  We can make them spawn around that area, but only from the spawn points.
        //First, get the random spawn location.
        pos = gameManager.GetRandomSheepSpawnLocation().position;
        //Next, get some random point in a unit sphere.
        var sph = Random.insideUnitSphere;
        //Next, get a random distance between the min and max spawn radius as determined by the game manager.
        var multiplier = Random.Range(gameManager.minSpawnRadius, gameManager.spawnRadius);

        //Finally, multiply that unit sphere variable by some random value to ensure the sheep spawns not exactly on the point, but some random distance from it.
        pos = pos + sph * multiplier;

        //Debug.Log("Intial spawn location: " + pos.ToString());

        //For real last part on the position, we have to stay above the ground.  So if the sheep is too low, make it at least on at the height we need.
        if(pos.y < 3)
        {
            pos = new Vector3(pos.x, 3, pos.z);
            //Debug.Log("Intial spawn location to low.  New spawn location: " + pos.ToString());
        }

        //Here, we are setting a random starting direction and the initial velocity.
        Vector3 vel = Random.onUnitSphere * GameManager.singleton.velocity;
        rigid.velocity = vel;

        LookAhead();
    }

    /// <summary>
    /// Have the object look in the direction it is going.
    /// </summary>
    void LookAhead()
    {

        //Orients the Boid to look at the direction it's flying.
        transform.LookAt(pos + rigid.velocity);
    }

    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    /// <summary>
    /// FixedUpdate is called once per physics update (i.e. 50x/sec
    /// Here, we will determine how to move the sheep.
    /// </summary>
    private void FixedUpdate()
    {
        //First, make sure we're not dead.
        if (!isDead)
        {
            //To start, we'll get the current velocity.
            Vector3 vel = rigid.velocity;
            //Debug.Log("Starting velocity: " + vel.ToString());

            //COLLISION AVOIDANCE - Avoid neighbors who are too close.
            Vector3 velAvoid = Vector3.zero;
            Vector3 tooClosePos = flock.avgClosePos;

            //If the response is Vector3.zero, then no need to react.
            if (tooClosePos != Vector3.zero)
            {
                velAvoid = pos - tooClosePos;
                //Debug.Log("Starting velAvoid: " + velAvoid);
                velAvoid.Normalize();
                //Debug.Log("Normalized velAvoid: " + velAvoid);
                velAvoid *= gameManager.velocity;
                //Debug.Log("Spun velocity: " + velAvoid);
            }

            //VELOCITY MATCHING - Try to match velocity with neighbors.
            Vector3 velAlign = flock.avgVel;

            //Only do more if the velAlign is not Vector3.zero.
            if (velAlign != Vector3.zero)
            {
                //We're really intereted in direction, so normalize the velocity.
                velAlign.Normalize();
                //And then set it to the speed we chose.
                velAlign *= gameManager.velocity;
            }

            //FLOCK CENTERING - Move towards the center of local neighbors
            Vector3 velCenter = flock.avgPos;
            if (velCenter != Vector3.zero)
            {
                velCenter -= transform.position;
                velCenter.Normalize();
                velCenter *= gameManager.velocity;
            }

            //ATTRATION - Move towards the Attractor.
            Vector3 delta = Attractor.POS - pos;
            //Debug.Log("Atrractor position: " + Attractor.POS.ToString());

            //Check whether we're attracted or avoiding the Attractor.
            bool attracted = (delta.magnitude > gameManager.attractPushDist);
            //Debug.Log("Delta Magnitude: " + delta.magnitude);
            //Debug.Log("Spn AttactPushDist: " + gameManager.attractPushDist.ToString());

            Vector3 velAttract = delta.normalized * gameManager.velocity;
            //Debug.Log("Vel Attract: " + velAttract);

            //Apply all the velocities
            float fdt = Time.fixedDeltaTime;

            //Gonna be honest here.  I have no idea how any of this works anymore
            //or what any of it is supposed to be doing.
            if (velAvoid != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAvoid, gameManager.collAvoid * fdt);
            }
            else
            {
                if (velAlign != Vector3.zero)
                {
                    //Debug.Log("velAlign != Vector3.zero");
                    vel = Vector3.Lerp(vel, velAlign, gameManager.velMatching * fdt);
                }
                if (velCenter != Vector3.zero)
                {
                    //Debug.Log("velCenter != Vector3.zero");
                    vel = Vector3.Lerp(vel, velCenter, gameManager.flockCentering * fdt);
                }
                if (velAttract != Vector3.zero)
                {
                    //Debug.Log("velAttract != Vector3.zero");
                    if (attracted)
                    {
                        vel = Vector3.Lerp(vel, velAttract, gameManager.attractPull * fdt);
                        //Debug.Log("Attracted");
                    }
                    else
                    {
                        vel = Vector3.Lerp(vel, -velAttract, gameManager.attractPush * fdt);
                        //Debug.Log("Repelled");
                    }
                }
            }

            //Debug.Log("Velocity before normalization: " + vel.ToString());
            //Set vel to the velocity set on the GameManager singleton.
            vel = vel.normalized * gameManager.velocity;

            //Finally assign this to the rigidbody.
            rigid.velocity = vel;
            //Debug.Log("Final velocity: " + vel.ToString());


            //Look in the directonof the new velocity.
            LookAhead();
        }
    }

    /// <summary>
    /// This is happening when the sheep takes damage.  Told to do so by projectiles.  It's just how I decided
    /// to do it.  The sheep tell the player to take damage.  The projectiles tell the sheep to take damage.
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(int amount)
    {
        //Check to see if we're dead.  If so, do nothing.  This is important because sheep bodies can be in 
        //the way of living sheep.  They are basically sheep sheilds.
        //Debug.Log("I took " + amount + "damage and have " + health +" health.");
        if (isDead)
        {
            return;
        }

        //Decrease Health
        health -= amount;

        //Check to see if the sheep has any health left.
        if (health <= 0)
        {
            //TODO:
            //Remove this sheep from the flock.  I'm not sure how it get's added to the flock.
            //Set dead boolean to trun and remove this sheep from the flock.
            isDead = true;
            GameManager.RemoveSheep(this);

            //Start the death coroutine.
            StartCoroutine("DestroySheep");
            //GetComponent<AudioSource>().PlayOneShot(sheepDeath);
        }
        else
        {
            //GetComponent<AudioSource>().PlayOneShot(sheepHit);
        }
    }

    /// <summary>
    /// Colision check.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Sheep collided with: " + collision.collider.gameObject.name);
        //Check to see if the player was hit.  If so, give the player damage.
        if(collision.gameObject.tag == "Player")
        {
            //Debug.Log("I killed the player!");
            collision.gameObject.GetComponent<Player>().TakeDamage(damage);

            //We'll also destroy the sheep at this point.
            TakeDamage(health);
        }

    }

    #region CoRoutines
    /// <summary>
    /// This is just going to wait a moment before it destroys the sheep object.  Yay!
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroySheep()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
    #endregion
}
