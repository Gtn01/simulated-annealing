using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static packing3D.MatrixOP;


namespace packing3D{
    public class Util
    {
        /*------------
        * Optimisation pour faire des comparaisons plus rapides entre vecteur (a l'initialisation du model)
         ------------*/
         public static short Vector0 = 0; 
        public static short VectorX = 1; 
        public static short VectorY = 2; 
        public static short VectorZ = 3; 
        

        public static double Radians(float degrees){
            return (double)(degrees * Math.PI / 180.0f);
        }

        public static double Degrees(float radians){
            return (double)(radians * 180.0f / Math.PI);
        }

        public static Vector3 Axis(int i){
            if(i == 1){
                return Vector3.right;
            }
            else if(i == 2){
                return Vector3.up;

            }
            else if(i == 3){
                return Vector3.forward;

            }
            
            return Vector3.zero;    
        }


        public static Vector3 RandomUnitVector(System.Random rand){

            int i = 0;

            while(i < 10000){
                float x = RandTinyFloat(rand);
                float y = RandTinyFloat(rand);
                float z = RandTinyFloat(rand);
                
                if(x*x+y*y+z*z > 1){
                    return new Vector3(x, y, z).normalized;
                }

                i++;
            }

            return Vector3.zero;
        }

        public static Vector3 RandomVector(System.Random rand, Vector2 rangeX, Vector2 rangeY, Vector2 rangeZ){

            int i = 0;

            while(i < 10000){

                float x = RandTinyFloat(rand);
                float y = RandTinyFloat(rand);
                float z = RandTinyFloat(rand);
                
                if(x*x+y*y+z*z > 1){

                    x *= rangeX.y - rangeX.x;
                    x += rangeX.x;

                    
                    y *= rangeY.y - rangeY.x;
                    y += rangeY.x;

                    
                    z *= rangeZ.y - rangeZ.x;
                    z += rangeZ.x;
                    return new Vector3(x, y, z);
                }

                i++;
            }

            return Vector3.zero;
        }

        public static float randFloat(System.Random rand){
            double mantissa = (rand.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, rand.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        public static float RandTinyFloat(System.Random rand, float min , float max){
            
            float random = (float)rand.Next(0,10000);
            random /=10000.0f;
            random*=(max-min);
            return random +min;
        }

        public static float RandTinyFloat(System.Random rand){

            float random = (float)rand.Next(0,1000);
            random /=1000.0f;
            return random;
        }
        

        public static Mesh NewCube(){

            List<Vector3> v = new List<Vector3>{

                new Vector3(-1, -1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(-1, 1, -1),
                new Vector3(-1, 1, 1),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1),
                new Vector3(1, 1, -1),
                new Vector3(1, 1, 1)
            };

            Mesh mesh = Mesh.NewTriangleMesh(new List<Triangle>(){
                Triangle.NewTriangleForPoints(v[3], v[5], v[7]),
                Triangle.NewTriangleForPoints(v[5], v[3], v[1]),
                Triangle.NewTriangleForPoints(v[0], v[6], v[4]),
                Triangle.NewTriangleForPoints(v[6], v[0], v[2]),
                Triangle.NewTriangleForPoints(v[0], v[5], v[1]),
                Triangle.NewTriangleForPoints(v[5], v[0], v[4]),
                Triangle.NewTriangleForPoints(v[5], v[6], v[7]),
                Triangle.NewTriangleForPoints(v[6], v[5], v[4]),
                Triangle.NewTriangleForPoints(v[6], v[3], v[7]),
                Triangle.NewTriangleForPoints(v[3], v[6], v[2]),
                Triangle.NewTriangleForPoints(v[0], v[3], v[2]),
                Triangle.NewTriangleForPoints(v[3], v[0], v[1])
            });

            mesh.Transform( MatrixOP.Scale(new Vector3( 0.5f, 0.5f, 0.5f) ));
            return mesh;
        }

        public static Mesh NewCubeForBox(Box box) {

            Matrix m = MatrixOP.Translate(new Vector3(0.5f, 0.5f, 0.5f));
            m = m.Scale(box.Size());
            m = m.Translate(box.Min);
            Mesh cube = NewCube();
            cube.Transform(m);
            return cube;
        }

        public static void Recenter(ref UnityEngine.Mesh mesh){
            
            Vector3[] vertices = mesh.vertices;
            double cx = 0;
            double cy = 0;
            double cz = 0;

            foreach(Vector3 v in vertices){
                cx += v.x;
                cy += v.y;
                cz += v.z;
            }

            cx/=vertices.Length;
            cy/=vertices.Length;
            cz/=vertices.Length;

            Vector3 translation = new Vector3((float)cx, (float)cy, (float)cz);

            for(int i = 0;i<vertices.Length;i++){
                vertices[i] -= translation;
            }

            mesh.vertices = vertices;

        }

        public static void Recenter(Dictionary<string,GameObject> go_toPack, ref Camera cam){

            if(go_toPack == null || go_toPack.Count == 0){
                return;
            }

            Vector3 min = new Vector3();
            Vector3 max = new Vector3();

            foreach(KeyValuePair<string, GameObject> item in go_toPack){

                Vector3[] vertices = item.Value.GetComponent<MeshFilter>().mesh.vertices;

                foreach(Vector3 v in vertices){

                    min.x = Mathf.Min( min.x , v.x);
                    min.y = Mathf.Min( min.y , v.y);
                    min.z = Mathf.Min( min.z , v.z);

                    max.x = Mathf.Max( max.x , v.x);
                    max.y = Mathf.Max( max.y , v.y);
                    max.z = Mathf.Max( max.z , v.z);
                }
            }


            float cameraDistance = 2.0f; 
            Vector3 objectSizes = (max - min)*1.10f;
            float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
            float distance = cameraDistance * objectSize / cameraView; 
            distance += 0.5f * objectSize; 
            cam.transform.position = Vector3.zero - distance * cam.transform.forward;
        }
    }

}
