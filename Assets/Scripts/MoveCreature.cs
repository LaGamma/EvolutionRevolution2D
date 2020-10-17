using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCreature : MonoBehaviour
{
  public bool player=false;
  public Dictionary<string,float> genetics;
  private Rigidbody2D rb;
  public bool dead=false;

    // Start is called before the first frame update
    void Start()
    {
      rb = GetComponent<Rigidbody2D> ();
      if (genetics == null) {
      //creature attributes
        genetics = new Dictionary<string,float> {
          { "speed", RandNormal(1f, 0.2f, true) },
          { "acceleration" , RandNormal(5f, 1f, true) },
          { "vision", RandNormal(10f, 2f, true) }
        };
      }
    }

    // Update is called once per frame
    void Update()
    {
      //calculated acceleration direction (normalized)
      if (player) {
          float moveHorizontal = Input.GetAxis ("Horizontal");
          float moveVertical = Input.GetAxis ("Vertical");
          Vector2 movement = new Vector2 (moveHorizontal, moveVertical);
          rb.AddForce (movement);
        }
      else { //Simulation creature
          GameObject closestFood = FindClosestTag("food");

          if (closestFood != null) {
            //add force towards nearest food (within vision)
            Vector2 foodPointer = closestFood.transform.position - transform.position;
            rb.AddForce (foodPointer.normalized);
          }
        }
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
        Destroy(col.gameObject); }
      if (col.gameObject.tag == "background" && player == false) {
        float angle = UnityEngine.Random.Range(0,(float) Mathf.PI*2);
        rb.AddForce (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }
    }

    public void SetGenetics(Dictionary<string,float> jeans) {
      genetics = jeans;
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
