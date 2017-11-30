using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.Util;

using UnityExtension;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using MyGeometry;
using System.Drawing;

public class CanvasManager : MonoBehaviour
{
    // Canvas params
    public Canvas m_canvas;
    public RenderEngine m_renderEngine;
    Vector3 anchor;
    float scale;                            // Scale Canvas2D to RealWorld3D 
    Rect canvasPlane2D;                     // Canvas Rect 2D, Canvas Space
    Rect canvasPlane3D;                     // Canvas Rect 3D, World Space

    // UI Button
    public UnityEngine.UI.Button button;
    public UnityEngine.UI.Button button2;
    public UnityEngine.UI.Button button3;
    public UnityEngine.UI.Button button4;
    public UnityEngine.UI.Button button5;
    public UnityEngine.UI.Button button6;
    public UnityEngine.UI.Button button7;
    public UnityEngine.UI.Button button8;
    public UnityEngine.UI.Button button9;
    public UnityEngine.UI.Button button10;

    public UnityEngine.UI.Image backimg;
    public UnityEngine.UI.Image maskTop;
    public UnityEngine.UI.Image maskBody;
    public UnityEngine.UI.Image realPhoto;

    // Engine
    public GraphicsEngine m_engine;
    Image<Rgb, byte> m_MaskRawImg = null;
    bool IsTopGeometryLoaded = false;
    //string m_ImgPath = null;

    void Awake()
    {
        // Canvas
        scale = m_canvas.transform.lossyScale.x;
        float height3D = m_canvas.pixelRect.height * scale;
        float width3D = m_canvas.pixelRect.width * scale;

        anchor = m_canvas.transform.position - new Vector3(width3D / 2, height3D / 2, 0);
        canvasPlane2D = m_canvas.pixelRect;
        canvasPlane3D = new Rect(anchor, new Vector2(width3D, height3D));
        backimg.rectTransform.sizeDelta = canvasPlane2D.size;

        // Add Event to Button
        button.onClick.AddListener(LoadImageAndCircle);
        button2.onClick.AddListener(ReStartSweep);
        button3.onClick.AddListener(StopSweep);
        button4.onClick.AddListener(RunRBF);
        button5.onClick.AddListener(ResetFace);
        button6.onClick.AddListener(RunIDW);
        button7.onClick.AddListener(RunMyIDW);
        button8.onClick.AddListener(LoadXML);
        button9.onClick.AddListener(SolvePNP);
        button10.onClick.AddListener(MergeMesh);
    }

    void Start()
    {
    }

    void Update()
    {
    }

    void LoadImageAndCircle()
    {
        IsTopGeometryLoaded = false;
        string realPhoto_path = EditorUtility.OpenFilePanel("Load Image", "", "jpg");
        if (realPhoto_path.Length != 0)
        {

            //--------------------------------------------------------------------
            // Data Prepare -- Background Image
            Texture2D CanvasMaskRawImg = new Texture2D(4, 4);
            if (File.Exists(realPhoto_path))
            {
                m_MaskRawImg = new Image<Rgb, byte>(realPhoto_path);
                CanvasMaskRawImg = ResizeImgWithHeight(m_MaskRawImg, canvasPlane2D.height);
                m_MaskRawImg = m_MaskRawImg.Resize(CanvasMaskRawImg.width, CanvasMaskRawImg.height, Inter.Linear);

                // Background Image
                backimg.sprite = Sprite.Create(CanvasMaskRawImg, new Rect(0, 0, CanvasMaskRawImg.width, CanvasMaskRawImg.height), Vector2.zero);
                backimg.rectTransform.sizeDelta = new Vector2(CanvasMaskRawImg.width, CanvasMaskRawImg.height);
                IExtension.SetTransparency(backimg, 0.3f);

                //m_engine.m_faceEngine.img = m_MaskRawImg.Convert<Gray, byte>();
                //m_engine.m_faceEngine.imgPath = maskRawImg_path;

                IsTopGeometryLoaded = false;

            }

            // Updata Path
            string folderPath = Path.GetDirectoryName(realPhoto_path);
            string fileName = Path.GetFileName(realPhoto_path);
                m_engine.m_Img = CanvasMaskRawImg;

            // Load Mesh  Text
            m_engine.LoadMesh(realPhoto_path.Substring(0, realPhoto_path.Length - 4) + ".obj");
            // Save Img
            GraphicsEngine.SaveImg(CanvasMaskRawImg, fileName);
            m_engine.Invoke_CaptureMeshMask();

        }
    }

