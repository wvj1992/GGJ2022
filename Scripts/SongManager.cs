using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongManager : MonoBehaviour
{
    private SongFileRW songReader;
    private AudioSource aud;
    private List<Tuple<float, int>> songData;
    float time = 0f;
    float startTime;

    public List<Image> imagesToManipulate;

    private void Awake()
    {
        songReader = new SongFileRW();
        aud = GetComponent<AudioSource>();

        string title = aud.clip.name;
        string filePath = "Assets/Songs/" + title + ".txt";
        songData = songReader.ReadSongFile(filePath);
        //PrintResults();
        StartCoroutine(play());
    }
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        time = Time.time - startTime;
    }

    void PrintResults()
    {
        foreach(Tuple<float, int> data in songData) {
            Debug.Log(string.Format("{0} {1}", data.Item1, data.Item2));
        }
    }

    public IEnumerator play()
    {
        startTime = Time.time;
        aud.Play();
        for(int i = 0; i < songData.Count; ++i)
        {
            while(time < songData[i].Item1)
            {
                yield return new WaitForFixedUpdate();
            }
            imagesToManipulate[songData[i].Item2].color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            Debug.Log(string.Format("Actual Time: {0} vs Hit Time: {1}. Difference = {2}", time, songData[i].Item1, time - songData[i].Item1));
        }
    }
}
