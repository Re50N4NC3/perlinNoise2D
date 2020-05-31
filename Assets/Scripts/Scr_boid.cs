using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_boid : MonoBehaviour{
    public float targetX = 0;  // target position to reach
    public float targetY = 0;  // target position to reach
    public float targetZ = 0;  // target position to reach
    public float targetScale = 0;  // target scale to reach

    float posChange = 0;  // delta position
    float scaleChange = 0;  // delta scale

    public int xx;  // position on the grid
    public int yy;  // position on the grid

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        Move();
    }

    // change position and scale
    void Move() {
        int smooth = 6;  // smoothing value for deltas

        scaleChange = (targetScale - transform.localScale.y) / smooth;  // calculate change of the scale
        posChange = (Mathf.Abs(targetY) - transform.position.y) / smooth;  // calculate chnge of the position

        // check if above values are small enough
        if (Mathf.Abs(scaleChange) < 0.01) { scaleChange = 0; }
        if (Mathf.Abs(posChange) < 0.01) { posChange = 0; }


        transform.localScale = new Vector3(1, transform.localScale.y + scaleChange, 1);  // scale the boid
        transform.position = new Vector3(targetX, transform.position.y + posChange, targetZ);  // move the boid

    }
}
