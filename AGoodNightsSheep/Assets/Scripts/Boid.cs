using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Boid : MonoBehaviour
{

    [Header("Set Dynamically")]
    public Rigidbody rigid;

    private Neighborhood neighborhood;

    //Use this for initialization
    private void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();

        //Set a random initial position.
        pos = new Vector3(Random.Range(-Spawner.S.spawnRadius, Spawner.S.spawnRadius), 40, Random.Range(-Spawner.S.spawnRadius, Spawner.S.spawnRadius));

        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;

        LookAhead();
    }

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

    //FixedUpdate is called once per physics update (i.e. 50x/sec
    private void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;

        //COLLISION AVOIDANCE - Avoid neighbors who are too close.
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;

        //If the response is Vector3.zero, then no need to react.
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }

        //VELOCITY MATCHING - Try to match velocity with neighbors.
        Vector3 velAlign = neighborhood.avgVel;

        //Only do more if the velAlign is not Vector3.zero.
        if (velAlign != Vector3.zero)
        {
            //We're really intereted in direction, so normalize the velocity.
            velAlign.Normalize();
            //And then set it to the speed we chose.
            velAlign *= spn.velocity;
        }

        //FLOCK CENTERING - Move towards the center of local neighbors
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }

        //ATTRATION - Move towards the Attractor.
        Vector3 delta = Attractor.POS - pos;

        //Check whether we're attracted or avoiding the Attractor.
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Debug.Log("Delta Magnitude: " + delta.magnitude);
        Debug.Log("Spn AttactPushDist: " + spn.attractPushDist);

        Vector3 velAttract = delta.normalized * spn.velocity;
        Debug.Log("Vel Attract: " + velAttract);

        //Apply all the velocities
        float fdt = Time.fixedDeltaTime;

        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid * fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velCenter, spn.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                    //Debug.Log("Attracted");
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                    Debug.Log("Repelled");
                }
            }
        }

        //Set vel to the velocity set on the Spawner singleton.
        vel = vel.normalized * spn.velocity;

        //Finally assign this to the rigidbody.
        rigid.velocity = vel;

        //Look in the directonof the new velocity.
        LookAhead();
    }
}