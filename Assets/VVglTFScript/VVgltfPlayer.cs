using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video; 
using UnityGLTF; 
using System.Runtime.ExceptionServices; 
using System;  
using UnityEngine.Networking;

public class VVgltfPlayer : MonoBehaviour
{ 
    public VolEnums.PathType volFolderPathType;
    public string gltfFilePath; 
    public VolEnums.PathType volVideoTexturePathType;
    public string videoFilePath;
    public bool isLooping = true; 
    private VideoPlayer videoPlayer;
     MeshFilter _meshFilter = null;
     MeshRenderer _meshRenderer = null;
    public bool audioOn = true;
    public bool IsOpen { get; private set; }
    public bool playOnStart = true;
    public bool isRuntimeLoadMesh = true;
     GameObject outGltf;
    string fullpathgltf;
    public Material gltfMaterial; 
    public Action<GameObject, ExceptionDispatchInfo> initVvAction = delegate { };
    public Action<GameObject, ExceptionDispatchInfo> nextVvAction = delegate { };
    public Action<int> MeshloadedAction = delegate { };
    public Action<int> MeshLoadedVideoPlayAction = delegate { };
    public GLTFSceneImporter gltfImporter; 
      int currentFrame = 0;
      int lastFrame = 0;
    public int currentVVid = 0;
    public bool isSequence = false; 
    public string[] gltflist;
    bool isfileLoaded = true;
    public bool isnode = false; 
    public MemoryStream[] streamsInMemory = null;
    public UnityWebRequest[] uwrs;
    public GameObject nextActive = null;
    bool isMeshAccessListAvaialble = false;
    List<int> meshIDList = null;
    int preloadFrameCount = 5;

