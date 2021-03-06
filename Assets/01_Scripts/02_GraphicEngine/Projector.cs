﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGeometry;
using UnityEngine.UI;

public class Projector : MonoBehaviour
{
    public Canvas m_canvas;
    public Camera m_mainCamera;
    public Image m_canvasImg;

    private Rect canvasRect3d;
    private float imagewidth, imageheight;

    public void UpdateProjector()
    {
        float scale = m_canvas.transform.lossyScale.x;
        float height3D = m_canvas.pixelRect.height * scale;
        float width3D = m_canvas.pixelRect.width * scale;

        Vector3 anchor = m_canvas.transform.position - new Vector3(width3D / 2.0f, height3D / 2.0f, 0);
        canvasRect3d = new Rect(anchor, new Vector2(width3D, height3D));

        imagewidth = m_canvasImg.rectTransform.sizeDelta.x;
        imageheight = m_canvasImg.rectTransform.sizeDelta.y;
    }

    public Vector2 WorldToImage(Vector3 worldp, bool clamp = false)
    {
        UpdateProjector();

        // Need canvas size to clamp point position
        Plane canvasplane = new Plane((m_mainCamera.transform.position - m_canvas.transform.position).normalized, m_canvas.transform.position);
        Vector3 imgp = Utility.PlaneRayIntersect(canvasplane,
            new Ray(m_mainCamera.transform.position, (worldp - m_mainCamera.transform.position).normalized));

        //// clamping
        if (clamp)
        {
            imgp.x = Mathf.Max(imgp.x, canvasRect3d.xMin);
            imgp.x = Mathf.Min(imgp.x, canvasRect3d.xMax);
            imgp.y = Mathf.Max(imgp.y, canvasRect3d.yMin);
            imgp.y = Mathf.Min(imgp.y, canvasRect3d.yMax);
        }
        // move axes center to top left
        float scale = m_canvas.transform.lossyScale.x;
        float imagewidth3d = imagewidth * scale;
        float imageheight3d = imageheight * scale;
        imgp.x += 0.5f * imagewidth3d;
        imgp.y -= 0.5f * imageheight3d;
        imgp.y = -imgp.y;
        // change to 2d scale
        imgp.x = imgp.x / scale;
        imgp.y = imgp.y / scale;

        return new Vector2(imgp.x, imgp.y);
    }

    public Vector3 ImageToWorld(Vector2 imgp)
    {
        UpdateProjector();

        Vector3 worldp = new Vector3(imgp.x, imgp.y, m_canvas.transform.position.z);
        
        // change to world scale
        float scale = m_canvas.transform.lossyScale.x;

        worldp.x -= 0.5f * imagewidth;
        worldp.x = -worldp.x;
        worldp.y -= 0.5f * imageheight;
        worldp.y = -worldp.y;

        worldp.x = worldp.x * scale;
        worldp.y = worldp.y * scale;
    
        return worldp;
    }

    public List<Vector3> Proj2dToPlane(Plane plane, List<Vector2> points)
    {
        Vector3 camera = m_mainCamera.transform.position;
        List<Vector3> output = new List<Vector3>();
        foreach (Vector2 p in points)
        {
            Vector3 p3 = this.ImageToWorld(p);
            Ray ray = new Ray(camera, (p3 - camera).normalized);
            output.Add(Utility.PlaneRayIntersect(plane, ray));
        }
        return output;
    }

    public Vector3 Proj2dToPlane(Plane plane, Vector2 p)
    {
        Vector3 camera = m_mainCamera.transform.position;
        Vector3 p3 = this.ImageToWorld(p);
        Ray ray = new Ray(camera, (p3 - camera).normalized);
        return Utility.PlaneRayIntersect(plane, ray);
    }

    public List<Vector2> Proj3dToImage(List<Vector3> points_3d)
    {
        List<Vector2> points_2d = new List<Vector2>();
        for (int i = 0; i < points_3d.Count; i++)
        {
            points_2d.Add(this.WorldToImage(points_3d[i]));
        }
        return points_2d;
    }

    public Ray GenerateRay(Vector2 imgp)
    {
        return new Ray(m_mainCamera.transform.position, (this.ImageToWorld(imgp) - m_mainCamera.transform.position).normalized);
    }

    public List<Vector2> WorldToImage(List<Vector3> points_3d)
    {
        List<Vector2> points_2d = new List<Vector2>();
        for (int i = 0; i < points_3d.Count; i++)
        {
            points_2d.Add(this.WorldToImage(points_3d[i]));
        }
        return points_2d;
    }

    public List<Vector3> ImageToWorld(List<Vector2> points_2d)
    {
        List<Vector3> points_3d = new List<Vector3>();
        for (int i = 0; i < points_2d.Count; i++)
        {
            points_3d.Add(this.ImageToWorld(points_2d[i]));
        }
        return points_3d;
    }

    public Line3 Line2ToLine3(Line2 line)
    {
        Vector3 start3 = this.ImageToWorld(line.start);
        Vector3 end3 = this.ImageToWorld(line.end);
        return new Line3(start3, end3, 1);
    }

    public Quad ProjRectToImage3d(Quad world_rect, bool clamp = false)
    {
        List<Vector3> imgpoints = new List<Vector3>();
        foreach (Vector3 p in world_rect.CornerPoints3d)
            imgpoints.Add(ImageToWorld(WorldToImage(p, clamp)));
        return new Quad(imgpoints);
    }

}
