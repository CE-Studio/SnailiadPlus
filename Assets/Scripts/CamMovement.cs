using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour
{
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

            transform.position = new Vector2(
                Mathf.Clamp(Mathf.Lerp(transform.position.x, PlayState.player.transform.position.x, 0.1f), camBoundsX.x, camBoundsX.y),
                Mathf.Clamp(Mathf.Lerp(transform.position.y, PlayState.player.transform.position.y, 0.1f), camBoundsY.x, camBoundsY.y));

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
