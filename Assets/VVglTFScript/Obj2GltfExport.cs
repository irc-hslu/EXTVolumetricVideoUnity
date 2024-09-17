using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityGLTF;
using UnityEngine.Video;

public class Obj2GltfExport : MonoBehaviour
{
    List<GameObject> listVV; 
    public string volFolder;
    public string ObjNamePrefix;
    public string ObjNamePostfix;
    private List<Mesh> mesheList;
    private List<Texture> texturesList = null;
    public bool isExportGltf = false;
    public bool isExportTexture = false;
    public string volVideoName;
    int startCount = 0;
    public bool isNodemode = false;
    int countMesh = 0;
    public int i = 1;
    public int iend = 301; 
    float startTime;
    public int exportMeshCount = 25;

    // Start is called before the first frame update
    void Start()
    {
        mesheList = new List<Mesh>();

        startTime = Time.realtimeSinceStartup;
        if (isExportGltf)
        {
            if (isExportTexture) texturesList = new List<Texture>();
            for (; i <= iend; i++)
            {
                string idx = i.ToString("D4");
                string modelName = volFolder + "/"+ ObjNamePrefix + idx+ ObjNamePostfix;
                // Debug.Log(modelName);
                if (Resources.Load(modelName, typeof(GameObject)) != null)
                {
                    GameObject mesh = (GameObject)Resources.Load(modelName, typeof(GameObject));

                    Mesh destMesh = mesh.GetComponentInChildren<MeshFilter>().sharedMesh;
                    if (countMesh == 0)
                    {
                        gameObject.GetComponent<MeshFilter>().sharedMesh = destMesh;
                        gameObject.GetComponent<MeshRenderer>().sharedMaterial = mesh.GetComponentInChildren<MeshRenderer>().sharedMaterial;
                    }
                    mesheList.Add(destMesh);
                    if (isExportTexture) texturesList.Add(mesh.GetComponentInChildren<MeshRenderer>().sharedMaterial.mainTexture);
                    countMesh++;
                    if (countMesh == exportMeshCount)
                        SaveGltf();
                }
                else
                {
                    Debug.Log(modelName + " not found");
                }

            }
            if (countMesh > 0)
                SaveGltf();
        }
        float finalTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Total time used: " + finalTime); 
    }

    void SaveGltf()
    {
        GLTFSceneExporter gltfexporter = null;
        Transform[] tfs = null;
        if (isNodemode)
        {
            tfs = transform.GetComponentsInChildren<Transform>();
            Debug.Log("tfs " + tfs.Length);
            gltfexporter = new GLTFSceneExporter(tfs, new ExportOptions { });
        }
        else
        {
            gltfexporter = new GLTFSceneExporter(this.transform, new ExportOptions { });
        }
        gltfexporter.RegisterVV("h264", Path.GetFileName(volVideoName), "video/mp4", Path.GetFileName(volFolder));
        gltfexporter.SaveGlbVolo(volFolder, volFolder + "_" + startCount, mesheList, texturesList, isNodemode);

        if (isNodemode)
        {
            foreach (GameObject child in listVV)
                Destroy(child);
            foreach (Transform child in tfs)
            {
                if (child != transform)
                    Destroy(child.gameObject);
            }
            Debug.Log("Node count: " + mesheList.Count);
            listVV.Clear();
        }
        else
        {
            Debug.Log("mesheList count: " + mesheList.Count);
        }
        mesheList.Clear();
        if (isExportTexture) { texturesList.Clear(); }
        startCount++;

        countMesh = 0;
    }
    void Update()
    {
    }

}
