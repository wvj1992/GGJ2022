using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageVisualizer : MonoBehaviour
{
    public Image myImage1, myImage2, mainImage;
    float totalImages = 10;
    float radius = 175;
    Transform center;

    // Start is called before the first frame update
    void Start()
    {
        center = mainImage.transform;
        SpawnObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnObjects()
    {
        for (int i = 0; i < totalImages; i++)
        {
            float point = i / totalImages;
            float angle = (point * Mathf.PI * 2);
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            Vector3 pos = new Vector3(x, y, 0) + center.position;

            switch (i / 5)
            {
                case 0:
                    Image image = Instantiate(myImage1, pos, Quaternion.Euler(0, 0, -Mathf.Rad2Deg * angle), center);
                    break;
                case 1:
                    image = Instantiate(myImage2, pos, Quaternion.Euler(0, 0, -Mathf.Rad2Deg * angle), center);
                    break;
            }
        }
    }
}
