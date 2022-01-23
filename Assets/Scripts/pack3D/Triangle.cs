using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    
    public class Triangle 
    {
        public Vertex V1;
        public Vertex V2;
        public Vertex V3;

        public Triangle(Vertex V1, Vertex V2, Vertex V3){
        
            this.V1 = V1;
            this.V2 = V2;
            this.V3 = V3;

        }

        public static Triangle NewTriangle(Vertex V1, Vertex V2, Vertex V3){
            Triangle t = new Triangle(V1,V2,V3);
            t.FixNormals();
            return t;
        }

        public static Triangle NewTriangleForPoints(Vector3 p1, Vector3 p2, Vector3 p3){
            Vertex v1 = new Vertex();
            v1.Position = p1;
            Vertex v2 = new Vertex();
            v2.Position = p2;
            Vertex v3 = new Vertex();
            v3.Position = p3;
            return NewTriangle(v1, v2, v3);
        }

        public bool IsDegenerate(){
            Vector3 p1 = this.V1.Position;
            Vector3 p2 = this.V2.Position;
            Vector3 p3 = this.V3.Position;

            if(p1 == p2 || p1 == p3 || p2 == p3){
                return true;
            }

            if(p1.IsDegenerate() || p2.IsDegenerate() || p3.IsDegenerate() ){
                return true;
            }
            return false;
        }

        public Vector3 Normal(){
            Vector3 e1 = this.V2.Position.Sub(this.V1.Position);
            Vector3 e2 = this.V3.Position.Sub(this.V1.Position);
            return Vector3.Cross(e1, e2).normalized;
        }

        public float Area(){
            Vector3 e1 = this.V2.Position.Sub(this.V1.Position);
            Vector3 e2 = this.V3.Position.Sub(this.V1.Position);
            Vector3 n = Vector3.Cross(e1, e2);
            
            return n.magnitude / 2.0f;
        }

        public void FixNormals(){
            Vector3 n = this.Normal();
            Vector3 zero = Vector3.zero;

            if(this.V1.Normal == zero) {
                this.V1.Normal = n;
            }
            if(this.V2.Normal == zero) {
                this.V2.Normal = n;
            }
            if(this.V3.Normal == zero){
                this.V3.Normal = n;
            }
        }

        public Box BoundingBox(){

            Vector3 min = (this.V1.Position.Min(this.V2.Position)).Min(this.V3.Position);
            Vector3 max = (this.V1.Position.Max(this.V2.Position)).Max(this.V3.Position);
            return new Box(min, max);
        }

        public void Transform(Matrix matrix){

            this.V1.Position = matrix.MulPosition(this.V1.Position);
            this.V2.Position = matrix.MulPosition(this.V2.Position);
            this.V3.Position = matrix.MulPosition(this.V3.Position);
            this.V1.Normal   = matrix.MulDirection(this.V1.Normal);
            this.V2.Normal   = matrix.MulDirection(this.V2.Normal);
            this.V3.Normal   = matrix.MulDirection(this.V3.Normal);
        }

        public void ReverseWinding(){
            this.V1 = this.V3;
            this.V2 = this.V2;
            this.V3 = this.V1; 

            this.V1.Normal = this.V1.Normal.Negate();
            this.V2.Normal = this.V2.Normal.Negate();
            this.V3.Normal = this.V3.Normal.Negate();
        }

        public Triangle DeepCopy(){
            Triangle copy = new Triangle(this.V1.Copy(), this.V2.Copy(), this.V3.Copy());
            return copy;

        }
    }
}
