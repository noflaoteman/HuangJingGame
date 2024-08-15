// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if ! UNITY_EDITOR
#pragma warning disable CS0618 // Type or member is obsolete (for LinearMixerTransition in Animancer Lite).
#endif
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.Units;
using UnityEngine;

namespace Animancer.Examples.StateMachines
{
    /// <summary>
    /// A <see cref="CharacterState"/> which moves the character according to their
    /// <see cref="CharacterParameters.MovementDirection"/>.
    /// </summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/fsm/brains">Brains</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.StateMachines/MoveState
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Brains - Move State")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(StateMachines) + "/" + nameof(MoveState))]
    public sealed class MoveState : CharacterState
    {
        /************************************************************************************************************************/

        [SerializeField, DegreesPerSecond] private float _TurnSpeed = 360;
        [SerializeField] private float _ParameterFadeSpeed = 2;
        [SerializeField] private LinearMixerTransition _Animation;

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Character.Animancer.Play(_Animation);
            _Animation.State.Parameter = Character.Parameters.WantsToRun ? 1 : 0;
        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateSpeed();
            UpdateTurning();
        }

        /************************************************************************************************************************/

        private void UpdateSpeed()
        {
            int target = Character.Parameters.WantsToRun ? 1 : 0;
            _Animation.State.Parameter = Mathf.MoveTowards(
                _Animation.State.Parameter,
                target,
                _ParameterFadeSpeed * Time.deltaTime);
        }

        /************************************************************************************************************************/

        private void UpdateTurning()
        {
            // Don't turn if we aren't trying to move.
            Vector3 movement = Character.Parameters.MovementDirection;
            if (movement == default)
                return;

            // Determine the angle we want to turn towards.
            // Without going into the maths behind it, Atan2 gives us the angle of a vector in radians.
            // So we just feed in the x and z values because we want an angle around the y axis,
            // then convert the result to degrees because Transform.eulerAngles uses degrees.
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

            // Determine how far we can turn this frame (in degrees).
            float turnDelta = _TurnSpeed * Time.deltaTime;

            // Get the current rotation, move its y value towards the target, and apply it back to the Transform.
            Transform transform = Character.Animancer.transform;
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y = Mathf.MoveTowardsAngle(eulerAngles.y, targetAngle, turnDelta);
            transform.eulerAngles = eulerAngles;
        }

        /************************************************************************************************************************/
    }
}
