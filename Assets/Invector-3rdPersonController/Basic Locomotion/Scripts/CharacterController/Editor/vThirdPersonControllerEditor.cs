using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Invector.vCharacterController
{
    [CustomEditor(typeof(vThirdPersonController), true)]
    public class vThirdPersonControllerEditor : vEditorBase
    {
        public vThirdPersonController tp;
        public CapsuleCollider _capsuleCollider;
        public GUIStyle fontLabelStyle = new GUIStyle();

        protected override void OnEnable()
        {
            base.OnEnable();
            tp = (vThirdPersonController)target;
            _capsuleCollider = tp.GetComponent<CapsuleCollider>();
        }

        protected virtual void OnSceneGUI()
        {
            if (tp && tp.debugWindow)
            {
                if (!_capsuleCollider) return;

                DrawGroundDetection();
                DrawStepOffset();
            }
        }

        protected virtual void DrawGroundDetection()
        {

            var checkGroundCenter = tp.transform.position + Vector3.up * (_capsuleCollider.radius);
            var pMin = checkGroundCenter + Vector3.down * (tp.groundMinDistance + _capsuleCollider.radius);
            var pMax = checkGroundCenter + Vector3.down * (tp.groundMaxDistance + _capsuleCollider.radius);

            var pValid = pMin;
            var colorMin = Color.yellow;
            var colorMax = Color.yellow;

            if (tp.isGrounded)
            {
                pValid = pMax;
                colorMax = Color.green;
            }
            else
            {
                pValid = pMin;
                colorMin = Color.green;
            }
            Handles.color = colorMin;
            Handles.DrawSolidDisc(pMin, tp.transform.up, _capsuleCollider.radius * .25f);
            Handles.DrawWireDisc(pMin + Vector3.up * _capsuleCollider.radius, tp.transform.up, _capsuleCollider.radius * 1f);
            Handles.color = colorMax;
            if (tp.isGrounded)
            {
                Handles.DrawWireDisc(pMax + Vector3.up * _capsuleCollider.radius, tp.transform.up, _capsuleCollider.radius * 1f);
            }
            Handles.DrawSolidDisc(pMax, tp.transform.up, _capsuleCollider.radius * .25f);
            Handles.color = Color.green;
            Handles.DrawWireArc(pValid + Vector3.up * _capsuleCollider.radius, tp.transform.right, tp.transform.forward, 180, _capsuleCollider.radius * 1f);
            Handles.DrawWireArc(pValid + Vector3.up * _capsuleCollider.radius, tp.transform.forward, tp.transform.right, -180, _capsuleCollider.radius * 1f);
            Handles.color = Color.red * 0.5f;
            if (Application.isPlaying)
            {
                if (tp.groundHit.collider)
                {
                    Vector3 p = tp.transform.position;
                    p.y = tp.groundHit.point.y;
                    Handles.DrawSolidDisc(p, tp.groundHit.normal, _capsuleCollider.radius * 1f);
                    DrawLabel(p, "GroundHit");
                }
                else Handles.DrawSolidDisc(checkGroundCenter + Vector3.down * (tp.groundMaxDistance + _capsuleCollider.radius), tp.transform.up, _capsuleCollider.radius * 1f);
            }

            Handles.color = Color.white;
            DrawLabel(pMin, "GroundMin");
            DrawLabel(pMax, "GroundMax");
        }

        protected virtual void DrawStepOffset()
        {
            var dir = Vector3.Lerp(tp.transform.forward, tp.moveDirection.normalized, tp.inputSmooth.magnitude);
            Ray stepRay = new Ray((tp.transform.position + tp.transform.up * tp.stepOffsetMaxHeight) + dir.normalized * (_capsuleCollider.radius + tp.stepOffsetDistance), Vector3.down);
            var pDown = stepRay.GetPoint((tp.stepOffsetMaxHeight - tp.stepOffsetMinHeight - 0.025f));
            Handles.DrawLine(stepRay.origin, stepRay.GetPoint((tp.stepOffsetMaxHeight - tp.stepOffsetMinHeight)));

            dir = pDown - stepRay.origin;
            if (dir.magnitude < 0.001f) dir = Vector3.down;
            var pHit = pDown;
            if (tp.applyingStepOffset) pHit.y = tp.stepOffsetHit.point.y;
            var stepConePosition = pHit + Vector3.up * 0.025f;
            Handles.DrawLine(stepRay.origin, stepConePosition);
            Handles.DrawSolidDisc(stepRay.origin, tp.transform.up, 0.05f);
            Handles.color = Color.grey * 0.5f;
            Handles.ConeHandleCap(0, pDown, Quaternion.LookRotation(dir), 0.05f, EventType.Repaint);

            Handles.color = tp.applyingStepOffset ? Color.red * 0.5f : Color.green * 0.5f;
            Handles.DrawSolidDisc(pHit, tp.transform.up, 0.05f);
            if (tp.applyingStepOffset) DrawLabel(pHit, "StepHit");
            Handles.color = tp.applyingStepOffset ? Color.red : Color.green;

            Handles.color = Color.green;

            Handles.ConeHandleCap(0, stepConePosition, Quaternion.LookRotation(dir), 0.045f, EventType.Repaint);


            Handles.color = Color.white;
            DrawLabel(stepRay.origin, "StepMaxHeight");
            DrawLabel(pDown, "StepMinHeight");
        }

        protected virtual void DrawLabel(Vector3 position, string label, int size = 25)
        {
            float zoom = Vector3.Distance(position, SceneView.currentDrawingSceneView.camera.transform.position);
            int fontSize = size;
            fontLabelStyle.fontSize = Mathf.FloorToInt(fontSize / zoom);
            fontLabelStyle.normal.textColor = Color.black;
            fontLabelStyle.alignment = TextAnchor.MiddleLeft;
            Handles.Label(position, label, fontLabelStyle);
        }
    }
}