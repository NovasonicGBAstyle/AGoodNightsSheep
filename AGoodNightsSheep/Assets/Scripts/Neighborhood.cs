using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Neighborhood : MonoBehaviour
{

    [Header("Set Dynamically")]
    public List<Boid> neighbors;
    private CapsuleCollider coll;

    //This is creating a list of neighbors.  It sets the radius of the colider to half that of the spawner so that it should see less items.
    private void Start()
    {
        neighbors = new List<Boid>();
        coll = GetComponent<CapsuleCollider>();
        coll.radius = Spawner.S.neighborDist / 2;
    }

    //Here, check to see if the dist has changed.  If so, it sets our radius.  I don't think this should change unless
    //We are told to manually change it during runtime later.  I didn't think that should happen with what has currently
    //been done so far.
    private void FixedUpdate()
    {
        if (coll.radius != Spawner.S.neighborDist / 2)
        {
            coll.radius = Spawner.S.neighborDist / 2;
        }
    }

    //Now, we'll add boid game objects that get close to us.
    private void OnTriggerEnter(Collider other)
    {
        Boid b = other.GetComponent<Boid>();
        if (b != null)
        {
            if (neighbors.IndexOf(b) == -1)
            {
                neighbors.Add(b);
            }
        }
    }

    //Now, we'll remove boid objects that move away from us.
    private void OnTriggerExit(Collider other)
    {
        Boid b = other.GetComponent<Boid>();
        if (b != null)
        {
            if (neighbors.IndexOf(b) != -1)
            {
                neighbors.Remove(b);
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
            if (neighbors.Count == 0) return avg;

            for (int i = 0; i < neighbors.Count; i++)
            {
                avg += neighbors[i].pos;
            }

            avg /= neighbors.Count;

            return avg;
        }
    }

    //Same thing but for velocity.
    public Vector3 avgVel
    {
        get
        {
            Vector3 avg = Vector3.zero;
            if (neighbors.Count == 0) return avg;

            for (int i = 0; i < neighbors.Count; i++)
            {
                avg += neighbors[i].rigid.velocity;
            }

            avg /= neighbors.Count;

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
            for (int i = 0; i < neighbors.Count; i++)
            {
                delta = neighbors[i].pos - transform.position;

                if (delta.magnitude <= Spawner.S.collDist)
                {
                    avg += neighbors[i].pos;
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