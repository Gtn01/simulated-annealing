using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    public class Line 
    {
        Vertex V1;
        Vertex V2;

        public static Line NewLine(Vertex v1, Vertex v2){
            Line tmp = new Line();
            tmp.V1 = v1;
            tmp.V2 = v2;
            return tmp;
        }

        public Line newLineForPoints(Vector3 p1, Vector3 p2){
            Vertex v1 = new Vertex();
            v1.Position = p1;
            Vertex v2 = new Vertex();
            v2.Position = p2;
            return NewLine(v1, v2);
        }

        public Box BoundingBox(){
            Vector3 min = this.V1.Position.Min(this.V2.Position);
            Vector3 max = this.V1.Position.Max(this.V2.Position);
            return new Box(min,max);
        }

        public void Transform(Matrix matrix){
            this.V1.Position = matrix.MulPosition(this.V1.Position);
            this.V2.Position = matrix.MulPosition(this.V2.Position);
            this.V1.Normal   = matrix.MulDirection(this.V1.Normal);
            this.V2.Normal   = matrix.MulDirection(this.V2.Normal);
        }

        public Line DeepCopy(){
            Line copy = new Line();
            copy.V1 = V1.Copy();
            copy.V2 = V2.Copy();
            return copy;
        }
    }
}
