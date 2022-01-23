using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static packing3D.MatrixOP;

namespace packing3D{
    public class Box
    {

        public Vector3 Min;
        public Vector3 Max;
        

        public Box(){
            this.Min = Vector3.positiveInfinity; 
            this.Max = Vector3.negativeInfinity; 
        }

        public Box(Vector3 min , Vector3 max){
            this.Min = min;
            this.Max = max;
        }

        public static Box BoxForBoxes(Box[] boxes){

            if(boxes.Length == 0){
                return new Box(Vector3.zero, Vector3.zero);
            }

            float x0 = boxes[0].Min.x;
            float y0 = boxes[0].Min.y;
            float z0 = boxes[0].Min.z;
            
            float x1 = boxes[0].Max.x;
            float y1 = boxes[0].Max.y;
            float z1 = boxes[0].Max.z;

            for(int i = 0;i<boxes.Length;i++) {
                x0 = Mathf.Min(x0, boxes[i].Min.x);
                y0 = Mathf.Min(y0, boxes[i].Min.y);
                z0 = Mathf.Min(z0, boxes[i].Min.z);

                x1 = Mathf.Max(x1, boxes[i].Max.x);
                y1 = Mathf.Max(y1, boxes[i].Max.y);
                z1 = Mathf.Max(z1, boxes[i].Max.z);
            }

            return new Box(new Vector3(x0, y0, z0),new Vector3(x1, y1, z1));
        }

        public float Volume(){
            return (this.Max.x - this.Min.x)*(this.Max.y - this.Min.y)*(this.Max.z - this.Min.z);

        }

 

        public Vector3 Anchor(Vector3 anchor){
            return this.Min.Add(this.Size().Mul(anchor));
        }

        public Vector3 Center(){
            return this.Anchor(new Vector3(0.5f, 0.5f, 0.5f));
        }

        public Vector3 Size(){
            return this.Max.Sub(this.Min);
        }

        public Box Extend(Box b){
            if(this.Max == Vector3.zero && this.Min == Vector3.zero){
                return b;
            }

            return new Box(this.Min.Min(b.Min), this.Max.Max(b.Max));
        }

        public Box ExtendForce(Box b){

            if(float.IsInfinity(this.Max.x) && float.IsInfinity(this.Max.y) && float.IsInfinity(this.Max.z) && 
               float.IsInfinity(this.Min.x) && float.IsInfinity(this.Min.y) && float.IsInfinity(this.Min.z)){

                return b;
            }
            
            if(float.IsInfinity(b.Max.x) && float.IsInfinity(b.Max.y) && float.IsInfinity(b.Max.z) && 
               float.IsInfinity(b.Min.x) && float.IsInfinity(b.Min.y) && float.IsInfinity(b.Min.z)){
                   
                return this;
            }

            return new Box(this.Min.Min(b.Min), this.Max.Max(b.Max));
        }

        public void ApplyExtend(float minX, float minY,  float minZ, 
                                float maxX, float maxY , float maxZ, 
                                Vector3 translation){
            
            if(this.Max == Vector3.zero && this.Min == Vector3.zero){
                return;
            }


            this.Min.x = Mathf.Min(this.Min.x, minX + translation.x );
            this.Min.y = Mathf.Min(this.Min.y, minY + translation.y );
            this.Min.z = Mathf.Min(this.Min.z, minZ + translation.z );

            this.Max.x = Mathf.Max(this.Max.x, maxX + translation.x );
            this.Max.y = Mathf.Max(this.Max.y, maxY + translation.y );
            this.Max.z = Mathf.Max(this.Max.z, maxZ + translation.z );
        }


        public Box Offset(float x)  {
            return new Box(this.Min.SubScalar(x), this.Max.AddScalar(x));
        }

        public Box Translate(Vector3 v){
            return new Box(this.Min.Add(v), this.Max.Add(v));
        }

        public void ApplyTranslate(Vector3 v){
            this.Min += v;
            this.Max += v;
        }
    
