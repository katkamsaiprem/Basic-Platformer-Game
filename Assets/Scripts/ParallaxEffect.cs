using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxEffect : MonoBehaviour
{
    [FormerlySerializedAs("BGS")] [SerializeField]
    private Transform[] bgs;
    private float[] parallaxScales;

    public float smoothing;

    private Transform cam;

    private Vector3 prevCamPos;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    private void Start()
    {
        prevCamPos = cam.position;

        parallaxScales = new float[bgs.Length];
        for (int i = 0; i < bgs.Length; i++)
        {
            parallaxScales[i] = bgs[i].position.z * -1f;
        }
    }

    private void Update()
    {
        ParallaxEffector();
    }

    void ParallaxEffector()
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            float parallax = (prevCamPos.x - cam.position.x)
                             * parallaxScales[i];

            float targetPosX = bgs[i].position.x + parallax;

            Vector3 bgTargetPos = new Vector3(targetPosX,
                bgs[i].position.y, bgs[i].position.z);

            bgs[i].position = Vector3.Lerp(bgs[i].position,bgTargetPos,smoothing*Time.deltaTime);

        }
        prevCamPos = cam.position;
    }

}