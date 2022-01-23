using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    public class Mesh 
    {
        public List<Triangle> Triangles;
        public List<Line> Lines;
        public Box box;

        public string name;

        public static Mesh NewEmptyMesh(){
            Mesh mesh = new Mesh();
            return mesh;
        }

        public Mesh NewMesh(List<Triangle> triangles, List<Line> lines){
            Mesh tmp = NewEmptyMesh();
            tmp.Triangles = triangles;
            tmp.Lines = lines;
            return tmp;
        }

        public static Mesh NewTriangleMesh(List<Triangle> triangles){
            Mesh tmp = NewEmptyMesh();
            tmp.Triangles = triangles;
            return tmp;
        }

        public Mesh NewLineMesh( List<Line> lines){
            Mesh tmp = NewEmptyMesh();
            tmp.Lines = lines;
            return tmp;
        }

        public void dirty(){
            this.box = null;
        }

        public Mesh DeepCopy(){
            Mesh copy = new Mesh();
            List<Triangle> tmpTriangles = new List<Triangle>();
            List<Line> tmpLines = new List<Line>();
            
    
            Box tmpbox = null;
            
            if(this.box != null){
                tmpbox = this.box.DeepCopy();
            }            


            for(int i = 0;i<Triangles.Count;i++){
                tmpTriangles.Add(Triangles[i].DeepCopy());
            }

            if(Lines != null){

                for(int i = 0;i<Lines.Count;i++){
                    tmpLines.Add(Lines[i].DeepCopy());
                }
            }

            copy.Triangles = tmpTriangles;
            copy.Lines     = tmpLines;
            copy.box       = tmpbox;
            copy.name      = name;
            return copy;    
        }

        public void Add(Mesh b){
            if(this.Triangles == null)
                this.Triangles = new List<Triangle>();
            
            if(this.Lines == null)
                this.Lines = new List<Line>();

            this.Triangles.AddRange(b.Triangles);
            this.Lines.AddRange(b.Lines);
            this.dirty();
        }


        public float Volume(){

            float v = 0;

            for(int i = 0;i<Triangles.Count;i++){

                Vector3 p1 = Triangles[i].V1.Position;
                Vector3 p2 = Triangles[i].V2.Position;
                Vector3 p3 = Triangles[i].V3.Position;
                v += p1.x*(p2.y*p3.z-p3.y*p2.z) - p2.x*(p1.y*p3.z-p3.y*p1.z) + p3.x*(p1.y*p2.z-p2.y*p1.z);
            }

            return Mathf.Abs(v/6);
        }

        public float SurfaceArea(){

            float a = 0;

            for(int i = 0;i<Triangles.Count;i++){
                a += Triangles[i].Area();
            }

            return a;
        }

        public Vector3 smoothNormalsThreshold(Vector3 normal, Vector3[] normals, float threshold){

            Vector3 result = Vector3.zero;
            
            for(int i = 0; i<normals.Length; i++){

                if( Vector3.Dot(normals[i], normal) > threshold ){
                    result = result.Add(normals[i]);
                }

            }

            return result.normalized;
        }   


        public void smoothNormalsThreshold(float radians){

            float threshold = (float)Math.Cos(radians);

            Dictionary<Vector3, List<Vector3>> lookup = new Dictionary<Vector3, List<Vector3>>();

            for(int i = 0;i<Triangles.Count;i++){

                if(lookup[Triangles[i].V1.Position] == null){
                    lookup[Triangles[i].V1.Position] = new List<Vector3>();
                }
                if(lookup[Triangles[i].V2.Position] == null){
                    lookup[Triangles[i].V2.Position] = new List<Vector3>();
                }

                if(lookup[Triangles[i].V3.Position] == null){
                    lookup[Triangles[i].V3.Position] = new List<Vector3>();
                }

                lookup[Triangles[i].V1.Position].Add(Triangles[i].V1.Normal);
                lookup[Triangles[i].V2.Position].Add(Triangles[i].V2.Normal);
                lookup[Triangles[i].V3.Position].Add(Triangles[i].V3.Normal);
            }

            for(int i = 0;i<Triangles.Count;i++){
                Triangles[i].V1.Normal = smoothNormalsThreshold(Triangles[i].V1.Normal, lookup[Triangles[i].V1.Position].ToArray(), threshold);   
                Triangles[i].V2.Normal = smoothNormalsThreshold(Triangles[i].V2.Normal, lookup[Triangles[i].V2.Position].ToArray(), threshold);   
                Triangles[i].V3.Normal = smoothNormalsThreshold(Triangles[i].V3.Normal, lookup[Triangles[i].V3.Position].ToArray(), threshold);   
            }

        }

        public void SmoothNormals(){
            Dictionary<Vector3, Vector3> lookup = new Dictionary<Vector3, Vector3>();

            for(int i = 0;i<Triangles.Count;i++){
                
                lookup[Triangles[i].V1.Position].Add(Triangles[i].V1.Normal);
                lookup[Triangles[i].V2.Position].Add(Triangles[i].V2.Normal);
                lookup[Triangles[i].V3.Position].Add(Triangles[i].V3.Normal);
            }

            foreach(KeyValuePair<Vector3, Vector3> entry in lookup)
            {
                lookup[entry.Key] = entry.Value.normalized;
            }

            for(int i = 0;i<Triangles.Count;i++){
                Triangles[i].V1.Normal = lookup[Triangles[i].V1.Position];
                Triangles[i].V2.Normal = lookup[Triangles[i].V2.Position];
                Triangles[i].V3.Normal = lookup[Triangles[i].V3.Position];
            }

        }

        public Matrix UnitCube(){
            float r = 0.5f;
            return FitInside(new Box(new Vector3(-r, -r, -r), new Vector3(r, r, r)), new Vector3(0.5f, 0.5f, 0.5f));

        }

        public Matrix BiUnitCube(){
            float r = 1.0f;
            return FitInside(new Box(new Vector3(-r, -r, -r), new Vector3(r, r, r)), new Vector3(0.5f, 0.5f, 0.5f));

        }

        public Matrix MoveTo(Vector3 position, Vector3 anchor){
            Matrix matrix = MatrixOP.Translate(position.Sub(this.BoundingBox().Anchor(anchor)));
            this.Transform(matrix);
            return matrix;
        }

        public Matrix Center(){
            return MoveTo(Vector3.zero, new Vector3(0.5f, 0.5f, 0.5f));
        }

        public Matrix FitInside(Box box, Vector3 anchor){

            float scale = box.Size().Div(BoundingBox().Size()).MinComponent();
            Vector3 extra = box.Size().Sub(BoundingBox().Size().MulScalar(scale));
            Matrix matrix = MatrixOP.Identity();

            matrix = matrix.Translate(BoundingBox().Min.Negate());
            matrix = matrix.Scale(new Vector3(scale, scale, scale));
            matrix = matrix.Translate(box.Min.Add(extra.Mul(anchor)));
            Transform(matrix);
            return matrix;
        }

        public Box BoundingBox(){

            if(box == null) {
                box = new Box();

                for(int i = 0;i<Triangles.Count;i++){
                    box = box.Extend(Triangles[i].BoundingBox());
                }

                if(Lines != null){
                    for(int i = 0;i<Lines.Count;i++){
                        box = box.Extend(Lines[i].BoundingBox());
                    }
                }
            }
            return box;
        }

        public void Transform(Matrix matrix){

            for(int i = 0;i<Triangles.Count;i++){
                Triangles[i].Transform(matrix);
            }

            if(Lines != null){

                for(int i = 0;i<Lines.Count;i++){
                    Lines[i].Transform(matrix);
                }
            }

            dirty();
        }

        public void ReverseWinding(){
            for(int i = 0;i<Triangles.Count;i++){
                Triangles[i].ReverseWinding();
            }
        }

        public static Mesh FromUnity(Vector3[] vertices, int[] triangles, string name = ""){

            List<Triangle> Ltri = new List<Triangle>();
            List<Line> Lline = new List<Line>();

            for(int i = 0 ; i< triangles.Length;i+=3){

                int i1 = triangles[i];
                int i2 = triangles[i+1];
                int i3 = triangles[i+2];

                Vertex V1 = Vertex.VertexFromPosition( toRepClasique( vertices[i1]) );
                Vertex V2 = Vertex.VertexFromPosition( toRepClasique( vertices[i2]) );
                Vertex V3 = Vertex.VertexFromPosition( toRepClasique( vertices[i3]) );
                Triangle tri  = new Triangle(V1, V2, V3);

                Ltri.Add(tri);

            }


            Mesh m = Mesh.NewTriangleMesh(Ltri);
            m.name = name;
            return m;
        }



        public System.Tuple<Vector3[], int[], string> toUnity(){

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            Dictionary<Vector3, int> coordIndex = new Dictionary<Vector3, int>(); 
        
            int counter = 0;
            for(int i = 0;i<Triangles.Count;i++){

                    if(!coordIndex.ContainsKey(Triangles[i].V1.Position)){

                        coordIndex[Triangles[i].V1.Position] = counter;
                        counter ++;
                    }

                    if(!coordIndex.ContainsKey(Triangles[i].V2.Position)){

                        coordIndex[Triangles[i].V2.Position] = counter;
                        counter ++;
                    }
                    
                    if(!coordIndex.ContainsKey(Triangles[i].V3.Position)){

                        coordIndex[Triangles[i].V3.Position] = counter;
                        counter ++;
                    } 

            }

            for(int i = 0;i<coordIndex.Count;i++){
                vertices.Add(Vector3.zero);
            }


            foreach(KeyValuePair<Vector3, int> entry in coordIndex)
            {
                vertices[entry.Value] = toRepUnity(entry.Key);
            }

            for(int i = 0;i<Triangles.Count;i++){

                triangles.Add(coordIndex[Triangles[i].V1.Position]);
                triangles.Add(coordIndex[Triangles[i].V2.Position]);
                triangles.Add(coordIndex[Triangles[i].V3.Position]);
            }

            return new System.Tuple<Vector3[], int[], string>(vertices.ToArray(), triangles.ToArray(), name);    
        }

        public static Vector3 toRepClasique(Vector3 v){
		    return new Vector3(v.z, v.y, v.x);
        }


        public static Vector3 toRepUnity(Vector3 v){
            return new Vector3(v.z, v.y,v.x);
        }


    }
}
