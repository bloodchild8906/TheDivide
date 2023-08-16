using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
public class vUIIndicatorPosition : MonoBehaviour
{
    public Transform referencePosition;
    public RectTransform container;
    public Canvas canvas;

    protected RectTransform rectTransform;
    protected Camera _camera;
    private void Start()
    {
        _camera = Camera.main;
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (container == null) container = GetComponentInParent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void Update()
    {
        if(canvas && referencePosition)
        {
           
            rectTransform.anchoredPosition = ClampToWindow();
        }
    }

    Vector2 ClampToWindow()
    {
        var dir = referencePosition.position - _camera.transform.position;
        var angle = _camera.transform.forward.AngleFormOtherDirection(dir.normalized);
        float evaluate = Mathf.Clamp(Mathf.Abs(angle.y) - 60, 0, 20) / 20;

        Vector3 position = referencePosition.position;
        Vector3 position2 = position + (Quaternion.AngleAxis(-angle.y, Vector3.up) * dir);
     
        Vector2 pos = canvas.WorldToCanvas(Vector3.Lerp(position,position2,evaluate), _camera);
        //  var pos2 = pos.normalized* rectTransform.rect.height;
        Vector2 clamped = pos.ClampInsideRectagle(container,rectTransform.rect.size);
        return clamped;

    }

  
}
public static class CanvasExtensions
{
    public static Vector2 WorldToCanvas(this Canvas canvas,
                                        Vector3 world_position,
                                        Camera camera = null)
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        var viewport_position = camera.WorldToViewportPoint(world_position);
        var canvas_rect = canvas.GetComponent<RectTransform>();

        return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                           (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
    }

    public static Vector2 ClampInsideRectagle(this Vector2 pos,RectTransform container,Vector2 margin)
    {
       
        Vector2 clamped = pos;
        
        clamped.x = Mathf.Clamp(clamped.x, (-container.rect.width / 2) + margin.x, (container.rect.width / 2) - margin.x);
        clamped.y = Mathf.Clamp(clamped.y, (-container.rect.height / 2 )+ margin.y, (container.rect.height / 2) - margin.y);
        return clamped;
    }
}