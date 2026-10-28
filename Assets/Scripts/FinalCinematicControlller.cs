using UnityEngine;
using UnityEngine.Playables;

public class FinalCinematicController : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;
    [SerializeField, Min(0f)] private float transitionStartTime = 14f;
    [SerializeField, Min(0.01f)] private float transitionDuration = 2f;

    [Header("Targets")]
    [SerializeField] private Transform initialTarget;
    [SerializeField] private Transform finalTarget;

    [Header("Initial Framing")]
    [SerializeField] private Vector2 initialOffset = new Vector2(0f, 1f);

    [Header("Final Framing")]
    [SerializeField] private Vector2 finalOffset = new Vector2(0f, 1f);

    [Header("Camera Movement")]
    [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.35f;

    private float fixedZ;
    private Vector3 positionVelocity;

    private void Awake()
    {
        fixedZ = transform.position.z;

        // Keep the camera aligned with a 2D scene.
        transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (director == null || initialTarget == null || finalTarget == null)
        {
            return;
        }

        float timelineTime = (float)director.time;

        Vector2 desiredPosition;
        Vector2 initialPosition = GetTargetPosition(
            initialTarget,
            initialOffset
        );

        Vector2 finalPosition = GetTargetPosition(
            finalTarget,
            finalOffset
        );

        if (timelineTime < transitionStartTime)
        {
            desiredPosition = initialPosition;
        }
        else if (timelineTime < transitionStartTime + transitionDuration)
        {
            float progress = Mathf.Clamp01(
                (timelineTime - transitionStartTime) / transitionDuration
            );

            progress = Mathf.SmoothStep(0f, 1f, progress);

            desiredPosition = Vector2.Lerp(
                initialPosition,
                finalPosition,
                progress
            );
        }
        else
        {
            desiredPosition = finalPosition;
        }

        Vector3 targetPosition = new Vector3(
            desiredPosition.x,
            desiredPosition.y,
            fixedZ
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        // Prevent any rotation from making the 2D camera feel three-dimensional.
        transform.rotation = Quaternion.identity;
    }

    private Vector2 GetTargetPosition(Transform target, Vector2 offset)
    {
        // Offset is relative to world axes, not the target's rotation.
        return (Vector2)target.position + offset;
    }
}
