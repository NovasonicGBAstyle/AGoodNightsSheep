using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public float speed = 30f;
    public int damage = 10;

	// Use this for initialization
	void Start () {
        StartCoroutine("deathTimer");
    }
	
	// Update is called once per frame
	void Update () {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    /// <summary>
    /// This will check for collisions to see what was hit.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Missle collided with: " + collision.gameObject.tag + " while missle type is: " + missleType);
        //First, check to see if a sheep was hit.
        if (collision.gameObject.tag == "Sheep")
        {
            //Hit a sheep.  Tell it to take damage.
            collision.gameObject.GetComponent<Sheep>().TakeDamage(damage);

            //Destroy the projectile.
            Destroy(gameObject);
        }
        //Next, check to see if we hit a wall or the ground.
        else if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Ground")
        {
            //It a wall or the ground, so destory the projectile.
            Destroy(gameObject);
        }
    }

    #region CoRoutines
    /// <summary>
    /// So, projectiles are only going to live for three seconds.
    /// </summary>
    /// <returns></returns>
    IEnumerator deathTimer()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
    #endregion
}
