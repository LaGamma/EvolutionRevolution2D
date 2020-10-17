using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameKeeper : MonoBehaviour
{
  public GameObject creature;
  public GameObject food;
  public float numCreatures;
  public int numFood;
  public float creatureRange;
  public float foodRange;
  public bool creatureSquare;
  public bool foodSquare;
  public bool randomDrop;
  public bool includePlayer;
    // Start is called before the first frame update
    void Start()
    {
      //settings
      numCreatures = 10f;
      numFood = 5;
      creatureRange = 5f;
      foodRange = 5f;
      creatureSquare = false;
      foodSquare = true;
      includePlayer = true;

      SpawnBatch(numCreatures);
      SpawnFood(numFood);
    }

    // Update is called once per frame
    void Update()
    {
      GameObject[] foodList = GameObject.FindGameObjectsWithTag("food");

      if (foodList.Length == 0) SpawnFood(numFood);

    }

    public void SpawnBatch(float numCreatures) {
      //spawns initial batch of creatures
      Vector2 spawnPoint;
      bool player;

      for (int i=0; i<numCreatures; i++) {
        if (creatureSquare) {
          if (randomDrop) { //random square
              spawnPoint = new Vector2(UnityEngine.Random.Range(-creatureRange, creatureRange),
                                      UnityEngine.Random.Range(-creatureRange, creatureRange));
            }
          else {
              float dist;
              if (i < numCreatures/4) {
                dist = -creatureRange + 2*creatureRange*(i / (numCreatures/4));
                spawnPoint = new Vector2(-creatureRange, dist); }
              else if (i < numCreatures/2) {
                dist = creatureRange - 2*creatureRange*((i-(numCreatures/4)) / (numCreatures/4));
                spawnPoint = new Vector2(creatureRange, dist); }
              else if (i < 3*numCreatures/4) {
                dist = -creatureRange + 2*creatureRange*((i-(numCreatures/2)) / (numCreatures/4));
                spawnPoint = new Vector2(dist, creatureRange); }
              else {
                dist = creatureRange - 2*creatureRange*((i-(3*numCreatures/4)) / (numCreatures/4));
                spawnPoint = new Vector2(dist, -creatureRange); }
          }
        } else { //square
          if (randomDrop) { //random circle
                spawnPoint = new Vector2(UnityEngine.Random.Range(-creatureRange, creatureRange),
                                        UnityEngine.Random.Range(-creatureRange, creatureRange));
                while (spawnPoint.magnitude > creatureRange) {
                  spawnPoint = new Vector2(UnityEngine.Random.Range(-creatureRange, creatureRange),
                                          UnityEngine.Random.Range(-creatureRange, creatureRange));
                }
            }
           else { //circle
              float angle = 2 * Mathf.PI * (i / numCreatures);
              spawnPoint = new Vector2( creatureRange * Mathf.Cos(angle), creatureRange * Mathf.Sin(angle));
            }
          }

          if (includePlayer &&
              ((UnityEngine.Random.Range(0f,1f) <= (1f/numCreatures)) |
                (i == (numCreatures - 1)))) {
            player = true;
            includePlayer = false;}
          else {
            player = false;
          }

          SpawnCreature(spawnPoint, player, new Dictionary<string,float>());
      }
    }

    public void SpawnFood(int numFood) {
      Vector2 spawnPoint;
      Vector2 closestPoint;
      for (int i=0; i<numFood; i++) {
        do {
          if (foodSquare) { //spawn food in square
            spawnPoint = new Vector2(UnityEngine.Random.Range(-foodRange, foodRange),
                                      UnityEngine.Random.Range(-foodRange, foodRange)); }
          else { //spawn food in circle
            float r = UnityEngine.Random.Range(0f, foodRange);
            float theta = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
            spawnPoint = new Vector2(r*Mathf.Cos(theta),r*Mathf.Sin(theta));
              }

              closestPoint = FindClosestTag("creature").GetComponent<CircleCollider2D>().ClosestPoint(spawnPoint);
          } while (food.GetComponent<BoxCollider2D>().bounds.Contains(closestPoint)) ;

          Instantiate(food, spawnPoint, Quaternion.identity);
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

  public void SpawnCreature(Vector2 sp, bool play, Dictionary<string,float> parent) {
    GameObject simCreature = Instantiate(creature, sp, Quaternion.identity);
    MoveCreature mc = simCreature.GetComponent<MoveCreature>();
    mc.player = play;

    if (parent.Count !=0) {
        mc.SetGenetics(
          new Dictionary<string,float> {
            { "speed", RandNormal(parent["speed"], 0.2f, true) },
            { "acceleration" , RandNormal(parent["acceleration"], 1f, true) },
            { "vision", RandNormal(parent["vision"], 2f, true) }
          }
        );
    }
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
