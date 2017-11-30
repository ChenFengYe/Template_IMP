using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEngine;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.Util;

using NumericalRecipes;
using MyGeometry;
using UnityEditor;
using UnityExtension;
using System.Drawing;
using System.Xml;

public class GraphicsEngine : MonoBehaviour
{
    public Projector m_projector;
    public RenderEngine m_renderEngine;
    //public Raster m_raster;
    //public MeshMerger m_meshMerger;
    public GameObject m_meshViewer;
    public GameObject m_meshViewer_list_go_template;
    public GameObject meshViewer_list_go = null;
    public Canvas m_canvas;
    public Camera m_meshCaptor;

    [HideInInspector]
    public List<GameObject> m_Inst_list = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> m_meshViewer_list = new List<GameObject>();
    [HideInInspector]
    public string m_curPath = System.Environment.CurrentDirectory;

    //Image<Gray, byte> faceImg = null;
    //Image<Gray, byte> bodyImg = null;

    public Texture2D m_Img;
    public string m_ImgFolder;
    public string m_ImgName;
    public string m_ImgPath;
    public string m_prefab_path;
    public string m_obj_path;

    public bool be_isSymmetry;
    public bool be_isStop;
    public float be_bottomCover;
    public float be_offsetScale;

    private Camera m_mainCamera = null;
    private Rect canvasPlane2D;

