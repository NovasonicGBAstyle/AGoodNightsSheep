using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Attractor : MonoBehaviour
{

    //This static declaration makes the value a shared value through every instance of this class.  It's like all of them only have the one POS.
    static public Vector3 POS = Vector3.zero;

    [Header("Set in Inspector")]
    public float radius = 10f;
    public float xPhase = 0.5f;
    public float yPhase = 0.4f;
    public float zPhase = 0.1f;

    private void Awake()
    {
        POS = gameObject.transform.position;
    }

    //FixedUpdate is called once per physics update (i.e. 50x/second)
    private void FixedUpdate()
    {
        //Vector3 tPos = Vector3.zero;

        //Vector3 scale = transform.localScale;

        //tPos.x = Mathf.Sin(xPhase * Time.time) * radius * scale.x;
        //tPos.y = Mathf.Sin(yPhase * Time.time) * radius * scale.y;
        //tPos.z = Mathf.Sin(zPhase * Time.time) * radius * scale.z;

        //transform.position = tPos;
        //POS = tPos;
    }
}
