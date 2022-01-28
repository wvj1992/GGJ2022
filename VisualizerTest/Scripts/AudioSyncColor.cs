using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(Image))]
public class AudioSyncColor : AudioSyncer
{
    public Color[] beatColors;
    public Color restColor;

    private int m_randomIndex;
    private Image m_img;

    // Start is called before the first frame update
    void Start()
    {
        m_img = GetComponent<Image>();
    }

    private IEnumerator MoveToColor(Color _target)
    {
        Color _curr = m_img.color;
        Color _init = _curr;
        float _timer = 0;

        while (_curr != _target)
        {
            _curr = Color.Lerp(_init, _target, _timer / timeToBeat);
            _timer += Time.deltaTime;

            m_img.color = _curr;

            yield return null;
        }

        m_isBeat = false;
    }

    private Color RandomColor()
    {
        if (beatColors == null || beatColors.Length == 0)
            return Color.white;

        m_randomIndex = Random.Range(0, beatColors.Length);
        return beatColors[m_randomIndex];
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (m_isBeat)
            return;

        m_img.color = Color.Lerp(m_img.color, restColor, restSmoothTime * Time.deltaTime);
    }

    public override void OnBeat()
    {
        base.OnBeat();

        Color _c = RandomColor();

        StopCoroutine("MoveToColor");
        StartCoroutine("MoveToColor", _c);
    }
}
