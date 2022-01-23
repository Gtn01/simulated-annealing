using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace packing3D{

    public class MainPack3D 
    {
        /*------------------------------------------
         * Fonctions de callback pour ressortir 
         * des données du thread pendand l'execution
         ------------------------------------------*/

        private Action<string, bool> callback_newShapeAddToModel; //Une nouvelle forme à été ajoutée dans le model prend en paramettre le nom du mesh
        private Action<int, bool> callback_packingBegin; //L'algo de packing commence, il prend en paremettre le nom d'anneal qu'il va faire
       
        private Action<Model, int, float, float> callback_annealProgress; //Un anneal complet est terminé, il prend en paremettre un % de progression et un score & score actuel
        private Action callback_annealDone; //Un anneal complet est terminé
              
        private Action<Model, float> callback_newSolutionFound; //Un nouvelle solution est trouvée
        private Action callback_packingDone; //L'algo complet de packing est terminé

        public float _printerWidth = -1;
        public float _printerHeight = -1; //Hauteur à +inf pour minimisation
        public float _printerDepth = -1;
        public Vector3 _printerCorner = Vector3.zero; //permettera de repositionner les mesh dans le volume si on minimise la hauteur (déifnit avant le début de l'algo)

        public int bvhDetail = 4;
        public int annealingIterations = 1000000;
        public int packingIteration = 1;

        public float minimalOffset;
        public Dictionary<string,Tuple<bool,bool,bool>> _rotationContraints; //Axes autorisés ou non pour chaque axe : <X,Y,Z>

        public int cpu;
        public List<System.Tuple<Vector3[] , int[], string >> meshes;


        public float bestScore;

        public void runSingleThread(){

            
            Model model = new Model();
            model.init();
            model.printerWidth      = this._printerWidth;
            model.printerHeight     = this._printerHeight;
            model.printerDepth      = this._printerDepth;
            model.printerCorner     = this._printerCorner;
            model._printerVolumeBox = new Box( new Vector3(-this._printerDepth/2,-this._printerHeight/2, - this._printerWidth/2),  new Vector3(this._printerDepth/2, this._printerHeight/2,this._printerWidth/2) );


            int count = 1;
            float totalVolume = 0.0f;

            for(int i = 0;i<meshes.Count;i++){
                
                Mesh mesh = Mesh.FromUnity(meshes[i].Item1, meshes[i].Item2, meshes[i].Item3);

                totalVolume += mesh.BoundingBox().Volume();
                Vector3 size = mesh.BoundingBox().Size();

                StringBuilder info = new StringBuilder();
                info.Append("["+i+"]"+" Load : "+mesh.Triangles.Count+" triangles");
                info.Append("\name : "+Path.GetFileNameWithoutExtension(meshes[i].Item3));
                info.Append("\nVolume : "+size.x+" , "+size.y+" , "+size.z);
                info.Append("\nMIN : "+mesh.BoundingBox().Min);
                info.Append("\nMAX : "+mesh.BoundingBox().Max);
                Debug.Log(info);

                mesh.Center();
                
                Tuple<bool,bool,bool> imo = null;

                bool itemCorrectyAdded = false;

                if(_rotationContraints != null && _rotationContraints.TryGetValue(meshes[i].Item3, out imo)){
                    itemCorrectyAdded = model.Add(mesh, bvhDetail, count, minimalOffset, imo.Item1, imo.Item2, imo.Item3);
                }else{
                    itemCorrectyAdded = model.Add(mesh, bvhDetail, count, minimalOffset, true, true, true);
                }

                callback_newShapeAddToModel?.Invoke(meshes[i].Item3, itemCorrectyAdded); //Retourne le fait qu'un mesh à été ajouté dans le model avec le nom du mesh
            }

            bool readyForPacking = model.Items.Count > 0;


            float side = (float)(Math.Pow(totalVolume, 1.0/3));
            model.Deviation = side / 32;

            this.bestScore = float.MaxValue;
            
            callback_packingBegin?.Invoke(packingIteration, readyForPacking);

            if(readyForPacking){

                for(int i = 0;i<packingIteration;i++){
            
                    Model newModel = model.Pack(annealingIterations, callback_annealProgress);
                    float score = newModel.Energy( (newModel.printerWidth == -1 && newModel.printerHeight == -1 && newModel.printerDepth == -1) );
                    callback_annealDone?.Invoke();
            
                    if(score < this.bestScore){
                        this.bestScore = score;
                        callback_newSolutionFound?.Invoke(newModel, this.bestScore);
                    }
            
                    model.Reset();
                
                    if(Main._abort3DPacking) //termine ici et permet d'avoir juste avant d'afficher le meilleur model 
                        break;
                    
                }
            }

            callback_packingDone?.Invoke();

        }

        public void runMultiThread(){
            
            Model model = new Model();
            model.init();
            model.printerWidth  = this._printerWidth;
            model.printerHeight = this._printerHeight;
            model.printerDepth  = this._printerDepth;
            model._printerVolumeBox = new Box( new Vector3(-this._printerDepth/2,-this._printerHeight/2, - this._printerWidth/2),  new Vector3(this._printerDepth/2, this._printerHeight/2,this._printerWidth/2) );
            model.printerCorner = this._printerCorner;


            int count = 1;
            float totalVolume = 0.0f;
            Mesh[] localMeshs = new Mesh[meshes.Count];

            for( int i = 0; i < meshes.Count ; i++ ){   
                localMeshs[i] = Mesh.FromUnity(meshes[i].Item1, meshes[i].Item2, meshes[i].Item3);
                totalVolume += localMeshs[i].BoundingBox().Volume();
                Vector3 size = localMeshs[i].BoundingBox().Size();
                localMeshs[i].Center();
            }

            Model.ItemBuilder itemBuilder = new Model.ItemBuilder(cpu, localMeshs, bvhDetail, count, minimalOffset, _rotationContraints, model);
            itemBuilder.onBuidProgress(callback_newShapeAddToModel);
            itemBuilder.onBuildDone( items =>{
                

                for(int i = 0;i<items.Length;i++){
                    
                    //check si le mesh a bien été inséré dans l'item, sinon cela signifie qu'il était trop gros
                    if(items[i].Mesh != null){
                        
                        model.add(items[i].Mesh, items[i].Trees , items[i].RotationConstraint.Item1,  items[i].RotationConstraint.Item2,  items[i].RotationConstraint.Item3,  items[i].offset);  

                    }
                }

                bool readyForPacking = model.Items.Count > 0; //a garder 


                float side = (float)(Math.Pow(totalVolume, 1.0/3));
                model.Deviation = side / 32;

                this.bestScore = float.MaxValue;
                
                callback_packingBegin?.Invoke(packingIteration, readyForPacking);

                if(readyForPacking){

                    for(int i = 0;i<packingIteration;i++){
                
                        Model newModel = model.Pack(annealingIterations, callback_annealProgress);
                        float score = newModel.Energy((newModel.printerWidth == -1 && newModel.printerHeight == -1 && newModel.printerDepth == -1));
                        callback_annealDone?.Invoke();
                
                        if(score < this.bestScore){
                            this.bestScore = score;
                            callback_newSolutionFound?.Invoke(newModel, this.bestScore);
                        }
                
                        model.Reset();
                    
                        if(Main._abort3DPacking){ //termine ici permet d'avoir juste avant afficher le meilleur model 
                            break;
                        }
                        
                    }
                }

                callback_packingDone?.Invoke();

            });
            itemBuilder.startBuilding();         

        }



        /*------------------------------------
         * callback setters
         ------------------------------------*/

        public void onNewShapeAddedIntoModel(Action<string, bool> callback){
            callback_newShapeAddToModel = callback;
        }

        public void onPackingBegin(Action<int, bool> callback){
            callback_packingBegin = callback;   
        }

        public void onAnnealDone(Action callback){
            callback_annealDone = callback;
        }


        public void onAnnealProgress(Action<Model, int, float, float> callback){
            callback_annealProgress = callback;
        }

        public void onNewSolutionFound(Action<Model, float> callback){
            callback_newSolutionFound = callback;
        }

        public void onPackingDone(Action callback){
            callback_packingDone = callback;
        }


    }
}
