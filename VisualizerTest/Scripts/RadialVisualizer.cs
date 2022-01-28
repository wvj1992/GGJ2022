using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialVisualizer : MonoBehaviour
{
    public GameObject enemy1, enemy2, enemy3;
    int numOfObjects = 12;
    float radius = 20;

    // Start is called before the first frame update
    void Start()
    {
        SpawnDiffObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnDiffObjects()
    {
        for (int i = 0; i < numOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numOfObjects;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            switch (i % 3)
            {
                case 0:
                    Instantiate(enemy1, pos, Quaternion.identity);
                    break;
                case 1:
                    Instantiate(enemy2, pos, Quaternion.identity);
                    break;
                case 2:
                    Instantiate(enemy3, pos, Quaternion.identity);
                    break;
            }
        }
    }
}