    private void Awake()
    {
        m_curPath = m_curPath.Replace(@"\", @"/");
        m_prefab_path = "Assets/Output_prefab";
        //m_obj_path = "ExportedObj";
        m_mainCamera = m_projector.m_mainCamera;
        m_Img = new Texture2D(4, 4);
    }

    private void Start()
    {
        canvasPlane2D = m_canvas.pixelRect;
    }

    private void InitEngine()
    {
        // Recover Stop flag
        be_isStop = false;

        // Clear renderdata
        m_renderEngine.ClearAll();

        // Clear gameobject and 3 lists
        foreach (Transform child in this.gameObject.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var go in m_meshViewer_list)
        {
            Destroy(go);
        }
        m_meshViewer_list.Clear();
    }

    private void Update()
    {
    }

    #region Load - function
    public void LoadMesh(string path)
    {
        if (meshViewer_list_go != null) Destroy(meshViewer_list_go);

        if (m_meshViewer_list_go_template.activeSelf) m_meshViewer_list_go_template.SetActive(false);

        AssetDatabase.Refresh();
        Mesh[] face = Resources.LoadAll<Mesh>("Data_Model/" + m_ImgName);
        meshViewer_list_go = GameObject.Instantiate(m_meshViewer_list_go_template);

        if (!meshViewer_list_go.activeSelf) meshViewer_list_go.SetActive(true);
        meshViewer_list_go.name = m_ImgName;

        for (int i = 0; i < face.Length; i++)
        {
            GameObject meshViewer = meshViewer_list_go.transform.GetChild(i).gameObject;
            meshViewer.name = m_ImgName + "_part" + i;
            meshViewer.GetComponent<MeshFilter>().mesh = (Mesh)face[i];
            Destroy(meshViewer.GetComponent<MeshCollider>());
            meshViewer.AddComponent<MeshCollider>();
        }
    }

    public double[,] LoadDmap(string filename = null)
    {
        string fileName = "D://Desktop//Template_CameraShoot//Assets//Resources//Data_Text//mergeDmap.txt";
        if (!File.Exists(fileName))
        {
            Debug.Log("No such File!");
            return null;
        }
        StreamReader sr = new StreamReader(File.Open(fileName, FileMode.Open));

        int rows = 1536;
        int cols = 2048;
        double[,] mergeDmap = new double[cols, rows];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                string s = sr.ReadLine();
                mergeDmap[col, row] = double.Parse(s);
            }
        }
        sr.Close();
        return mergeDmap;
    }

    public XmlDocument LoadXML(string fileName)
    {
        string prefix = m_curPath + "/Assets/Resources/";
        int i = fileName.IndexOf(prefix);
        fileName = Path.GetDirectoryName(fileName) + "/" + Path.GetFileNameWithoutExtension(fileName);
        fileName = fileName.Remove(i, prefix.Length);
        TextAsset textAsset = (TextAsset)Resources.Load(fileName);
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(textAsset.text);

        XmlNode xn = xmldoc.SelectSingleNode("/document/chunk/sensors/sensor[@id='2']");
        Debug.Log(xn.OuterXml);
        Debug.Log(xn.Attributes["label"].Value);
        return xmldoc;
    }

    public double[] LoadLandMark3d(string fileName = null)
    {
        double[] landmark3d = new double[68 * 3]{
        #region landmark3d Data
            -69.7412,16.8205,21.3072,
            -68.2515,-0.992366,24.2209,
            -65.7911,-17.8707,26.0595,
            -62.1374,-33.5895,29.8059,
            -56.9396,-50.1967,37.9879,
            -46.9439,-62.8066,52.7424,
            -34.8391,-70.5485,71.2516,
            -20.8354,-78.4519,88.654,
            0.190184,-83.3891,95.3481,
            20.8583,-78.3703,88.0234,
            35.0571,-70.801,70.3198,
            47.1744,-63.2156,52.1603,
            56.8906,-50.953,37.5647,
            61.6439,-34.3465,29.5364,
            64.9754,-18.168,26.0944,
            67.3658,-0.185738,24.2431,
            68.8733,18.2493,21.4527,
            -50.307,35.4686,80.7166,
            -43.6215,41.5938,91.7527,
            -33.7779,42.9988,99.4312,
            -25.3868,42.0242,103.443,
            -15.6714,40.4722,105.727,
            15.362,40.5429,105.949,
            24.581,41.3072,103.333,
            33.1858,42.5779,99.7281,
            43.8528,40.8886,91.6942,
            50.0777,36.8156,81.752,
            0.496983,21.9675,110.926,
            0.625325,11.8315,119.267,
            0.521262,1.82726,126.607,
            0.486763,-8.11222,132.57,
            -11.7127,-17.3475,112.766,
            -5.25761,-19.061,116.395,
            0.0649154,-20.3648,119.207,
            5.13567,-19.222,116.205,
            11.6942,-17.472,112.381,
            -40.5441,23.3796,86.7897,
            -33.5332,27.8984,93.0714,
            -24.1803,28.4448,94.8789,
            -16.6321,22.8732,92.5069,
            -26.4149,19.9629,95.1811,
            -34.4764,19.4609,93.1439,
            16.6675,23.8564,93.5287,
            25.0799,29.0289,95.3883,
            33.7226,27.8399,93.0682,
            40.8446,23.6921,86.8988,
            34.7665,19.8292,92.7668,
            26.9058,20.1732,95.1844,
            -24.7542,-38.929,99.4585,
            -16.1544,-34.3191,111.095,
            -6.51574,-31.6794,116.17,
            -0.146926,-32.7062,117.115,
            7.89307,-31.6559,115.612,
            17.6411,-34.3884,109.628,
            24.794,-38.7088,98.7474,
            17.8396,-43.5117,105.172,
            10.2352,-46.4069,111.794,
            -0.233704,-47.7274,114.324,
            -10.6379,-46.3837,112.258,
            -17.9203,-42.5485,105.537,
            -21.907,-38.5886,100.913,
            -8.78926,-38.0787,111.686,
            -0.313254,-38.6072,113.105,
            8.17586,-38.3726,111.835,
            23.008,-38.47,99.4615,
            7.7841,-39.4529,111.129,
            -0.347536,-40.0626,112.512,
            -9.5196,-39.1177,110.311,};
        return landmark3d;
        #endregion
    }

    public double[] LoadLandMark2d(string fileName = null)
    {
        double[] landmark2d = new double[68 * 2]{
            #region landmark2d Data  h-2048 w-1536
            941,429,
            940,461,
            942,493,
            946,523,
            953,553,
            967,580,
            985,604,
            1007,624,
            1033,633,
            1059,629,
            1082,610,
            1102,589,
            1118,566,
            1130,540,
            1140,512,
            1148,483,
            1153,452,
            967,431,
            984,423,
            1004,425,
            1023,431,
            1041,440,
            1077,442,
            1095,436,
            1112,432,
            1130,433,
            1141,444,
            1057,460,
            1056,483,
            1054,506,
            1052,529,
            1027,534,
            1038,539,
            1048,544,
            1059,542,
            1069,539,
            988,456,
            1001,454,
            1014,456,
            1024,463,
            1012,465,
            999,463,
            1085,467,
            1097,461,
            1110,461,
            1121,466,
            1110,471,
            1098,471,
            1002,562,
            1020,562,
            1034,561,
            1043,566,
            1053,564,
            1066,568,
            1078,570,
            1063,584,
            1050,590,
            1040,591,
            1029,588,
            1016,578,
            1009,565,
            1033,571,
            1042,574,
            1053,573,
            1072,572,
            1051,573,
            1041,574,
            1032,571};
        #endregion
        return landmark2d;
    }

    #endregion

    #region Save - function
    public void SaveAllMesh(Image<Rgb, byte> img)
    {
        foreach (var gb in m_meshViewer_list)
        {
            Mesh mesh = gb.GetComponent<MeshFilter>().mesh;
            GenerateMeshUVMaps(gb, mesh, img);

            // Create Texture
            gb.GetComponent<MeshRenderer>().material = gb.GetComponent<MeshRenderer>().material;
            gb.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load(m_ImgPath, typeof(Texture2D)) as Texture2D;
        }
        int i_inst = 0;
        foreach (var inst in m_meshViewer_list)
        {
            i_inst++;
            SavePrefab(inst, i_inst);
        }
        Selection.objects = m_meshViewer_list.ToArray();
        EditorObjExporter.ExportWholeSelectionToSingle();
        //EditorObjExporter.ExportEachSelectionToSingle(m_obj_path);
    }

    public void SaveAllMesh(Texture2D img)
    {
        m_meshViewer_list.Clear();
        foreach (Transform child in meshViewer_list_go.transform)
        {
            m_meshViewer_list.Add(child.gameObject);
        }
        foreach (var gb in m_meshViewer_list)
        {
            Mesh mesh = gb.GetComponent<MeshFilter>().mesh;
            GenerateMeshUVMaps(gb, mesh, img);

            // Create Texture
            gb.GetComponent<MeshRenderer>().material = gb.GetComponent<MeshRenderer>().material;
            m_ImgPath = "Data_Image/Temp/" + m_ImgName;
            gb.GetComponent<MeshRenderer>().material.mainTexture = Resources.Load(m_ImgPath, typeof(Texture2D)) as Texture2D;
        }
        int i_inst = 0;
        foreach (var inst in m_meshViewer_list)
        {
            i_inst++;
            SavePrefab(inst, i_inst);
        }
        Selection.objects = m_meshViewer_list.ToArray();
        EditorObjExporter.ExportWholeSelectionToSingle();
        //EditorObjExporter.ExportEachSelectionToSingle(m_obj_path);
    }

    public void SaveDepthMap(double[,] dmap)
    {
        if (dmap == null) { Debug.Log("No key Points detected!"); return; }
        int frame = Time.frameCount;
        //ScreenCapture.CaptureScreenshot("Model" + frame + ".png");

        string filePath = "D:/Desktop/Template_CameraShoot/Assets/Resources/Data_Text";
        FileStream fs = new FileStream(filePath + "/DepthMap_" + frame + ".txt", FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        int rows = 1546; // y
        int cols = 2048; // x
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                sw.WriteLine(dmap[col, row]);
            }
        }
        sw.Flush();
        sw.Close();
        fs.Close();
    }

    public void SavePrefab(GameObject g, int i_inst)
    {
        // Save Mesh .asset
        Mesh mesh = g.GetComponent<MeshFilter>().mesh;
        string ObjectPath = m_prefab_path + "/" + g.name + ".asset";
        AssetDatabase.CreateAsset(mesh, ObjectPath);
        AssetDatabase.SaveAssets();

        // Save Material .mat
        string MaterialPath = m_prefab_path + "/" + g.name + ".mat";
        AssetDatabase.CreateAsset(g.GetComponent<MeshRenderer>().material, MaterialPath);
        AssetDatabase.SaveAssets();

        // Save Prefab .prefab
        string PrefabPath = m_prefab_path + "/" + g.name + ".prefab";
        Object prefab = PrefabUtility.CreatePrefab(PrefabPath, g);
    }

    public void SaveOBJ(GameObject g, int i_inst)
    {
        Selection.activeGameObject = g;
        EditorObjExporter.ExportEachSelectionToSingle();
    }

    public static void SaveImg(Texture2D img, string imgName)
    {
        byte[] bytes = img.EncodeToPNG();
        // Save In "Assets/Data_Image/Temp/" Path
        string filename = UnityEngine.Application.dataPath + "/Data_Image/Temp/"
                          + imgName;
        System.IO.File.WriteAllBytes(filename, bytes);
    }
    #endregion

    public void UpdataFileName(string FolderPath = "", string ImgName = "")
    {
        m_ImgFolder = FolderPath;
        m_ImgName = ImgName;

        int Str_index = m_ImgFolder.IndexOf("Data_Image");
        int Str_len = m_ImgFolder.Length - m_ImgFolder.IndexOf("Data_Image");
        m_ImgPath = Path.Combine(m_ImgFolder.Substring(Str_index, Str_len), ImgName);
    }

    public void ResetMesh(Image<Rgb, byte> img)
    {

    }

    private bool HitMesh(Ray r, out int hitIndex, out Vector3 hitVertex, out GameObject go)
    {
        hitIndex = -1;
        go = null;
        hitVertex = new Vector3();

        RaycastHit hit;
        if (!Physics.Raycast(r, out hit)) return false;
        go = hit.transform.gameObject;
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null) return false;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
        Transform hitTransform = hit.collider.transform;
        //p0 = hitTransform.TransformPoint(p0);
        //p1 = hitTransform.TransformPoint(p1);
        //p2 = hitTransform.TransformPoint(p2);
        List<Vector3> hitTrangle = new List<Vector3> { p0, p1, p2 };

        double dist = double.MaxValue;
        hitVertex = new Vector3();
        hitIndex = 0;
        for (int i = 0; i < hitTrangle.Count; i++)
        {
            double temp_dist = Vector3.Distance(hitTrangle[i], hit.point);
            if (temp_dist < dist)
            {
                dist = temp_dist;
                hitVertex = hitTrangle[i];
                hitIndex = hit.triangleIndex * 3 + i;
            }
        }

        m_renderEngine.DrawLine(hitTransform.TransformPoint(p0), hitTransform.TransformPoint(p1), UnityEngine.Color.red);
        m_renderEngine.DrawLine(hitTransform.TransformPoint(p1), hitTransform.TransformPoint(p2), UnityEngine.Color.red);
        m_renderEngine.DrawLine(hitTransform.TransformPoint(p2), hitTransform.TransformPoint(p0), UnityEngine.Color.red);
        return true;
    }

    private void GenerateMeshUVMaps(GameObject gb, Mesh mesh, Image<Rgb, byte> img)
    {
        List<Vector3> vertexes = mesh.vertices.ToList();
        for (int i = 0; i < vertexes.Count; i++)
        {
            vertexes[i] = gb.transform.TransformPoint(vertexes[i]);
        }
        Vector2[] uvs = m_projector.WorldToImage(vertexes).ToArray();
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i].x /= img.Width;
            uvs[i].y = (img.Height - uvs[i].y) / img.Height;
        }
        mesh.uv = uvs;
    }

    private void GenerateMeshUVMaps(GameObject gb, Mesh mesh, Texture2D img)
    {
        List<Vector3> vertexes = mesh.vertices.ToList();
        for (int i = 0; i < vertexes.Count; i++)
        {
            vertexes[i] = gb.transform.TransformPoint(vertexes[i]);
        }
        Vector2[] uvs = m_projector.WorldToImage(vertexes).ToArray();
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i].x /= img.width;
            uvs[i].y = (img.height - uvs[i].y) / img.height;
        }
        mesh.uv = uvs;
    }

    public void Invoke_CaptureMeshMask()
    {
        Invoke("CaptureMeshMask", 0.05f);
    }

    public void CaptureMeshMask()
    {
        m_renderEngine.isRenderingGizmos = false;
        int w = m_Img.width;
        int h = m_Img.height;
        Texture2D meshMask = new Texture2D(w, h, TextureFormat.RGB24, false);
        RenderTexture rt = new RenderTexture((int)canvasPlane2D.width, (int)canvasPlane2D.height, 1);

        m_meshCaptor.pixelRect = canvasPlane2D;
        m_meshCaptor.targetTexture = rt;
        m_meshCaptor.Render();
        RenderTexture.active = rt;
        meshMask.ReadPixels(new Rect((canvasPlane2D.width - w) / 2, 0, w, h), 0, 0);
        //meshMask.Apply();

        m_meshCaptor.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(rt);

        GraphicsEngine.SaveImg(meshMask, m_ImgName + ".png");
        m_renderEngine.isRenderingGizmos = true;
    }
}