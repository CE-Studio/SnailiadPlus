using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour
{
    public GameObject player;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (PlayState.camBoundaryBuffers.x == 0 && PlayState.camBoundaryBuffers.y == 0)
        {
            transform.position = PlayState.camCenter;
        }
        else
        {
            transform.position = new Vector2(
                Mathf.Clamp(Mathf.Lerp(transform.position.x, player.transform.position.x, 0.125f), -PlayState.camBoundaryBuffers.x, PlayState.camBoundaryBuffers.x),
                Mathf.Clamp(Mathf.Lerp(transform.position.y, player.transform.position.y, 0.125f), -PlayState.camBoundaryBuffers.y, PlayState.camBoundaryBuffers.y));
        }

        Debug.DrawLine(
            new Vector2(PlayState.camCenter.x - PlayState.camBoundaryBuffers.x, PlayState.camCenter.y + PlayState.camBoundaryBuffers.y),
            new Vector2(PlayState.camCenter.x + PlayState.camBoundaryBuffers.x, PlayState.camCenter.y + PlayState.camBoundaryBuffers.y),
            Color.green,
            0,
            false);
        Debug.DrawLine(
            new Vector2(PlayState.camCenter.x - PlayState.camBoundaryBuffers.x, PlayState.camCenter.y - PlayState.camBoundaryBuffers.y),
            new Vector2(PlayState.camCenter.x + PlayState.camBoundaryBuffers.x, PlayState.camCenter.y - PlayState.camBoundaryBuffers.y),
            Color.green,
            0,
            false);
        Debug.DrawLine(
            new Vector2(PlayState.camCenter.x - PlayState.camBoundaryBuffers.x, PlayState.camCenter.y - PlayState.camBoundaryBuffers.y),
            new Vector2(PlayState.camCenter.x - PlayState.camBoundaryBuffers.x, PlayState.camCenter.y + PlayState.camBoundaryBuffers.y),
            Color.green,
            0,
            false);
        Debug.DrawLine(
            new Vector2(PlayState.camCenter.x + PlayState.camBoundaryBuffers.x, PlayState.camCenter.y - PlayState.camBoundaryBuffers.y),
            new Vector2(PlayState.camCenter.x + PlayState.camBoundaryBuffers.x, PlayState.camCenter.y + PlayState.camBoundaryBuffers.y),
            Color.green,
            0,
            false);
    }
}
