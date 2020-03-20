using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {


    [Header("Set Dynamically")]
    public List<Sheep> flock;
    private CapsuleCollider coll;
    private float colliderDivider = 22.0f;

    //This is creating a list of neighbors.  It sets the radius of the colider to half that of the GameManager so that it should see less items.
    private void Start()
    {
        flock = new List<Sheep>();
        coll = GetComponent<CapsuleCollider>();
        coll.radius = GameManager.singleton.neighborDist / colliderDivider;
    }

    //Here, check to see if the dist has changed.  If so, it sets our radius.  I don't think this should change unless
    //We are told to manually change it during runtime later.  I didn't think that should happen with what has currently
    //been done so far.
    private void FixedUpdate()
    {
        if (coll.radius != GameManager.singleton.neighborDist / colliderDivider)
        {
            coll.radius = GameManager.singleton.neighborDist / colliderDivider;
        }
    }

    //Now, we'll add boid game objects that get close to us.
    private void OnTriggerEnter(Collider other)
    {
        Sheep s = other.GetComponent<Sheep>();
        if (s != null)
        {
            if (flock.IndexOf(s) == -1)
            {
                flock.Add(s);
            }
        }
    }

    //Now, we'll remove boid objects that move away from us.
    private void OnTriggerExit(Collider other)
    {
        Sheep s = other.GetComponent<Sheep>();
        if (s != null)
        {
            if (flock.IndexOf(s) != -1)
            {
                flock.Remove(s);
            }
        }
    }

    //Use the pos of the voids in our list of neighbors to get the average of their postition.  If there are none,
    //we'll just get a Vector3.zero back.
    public Vector3 avgPos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (flock.Count == 0) return avg;

            for (int i = 0; i < flock.Count; i++)
            {
                avg += flock[i].pos;
            }

            avg /= flock.Count;

            return avg;
        }
    }

    //Same thing but for velocity.
    public Vector3 avgVel
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (flock.Count == 0) return avg;

            for (int i = 0; i < flock.Count; i++)
            {
                avg += flock[i].rigid.velocity;
            }

            avg /= flock.Count;

            return avg;
        }
    }

    //Now, get the average position of neighbors that are within collision distance
    //as set in the Spawer.
    public Vector3 avgClosePos
    {
        get
        {
            Vector3 avg = Vector3.zero;
            Vector3 delta;
            int nearCount = 0;
            for (int i = 0; i < flock.Count; i++)
            {
                delta = flock[i].pos - transform.position;

                if (delta.magnitude <= GameManager.singleton.collDist)
                {
                    avg += flock[i].pos;
                    nearCount++;
                }
            }

            //If there were neighbors too close, average their locations
            if (nearCount != 0)
            {
                //Otherwise, average their locations
                avg /= nearCount;
            }
            return avg;
        }
    }
}
