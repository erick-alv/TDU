using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float zoomSpeed = 40.0f;

    private Controls controls;
    private bool isZooming = false;
    private const float thresholdOppositeDir = -0.6f;
    private Vector2 prevPos1 = Vector2.zero;
    private Vector2 pos1 = Vector2.zero;
    private Vector2 prevPos2 = Vector2.zero;
    private Vector2 pos2 = Vector2.zero;
    private float prevDistance = -1;
    private float distance = -1;

    private void Awake()
    {
        controls = new Controls();
    }

    void Start()
    {
        controls.Touch.TwoFingersContact.started += ctx => BeginZoom();
        controls.Touch.TwoFingersContact.canceled += ctx => StopZoom();
#if UNITY_EDITOR
        controls.Mouse.Scroll.performed += ctx => { transform.Translate(ctx.ReadValue<Vector2>().y * Vector3.up * zoomSpeed * 0.2f * Time.deltaTime, Space.World); };
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (isZooming)
        {
            if (prevDistance == -1) //is the first time it enters so we just put the values
            {
                prevPos1 = controls.Touch.Finger1Pos.ReadValue<Vector2>();
                prevPos2 = controls.Touch.Finger2Pos.ReadValue<Vector2>();
                prevDistance = (prevPos1 - prevPos2).magnitude;
            }
            else
            {
                pos1 = controls.Touch.Finger1Pos.ReadValue<Vector2>();
                pos2 = controls.Touch.Finger2Pos.ReadValue<Vector2>();
                distance = (pos1 - pos2).magnitude;
                Vector2 dir1 = pos1 - prevPos1;
                Vector2 dir2 = pos2 - prevPos2;
                float dirDot = Vector2.Dot(dir1.normalized, dir2.normalized);
                if (dirDot < thresholdOppositeDir)//Moving in opposite Directions
                { 
                    if (distance < prevDistance)//Zooming out
                    {
                        transform.Translate(Vector3.up * zoomSpeed * Time.deltaTime, Space.World);
                    }
                    else if (distance > prevDistance) //Zooming in
                    {
                        transform.Translate(-Vector3.up * zoomSpeed * Time.deltaTime, Space.World);
                    }
                }

                prevPos1 = pos1;
                prevPos2 = pos2;
                prevDistance = distance;
            }

        }

    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void BeginZoom()
    {
        isZooming = true;
    }

    private void StopZoom()
    {
        isZooming = false;
        prevDistance = -1;
        distance = -1;
    }
}
