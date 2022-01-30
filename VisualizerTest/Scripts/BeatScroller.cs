using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    [SerializeField] public float beatTempo;
    public GameObject[] notePrefab;
    List<GameObject> spawnedNotes = new List<GameObject>();

    float numberOfNotes = 10;
    float speed = 50f;

    float beatTimer = 2f;

    // Start is called before the first frame update
    void Start()
    {
        beatTempo = beatTempo / 60f;
    }

    // Update is called once per frame
    void Update()
    {
        beatTimer -= Time.deltaTime;

        for (int i = 0; i < numberOfNotes; i++)
        {
            for (int j = 0; j < notePrefab.Length; j++)
            {
                if (beatTimer <= 0)
                {
                    GameObject image = Instantiate(notePrefab[Random.Range(0, notePrefab.Length)], transform.position, Quaternion.identity);
                    image.transform.SetParent(this.transform);
                    spawnedNotes.Add(image);
                    beatTimer = 2f;
                }
            }
        }

        foreach (GameObject notes in spawnedNotes)
        {
            if (notes != null)
                notes.transform.position += Vector3.down * speed * Time.deltaTime;
        }
    }
}
