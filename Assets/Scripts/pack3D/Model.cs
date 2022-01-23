using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Force.DeepCloner;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    public class Model
    {
        
        public List<Matrix> Rotations_XYZ; //toutes les rotations
        public List<Matrix> Rotations_XY;//que des rotations sur X et Y
        public List<Matrix> Rotations_XZ; //que des rotations sur X et Z
        public List<Matrix> Rotations_YZ; //que des rotations sur Y et Z
        public List<Matrix> Rotations_X; //que des rotations sur X 
        public List<Matrix> Rotations_Y; //que des rotations sur Y
        public List<Matrix> Rotations_Z; //que des rotations sur Z
        public	List<Item> Items;
        public float MinVolume;
        public float MaxVolume;
        public float MaxHeight;
        public  float Deviation;

        public float printerWidth;
        public float printerHeight;
        public float printerDepth;
        public Box _printerVolumeBox;
        public Vector3 printerCorner;

        public  System.Random _rand;



        public void init(){

            
            Rotations_XYZ = new List<Matrix>();

            for(int i = 0;i<4;i++){
                for(int s = -1;s<=1;s+=2){
                    for(int a = 1;a<=3;a++){

                        Vector3 up = new Vector3(0,0,1);
                        Matrix m = MatrixOP.Rotate(up, (double)(i)*Util.Radians(90));
                        m = m.RotateTo( up, Util.Axis(a).MulScalar( (float)(s) )  ) ;                        
                        Rotations_XYZ.Add(m);

                    }
                }
            }

            Rotations_X = new List<Matrix>();

            for(int i = 0;i<8;i++){
                Vector3 right = new Vector3(0,0,1);
                Matrix m = MatrixOP.Rotate(right, (double)(i)*Util.Radians(45));
                Rotations_X.Add(m);
            }


            Rotations_Y = new List<Matrix>();

            for(int i = 0;i<8;i++){
                Vector3 front = new Vector3(1,0,0);
                Matrix m = MatrixOP.Rotate(front, (double)(i)*Util.Radians(45));
                Rotations_Y.Add(m);
            }
            
            Rotations_Z = new List<Matrix>();

            for(int i = 0;i<8;i++){
                Vector3 right = new Vector3(0,1,0);
                Matrix m = MatrixOP.Rotate(right, (double)(i)*Util.Radians(45));
                Rotations_Z.Add(m);
            }

            
            Rotations_XY = new List<Matrix>();
            Rotations_XY.AddRange(Rotations_X);
            Rotations_XY.AddRange(Rotations_Y);


            Rotations_XZ = new List<Matrix>();
            Rotations_XZ.AddRange(Rotations_X);
            Rotations_XZ.AddRange(Rotations_Z);
            
           
            Rotations_YZ = new List<Matrix>();
            Rotations_YZ.AddRange(Rotations_Y);
            Rotations_YZ.AddRange(Rotations_Z);

            _rand = new System.Random();
        }

        public Matrix Matrix(Item i){
           
            if(i.RotationConstraint.Item1 && i.RotationConstraint.Item2 && i.RotationConstraint.Item3){
                return Rotations_XYZ[i.Rotation].Translate(i.Translation);
            
            }else if(i.RotationConstraint.Item1 && i.RotationConstraint.Item2){
                return Rotations_XY[i.Rotation].Translate(i.Translation);

            }else if(i.RotationConstraint.Item1 && i.RotationConstraint.Item3){
                return Rotations_XZ[i.Rotation].Translate(i.Translation);

            }else if(i.RotationConstraint.Item2 && i.RotationConstraint.Item3){
                return Rotations_YZ[i.Rotation].Translate(i.Translation);

            }else if(i.RotationConstraint.Item1){
                return Rotations_X[i.Rotation].Translate(i.Translation);

            }else if(i.RotationConstraint.Item2){
                return Rotations_Y[i.Rotation].Translate(i.Translation);

            }else if(i.RotationConstraint.Item3){
                return Rotations_Z[i.Rotation].Translate(i.Translation);

            }else{
                return Rotations_XYZ[i.Rotation].Translate(i.Translation);
            }

        }

        public Model(){

            this.Items = new List<Item>();
            this.MinVolume = 0;
            this.MaxVolume = 0;
            this.MaxHeight = 0;
            this.Deviation = 1;

            this.printerWidth  = -1;
            this.printerHeight = -1;
            this.printerDepth  = -1;

        }

        public bool Add(Mesh mesh, int detail, int count, float offset, bool allowX, bool allowY, bool allowZ){

            Tree tree = Tree.NewTreeForMesh(mesh, detail, offset);
            Tree[] trees = new Tree[0];

            if(allowX && allowY && allowZ){
                Debug.Log("run all");
                trees = new Tree[Rotations_XYZ.Count];
                for(int i = 0;i<Rotations_XYZ.Count;i++){
                    trees[i] = tree.Transform(Rotations_XYZ[i]);
                    Debug.Log("add rotation : "+i+"/"+trees.Length);
                }
            }else if(allowX && allowY && !allowZ){
                Debug.Log("run all XY");
                trees = new Tree[Rotations_XY.Count];
                for(int i = 0;i<Rotations_XY.Count;i++){
                    trees[i] = tree.Transform(Rotations_XY[i]);
                }
            }else if(allowX && !allowY && allowZ){
                Debug.Log("run all XZ");
                trees = new Tree[Rotations_XZ.Count];
                for(int i = 0;i<Rotations_XZ.Count;i++){
                    trees[i] = tree.Transform(Rotations_XZ[i]);
                }
            }else if(!allowX && allowY && allowZ){
                Debug.Log("run all YZ");
                trees = new Tree[Rotations_YZ.Count];
                for(int i = 0;i<Rotations_YZ.Count;i++){
                    trees[i] = tree.Transform(Rotations_YZ[i]);
                }
            }else if(allowX && !allowY && !allowZ){
                Debug.Log("run all X");
                trees = new Tree[Rotations_X.Count];
                for(int i = 0;i<Rotations_X.Count;i++){
                    trees[i] = tree.Transform(Rotations_X[i]);
                }
            }else if(!allowX && allowY && !allowZ){
                Debug.Log("run all Y");
                trees = new Tree[Rotations_Y.Count];
                for(int i = 0;i<Rotations_Y.Count;i++){
                    trees[i] = tree.Transform(Rotations_Y[i]);
                }
            }else if(!allowX && !allowY && allowZ){
                Debug.Log("run all Z");
                trees = new Tree[Rotations_Z.Count];
                for(int i = 0;i<Rotations_Z.Count;i++){
                    trees[i] = tree.Transform(Rotations_Z[i]);
                }
            }else{
                Debug.Log("run no rotation");
                trees = new Tree[1];
                trees[0] = tree;
            }


            /*------------------------
            * Affichage de chaque arbre pour chaque rotation
            ------------------------*/
            bool itemCorrectlyAdded = false;
            for(int i = 0;i<count;i++)
            {
                itemCorrectlyAdded = this.add(mesh, trees, allowX, allowY, allowZ, offset);  
            }

            return itemCorrectlyAdded;
            
        }

        public bool add(Mesh mesh, Tree[] trees, bool allowX, bool allowY, bool allowZ, float offset){
            

            /*------------------------------------------------------
            * premier check si on minimise la hauteur, on verifie
            * que le mesh passe dans le volume d'impression
            ------------------------------------------------------*/
            if(printerWidth > -1 && printerHeight > -1 && printerDepth > -1){

                Box mesh_boundingBox = mesh.BoundingBox(); 
                mesh_boundingBox.Max.x += 2*offset;
                mesh_boundingBox.Max.y += 2*offset;
                mesh_boundingBox.Max.z += 2*offset;

                if(!_printerVolumeBox.CanContainBoxWithRotation(mesh_boundingBox)){
                    return false;
                }
            }


            int index = Items.Count;
            Item item = new Item();
            item.Mesh  =  mesh;
            item.Trees = trees;
            item.Rotation = 0;
            item.Translation = Vector3.zero;
            item.RotationConstraint = new Tuple<bool,bool,bool>(allowX,allowY, allowZ);
            item.offset = offset;
            Items.Add(item);

            float d = 1.0f;
            
            //Premiere itération sans rien changer pour checker si la piece n'est pas déja placé corrrectement
            item.Rotation    = 0;
            item.Translation = Vector3.zero;
            Items[index] = item;

           /*------------------------------------
            * Si on cherche a minimiser la hauteur
            * On initialise les mesh en modifiant iterant sur la hauteur
            ------------------------------------*/
            if(printerWidth > -1 && printerHeight > -1 && printerDepth > -1){
                
                item.Translation = new Vector3( printerCorner.z, printerCorner.y, printerCorner.x);
                item.Rotation = 0;

                //recherche la rotation dans laquelle il est possible d'insérer la piece
                for(int i = 0;i<item.Trees.Length;i++){
                    item.Rotation = i;
                    if(_printerVolumeBox.CanContainBoxWithoutRotation(item.Trees[item.Rotation][0])){
                        break;
                    }
                }

                //permet de faire des deplacements en x/y sans dépasser de la boite
                Box ib = item.Trees[item.Rotation][0];
                float iw = ib.Max.z - ib.Min.z;
                float id = ib.Max.x - ib.Min.x;
                float ih = ib.Max.y - ib.Min.y;

                int errCounter = 10000;
                while(!ValidChange(index) && !Main._abort3DPacking && errCounter > 0){
                    
                    //V2
                    item.Translation.x += id/2.0f;

                    if(item.Translation.x > printerDepth/2.0f){
                        item.Translation.z += iw/2.0f;
                        item.Translation.x = printerCorner.z;
                    }
                    if(item.Translation.z > printerWidth/2.0f){
                        item.Translation.y += ih/2.0f;
                        item.Translation.z = printerCorner.x;
                        item.Translation.x = printerCorner.z;
                    }
       
                    Items[index] = item;

                    errCounter --;
                }

            }
            /*------------------------------------
            * Si il on cherche juste a minimiser le volume
            * On initalise les meshs en les deplacants partout dans l'aspace
            ------------------------------------*/
            else{

                while(!ValidChange(index)){

                    item.Rotation = _rand.Next(0, item.Trees.Length);
                    item.Translation = Util.RandomUnitVector(_rand).MulScalar(d);
                    d *= 1.2f;
                    Items[index] = item;
                }
            }


            Tree tree = trees[0];
            MinVolume = Math.Max(MinVolume, tree[0].Volume());
            MaxVolume += tree[0].Volume();
            MaxHeight += (tree[0].Max.z - tree[0].Min.z);

            return true;
        }

        public void Reset(){
            List<Item> items = Items;
            Items = new List<Item>();
            MinVolume = 0.0f;
            MaxVolume = 0.0f;
            MaxHeight = 0.0f;

            for(int i = 0;i<items.Count;i++){
                add(items[i].Mesh, items[i].Trees, items[i].RotationConstraint.Item1, items[i].RotationConstraint.Item2, items[i].RotationConstraint.Item3, items[i].offset);
            }
        }

        public Model Pack(int iterations, Action<Model, int, float, float> callback){

            float e = 0.05f;
            return Annealable.Anneal(this, 1.0f*e, 0.0001f*e, iterations, callback); //TODO a terminer
        }


        public List<Mesh> Meshs(){
            List<Mesh> result = new List<Mesh>();
            for(int i = 0;i<Items.Count;i++){

                Mesh tmpMesh = Items[i].Mesh.DeepCopy();
                tmpMesh.Transform( Matrix( Items[i]) );
                result.Add(tmpMesh);
            }

            return result;
        }

        public Mesh Mesh(){
            Mesh result = new Mesh();
            List<Mesh> meshes = Meshs();

            for(int i = 0;i<meshes.Count;i++){
                result.Add(meshes[i]);
            }

            return result;
        }

        public List<Mesh> TreeMeshes(){
            List<Mesh> result = new List<Mesh>();

            for(int i = 0;i<Items.Count;i++){

                Mesh mesh = new Mesh();
                Tree tree = Items[i].Trees[Items[i].Rotation];

                for(int j = 0;j<tree.Count/2;j++){

                    mesh.Add(Util.NewCubeForBox(tree[j]));
                }

                mesh.Transform(MatrixOP.Translate(Items[i].Translation));
                result[i] = mesh;
            }

            return result;
        }

        public Mesh TreeMesh(){
            Mesh result = new Mesh();
            List<Mesh> meshes = TreeMeshes();

            for(int i = 0;i<meshes.Count;i++){
                result.Add(meshes[i]);
            }
            return result;
        }


        public bool ValidChange(int i ){

            Item item1 = Items[i];
            Tree tree1 = item1.Trees[item1.Rotation];

            /*---------------------------------------------------
             * check la sortie du volume du printer
             ---------------------------------------------------*/
            if(printerWidth > -1 && printerHeight > -1 && printerDepth > -1){
                
                if(!tree1.Inside(_printerVolumeBox, item1.Translation, Vector3.zero) ){
                    return false;
                }
            }

            /*---------------------------------------------------
            * Check les collisions entres tous les éléments
            ---------------------------------------------------*/
            for(int j = 0;j<Items.Count;j++){

                if(j!=i){
                    
                    Item item2 = Items[j];
                    Tree tree2 = item2.Trees[item2.Rotation];

                    if(tree1.Intersects(tree2, item1.Translation, item2.Translation)){
                        return false;
                    }
                }
            }

            return true;
        }
        

        public Box BoundingBox(){

            Box box = new Box();

            for(int i = 0;i<Items.Count;i++){
                
                Item item          = Items[i];
                Box itemBox        = item.Trees[item.Rotation][0];
                Vector3 itemBoxMin = itemBox.Min;
                Vector3 itemBoxMax = itemBox.Max;
                box.ApplyExtend(itemBoxMin.x, itemBoxMin.y, itemBoxMin.z, itemBoxMax.x, itemBoxMax.y , itemBoxMax.z, item.Translation);
            }

            return box;
        }

        public float BoundingBoxFastHeight(){

            float box_minY = float.PositiveInfinity;
            float box_maxY = float.NegativeInfinity;

            float item_minY;
            float item_maxY;
           
            Item item;
            Box itemBox;

            for(int i = 0;i<Items.Count;i++){
                
                item = Items[i];
                itemBox = item.Trees[item.Rotation][0];

                item_minY  = itemBox.Min.y + item.Translation.y;              
                item_maxY  = itemBox.Max.y + item.Translation.y;

                box_minY = box_minY < item_minY ? box_minY : item_minY;                
                box_maxY = box_maxY > item_maxY ? box_maxY : item_maxY;
            }

            return (box_maxY - box_minY);
        }

        public float BoundingBoxFastVolume(){

            float box_minX = float.PositiveInfinity;
            float box_minY = float.PositiveInfinity;
            float box_minZ = float.PositiveInfinity;
            
            float box_maxX = float.NegativeInfinity;
            float box_maxY = float.NegativeInfinity;
            float box_maxZ = float.NegativeInfinity;

            float item_minX;
            float item_minY;
            float item_minZ;

            float item_maxX;
            float item_maxY;
            float item_maxZ;
           
            Item item;
            Box itemBox;

            for(int i = 0;i<Items.Count;i++){
                
                item = Items[i];
                itemBox = item.Trees[item.Rotation][0];

                item_minX  = itemBox.Min.x + item.Translation.x;              
                item_minY  = itemBox.Min.y + item.Translation.y;              
                item_minZ  = itemBox.Min.z + item.Translation.z;    

                item_maxX  = itemBox.Max.x + item.Translation.x;
                item_maxY  = itemBox.Max.y + item.Translation.y;
                item_maxZ  = itemBox.Max.z + item.Translation.z;

                box_minX = box_minX < item_minX ? box_minX : item_minX;                
                box_minY = box_minY < item_minY ? box_minY : item_minY;                
                box_minZ = box_minZ < item_minZ ? box_minZ : item_minZ;

                box_maxX = box_maxX > item_maxX ? box_maxX : item_maxX;
                box_maxY = box_maxY > item_maxY ? box_maxY : item_maxY;
                box_maxZ = box_maxZ > item_maxZ ? box_maxZ : item_maxZ;
            }

            return (box_maxX - box_minX) * (box_maxY - box_minY) * (box_maxZ - box_minZ);
        }

        public float Volume(){
            return BoundingBoxFastVolume();
        }

        public float Height(){
            return BoundingBoxFastHeight();
        }

        public float EnergyVolume(){
            return Volume()/MaxVolume;

        }

        public float EnergyHeight(){
            return Height()/MaxHeight;
        }

        public float Energy(bool isHeight){

            if(!isHeight){
            
                return EnergyVolume();

            }else{
                
                return EnergyHeight();
            }
        }


        public void DoMove(ref Undo undo, bool isHeight){

            int i = _rand.Next(0,Items.Count);

            Item item = Items[i];

            undo.Index       = i;
            undo.Rotation    = item.Rotation;
            undo.Translation = item.Translation;

        
            int counter = 2;
            while(counter < 10000){

                int random = _rand.Next(0,4);

                if(random == 0){

                    //rotate
                    item.Rotation = _rand.Next(0,item.Trees.Length);

                }else{


                    if(isHeight){
                                
                       Vector3 offset    = Util.Axis(_rand.Next(1,4));
                       float randomFloat = 0;
                       
                       if( offset.x != 0 ){
                            randomFloat = Util.RandTinyFloat(_rand, -printerDepth/counter, printerDepth/counter);
                       }
                       if( offset.z != 0){
                            randomFloat = Util.RandTinyFloat(_rand, -printerWidth/counter, printerWidth/counter);
                       }
                       
                       
                       offset *= randomFloat;  //rapide
                       offset.y = Util.RandTinyFloat(_rand,-1,1); //deplace toujours sur la hauteur

                       item.Translation += offset; //rapide
                
                    }else{

                        //translate si pas de volume d'impression (minimise le volume)
                        Vector3 offset = Util.Axis(_rand.Next(1,4));
                        float randomFloat = Util.RandTinyFloat(_rand,-1,1);
                        offset = offset.MulScalar(randomFloat);        
                        item.Translation = item.Translation.Add(offset);
                    }
     
                }

                if(ValidChange(i)){
                    Items[i] = item;
                    break;
                }

                item.Rotation    = undo.Rotation;
                item.Translation = undo.Translation;

                counter ++;
            }

        }

        public void UndoMove(Undo undo){
            Items[undo.Index].Rotation    = undo.Rotation;
            Items[undo.Index].Translation = undo.Translation;
        }

        /*------------------------------------
         * Copie complete de tout le model par duplication
         * de tous les objets
         ------------------------------------*/
        public Model DeepCopy(){

            List<Matrix> rotationsXYZ = new List<Matrix>();
            List<Matrix> rotationsXY  = new List<Matrix>();
            List<Matrix> rotationsXZ  = new List<Matrix>();
            List<Matrix> rotationsYZ  = new List<Matrix>();
            List<Matrix> rotationsX   = new List<Matrix>();
            List<Matrix> rotationsY   = new List<Matrix>();
            List<Matrix> rotationsZ   = new List<Matrix>();

            for(int i = 0;i<Rotations_XYZ.Count;i++){
                rotationsXYZ.Add(Rotations_XYZ[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_XY.Count;i++){
                rotationsXY.Add(Rotations_XY[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_XZ.Count;i++){
                rotationsXZ.Add(Rotations_XZ[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_YZ.Count;i++){
                rotationsYZ.Add(Rotations_YZ[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_X.Count;i++){
                rotationsX.Add(Rotations_X[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_Y.Count;i++){
                rotationsY.Add(Rotations_Y[i].DeepCopy());
            }
            for(int i = 0;i<Rotations_Z.Count;i++){
                rotationsZ.Add(Rotations_Z[i].DeepCopy());
            }

            List<Item> items = new List<Item>();
            for(int i = 0;i<Items.Count;i++){
                items.Add(Items[i].DeepCopy());
            }


            Model model = new Model();

            model.Rotations_XYZ = rotationsXYZ;
            model.Rotations_XY  = rotationsXY;
            model.Rotations_XZ  = rotationsXZ;
            model.Rotations_YZ  = rotationsYZ;
            model.Rotations_X   = rotationsX;
            model.Rotations_Y   = rotationsY;
            model.Rotations_Z   = rotationsZ;

            model.Items = items;
            model.MinVolume = MinVolume;
            model.MaxVolume = MaxVolume;
            model.MaxHeight = MaxHeight;
            model.Deviation = Deviation;

            model.printerHeight     = printerHeight;
            model.printerWidth      = printerWidth;
            model.printerDepth      = printerDepth;
            model._printerVolumeBox = _printerVolumeBox;
            model.printerCorner     = printerCorner;

            model._rand = _rand;
            
            return model;
        }

        /*------------------------------------
         * Copie superficielle, pas de recopie
         * de toutes les donnees
         * seule la rotation et la translation
         * sont copiées car c'est la dessus qu'on joue le recuit
         ------------------------------------*/
        public Model ShallowCopy(){

            List<Item> items = new List<Item>();
            for(int i = 0;i<Items.Count;i++){
                items.Add(Items[i].ShallowCopy());
            }
            
            Model model = new Model();

            model.Rotations_XYZ = Rotations_XYZ;
            model.Rotations_XY  = Rotations_XY;
            model.Rotations_XZ  = Rotations_XZ;
            model.Rotations_YZ  = Rotations_YZ;
            model.Rotations_X   = Rotations_X;
            model.Rotations_Y   = Rotations_Y;
            model.Rotations_Z   = Rotations_Z;

            model.Items = items;
            model.MinVolume = MinVolume;
            model.MaxVolume = MaxVolume;
            model.MaxHeight = MaxHeight;
            model.Deviation = Deviation;

            model.printerHeight     = printerHeight;
            model.printerWidth      = printerWidth;
            model.printerDepth      = printerDepth;
            model._printerVolumeBox = _printerVolumeBox;
            model.printerCorner     = printerCorner;

            model._rand = _rand;
            
            return model;
        }

        /*------------------------------------
         *  Met a jour les meshFilters dans l'interface
         ------------------------------------*/
        public void showAsGo(ref Dictionary<string,GameObject> sharedMeshes){

                //Remet tout le monde dans le repere UNity + reherche du milieu 
                List<Mesh> meshes = Meshs();
                List<System.Tuple<Vector3[], int[], string>> unityMesh = new List<System.Tuple<Vector3[], int[], string>>();
                Vector3 mean = Vector3.zero;
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                int countVertices = 0;

                for(int k = 0;k<meshes.Count;k++){
                    
                    unityMesh.Add( meshes[k].toUnity());

                    Vector3[] tmpMeshVertices = unityMesh[unityMesh.Count-1].Item1;

                    for(int l = 0; l<tmpMeshVertices.Length; l++ ){
                        mean = mean.Add(tmpMeshVertices[l]);
                        countVertices ++;

                        min.x = tmpMeshVertices[l].x < min.x ? tmpMeshVertices[l].x : min.x; 
                        min.y = tmpMeshVertices[l].y < min.y ? tmpMeshVertices[l].y : min.y; 
                        min.z = tmpMeshVertices[l].z < min.z ? tmpMeshVertices[l].z : min.z; 
                    }
                }

                mean /= countVertices;
            
                min = min.Sub(printerCorner);
                            
                //Translation des mesh + update
                for(int k = 0;k<unityMesh.Count;k++){

                    System.Tuple<Vector3[], int[], string> meshData = unityMesh[k];

                    //translate coin vers coin                    
                    Vector3[] tmpVertices = meshData.Item1;
                    for(int l = 0;l<tmpVertices.Length;l++){
                        tmpVertices[l] = tmpVertices[l].Sub(min);
                    }
                    
                    //update mesh
                    UnityEngine.Mesh tmpMesh = new UnityEngine.Mesh();
                    if(tmpVertices.Length > short.MaxValue -1){
                        tmpMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    }

                    tmpMesh.vertices = tmpVertices;
                    tmpMesh.triangles = meshData.Item2;
                    tmpMesh.RecalculateNormals();

                    sharedMeshes[meshData.Item3].GetComponent<MeshFilter>().mesh = tmpMesh;

                    //Reset position
                    sharedMeshes[meshData.Item3].transform.position = Vector3.zero;
                    sharedMeshes[meshData.Item3].transform.rotation = Quaternion.identity;

                }

                
        }






        public class Undo{
        public int Index;
        public int Rotation;
        public Vector3 Translation;
        }


        public class Item{    
            public Mesh Mesh;
            public Tree[] Trees;
            public int Rotation;
            public Tuple<bool,bool,bool> RotationConstraint;
            public float offset;

            public Vector3 Translation;
            public Vector3 Pivot;

            /*------------------------------------
            * Copie complete de tout l'item par duplication
            * de tous les objets
            ------------------------------------*/
            public Item DeepCopy(){

                Item copy = new Item();

                Mesh tmpMesh = Mesh.DeepCopy();

                Tree[] tmpTrees = new Tree[this.Trees.Length];
                for(int i = 0;i<this.Trees.Length;i++){
                    tmpTrees[i] = Trees[i];
                }

                int tmpRotation = this.Rotation;

                Vector3 tmpTranslation = new Vector3(this.Translation.x, this.Translation.y, this.Translation.z);

                copy.Mesh = tmpMesh;
                copy.Trees = tmpTrees;
                copy.Rotation = tmpRotation;
                copy.Translation = tmpTranslation;
                copy.RotationConstraint = new Tuple<bool,bool,bool>(RotationConstraint.Item1, RotationConstraint.Item2, RotationConstraint.Item3);
                copy.offset = offset;
                return copy;          
            }

            public Item ShallowCopy(){

                Item copy = new Item();

                Mesh tmpMesh = Mesh;

                Tree[] tmpTrees = new Tree[this.Trees.Length];
                for(int i = 0;i<this.Trees.Length;i++){
                    tmpTrees[i] = Trees[i];
                }

                int tmpRotation = this.Rotation;

                Vector3 tmpTranslation = new Vector3(this.Translation.x, this.Translation.y, this.Translation.z);

                copy.Mesh = tmpMesh;
                copy.Trees = tmpTrees;
                copy.Rotation = tmpRotation;
                copy.Translation = tmpTranslation;
                copy.RotationConstraint = new Tuple<bool,bool,bool>(RotationConstraint.Item1, RotationConstraint.Item2, RotationConstraint.Item3);
                copy.offset = offset;
                return copy;          
            }
        }


        /*******************************************************
        * Class qui permet de gerer un ensemble de thread pour
        * le build d'item qui sont des operations longues
        *******************************************************/
        public class ItemBuilder{
            
            /*--------------------------------------------------------
            * callback functions
            --------------------------------------------------------*/
            private Action<string,bool> _onBuilderProgress;
            private Action<Item[]> _onBuildrDone;

            /*--------------------------------------------------------
            * input
            --------------------------------------------------------*/
            private int _cpu;
            private Mesh[] _meshs;
            private int _bvhDetail;
            private int _count;
            private float _minimalOffset;
            private Dictionary<string,Tuple<bool,bool,bool>> _rotationContraints;
            private Model _model;

            /*--------------------------------------------------------
            * runtime data
            --------------------------------------------------------*/
            private bool _isActive;
            private int _countThreadRunning;

            /*--------------------------------------------------------
            * output
            --------------------------------------------------------*/
            public Dictionary<string,Item> _items;

            public ItemBuilder(int cpu, Mesh[] meshs, int bvhDetail, int count, float minimalOffset, Dictionary<string,Tuple<bool,bool,bool>> rotationContraints, Model model){

                _cpu                = cpu; 
                _meshs              = meshs;
                _bvhDetail          = bvhDetail;
                _count              = count;
                _minimalOffset      = minimalOffset;
                _rotationContraints = rotationContraints;
                _model              = model;

                _isActive = false;

                _items              = new Dictionary<string, Item>();
            }

            public void onBuidProgress(Action<string,bool> onBuilderProgress){
                _onBuilderProgress = onBuilderProgress;
            }

            public void onBuildDone (Action<Item[]> onBuildDone){
                _onBuildrDone = onBuildDone;
            }


            public void startBuilding(){

                _isActive = true;
                _countThreadRunning = _cpu;

                for(int i = 0;i<_meshs.Length;i++){
                    _items[_meshs[i].name] = null;
                }

                for(int i = 0;i<_cpu;i++){
                    Thread t = new Thread( BuildJob );
                    t.IsBackground = true;
                    t.Priority = System.Threading.ThreadPriority.Highest;
                    t.Start();
                } 
            }

            private void stopBuilding(){
                _isActive = false;
            }


            private void BuildJob(){
                bool can_continue = true;

                while(can_continue && _isActive){

                    if(Main._abort3DPacking){
                        break;
                    }
                    
                    Mesh workingMesh   = null;
                    Tuple<bool,bool,bool> imo = null;

                    lock(_meshs){

                        lock(_items){

                            for(int i = 0;i<_meshs.Length;i++){
                                
                                if(_items[_meshs[i].name] == null){
                                    _items[_meshs[i].name] = new Item();
                                    workingMesh = _meshs[i];
                                    break;
                                }
                            }
                        }
                    }

                    //Si on a pas trouvé de mesh, on arrete ici
                    if(workingMesh == null ){

                        can_continue = false;
                        _countThreadRunning --;

                        //Si on est le dernier à ne rien avoir trouvé, on termine tout le process
                        if(_countThreadRunning == 0){
                            _onBuildrDone?.Invoke(_items.Values.ToArray());
                        }  
                        break; 
                    }


                    imo = new Tuple<bool, bool, bool>(true,true,true);


                    Tree tree = Tree.NewTreeForMesh(workingMesh, _bvhDetail, _minimalOffset);
                    Tree[] trees = new Tree[0];  


                    if(imo.Item1 && imo.Item2 && imo.Item3){
                        trees = new Tree[_model.Rotations_XYZ.Count];
                        for(int i = 0;i<_model.Rotations_XYZ.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_XYZ[i]);
                        }
                    }else if(imo.Item1 && imo.Item2 && !imo.Item3){
                        trees = new Tree[_model.Rotations_XY.Count];
                        for(int i = 0;i<_model.Rotations_XY.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_XY[i]);
                        }
                    }else if(imo.Item1 && !imo.Item2 && imo.Item3){
                        trees = new Tree[_model.Rotations_XZ.Count];
                        for(int i = 0;i<_model.Rotations_XZ.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_XZ[i]);
                        }
                    }else if(!imo.Item1 && imo.Item2 && imo.Item3){
                        trees = new Tree[_model.Rotations_YZ.Count];
                        for(int i = 0;i<_model.Rotations_YZ.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_YZ[i]);
                        }
                    }else if(imo.Item1 && !imo.Item2 && !imo.Item3){
                        trees = new Tree[_model.Rotations_X.Count];
                        for(int i = 0;i<_model.Rotations_X.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_X[i]);
                        }
                    }else if(!imo.Item1 && imo.Item2 && !imo.Item3){
                        trees = new Tree[_model.Rotations_Y.Count];
                        for(int i = 0;i<_model.Rotations_Y.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_Y[i]);
                        }
                    }else if(!imo.Item1 && !imo.Item2 && imo.Item3){
                        trees = new Tree[_model.Rotations_Z.Count];
                        for(int i = 0;i<_model.Rotations_Z.Count;i++){
                            trees[i] = tree.Transform(_model.Rotations_Z[i]);
                        }
                    }else{
                        trees = new Tree[1];
                        trees[0] = tree;
                    } 

                    bool meshFitInPrinter = true;

                    //Check s'il est possible d'ajouter la piece dans le volume si on cherche a minimiser le volume
                    if(_model.printerWidth > -1 && _model.printerHeight > -1 && _model.printerDepth > -1){

                        Box mesh_boundingBox = workingMesh.BoundingBox(); 
                        mesh_boundingBox.Max.x += 2*_minimalOffset;
                        mesh_boundingBox.Max.y += 2*_minimalOffset;
                        mesh_boundingBox.Max.z += 2*_minimalOffset;

                        if(!_model._printerVolumeBox.CanContainBoxWithRotation(mesh_boundingBox)){
                                _onBuilderProgress?.Invoke(workingMesh.name, false);
                                meshFitInPrinter = false;
                        }

                    }

                    //check si le mesh rentre bien dans le printer, ce bool est modifié seulement dans le cas ou on est en mode minimisation de la hauteur
                    if(meshFitInPrinter){

                        Item item = new Item();
                        item.Mesh  =  workingMesh;
                        item.Trees = trees;
                        item.Rotation = 0;
                        item.Translation = Vector3.zero;
                        item.RotationConstraint = new Tuple<bool,bool,bool>(imo.Item1, imo.Item2, imo.Item3);
                        item.offset = _minimalOffset;

                        lock(_items){
                            _items[workingMesh.name] = item;
                            _onBuilderProgress?.Invoke(workingMesh.name, true);

                        } 
                    }                
                }
            }


        }
        
    }
}


