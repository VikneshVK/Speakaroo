using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;
    private float initialSize;
    private Vector3 initialPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
        initialSize = cam.orthographicSize;
        initialPosition = cam.transform.position;
    }

    public void ZoomTo(Transform target, float zoomInSize, float zoomDuration)
    {
        StartCoroutine(ZoomCoroutine(target, zoomInSize, zoomDuration));
    }

    public void ZoomOut(float zoomDuration)
    {
        StartCoroutine(ZoomCoroutine(initialPosition, initialSize, zoomDuration));
    }

    IEnumerator ZoomCoroutine(Transform target, float targetSize, float duration)
    {
        Vector3 startPosition = cam.transform.position;
        float startSize = cam.orthographicSize;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, startPosition.z);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cam.orthographicSize = targetSize;
        cam.transform.position = targetPosition;
    }

    IEnumerator ZoomCoroutine(Vector3 targetPosition, float targetSize, float duration)
    {
        Vector3 startPosition = cam.transform.position;
        float startSize = cam.orthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cam.orthographicSize = targetSize;
        cam.transform.position = targetPosition;
    }
}
