using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace packing3D{
    public static class Vector3Extensions 
    {

        public static Vector3 Perpendicular(this Vector3 a){

            if(a.x == 0 && a.y == 0){

                if(a.z == 0) {
                    return Vector3.zero;
                }
                return new Vector3(0,1,0);
            }

            return new Vector3(-a.y, a.x, 0).normalized;
        }

        public static Vector3 Sub(this Vector3 a, Vector3 b){
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3 Add(this Vector3 a, Vector3 b){
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 Mul(this Vector3 a, Vector3 b){
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 Div(this Vector3 a, Vector3 b){
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 Min(this Vector3 a, Vector3 b){
            return new Vector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));            
        }
        public static Vector3 Max(this Vector3 a, Vector3 b){
            return new Vector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
        }


        public static float MinComponent (this Vector3 a ){
            return Mathf.Min(Mathf.Min(a.x, a.y), a.z);
        }


        public static Vector3 AddScalar(this Vector3 a, float b){
            return new Vector3(a.x + b, a.y + b, a.z + b);
        }
        public static Vector3 SubScalar(this Vector3 a, float b){
            return new Vector3(a.x - b, a.y - b, a.z - b);
        }
        public static Vector3 MulScalar(this Vector3 a, float b){
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static bool IsDegenerate(this Vector3 a){
            bool nan = float.IsNaN(a.x) || float.IsNaN(a.y) || float.IsNaN(a.z);
            bool inf = float.IsInfinity(a.x) || float.IsInfinity(a.y) || float.IsInfinity(a.y);
            return nan || inf;
        }

        public static Vector3 Negate(this Vector3 a){
            return new Vector3(-a.x,-a.y, -a.z);
        }

    
    }







    public static class Vector4Extensions{
        public static bool Outside(this Vector4 a){
            float x = a.x;
            float y = a.y;
            float z = a.z;
            float w = a.w;
            return x < -w || x > w || y < -w || y > w || z < -w || z > w;
        }

        public static Vector4 Add(this Vector4 a, Vector4 b){
            return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4 MulScalar(this Vector4 a, float b){
            return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
        }

    }
}