        public bool Contains(Vector3 b){
            return this.Min.x <= b.x && this.Max.x >= b.x &&
                this.Min.y <= b.y && this.Max.y >= b.y &&
                this.Min.z <= b.z && this.Max.z >= b.z;
        }
    

        public bool ContainsBox(Box b){
            return this.Min.x <= b.Min.x && this.Max.x >= b.Max.x &&
                   this.Min.y <= b.Min.y && this.Max.y >= b.Max.y &&
                   this.Min.z <= b.Min.z && this.Max.z >= b.Max.z;
        }

        public bool ContainsBox(Vector3 min, Vector3 max){
            return this.Min.x <= min.x && this.Max.x >= max.x &&
                   this.Min.y <= min.y && this.Max.y >= max.y &&
                   this.Min.z <= min.z && this.Max.z >= max.z;
        }

        public bool CanContainBoxWithRotation(Box b){

            float b_w = b.Max.x - b.Min.x;
            float b_d = b.Max.y - b.Min.y;
            float b_h = b.Max.z - b.Min.z;
           
            float this_w = this.Max.x - this.Min.x;
            float this_d = this.Max.y - this.Min.y;
            float this_h = this.Max.z - this.Min.z;

            List<float> b_dim = new List<float>{b_w,b_d,b_h};
            List<float> this_dim = new List<float>{this_w,this_d,this_h};

            b_dim.Sort();
            this_dim.Sort();

            return (b_dim[0] <= this_dim[0] && b_dim[1] <= this_dim[1] && b_dim[2] <= this_dim[2]);
        }

        public bool CanContainBoxWithoutRotation(Box b){

            float b_w = b.Max.x - b.Min.x;
            float b_d = b.Max.y - b.Min.y;
            float b_h = b.Max.z - b.Min.z;
           
            float this_w = this.Max.x - this.Min.x;
            float this_d = this.Max.y - this.Min.y;
            float this_h = this.Max.z - this.Min.z;

            List<float> b_dim = new List<float>{b_w,b_d,b_h};
            List<float> this_dim = new List<float>{this_w,this_d,this_h};

            return (b_dim[0] < this_dim[0] && b_dim[1] < this_dim[1] && b_dim[2] < this_dim[2]);
        }

        public bool Intersects(Box b){
            return !(this.Min.x > b.Max.x || this.Max.x < b.Min.x || this.Min.y > b.Max.y ||
                     this.Max.y < b.Min.y || this.Min.z > b.Max.z || this.Max.z < b.Min.z);
        }

        
        public Box Intersection(Box b){
            if(!this.Intersects(b)){
                return new Box(Vector3.zero, Vector3.zero);
            }

            Vector3 min = this.Min.Max(b.Min);
            Vector3 max = this.Max.Min(b.Max);
            min = min.Min(max);
            max = min.Max(max);

            return new Box(min, max);
        }

        public float IntersectionVolume(Box b){

            if(!this.Intersects(b)){
                return 0;
            }

            float minX = Math.Max(this.Min.x, b.Min.x);
            float minY = Math.Max(this.Min.y, b.Min.y);
            float minZ = Math.Max(this.Min.z, b.Min.z);

            float maxX = Math.Min(this.Max.x, b.Max.x);
            float maxY = Math.Min(this.Max.y, b.Max.y);
            float maxZ = Math.Min(this.Max.z, b.Max.z);

            minX = Math.Min(minX, maxX);
            minY = Math.Min(minY, maxY);
            minZ = Math.Min(minZ, maxZ);

            maxX = Math.Max(minX, maxX);
            maxY = Math.Max(minY, maxY);
            maxZ = Math.Max(minZ, maxZ);

            return ( (maxX-minX) * (maxY-minY) * (maxZ-minZ) );
        }


        public Box Transform(Matrix m){
            return m.MulBox(this);

        }

        public Box DeepCopy(){
            Box copy = new Box();
            copy.Min = this.Min;
            copy.Max = this.Max;
            return copy;
        }
        

    }
}
