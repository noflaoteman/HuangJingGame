// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

using UnityEngine;

namespace Animancer.Examples.InverseKinematics
{
    /// <summary>Allows the user to drag any object with a collider around on screen with the mouse.</summary>
    /// <example><see href="https://kybernetik.com.au/animancer/docs/examples/ik/puppet">Puppet</see></example>
    /// https://kybernetik.com.au/animancer/api/Animancer.Examples.InverseKinematics/MouseDrag
    /// 
    [AddComponentMenu(Strings.ExamplesMenuPrefix + "Inverse Kinematics - Mouse Drag")]
    [HelpURL(Strings.DocsURLs.ExampleAPIDocumentation + nameof(InverseKinematics) + "/" + nameof(MouseDrag))]
    public sealed class MouseDrag : MonoBehaviour
    {
        /************************************************************************************************************************/

        private Transform _Dragging;
        private float _Distance;

        /************************************************************************************************************************/

        private void Update()
        {
            // On click, do a raycast from the mouse, grab whatever it hits, and calculate how far away it is.
            if (ExampleInput.LeftMouseDown)
            {
                Ray ray = Camera.main.ScreenPointToRay(ExampleInput.MousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _Dragging = hit.transform;

                    Transform cameraTransform = Camera.main.transform;
                    _Distance = Vector3.Dot(_Dragging.position - cameraTransform.position, cameraTransform.forward);
                }

                return;
            }
            // While holding the button, move the object in line with the mouse ray.
            else if (_Dragging != null && ExampleInput.LeftMouseHold)
            {
                Ray ray = Camera.main.ScreenPointToRay(ExampleInput.MousePosition);

                Transform cameraTransform = Camera.main.transform;
                Vector3 forward = cameraTransform.forward;

                float dot = Vector3.Dot(ray.direction, forward);
                if (dot > 0)
                {
                    Vector3 planeCenter = cameraTransform.position + forward * _Distance;
                    Vector3 intersection = ray.origin + ray.direction * Vector3.Dot(planeCenter - ray.origin, forward) / dot;
                    _Dragging.position = intersection;
                    return;
                }
            }

            _Dragging = null;
        }

        /************************************************************************************************************************/
    }
}
