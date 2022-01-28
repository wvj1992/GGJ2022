using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
    public float bias, timeStep, timeToBeat, restSmoothTime;

    private float m_prevAudioValue, m_audioValue, m_timer;

    protected bool m_isBeat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        m_prevAudioValue = m_audioValue;
        m_audioValue = AudioSpectrum.spectrumValue;

        if (m_prevAudioValue > bias && m_audioValue <= bias)
            if (m_timer > timeStep)
                OnBeat();

        if (m_prevAudioValue <= bias && m_audioValue > bias)
            if (m_timer > timeStep)
                OnBeat();

        m_timer += Time.deltaTime;
    }

    public virtual void OnBeat()
    {
        Debug.Log("Beat");
        m_timer = 0;
        m_isBeat = true;
    }
}
