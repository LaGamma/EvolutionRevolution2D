using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCreature : MonoBehaviour
{
  public bool player=true;
  public Dictionary<string,float> genetics;
  public Vector3 lastPosition;
  Vector3 acceleration = Vector3.zero;
  Vector3 velocity = Vector3.zero;
  Vector3 direction = Vector3.zero;
  Vector3 colliderSize;
  Rigidbody rb;
  public bool dead=false;

    // Start is called before the first frame update
    void Start()
    {
      if (genetics == null) {
      //creature attributes
        genetics = new Dictionary<string,float> {
          { "speed", RandNormal(1f, 0.2f, true) },
          { "acceleration" , RandNormal(5f, 1f, true) },
          { "vision", RandNormal(10f, 2f, true) },
          { "jumpSpeed", RandNormal(5f, 1f, true) },
          { "volatility", RandNormal(0.01f, 0.1f, true) },
          { "attack", RandNormal(100f, 20f, true) },
          { "defense", RandNormal(100f, 20f, true) },
          { "responsiveness", RandNormal(0.01f, 0.003f, true) }, //0=no forces towards other creatures,1=max
          { "hunger", RandNormal(0.5f, 0.1f, true) }, //greater numbers=food is a motivating factor
          { "aggression", RandNormal(0.5f, 0.1f, true) } //greater numbers=other creatures are a motivating factor
        };
      }

      //get dimensions of creature
      BoxCollider2D collider = GetComponent<BoxCollider2D>();
      colliderSize = collider.size;
      colliderSize = Vector3.Scale(colliderSize, transform.localScale);
    }

    // Update is called once per frame
    void Update()
    {
      //calculated acceleration direction (normalized)
      if (player) {
          if (Input.GetKey(KeyCode.UpArrow)){
            acceleration[1] = 1; }
          else if (Input.GetKey(KeyCode.DownArrow)){
            acceleration[1] = -1; }
          else {
            acceleration[1] = 0f;}

          if (Input.GetKey(KeyCode.RightArrow)){
            acceleration[0] = 1; }
          else if (Input.GetKey(KeyCode.LeftArrow)){
            acceleration[0] = -1; }
          else {
            acceleration[0] = 0; }

          acceleration = acceleration.normalized;
        }
      else { //Simulation creature
        //if (GameObject.FindGameObjectsWithTag("food").Length != 0) {
          GameObject closestFood = FindClosestTag("food");
          Vector3 foodPointer = closestFood.transform.position - transform.position;

          Vector3 progress = transform.position - lastPosition;
          progress = Vector3.Project(progress, acceleration);

          Vector3 foodForce = Vector3.zero;

          if (closestFood != null && Vector3.Magnitude(foodPointer) < genetics["vision"]) {
            //add force towards nearest food (within vision)
            foodForce = foodPointer.normalized;
          }

          if (acceleration == Vector3.zero ) {
            //redirect acceleration
            print(gameObject.name + " rerouting");
            float angle = UnityEngine.Random.Range(0,(float) Mathf.PI*2);
            acceleration = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
          }
          acceleration = acceleration.normalized * genetics["acceleration"];
        }

        //update velocity
        velocity += acceleration * genetics["acceleration"] * Time.deltaTime;

        //clamp to maximum speed
        velocity = Vector3.ClampMagnitude(velocity, genetics["speed"]);

        //update position
        lastPosition = transform.position;
        transform.position += (velocity * Time.deltaTime);

        //look in the direction you're moving
        // if (velocity != Vector3.zero) {
        //   direction = velocity.normalized;
        //   transform.rotation = Quaternion.Slerp(transform.rotation,
        //                                         Quaternion.LookRotation(direction), Time.deltaTime);
        // }

        Vector3 p = transform.position;
    }

    public GameObject FindClosestTag(string tag) {
      GameObject[] tagList = GameObject.FindGameObjectsWithTag(tag);
      GameObject tMin = null;
      float min = Mathf.Infinity;
      foreach (GameObject t in tagList) {
        float dist = Vector3.Distance(t.transform.position, this.transform.position);
        if (dist < min) {
          tMin = t;
          min = dist;
        }
      }
      return tMin;
    }

    void OnCollisionEnter2D(Collision2D col) {
      if (col.gameObject.tag == "food") {
        Destroy(col.gameObject);
      print("eat"); }
    }

    public float RandNormal(float mean, float stdDev, bool pos) {
      float val;
      //set pos to true to only generate positive values
      do {
        //Box-Muller transform
        float z1 = UnityEngine.Random.Range(0f,1f);
        float z2 = UnityEngine.Random.Range(0f,1f);
        float stdnorm = Mathf.Sqrt(-2 * Mathf.Log(z1)) *
                         Mathf.Sin(2 * Mathf.PI * z2);
        val = mean + stdDev * stdnorm; }
        while (val < 0 | !pos);
      return val;
    }
}
