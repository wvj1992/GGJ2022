using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] public Sprite defaultImage, pressedImage;

    public KeyCode keyToPress;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        //sr.size = new Vector2(256f, 256f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
            sr.sprite = pressedImage;
        if (Input.GetKeyUp(keyToPress))
            sr.sprite = defaultImage;
    }
}
