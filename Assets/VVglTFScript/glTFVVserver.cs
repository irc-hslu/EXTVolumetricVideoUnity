using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
public class glTFVVserver : MonoBehaviour
{
    public string serverURL= "http://127.0.0.1:5000";
    public string videoServerUrl= "http://127.0.0.1:8000/";
    int currentObj = 0;
    public string currentServerStr="";
    UnityWebRequest loadingRequest;
    public bool isHLS=false;
    public bool isnode = false; 
    // Start is called before the first frame update
    void Start()
    { 
        InvokeRepeating("GetNewGltf", 0.02f, 2); 
    }
    void GetNewGltf()
    {
        loadingRequest = UnityWebRequest.Get(serverURL + "/getlist");
        loadingRequest.SendWebRequest();

        while (!loadingRequest.isDone)
        {
            if (loadingRequest.result == UnityWebRequest.Result.ConnectionError || loadingRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning("loadingRequest.isNetworkError " + loadingRequest.result.ToString() + "; " + loadingRequest.uri.ToString());
                break;
            }
        }
        Debug.LogWarning("serverURL: " + serverURL + "; " + loadingRequest.result.ToString() + loadingRequest.downloadHandler.text);
       
        if(currentServerStr != loadingRequest.downloadHandler.text)
        {
            currentServerStr = loadingRequest.downloadHandler.text;  
            string[] gltfUrls  = currentServerStr.Split(';');
            foreach (string gltfurl in gltfUrls)
            {
                if (GameObject.Find(gltfurl) == null)
                {
                    if (gltfurl.Contains(".glvv"))
                    {
                       
                        loadingRequest = UnityWebRequest.Get(serverURL + "/gltf/" + gltfurl);
                        loadingRequest.SendWebRequest(); 
                        while (!loadingRequest.isDone)
                        {
                            if (loadingRequest.result == UnityWebRequest.Result.ConnectionError || loadingRequest.result == UnityWebRequest.Result.ProtocolError)
                            {
                                Debug.LogWarning("loadingRequest.isNetworkError " + loadingRequest.result.ToString() + "; " + loadingRequest.uri.ToString());
                                break;
                            }
                        }
                        Debug.Log("VV streaming" + loadingRequest.downloadHandler.text);
                        string[] wholeText = loadingRequest.downloadHandler.text.Split('\n'); 
                        string[] list;
                        if (wholeText.Length >= 2)
                        {
                            list = wholeText[1].Split(',');
                            string[] RotScaleStr = wholeText[0].Split(',');   
                            StartCoroutine(save(list[0], true, gltfurl, list, true, float.Parse(RotScaleStr[0]), float.Parse(RotScaleStr[1]), float.Parse(RotScaleStr[2]), 
                                float.Parse(RotScaleStr[3]), float.Parse(RotScaleStr[4]), float.Parse(RotScaleStr[5]), float.Parse(RotScaleStr[6])));  
                        } 
                        else
                        {
                            list = loadingRequest.downloadHandler.text.Split(',');
                            StartCoroutine(save(list[0], true, gltfurl, list));   
                        } 
                    }
                    else
                    {
                        StartCoroutine(save(gltfurl, false, gltfurl));
                    }
                    
                }
            } 
        } 
    }
  IEnumerator vvDownloadTimer(string[] vvf, string gltfurl, int currentPoint, float timer)
    {
        yield return new WaitForSeconds(timer);
        Debug.Log(timer + " New VV " + vvf);
        StartCoroutine(save(vvf[currentPoint], true, gltfurl));
        currentPoint++;
        if (currentPoint< vvf.Length)
        {

            StartCoroutine(vvDownloadTimer(vvf, gltfurl, currentPoint, 3));
        } 
    } 
    IEnumerator save( string gltfurl, bool isSequence=false, string vvOrigin=null, string[] list=null, bool isMove=false,
        float posX = 0, float posY = 0, float posZ = 0, float rotX=0, float rotY=0, float rotZ=0,  float scaleV=0)
    {
        var uwr = new UnityWebRequest(serverURL + "/gltf/" + gltfurl);
        uwr.method = UnityWebRequest.kHttpVerbGET;
      
        var resultFile = Path.Combine(Application.persistentDataPath, gltfurl);
        DownloadHandlerFile dh = new DownloadHandlerFile(resultFile);

        dh.removeFileOnAbort = true;
        uwr.downloadHandler = dh;
       
        yield return uwr.SendWebRequest();
        if (uwr.result != UnityWebRequest.Result.Success)
            Debug.Log(uwr.error);
        else
        { 
            Debug.Log("Download saved to: " + resultFile);
            GameObject newRTgltf=null; 
            newRTgltf = new GameObject();
            Vector3 newPos = newRTgltf.transform.position;
            newPos.x = newPos.x + currentObj+posX;
            newPos.y = newPos.y + posY;
            newPos.z = newPos.z + posZ;
            if (isMove)
            {   newRTgltf.transform.Rotate(rotX, rotY, rotZ);
                newRTgltf.transform.localScale = new Vector3(scaleV, scaleV, scaleV);
            }
            currentObj++;
            newRTgltf.transform.position = newPos;
            VVgltfPlayer vVgltf = newRTgltf.AddComponent<VVgltfPlayer>();
            if (isSequence)
            { 
                newRTgltf.name = vvOrigin;   
                vVgltf.gltflist = list;
                vVgltf.serverURL = serverURL;
                //vVgltf.isLooping = false;
                vVgltf.streamsInMemory = new MemoryStream[list.Length];
                vVgltf.uwrs = new UnityWebRequest[list.Length];
                if (list.Length > 1)
                    StartCoroutine( vVgltf.save(list[1], 1)); 
 
            }
            else
            { 
                newRTgltf.name = gltfurl; 
            }
            vVgltf.isLooping = true;
            vVgltf.volFolderPathType = VolEnums.PathType.Persistent;
            vVgltf.gltfFilePath = gltfurl; 
            vVgltf.isRuntimeLoadMesh = true; 

            if (vvOrigin.Contains("_node"))
            {
                vVgltf.isnode = true; 
            }
           
            if (isHLS)
                vVgltf.videoFilePath = videoServerUrl + Path.GetFileNameWithoutExtension(newRTgltf.name) + ".m3u8";
            else
                vVgltf.videoFilePath = serverURL + "/gltf/" + Path.GetFileNameWithoutExtension(newRTgltf.name) + ".mp4";
          
            vVgltf.isSequence = isSequence;
            vVgltf.volVideoTexturePathType = VolEnums.PathType.Absolute;

        } 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnApplicationQuit()
    {
        loadingRequest.Dispose();
    }
    IEnumerator GetText()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serverURL + "/getlist"))
        {
            yield return www.SendWebRequest();

            if (www.result== UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                byte[] results = www.downloadHandler.data;
                using (var stream = new MemoryStream(results))
                using (var binaryStream = new BinaryReader(stream))
                {
                    GetNewGltf( );
                }
            }
        }
    }
    IEnumerator MultiVVpartDownload(string gltfurl)
    {
        UnityWebRequest www = UnityWebRequest.Get(serverURL + "/gltf/" + gltfurl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        { 
            Debug.Log(www.downloadHandler.text);
            string[] list = www.downloadHandler.text.Split(',');
            for (int i = 0; i < list.Length; i++)
                StartCoroutine(save(list[i], true)); 
        }
    }
}
