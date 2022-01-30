using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour, ICollectNotes
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] public int playerID, currScore;

    [SerializeField] int scorePerNote = 100;
    [SerializeField] int scorePerGoodNote = 125;
    [SerializeField] int scorePerPerfectNote = 200;

    int currMultiplier, multiplierTracker;
    int[] multiplierThreshold;

    public float currLife, totalNotes;
    public float normalHits, goodHits, perfectHits, missedHits;

    // Start is called before the first frame update
    void Start()
    {
        currScore = 0;
        currMultiplier = 1;
        currLife = 100f;
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
