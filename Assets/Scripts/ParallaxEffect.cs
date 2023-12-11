using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField]
    private Transform[] BGS; // Array of background elements to apply the parallax effect to
    private float[] parallaxScales; // Scales of the parallax effect for each background element

    public float smoothing; // Smoothing factor for how smooth the parallax effect is

    private Transform cam; // Reference to the main camera's transform

    private Vector3 prevCamPos; // The previous frame's camera position

    private void Awake()
    {
        // Initialize the camera reference
        cam = Camera.main.transform;
    }

    private void Start()
    {
        // Store the initial position of the camera
        prevCamPos = cam.position;

        // Initialize the parallax scales array based on the number of background elements
        parallaxScales = new float[BGS.Length];
        for (int i = 0; i < BGS.Length; i++)
        {
            // The parallax scale is inversely proportional to the z position of the background element
            // Elements further away (larger z) move slower than those closer
            parallaxScales[i] = BGS[i].position.z * -1f;
        }
    }

    private void Update()
    {
        // Update the parallax effect each frame
        ParallaxEffector();
    }

    void ParallaxEffector()
    {
        // Loop through all background elements
        for (int i = 0; i < BGS.Length; i++)
        {
            // Calculate the parallax effect based on the change in camera position
            // Elements move a fraction of the distance the camera moved, scaled by the parallax scale
            float parallax = (prevCamPos.x - cam.position.x) * parallaxScales[i];

            // Determine the target x position for the background element
            float targetPosX = BGS[i].position.x + parallax;

            // Create a target position which is the current position of the background element
            // but with the target x position
            Vector3 bgTargetPos = new Vector3(targetPosX, BGS[i].position.y, BGS[i].position.z);

            // Smoothly interpolate from the current position to the target position
            // The smoothing factor controls the rate of this transition
            BGS[i].position = Vector3.Lerp(BGS[i].position, bgTargetPos, smoothing * Time.deltaTime);
        }

        // Update the previous camera position to the current camera position
        // for use in the next frame
        prevCamPos = cam.position;
    }
}
