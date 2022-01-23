using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    public class Tree 
    {

        public static int debugConteurIterationDeBox = 0;

        public List<Box> tree;

        public int Count { 

            get { return tree.Count; }
        }

        public Tree(){
            tree = new List<Box>();
        }

        public Box this[int i]
        {
            get { return tree[i]; }
            set { tree[i] = value; }
        }

        public void AddEmpty(){
            tree.Add(new Box(Vector3.zero, Vector3.zero));
        }

        public static Tree NewTreeForMesh(Mesh m, int depth, float offset){

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Mesh mesh = m.DeepCopy();

            mesh.Center();

            List<Box> boxes = new List<Box>();

            StringBuilder sb = new StringBuilder();

            for(int i = 0;i< mesh.Triangles.Count;i++){
                boxes.Add(mesh.Triangles[i].BoundingBox());
            }

            //boxes = 1 box / triangle
            Node root = Node.NewNode(boxes.ToArray(), depth, offset);

            //2^depth boxes -1
            Tree tree = new Tree(); 

            int computeSize = (int)Math.Pow(2,depth+1);
            computeSize -=1;

            for(int i = 0;i<computeSize;i++){
                tree.AddEmpty();
            }

            root.Flatten(ref tree, 0);

            return tree;
        }

        public Tree Transform(Matrix m){
            Tree b = new Tree();

            for(int i = 0;i<tree.Count;i++){
                b.tree.Add(tree[i].Transform(m));
 
            }

            return b;
        }

        /*------------------------------------------------
        * Methode iterative
        ------------------------------------------------*/
        public bool IntersectsV2(Tree b, Vector3 t1, Vector3 t2){
            
            Stack<Tuple<int,int>> stack = new Stack<Tuple<int,int>>();
            stack.Push(new Tuple<int,int>(0,0));


            while(stack.Count > 0){

                Tuple<int, int> currentIndex = stack.Pop();
                int i = currentIndex.Item1;
                int j = currentIndex.Item2;

                //J'ai une intersection donc je continue a descendre dans les boites a l'interieur
                if(boxesIntersect(this[i], b[j], t1, t2)){

                    int i1 = i*2 + 1;
                    int i2 = i*2 + 2;
                    int j1 = j*2 + 1;
                    int j2 = j*2 + 2;

                    //mais je ne peut pas descndre plus bas, je suis sur une des plus petites boites, je suis donc sur d'avoir une intersection
                    if(i1 >= tree.Count && j1 >= b.tree.Count ){
                        return true;
                    } 

                    if(i1 >= tree.Count) {

                        stack.Push(new Tuple<int, int>(i,j1));
                        stack.Push(new Tuple<int, int>(i,j2));

                    }else if( j1 >= b.tree.Count ){

                        stack.Push(new Tuple<int, int>(i1,j));
                        stack.Push(new Tuple<int, int>(i2,j));

                    }else {

                        stack.Push(new Tuple<int, int>(i1,j1));
                        stack.Push(new Tuple<int, int>(i1,j2));
                        stack.Push(new Tuple<int, int>(i2,j1));
                        stack.Push(new Tuple<int, int>(i2,j2));
                    }    
                }            
            }

            //J'ai jamais eu d'intersection jusqu'au bout
            return false;
        }

        /*------------------------------------------------
        * Methode recursive
        ------------------------------------------------*/
        public bool Intersects(Tree b , Vector3 t1, Vector3 t2){
            return intersects(b, t1, t2, 0,0);
        }  
        
        private bool intersects(Tree b, Vector3 t1, Vector3 t2, int i, int j ){

            if(!boxesIntersect(this[i], b[j], t1, t2)){
                return false;
            }

            int i1 = i*2 + 1;
            int i2 = i*2 + 2;
            int j1 = j*2 + 1;
            int j2 = j*2 + 2;

            if(i1 >= tree.Count && j1 >= b.tree.Count ){
                return true;
            } else if(i1 >= tree.Count) {
                return (intersects(b, t1, t2, i, j1) || intersects(b, t1, t2, i, j2));

            } else if( j1 >= b.tree.Count ){
                return intersects(b, t1, t2, i1, j) || intersects(b, t1, t2, i2, j);

            } else {

                return intersects(b, t1, t2, i1, j1) ||
                       intersects(b, t1, t2, i1, j2) ||
                       intersects(b, t1, t2, i2, j1) ||
                       intersects(b, t1, t2, i2, j2);
            }
        }

        private bool boxesIntersect(Box b1, Box b2, Vector3 t1, Vector3 t2){

            Vector3 B1_MAX  = b1.Max+t1; 
            Vector3 B1_MIN  = b1.Min+t1; 
            Vector3 B2_MAX  = b2.Max+t2; 
            Vector3 B2_MIN  = b2.Min+t2; 


            return !(B1_MIN.x > B2_MAX.x ||
                     B1_MAX.x < B2_MIN.x ||
                     B1_MIN.y > B2_MAX.y ||
                     B1_MAX.y < B2_MIN.y ||
                     B1_MIN.z > B2_MAX.z ||
                     B1_MAX.z < B2_MIN.z);

        }

    
        public bool Inside(Box b, Vector3 t1, Vector3 t2){
            return inside(b, t1, t2, 0);
        }

        private bool inside(Box b, Vector3 t1 , Vector3 t2, int i  ){
            
            if( !boxesInside(this[i], b , t1, t2) ){
                return false;
            }

            int i1 = i*2 + 1;
            int i2 = i*2 + 2;
 
            if(i1 >= tree.Count ){
                return true;

            } else {
                return inside(b, t1, t2, i1) || inside(b, t1, t2, i2);
            }
        }

        private bool boxesInside(Box b1, Box b2,  Vector3 t1, Vector3 t2){
           

            //check si 2 est dans 1 (= max b1 < max b2 et min b1 > min b2)

            Vector3 B1_MAX  = b1.Max+t1; 
            Vector3 B1_MIN  = b1.Min+t1; 
            Vector3 B2_MAX  = b2.Max+t2; 
            Vector3 B2_MIN  = b2.Min+t2; 

            return (B1_MIN.x >= B2_MIN.x &&
                     B1_MAX.x <= B2_MAX.x &&
                     B1_MIN.y >= B2_MIN.y &&
                     B1_MAX.y <= B2_MAX.y &&
                     B1_MIN.z >= B2_MIN.z &&
                     B1_MAX.z <= B2_MAX.z);
                 
        }




        private class Node{
            public Box Box;
            public Node Left;
            public Node Right;

            public static Node NewNode(Box[] boxes , int depth, float offset){

                //boxes = 1 box / triangle
                Box box = Box.BoxForBoxes(boxes).Offset(offset);
                

                Node node = new Node();
                node.Box  = box;
                node.Left = null;
                node.Right = null;

                node.Split(boxes, depth, offset);

                return node;
            }

            public void Flatten(ref Tree tree, int i){
                                
                tree[i] = this.Box;
                if (Left != null) {
                    Left.Flatten(ref tree, i*2+1);
                }
                if(Right != null) {
                    Right.Flatten(ref tree, i*2+2);
                } 
            } 

            public void Split(Box[] boxes, int depth, float offset){

                if(depth == 0){
                    return;
                }

                Box box         = this.Box;
                float best      = box.Volume();
                int bestAxis    = Util.Vector0;
                float bestPoint = 0.0f;
                bool bestSide   = false;
                int N = 16;


                for(int s = 0;s<2;s++){

                    bool side = (s == 1);

                    for(int i = 1;i<N;i++){

                        float p = (float)(i) / (float)(N);
                        float x = box.Min.x + (box.Max.x-box.Min.x)*p;
                        float y = box.Min.y + (box.Max.y-box.Min.y)*p;
                        float z = box.Min.z + (box.Max.z-box.Min.z)*p;


                        float sx = partitionScoreFast(boxes, Util.VectorX, x, side);

                        if(sx < best){
                            best = sx;
                            bestAxis = Util.VectorX;
                            bestPoint = x;
                            bestSide = side;
                        }

                        float sy = partitionScoreFast(boxes, Util.VectorY, y, side);
                        
                        if(sy < best) {
                            best = sy;
                            bestAxis = Util.VectorY;
                            bestPoint = y;
                            bestSide = side;
                        }

                        float sz = partitionScoreFast(boxes, Util.VectorZ, z, side);
                        
                        if(sz < best) {

                            best = sz;
                            bestAxis = Util.VectorZ;
                            bestPoint = z;
                            bestSide = side;

                        }
                    }
                }

                if(bestAxis == Util.Vector0){
                    return;
                }

                List<Box> l;
                List<Box> r;
                partitionFast(boxes, bestAxis, bestPoint, bestSide, out l, out r);


                this.Left  = NewNode(l.ToArray(), depth-1, offset);
                this.Right = NewNode(r.ToArray(), depth-1, offset);

            }  

            public void partitionBox(Box box, int axis, float point, out bool left, out bool right){
                
                left = false;
                right = false;

                if(axis == Util.VectorX){
                    left = box.Min.x <= point;
                    right = box.Max.x >= point;

                }else if(axis == Util.VectorY){
                    left = box.Min.y <= point;
                    right = box.Max.y >= point;

                }else if(axis == Util.VectorZ){
                    left = box.Min.z <= point;
                    right = box.Max.z >= point;
                }

            }  




            /*------------------------------------------------
            * V2 SANS OBJET PLUS RAPIDE
            -------------------------------------------------*/

            public float partitionScoreFast(Box[] boxes, int axis, float point, bool side){
                

                float major_minX = float.PositiveInfinity;
                float major_minY = float.PositiveInfinity;
                float major_minZ = float.PositiveInfinity;

                float major_maxX = float.NegativeInfinity;
                float major_maxY = float.NegativeInfinity;
                float major_maxZ = float.NegativeInfinity;

                int majorCounter = 0;
               
                bool l = false;
                bool r = false;
                
                if(axis == Util.VectorX){
                   
                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.x <= point;
                        r = boxes[i].Max.x >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x;
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y;
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z;
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x;
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y;
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z;
                            majorCounter ++;
                        }
                    }

                }else if(axis == Util.VectorY){

                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.y <= point;
                        r = boxes[i].Max.y >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x;
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y;
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z;
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x;
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y;
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z;
                            majorCounter ++;
                        }
                    }

                }else if(axis == Util.VectorZ){
                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.z <= point;
                        r = boxes[i].Max.z >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x; 
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y; 
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z; 
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x; 
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y; 
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z; 
                            majorCounter ++;
                        }
                    }
                }

      
                if(majorCounter == 0){
                    major_minX = 0; major_minY = 0;major_minZ = 0;
                    major_maxX = 0; major_maxY = 0;major_maxZ = 0;
                }

                //Box minor = new Box();
                float minor_minX = float.PositiveInfinity;float minor_minY = float.PositiveInfinity;float minor_minZ = float.PositiveInfinity;
                float minor_maxX = float.NegativeInfinity;float minor_maxY = float.NegativeInfinity;float minor_maxZ = float.NegativeInfinity;
               
                int minorCounter = 0;

                for(int i = 0;i<boxes.Length;i++){
                    
                    //si major contient la box
                    if( !(major_minX <= boxes[i].Min.x && major_maxX >= boxes[i].Max.x &&
                          major_minY <= boxes[i].Min.y && major_maxY >= boxes[i].Max.y &&
                          major_minZ <= boxes[i].Min.z && major_maxZ >= boxes[i].Max.z)){

                        minor_minX = minor_minX < boxes[i].Min.x ? minor_minX : boxes[i].Min.x;
                        minor_minY = minor_minY < boxes[i].Min.y ? minor_minY : boxes[i].Min.y;
                        minor_minZ = minor_minZ < boxes[i].Min.z ? minor_minZ : boxes[i].Min.z;
                        minor_maxX = minor_maxX > boxes[i].Max.x ? minor_maxX : boxes[i].Max.x;
                        minor_maxY = minor_maxY > boxes[i].Max.y ? minor_maxY : boxes[i].Max.y;
                        minor_maxZ = minor_maxZ > boxes[i].Max.z ? minor_maxZ : boxes[i].Max.z;
                        
                        minorCounter ++;
                    }
                }

                if(minorCounter == 0){
                    minor_minX = 0;minor_minY = 0;minor_minZ = 0;
                    minor_maxX = 0;minor_maxY = 0;minor_maxZ = 0;
                }


                //calcul intersection
                float minX = major_minX > minor_minX ? major_minX : minor_minX; 
                float minY = major_minY > minor_minY ? major_minY : minor_minY; 
                float minZ = major_minZ > minor_minZ ? major_minZ : minor_minZ; 

                float maxX = major_maxX < minor_maxX ? major_maxX : minor_maxX; 
                float maxY = major_maxY < minor_maxY ? major_maxY : minor_maxY; 
                float maxZ = major_maxZ < minor_maxZ ? major_maxZ : minor_maxZ; 

                minX = minX < maxX ? minX : maxX; 
                minY = minY < maxY ? minY : maxY; 
                minZ = minZ < maxZ ? minZ : maxZ; 

                maxX = minX > maxX ? minX : maxX; 
                maxY = minY > maxY ? minY : maxY; 
                maxZ = minZ > maxZ ? minZ : maxZ; 


                float intersection = ( (maxX-minX) * (maxY-minY) * (maxZ-minZ) );
                
                intersection = !(major_minX > minor_maxX || major_maxX < minor_minX ||
                                 major_minY > minor_maxY || major_maxY < minor_minY || 
                                 major_minZ > minor_maxZ || major_maxZ < minor_minZ) ? intersection : 0;
                

                float volumeMajor = (major_maxX-major_minX) * (major_maxY-major_minY) * (major_maxZ-major_minZ) ;
                float volumeMinor = (minor_maxX-minor_minX) * (minor_maxY-minor_minY) * (minor_maxZ-minor_minZ);
                

                return ( volumeMajor ) + ( volumeMinor ) - ( intersection );
            }



            public void partitionSlow(Box[] boxes, int axis, float point, bool side, out List<Box> left, out List<Box> right){
                
                left  = new List<Box>();
                right = new List<Box>();

                Box major = new Box();
                int majorCounter =0;

                for(int i = 0;i<boxes.Length;i++){

                    bool l; 
                    bool r;
                    partitionBox(boxes[i], axis, point, out l , out r);

                
                    if ((l && r) || (l && side) || (r && !side)) {
                                                
                        Vector3 itemBoxMin = boxes[i].Min; 
                        Vector3 itemBoxMax = boxes[i].Max; 

                        major.ApplyExtend(itemBoxMin.x, itemBoxMin.y, itemBoxMin.z, itemBoxMax.x, itemBoxMax.y , itemBoxMax.z, Vector3.zero); 

                        majorCounter ++;
                    }
                }

                if(majorCounter == 0){
                    major = new Box(Vector3.zero, Vector3.zero);
                }

                
                for(int i = 0;i<boxes.Length;i++){

                    if( major.ContainsBox(boxes[i]) ){

                        left.Add(boxes[i]);

                    } else {

                        right.Add(boxes[i]);
                    }
                }

                if(!side){

                    List<Box> tmpLeft = new List<Box>();
                    tmpLeft = left;
                    left = right;
                    right = tmpLeft;
                }

            }

            public void partitionFast(Box[] boxes, int axis, float point, bool side, out List<Box> left, out List<Box> right){
                
                left  = new List<Box>();
                right = new List<Box>();

                float major_minX = float.PositiveInfinity;
                float major_minY = float.PositiveInfinity;
                float major_minZ = float.PositiveInfinity;

                float major_maxX = float.NegativeInfinity;
                float major_maxY = float.NegativeInfinity;
                float major_maxZ = float.NegativeInfinity;

                int majorCounter = 0;
               
                bool l = false;
                bool r = false;
                
                if(axis == Util.VectorX){
                   
                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.x <= point;
                        r = boxes[i].Max.x >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x;
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y;
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z;
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x;
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y;
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z;
                            majorCounter ++;
                        }
                    }

                }else if(axis == Util.VectorY){

                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.y <= point;
                        r = boxes[i].Max.y >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x;
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y;
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z;
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x;
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y;
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z;
                            majorCounter ++;
                        }
                    }

                }else if(axis == Util.VectorZ){
                    for(int i = 0;i<boxes.Length;i++){
                        l = boxes[i].Min.z <= point;
                        r = boxes[i].Max.z >= point;
                        if( (l && r) || (l && side) || (r && !side) ){

                            major_minX = major_minX < boxes[i].Min.x ? major_minX : boxes[i].Min.x; 
                            major_minY = major_minY < boxes[i].Min.y ? major_minY : boxes[i].Min.y; 
                            major_minZ = major_minZ < boxes[i].Min.z ? major_minZ : boxes[i].Min.z; 
                            major_maxX = major_maxX > boxes[i].Max.x ? major_maxX : boxes[i].Max.x; 
                            major_maxY = major_maxY > boxes[i].Max.y ? major_maxY : boxes[i].Max.y; 
                            major_maxZ = major_maxZ > boxes[i].Max.z ? major_maxZ : boxes[i].Max.z; 
                            majorCounter ++;
                        }
                    }
                }

      
                if(majorCounter == 0){
                    major_minX = 0; major_minY = 0;major_minZ = 0;
                    major_maxX = 0; major_maxY = 0;major_maxZ = 0;
                }

                
                for(int i = 0;i<boxes.Length;i++){

                    if( !(major_minX <= boxes[i].Min.x && major_maxX >= boxes[i].Max.x &&
                          major_minY <= boxes[i].Min.y && major_maxY >= boxes[i].Max.y &&
                          major_minZ <= boxes[i].Min.z && major_maxZ >= boxes[i].Max.z)){

                        left.Add(boxes[i]);

                    } else {

                        right.Add(boxes[i]);
                    }
                }

                if(!side){

                    List<Box> tmpLeft = new List<Box>();
                    tmpLeft = left;
                    left = right;
                    right = tmpLeft;
                }

            }
        }
    }

}