    void Start()
    {
        if (gltfFilePath != "" || gltflist.Length >= 1)
        {
            initVvAction += setLOADED;
            nextVvAction += setNextLOADED;
            MeshLoadedVideoPlayAction += SetMeshLoadedVideoPlay;
            MeshloadedAction += SetPreMeshLoaded;
            SetNewGltfImporter();
        }
        else if (!isnode)
        {
            SetVideoPlayer();
        }
    }
    public void SetVVNextGltf()
    { 
        isfileLoaded = false;
        currentVVid = (currentVVid + 1) % gltflist.Length; 
        videoPlayer.Pause(); 
        MeshCountLimit += gltfImporter.GetMeshCount();
        if (currentVVid == 0)
        {
            MeshCountLimit = 0;
        }
        fullpathgltf = volFolderPathType.ResolvePath(gltflist[currentVVid]);
        gltfImporter.Dispose();
        gltfImporter = null;
        gltfImporter = new GLTFSceneImporter(fullpathgltf, nextVvAction);
    }
    void OffAllChild(GameObject target)
    {
        if (target.transform.childCount > 1)
        {
            for (int i = 0; i < target.transform.childCount; i++)
                target.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    void setNextLOADED(GameObject loadedObject, ExceptionDispatchInfo ex)
    {
        Destroy(outGltf);
        outGltf = loadedObject;
        if (isnode)
        {
            OffAllChild(loadedObject);
        }
        else SetRenderer(loadedObject);
        videoPlayer.Play();
        isfileLoaded = true;   
    } 
    public void SetNewGltfImporter()
    { 
        fullpathgltf = volFolderPathType.ResolvePath(gltfFilePath);
        int loadnodecount = 1;
        if (isnode) loadnodecount = 0;
        if (streamsInMemory != null)
            gltfImporter = new GLTFSceneImporter(fullpathgltf, initVvAction, loadnodecount, streamsInMemory[0]);
        else
            gltfImporter = new GLTFSceneImporter(fullpathgltf, initVvAction, loadnodecount);
    }
    public void SetRenderer(GameObject target)
    {
        if (target.GetComponentInChildren<MeshFilter>() != null)
        {
            _meshFilter = target.GetComponentInChildren<MeshFilter>();
            _meshRenderer = target.GetComponentInChildren<MeshRenderer>();
            target.transform.position = gameObject.transform.position;
        }
        else
        {
            if (gameObject.GetComponent<MeshFilter>() == null)
            {
                _meshFilter = gameObject.AddComponent<MeshFilter>();
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }
        if (_meshRenderer != null)
        {
            _meshRenderer.material.shader = Shader.Find("Unlit/Texture");
            _meshRenderer.material = gltfMaterial;
        }

        else
            Debug.LogError("Null_meshRenderer");
        target.transform.parent = gameObject.transform;
        target.transform.localPosition = Vector3.zero;
        target.transform.rotation = new Quaternion();
        target.transform.localScale = new Vector3(1, 1, 1);
        videoPlayer.targetMaterialRenderer = _meshRenderer;
    }

    public void SetMesh(int meshID, Mesh resultMesh)
    { 
        _meshFilter.mesh = resultMesh; 
        videoPlayer.Play();
    }
    public void SetPreMeshLoaded(int meshID)
    {
        Debug.Log("SetPreMeshLoaded" + meshID);
        if (meshID < gltfImporter.GetMeshCount())
            StartCoroutine(gltfImporter.LoadMesh(meshID + 1, SetPreMeshLoaded));
        else
            StartCoroutine(gltfImporter.LoadMesh(meshID + 1, MeshLoadedVideoPlayAction));
    }
    public void SetMeshLoadedVideoPlay(int meshID)
    {
        Debug.Log("MeshLoadedVideoPlayAction videoPlay "+ meshID);
        videoPlayer.Play();
    }
    public void SetVideoPlayer()
    {
        AudioSource asource = gameObject.AddComponent<AudioSource>();
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = volVideoTexturePathType.ResolvePath(videoFilePath); ;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = isLooping;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, asource);
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.frameReady -= AudioVideoPlayerOnFrameReady;
        videoPlayer.frameReady += AudioVideoPlayerOnFrameReady;
        videoPlayer.loopPointReached -= AudioVideoPlayerOnLoopPointReached;
        videoPlayer.loopPointReached += AudioVideoPlayerOnLoopPointReached;
        videoPlayer.errorReceived -= AudioVideoPlayerOnErrorReceived;
        videoPlayer.errorReceived += AudioVideoPlayerOnErrorReceived;
        videoPlayer.prepareCompleted -= AudioVideoPlayerOnPrepareCompleted;
        videoPlayer.prepareCompleted += AudioVideoPlayerOnPrepareCompleted;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioMute(0, false);
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.playOnAwake = playOnStart;
        videoPlayer.Prepare();
        videoPlayer.Pause();
        Debug.Log("VideoPlayer configured correctly");
    }
    void setLOADED(GameObject loadedObject, ExceptionDispatchInfo ex)
    {
        outGltf = loadedObject;
        if (outGltf.transform.childCount >= 3) isnode = true;
        Debug.Log("scene = loader.LastLoadedScene;" + loadedObject.name);
        loadedObject.transform.parent = gameObject.transform;
        SetVideoPlayer();
        if (isnode)
        {
            OffAllChild(loadedObject);
        }
        else
        { 
            SetRenderer(loadedObject); 
            meshIDList = gltfImporter.GetEXT_VolumetricVideoMeshList();
            if (meshIDList != null && meshIDList.Count > 1)
            {
                Debug.Log("meshIDList != null " + meshIDList.ToString());
                isMeshAccessListAvaialble = true; 
            } 
        }
        if (videoFilePath == "")
        {
            Debug.Log("video file path" + videoFilePath);//     videoFilePath = gltfImporter.getMpegVideoURL();
        }
        if (nextActive != null) nextActive.SetActive(true);
       
    }

    int MeshCountLimit = 0;  
    void Update()
    {   
    }
    public void AudioVideoPlayerOnFrameReady(VideoPlayer vp, long frameidx)
    {
        if (isfileLoaded)
        {
            int meshID =   (int)(frameidx) - MeshCountLimit;
            if (isMeshAccessListAvaialble)
            {
                meshID = meshIDList[meshID];
            }
            if ((meshID >= (gltfImporter.GetMeshCount()) || frameidx >= (long)(vp.frameCount - 1)) && isSequence)
            {
                Debug.Log("meshID >= gltfImporter.GetMeshCount() " + meshID + " ,  " + gltfImporter.GetMeshCount());
                SetVVNextGltf();
            }
            else
            {
                if (isnode)
                {
                    if (outGltf.transform.childCount >= meshID)
                    {
                        outGltf.transform.GetChild(lastFrame).gameObject.SetActive(false);
                        outGltf.transform.GetChild(meshID).gameObject.SetActive(true);
                    }
                    lastFrame = meshID;
                }
                else
                {
                    //if (meshID < gltfImporter.GetMeshCount() && gltfImporter._assetCache.MeshCache[meshID] == null)
                    //{
                    //   gltfImporter.LoadMesh(meshID); 
                    //} 
                    //_meshFilter.mesh = gltfImporter._assetCache.MeshCache[meshID].LoadedMesh;
                    StartCoroutine(gltfImporter.LoadMesh(meshID, returnValue =>
                    {
                        _meshFilter.mesh = returnValue;
                    }
                    ));

                }
            }

        }
    }
    private void AudioVideoPlayerOnErrorReceived(VideoPlayer source, string message)
    {
        Debug.LogError(message);
    }

    private void AudioVideoPlayerOnPrepareCompleted(VideoPlayer source)
    {
 
        Debug.Log("AudioVideoPlayerOnPrepareCompleted");
        int advancedID = 0; 
        if (isMeshAccessListAvaialble)
        { 
            advancedID = meshIDList[0];
        }
        StartCoroutine(gltfImporter.LoadMesh(advancedID, SetPreMeshLoaded));
       
        source.Play();
        if (_meshRenderer != null)
        {
#if UNITY_EDITOR
            _meshRenderer.sharedMaterial.mainTexture = source.texture;
#else
        _meshRenderer.material.mainTexture = source.texture;
#endif
        } 
    }

    private void AudioVideoPlayerOnLoopPointReached(VideoPlayer source)
    {
        Debug.Log("AudioVideoPlayerOnLoopPointReached");
        Restart();
    } 
    public bool Restart()
    {
        if (!IsOpen)
            return false;

        IsOpen = true;
        return true;
    }
    public string serverURL;
    public IEnumerator save(string gltfurl, int downloadid)
    {
        var uwr = new UnityWebRequest(serverURL + "/gltf/" + gltfurl);
        uwr.method = UnityWebRequest.kHttpVerbGET;
        Debug.Log(gltfurl);
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
            if (isSequence)
            {
                downloadid++;
                if (downloadid < gltflist.Length)
                    StartCoroutine(save(gltflist[downloadid], downloadid));
            }
        }
    }
}
