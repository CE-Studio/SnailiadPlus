using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour
{
    public Transform focusPoint;

    void FixedUpdate()
    {
        if (PlayState.gameState != "Menu")
        {
            Vector2 camBoundsX = new Vector2(
                PlayState.camCenter.x - PlayState.camBoundaryBuffers.x + PlayState.camTempBuffersX.x,
                PlayState.camCenter.x + PlayState.camBoundaryBuffers.x - PlayState.camTempBuffersX.y);
            Vector2 camBoundsY = new Vector2(
                PlayState.camCenter.y - PlayState.camBoundaryBuffers.y + PlayState.camTempBuffersY.x,
                PlayState.camCenter.y + PlayState.camBoundaryBuffers.y - PlayState.camTempBuffersY.y);
            float xDif = camBoundsX.y - camBoundsX.x;
            float yDif = camBoundsY.y - camBoundsY.x;

            if (PlayState.gameState == "Game")
            {
                bool inBoundsX = ((camBoundsX.x <= (focusPoint.position.x + 13.5f)) && ((focusPoint.position.x - 13.5f) <= camBoundsX.y));
                bool inBoundsY = ((camBoundsY.x <= (focusPoint.position.y + 8.5f)) && ((focusPoint.position.y - 8.5f) <= camBoundsY.y));
                if (inBoundsX && inBoundsY) 
                {
                    transform.position = new Vector2(
                        xDif >= 0 ? Mathf.Clamp(Mathf.Lerp(transform.position.x, focusPoint.position.x, 0.1f), camBoundsX.x, camBoundsX.y) : camBoundsX.x + (xDif * 0.5f),
                        yDif >= 0 ? Mathf.Clamp(Mathf.Lerp(transform.position.y, focusPoint.position.y, 0.1f), camBoundsY.x, camBoundsY.y) : camBoundsY.x + (yDif * 0.5f));
                } 
                else 
                {
                    transform.position = new Vector2(
                        Mathf.Lerp(transform.position.x, focusPoint.position.x, 0.1f),
                        Mathf.Lerp(transform.position.y, focusPoint.position.y, 0.1f));
                }
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
}
