using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Crosstales.FB;
using packing3D;
using QuantumConcepts.Formats.StereoLithography;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{

    /*-------------------------
    * 3DPacking data
    --------------------------*/
    private string[] EXTENSION = new []{ "stl"};
    private static bool _isPacking;
    private static bool _lock_UI;
    public static bool _abort3DPacking;
    private List<Thread> _packing3DThreads; 
    private Dictionary<string,GameObject> _go_toPack;
    

    /*-------------------------
    * 3DPacking Options
    -------------------------*/

    [Header("UI Settings packing")]
    public Camera cam_meshses;
    public Toggle cb_MinimiseHeight;
    public Toggle cb_MinimiseVolume;
    public Toggle cb_realtimeDisplay;
    public Slider s_accuracy;
    public InputField if_minOffset;
    public InputField if_volume_x;
    public InputField if_volume_y;
    public Button b_run;
    public Button b_cancel;
    public Button b_add_file;
    public Button b_add_folder;

    void Start()
    {

        _isPacking = false;
        updateUI();

        cb_MinimiseHeight.onValueChanged.AddListener( isOn=>{
            if(_lock_UI){
                return;
            }
            _lock_UI = true;
            cb_MinimiseVolume.isOn = false;
            updateUI();
            _lock_UI = false;
        });

        cb_MinimiseVolume.onValueChanged.AddListener( isOn=>{
            if(_lock_UI){
                return;
            }
            _lock_UI = true;
            cb_MinimiseHeight.isOn = false;
            updateUI();
            _lock_UI = false;

        });

        cb_realtimeDisplay.onValueChanged.AddListener( isOn=>{
            if(isOn){
                Debug.Log("Real time display option will decrease the speed of the process");
            }
        });

        s_accuracy.onValueChanged.AddListener( value=>{
            updateUI();
        });

        if_minOffset.onValueChanged.AddListener(value=>{
            updateUI();
        });

        if_volume_x.onValueChanged.AddListener(value=>{
            updateUI();
        });

        if_volume_y.onValueChanged.AddListener(value=>{
            updateUI();
        });

        b_cancel.onClick.AddListener(()=>{ 
            _abort3DPacking = true;
        });

        b_run.onClick.AddListener(()=>{  
            _abort3DPacking = false; 
            run3DPacking();
        });

        b_add_file.onClick.AddListener(()=>{
            addAFile( FileBrowser.OpenFiles("Open File", "~", EXTENSION) );
            Util.Recenter(_go_toPack, ref cam_meshses);
        });

        b_add_folder.onClick.AddListener(()=>{
            AddAFolder();
            Util.Recenter(_go_toPack, ref cam_meshses);

        });

    }

    private void addAFile(string[] path){

        if(path == null){
            return;
        }

        for(int i = 0;i<path.Length;i++){

            STLDocument file = STLDocument.Open(path[i]);
            List<Facet> faces = file.Facets.ToList();

            Dictionary<Vector3, int> dico_facets = new Dictionary<Vector3, int>();
            HashSet<Vector3> hash_vertices       = new HashSet<Vector3>();
            List<Vector3> vertices               = new List<Vector3>();
            List<Vector3> v3_facettes            = new List<Vector3>();
            List<int> triangles                  = new List<int>();

            foreach(Facet f in faces){
                Vector3 v1 =  new Vector3(f.Vertices[0].X, f.Vertices[0].Y, f.Vertices[0].Z );
                Vector3 v2 =  new Vector3(f.Vertices[1].X, f.Vertices[1].Y, f.Vertices[1].Z );
                Vector3 v3 =  new Vector3(f.Vertices[2].X, f.Vertices[2].Y, f.Vertices[2].Z );
                hash_vertices.Add( v1 );
                hash_vertices.Add( v2 );
                hash_vertices.Add( v3 );
                v3_facettes.Add( v1 );
                v3_facettes.Add( v2 );
                v3_facettes.Add( v3 );
            }

            Vector3[] arr_vertices = hash_vertices.ToArray();

            for(int j = 0;j<arr_vertices.Length;j++){
                dico_facets[arr_vertices[j]] = j;
                vertices.Add(arr_vertices[j]);
            }

            foreach(Vector3 v in v3_facettes){
                triangles.Add(dico_facets[v]);
            }

            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            GameObject go = new GameObject(Guid.NewGuid().ToString());
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            Util.Recenter(ref mesh);
            mf.mesh = mesh;
            mf.mesh.RecalculateNormals();
            mr.material = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

            if(_go_toPack == null){
                _go_toPack = new Dictionary<string, GameObject>();
            }
            
            _go_toPack[go.name] = go;

        }
    }

    private void AddAFolder(){

        string path = FileBrowser.OpenSingleFolder("Open File", "~");

        if(string.IsNullOrEmpty(path)){
            return;
        }

        string[] path_files = Directory.GetFiles(path);

        List<string> list_files = new List<string>();
        foreach(string s in path_files){
            
            if(Path.GetExtension(s).ToLower() == ".stl"){
                list_files.Add(s);
            }
        }

        addAFile( list_files.ToArray() );

    }


    private void updateUI(){


        bool isMinimiseHeight  = cb_MinimiseHeight.isOn;
        bool isMinimiseVolume  =  cb_MinimiseVolume.isOn;
        bool isRealTimeDisplay = cb_realtimeDisplay.isOn;
        bool isMinOffset       = !string.IsNullOrEmpty(if_minOffset.text);
        bool isVolumeWidth     = !string.IsNullOrEmpty(if_volume_x.text);
        bool isVolumeHeight    = !string.IsNullOrEmpty(if_volume_y.text);


        if(!isMinimiseHeight && !isMinimiseVolume ){
            cb_MinimiseVolume.isOn = true;
            isMinimiseVolume = true;
        }

    

    
        if(_isPacking){

            b_run.gameObject.SetActive(false);
            b_cancel.gameObject.SetActive(true);

        }else{

            b_run.gameObject.SetActive(true);
            b_cancel.gameObject.SetActive(false);

            if(isMinOffset){
                if(isMinimiseHeight && isVolumeWidth && isVolumeHeight){
                    b_run.interactable = true;
                }
                else if(isMinimiseVolume){

                    b_run.interactable = true;

                }else{
                    b_run.interactable = false;
                }
            }
        }
    }

    
    private void run3DPacking(){

        Debug.Log("run 3D packing : "+_go_toPack.Count+" shapes");
    

        //Recupere vertices/triangle/name
        List<System.Tuple<Vector3[], int[], string>> meshes = new List<System.Tuple<Vector3[], int[], string>>();
    
        foreach(KeyValuePair<string,GameObject> entry in _go_toPack){
            
            MeshFilter mf = entry.Value.GetComponent<MeshFilter>();

            Vector3[] v = mf.mesh.vertices;
            int[] t     = mf.mesh.triangles;
            string s    = entry.Key;

            Vector3 translation = entry.Value.transform.position;
            Quaternion rotation = entry.Value.transform.rotation;

            for(int i = 0;i<v.Length;i++){
                    v[i] = rotation * v[i];
                    v[i] += entry.Value.transform.position;
            }; 
            
            System.Tuple<Vector3[], int[], string> tmpMesh = new System.Tuple<Vector3[], int[], string>(v, t, s);

            meshes.Add(tmpMesh);
        }

        

        if(meshes.Count == 0){
            Debug.LogError("There is no shape selected");
            return;
        }

        //Prepare thread
        packing3D.MainPack3D p3d = new packing3D.MainPack3D();

        int nbShapeReady  =0;
        p3d.onNewShapeAddedIntoModel( (name, success)=>{
            UIExecution.Instance().Execute(()=>{
                 if(!success){
                    Debug.LogError("Object : "+Path.GetFileNameWithoutExtension(name)+" is too large for the printer volume, please try to decrease the offset otherwise it will be ignored for packing");
                 }else{
                     nbShapeReady ++;
                     Debug.Log("Prepare shapes : "+nbShapeReady+"/"+_go_toPack.Count);
                 }
            });
        });

        p3d.onPackingBegin( (nbPacking, success ) =>{
            UIExecution.Instance().Execute(()=>{

                 if(success){
                    Debug.Log("packing begin ...");

                 }else{
                    Debug.LogError("An error has occurred during the 3D packing process");
                 }
            });
        });

        p3d.onAnnealProgress( (model, pct, bestEnergy, energy ) =>{
            UIExecution.Instance().Execute(()=>{
                if(cb_realtimeDisplay.isOn && model != null && energy < p3d.bestScore){
                    model.showAsGo(ref _go_toPack);
                }
            });
        });

        p3d.onAnnealDone(()=>{
            UIExecution.Instance().Execute(()=>{
                Debug.Log("Anneal done !");                
            });
        });

        p3d.onNewSolutionFound( (model, energy) =>{
            UIExecution.Instance().Execute(()=>{
                model.showAsGo(ref _go_toPack);
            });
        });


        p3d.onPackingDone( () =>{
            UIExecution.Instance().Execute(()=>{
               
                Debug.Log("Packing done !");      
                _isPacking = false;
                updateUI();         

            });
        });



        //Si reste vide alors on minimize seulement le volume sans tenir compte du volume du printer
        if(cb_MinimiseHeight.isOn){

            if( !Int32.TryParse(if_volume_x.text, out int printerWidth) || !Int32.TryParse(if_volume_y.text, out int printerDepth)  ){
                Debug.LogError("Wrong volume size");
                return;
            }

            p3d._printerWidth  = printerWidth;
            p3d._printerHeight = float.MaxValue;
            p3d._printerDepth  = printerDepth;
            p3d._printerCorner = new Vector3(-printerWidth/2, 0 , -printerDepth/2);
        }

        //Met a jour la précision
        p3d.bvhDetail *= (int)s_accuracy.value;
        p3d.annealingIterations *= (int)s_accuracy.value;
        p3d.packingIteration *= (int)s_accuracy.value > 2 ? 2 : 1;


        //Met à jour le offset minimal demandé
        string offsetStringValue =  !string.IsNullOrEmpty(if_minOffset.text) ? if_minOffset.text : if_minOffset.placeholder.GetComponent<Text>().text;
        
        p3d.minimalOffset = float.Parse( offsetStringValue , CultureInfo.InvariantCulture.NumberFormat);


        p3d.meshes = meshes;
        p3d.cpu = SystemInfo.processorCount;

        //run
        _isPacking = true;
        updateUI();
        Thread thread = new Thread(p3d.runMultiThread);
        thread.Priority = System.Threading.ThreadPriority.Highest;
        thread.IsBackground = true;
        thread.Start();    
    }
  
}
