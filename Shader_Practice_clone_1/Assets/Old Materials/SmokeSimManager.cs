using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeSimManager : MonoBehaviour
{
    [SerializeField]
    CustomRenderTexture texture;
    [SerializeField]
    int iterationsperframe;
    // Start is called before the first frame update
    void Start()
    {
        texture.Initialize();
                
    }

    // Update is called once per frame
    void Update()
    {
        texture.Update(2);

    }
}
