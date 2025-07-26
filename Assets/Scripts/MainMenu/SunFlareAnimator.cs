using UnityEngine;

public class SunFlareAnimator : MonoBehaviour
{
    [SerializeField] private Transform sunLight; // Assign your sun/directional light here
    [SerializeField] private Vector3 baseRotation = new Vector3(50, 30, 0); // Starting rotation (Euler angles)
    [SerializeField] private float rotationAmplitude = 10f; // How far to swing (degrees)
    [SerializeField] private float rotationSpeed = 1f; // How fast to animate

    // Update is called once per frame
    void Update()
    {
        float time = Time.time * rotationSpeed;
        float swing = Mathf.Sin(time) * rotationAmplitude;
        // Only animate the Y axis for a "dancing" effect, but you can adjust as needed
        Vector3 animatedRotation = baseRotation + new Vector3(0, swing, 0);
        sunLight.rotation = Quaternion.Euler(animatedRotation);
    }
}
