using System.Collections;
using UnityEngine;
using TMPro;

public class TextEffect : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public Color fillColor = Color.white;       // Color for the text fill
    public Color outlineColor = Color.black;    // Outline color
    public float waveFrequency = 2f;            // Frequency of the wave
    public float waveSpeed = 0.5f;              // Speed of the wave fill moving upward

    private float currentFillHeight = 0f;       // Tracks how much of the text has been filled
    private Mesh mesh;

    void Start()
    {
        // Set initial outline and fill settings
        textMeshPro.outlineColor = outlineColor;
        textMeshPro.outlineWidth = 0.3f;
        textMeshPro.color = new Color(1, 1, 1, 0); // Transparent fill

        StartCoroutine(WaveFillEffect());
    }

    private IEnumerator WaveFillEffect()
    {
        textMeshPro.ForceMeshUpdate();
        mesh = textMeshPro.mesh;

        while (currentFillHeight < textMeshPro.bounds.extents.y * 2f)
        {
            // Gradually increase fill height to create upward motion
            currentFillHeight += Time.deltaTime * waveSpeed;

            // Force update the text mesh so we can access vertices
            textMeshPro.ForceMeshUpdate();
            mesh = textMeshPro.mesh;

            Vector3[] vertices = mesh.vertices;
            Color32[] colors = mesh.colors32;

            // Loop through each character's quad
            for (int i = 0; i < vertices.Length; i += 4)
            {
                // Calculate the wave offset based on x position
                float waveOffset = Mathf.Sin(vertices[i].x * waveFrequency) * 0.1f;

                for (int j = 0; j < 4; j++)
                {
                    // Calculate the vertex position relative to the fill height + wave offset
                    if (vertices[i + j].y <= textMeshPro.bounds.min.y + currentFillHeight + waveOffset)
                    {
                        colors[i + j] = fillColor; // Fill color
                    }
                    else
                    {
                        colors[i + j] = new Color32(255, 255, 255, 0); // Transparent to show only outline
                    }
                }
            }

            // Apply updated colors to the mesh and set it back on the TMP component
            mesh.colors32 = colors;
            textMeshPro.canvasRenderer.SetMesh(mesh);

            yield return null;
        }
    }
}
