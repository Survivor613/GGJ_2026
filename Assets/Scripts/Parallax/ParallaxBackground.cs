using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;

        lastCameraPositionX = mainCamera.transform.position.x;
        InitializeLayers();
    }

    private void Update()
    {
        float currentCameraPostionX = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPostionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPostionX;

        float cameraLeftEdge = currentCameraPostionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPostionX + cameraHalfWidth;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }

    private void InitializeLayers()
    {
        foreach (ParallaxLayer layer in backgroundLayers)
            layer.CalculateImageWidth();
    }
}
