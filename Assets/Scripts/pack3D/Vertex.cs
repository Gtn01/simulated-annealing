using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace packing3D{
    public class Vertex 
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Output; 

        public static Vertex VertexFromPosition(Vector3 position){
            Vertex v = new Vertex();
            v.Position = position;
            return v;
        }


        public bool Outside(){
            return this.Output.Outside();
        }

        public Vertex InterpolateVertexes(Vertex v1, Vertex v2, Vertex v3, Vector4 b){

            Vertex v = new Vertex();
            v.Position = InterpolateVectors(v1.Position, v2.Position, v3.Position, b);
            v.Normal   = InterpolateVectors(v1.Normal, v2.Normal, v3.Normal, b).normalized;
            v.Output   = InterpolateVectorWs(v1.Output, v2.Output, v3.Output, b);
            return v;

        }

        public float InterpolateFloats(float v1, float v2, float v3, Vector4 b){
            float n = 0.0f;
            n += v1 * b.x;
            n += v2 * b.y;
            n += v3 * b.z;
            return n * b.w;
        }

        public Vector3 InterpolateVectors(Vector3 v1, Vector3 v2, Vector3 v3, Vector4 b){
            Vector3 n = Vector3.zero;
            n = n.Add(v1.MulScalar(b.x));
            n = n.Add(v2.MulScalar(b.y));
            n = n.Add(v3.MulScalar(b.z));
            return n.MulScalar(b.w);

        }

        public Vector4 InterpolateVectorWs(Vector3 v1, Vector3 v2, Vector3 v3,  Vector4 b)  {
            Vector4 n = Vector4.zero;
            n = n.Add(v1.MulScalar(b.x));
            n = n.Add(v2.MulScalar(b.y));
            n = n.Add(v3.MulScalar(b.z));
            return n.MulScalar(b.w);
        }

        public Vector4 Barycentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p){
            Vector3 v0 = p2.Sub(p1);
            Vector3 v1 = p3.Sub(p1);
            Vector3 v2 = p.Sub(p1);

            float d00 = Vector3.Dot(v0,v0); 
            float d01 = Vector3.Dot(v0,v1);
            float d11 = Vector3.Dot(v1,v1);
            float d20 = Vector3.Dot(v2,v0);
            float d21 = Vector3.Dot(v2,v1);

            float d = d00*d11 - d01*d01;
            float v = (d11*d20 - d01*d21) / d;
            float w = (d00*d21 - d01*d20) / d;
            float u = 1 - v - w;
            
            return new Vector4(u,v,w,1);
        }

        public Vertex Copy(){
            Vertex copy = new Vertex();
            Vector3 tmpPosition = new Vector3(this.Position.x, this.Position.y, this.Position.z);
            Vector3 tmpNormal = new Vector3(this.Normal.x, this.Normal.y, this.Normal.z);
            Vector4 tmpOutput = new Vector4(this.Output.x, this.Output.y, this.Output.z, this.Output.w);

            copy.Position = tmpPosition;
            copy.Normal = tmpNormal;
            copy.Output = tmpOutput;
            return copy;
        }

    }
}
