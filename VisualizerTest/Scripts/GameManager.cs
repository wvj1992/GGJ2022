using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] AudioSource gameMusic;
    [SerializeField] BeatScroller beatScroller;
    public static GameManager instance;

    int currScore;
    [SerializeField] int scorePerNote = 100;
    [SerializeField] int scorePerGoodNote = 125;
    [SerializeField] int scorePerPerfectNote = 200;

    int currMultiplier, multiplierTracker;
    int[] multiplierThreshold;

    [SerializeField] Text scoreText;
    public float totalNotes;
    public float normalHits, goodHits, perfectHits, missedHits;

    // Start is called before the first frame update
    void Start()
    {
        currScore = 0;
        currMultiplier = 1;
        //currLife = 100f;

        totalNotes = FindObjectsOfType<NoteObject>().Length;
    }

    public virtual void NoteHit()
    {
        if (currMultiplier - 1 < multiplierThreshold.Length)
        {
            multiplierTracker++;

            if (multiplierThreshold[currMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currMultiplier++;
            }
        }
    }

    public virtual void NormalHit()
    {
        currScore += scorePerNote * currMultiplier;
        NoteHit();

        normalHits++;
    }

    public virtual void GoodHit()
    {
        currScore += scorePerGoodNote * currMultiplier;
        NoteHit();

        goodHits++;
    }

    public virtual void PerfectHit()
    {
        currScore += scorePerPerfectNote * currMultiplier;
        NoteHit();

        perfectHits++;
    }

    public virtual void NoteMiss()
    {
        currMultiplier = 1;
        multiplierTracker = 0;

        missedHits++;
    }
}