    void LoadXML()
    {
        string xml_path = EditorUtility.OpenFilePanel("Load XML", m_engine.m_curPath + ".\\Assets\\Resources\\Data_XML", "xml");
        if (xml_path.Length != 0)
        {
            m_engine.LoadXML(xml_path);
        }

    }

    void SolvePNP()
    {
        //m_engine.m_raster.CaptureDepthMap();
    }

    void MergeMesh()
    {
        //m_engine.m_meshMerger.BuildMesh();
    }

    Texture2D ResizeImgWithHeight(Image<Rgb, byte> img, float height)
    {
        img = img.Resize((int)((float)img.Width / (float)img.Height * height),
                (int)height,
                Inter.Nearest);

        return IExtension.ImageToTexture2D(img);
    }

    void ControlMask(bool toggleFlag)
    {
        if (toggleFlag)
            this.backimg.gameObject.SetActive(true);
        else
            this.backimg.gameObject.SetActive(false);
    }

    void ReStartSweep()
    {
        RunMyIDW();

        if (m_MaskRawImg == null) Debug.Log("No Image is loaded!");
        ChangeBackgroundImg(m_MaskRawImg);

        m_engine.SaveAllMesh(backimg.sprite.texture);
        ControlGizmos(false);
        ControlMask(false);
    }

    void ControlMesh(bool isRenderingMesh)
    {
        m_renderEngine.isRenderingMesh = isRenderingMesh;
        m_engine.meshViewer_list_go.SetActive(isRenderingMesh);
    }

    void ControlGizmos(bool isRenderingAll)
    {
        m_renderEngine.isRenderingGizmos = isRenderingAll;
    }

    void ControlSymmetry(bool isSymmetry)
    {
        m_engine.be_isSymmetry = isSymmetry;
    }

    void ControlTexture(bool isTexture)
    {
        string texture_path = m_engine.m_prefab_path + "/" + m_engine.m_ImgName + ".mat";
        string notexture_path = "Assets/Resources/OBJViewer_empty.mat";
        foreach (var gb in m_engine.m_meshViewer_list)
        {
            gb.GetComponent<MeshRenderer>().material = isTexture ?
                (Material)AssetDatabase.LoadAssetAtPath(texture_path, typeof(Material)) :
                (Material)AssetDatabase.LoadAssetAtPath(notexture_path, typeof(Material));
        }
    }

    void StopSweep()
    {
        m_engine.be_isStop = true;
    }

    void RunRBF()
    {
    }
    void RunIDW()
    {
    }
    void RunMyIDW()
    {
    }
    void ResetFace()
    {
        m_engine.ResetMesh(m_MaskRawImg);
    }

    string FixFilePath(string photo, string keyword_add, string extens)
    {
        return Path.GetDirectoryName(photo) + "\\" + Path.GetFileNameWithoutExtension(photo) + keyword_add + "." + extens;
    }

    void ChangeBackgroundImg(Image<Rgb, byte> img)
    {
        // Background Image
        Texture2D CanvasTexture = IExtension.ImageToTexture2D(img);
        backimg.sprite = Sprite.Create(CanvasTexture, new Rect(0, 0, CanvasTexture.width, CanvasTexture.height), Vector2.zero);
        backimg.rectTransform.sizeDelta = new Vector2(CanvasTexture.width, CanvasTexture.height);
        IExtension.SetTransparency(backimg, 0.3f);
    }
}