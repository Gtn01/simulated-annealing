using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace packing3D{
    
    public class MatrixOP
    {
    public struct Matrix
    {

        double X00; double X01; double X02; double X03;
        double X10; double X11; double X12; double X13;
        double X20; double X21; double X22; double X23;
        double X30; double X31; double X32; double X33;

            public Matrix(double v1, double v2, double v3, double v4, double v5, double v6, double v7, double v8, double v9, double v10, double v11, double v12, double v13, double v14, double v15, double v16) : this()
            {
                this.X00 = v1;
                this.X01 = v2;
                this.X02 = v3;
                this.X03 = v4;
                this.X10 = v5;
                this.X11 = v6;
                this.X12 = v7;
                this.X13 = v8;
                this.X20 = v9;
                this.X21 = v10;
                this.X22 = v11;
                this.X23 = v12;
                this.X30 = v13;
                this.X31 = v14;
                this.X32 = v15;
                this.X33 = v16;
            }

            public Matrix Translate(Vector3 v){
                return MatrixOP.Translate(v).Mul(this);
            }

            public Matrix Scale(Vector3 v){
                return MatrixOP.Scale(v).Mul(this);
            }

            public Matrix Rotate(Vector3 v, double a){
                return MatrixOP.Rotate(v, a).Mul(this);
            }

            public Matrix RotateTo(Vector3 a, Vector3 b){
                return MatrixOP.RotateTo(a,b).Mul(this);
            }

            public Matrix Frustum(double l, double r, double b , double t, double n, double f){
                return MatrixOP.Frustum(l, r, b, t, n, f).Mul(this);
            }

            public Matrix Orthographic(double l, double r, double b , double t, double n, double f){
                return MatrixOP.Orthographic(l, r, b, t, n, f).Mul(this);
            }

            public Matrix Perspective(double fovy, double aspect, double near, double far){
                return MatrixOP.Perspective(fovy, aspect, near, far).Mul(this);
            }

            public Matrix Viewport(double x, double y , double w, double h){
                return MatrixOP.Viewport(x, y, w, h).Mul(this);
            }

            public Matrix MulScalar(double b){
                return new Matrix(
                    this.X00 * b, this.X01 * b, this.X02 * b, this.X03 * b,
                    this.X10 * b, this.X11 * b, this.X12 * b, this.X13 * b,
                    this.X20 * b, this.X21 * b, this.X22 * b, this.X23 * b,
                    this.X30 * b, this.X31 * b, this.X32 * b, this.X33 * b
                );
            }

            public Matrix Mul(Matrix b){

                StringBuilder sb = new StringBuilder();

                Matrix m = new Matrix();
                m.X00 = this.X00*b.X00 + this.X01*b.X10 + this.X02*b.X20 + this.X03*b.X30;
                m.X10 = this.X10*b.X00 + this.X11*b.X10 + this.X12*b.X20 + this.X13*b.X30;
                m.X20 = this.X20*b.X00 + this.X21*b.X10 + this.X22*b.X20 + this.X23*b.X30;
                m.X30 = this.X30*b.X00 + this.X31*b.X10 + this.X32*b.X20 + this.X33*b.X30;
                m.X01 = this.X00*b.X01 + this.X01*b.X11 + this.X02*b.X21 + this.X03*b.X31;
                m.X11 = this.X10*b.X01 + this.X11*b.X11 + this.X12*b.X21 + this.X13*b.X31;
                m.X21 = this.X20*b.X01 + this.X21*b.X11 + this.X22*b.X21 + this.X23*b.X31;
                m.X31 = this.X30*b.X01 + this.X31*b.X11 + this.X32*b.X21 + this.X33*b.X31;
                m.X02 = this.X00*b.X02 + this.X01*b.X12 + this.X02*b.X22 + this.X03*b.X32;
                m.X12 = this.X10*b.X02 + this.X11*b.X12 + this.X12*b.X22 + this.X13*b.X32;
                m.X22 = this.X20*b.X02 + this.X21*b.X12 + this.X22*b.X22 + this.X23*b.X32;
                m.X32 = this.X30*b.X02 + this.X31*b.X12 + this.X32*b.X22 + this.X33*b.X32;
                m.X03 = this.X00*b.X03 + this.X01*b.X13 + this.X02*b.X23 + this.X03*b.X33;
                m.X13 = this.X10*b.X03 + this.X11*b.X13 + this.X12*b.X23 + this.X13*b.X33;
                m.X23 = this.X20*b.X03 + this.X21*b.X13 + this.X22*b.X23 + this.X23*b.X33;
                m.X33 = this.X30*b.X03 + this.X31*b.X13 + this.X32*b.X23 + this.X33*b.X33;
            
                return m;
            }


            public Vector3 MulPosition(Vector3 b){
                double x = this.X00*b.x + this.X01*b.y + this.X02*b.z + this.X03;
                double y = this.X10*b.x + this.X11*b.y + this.X12*b.z + this.X13;
                double z = this.X20*b.x + this.X21*b.y + this.X22*b.z + this.X23;
                return new Vector3((float)x, (float)y, (float)z);
            }

            public Vector4 MulPositionW(Vector3 b){
                double x = this.X00*b.x + this.X01*b.y + this.X02*b.z + this.X03;
                double y = this.X10*b.x + this.X11*b.y + this.X12*b.z + this.X13;
                double z = this.X20*b.x + this.X21*b.y + this.X22*b.z + this.X23;
                double w = this.X30*b.x + this.X31*b.y + this.X32*b.z + this.X33;
                return new Vector4((float)x, (float)y, (float)z, (float)w);
            }
            public Vector3 MulDirection(Vector3 b){

                double x = this.X00*b.x + this.X01*b.y + this.X02*b.z;
                double y = this.X10*b.x + this.X11*b.y + this.X12*b.z;
                double z = this.X20*b.x + this.X21*b.y + this.X22*b.z;
                return new Vector3((float)x, (float)y, (float)z).normalized;
            }

            public Box MulBox(Box box){
                
                Vector3 r = new Vector3((float)this.X00, (float)this.X10, (float)this.X20);
                Vector3 u = new Vector3((float)this.X01, (float)this.X11, (float)this.X21);
                Vector3 b = new Vector3((float)this.X02, (float)this.X12, (float)this.X22);
                Vector3 t = new Vector3((float)this.X03, (float)this.X13, (float)this.X23);

                Vector3 xa = r.MulScalar(box.Min.x);
                Vector3 xb = r.MulScalar(box.Max.x);

                Vector3 ya = u.MulScalar(box.Min.y);
                Vector3 yb = u.MulScalar(box.Max.y);
            
                Vector3 za = b.MulScalar(box.Min.z);
                Vector3 zb = b.MulScalar(box.Max.z);

                Vector3 tmp_xa = xa.Min(xb);
                Vector3 tmp_xb = xa.Max(xb);
                Vector3 tmp_ya = ya.Min(yb);
                Vector3 tmp_yb = ya.Max(yb);
                Vector3 tmp_za = za.Min(zb);
                Vector3 tmp_zb = za.Max(zb);

                xa = tmp_xa;
                xb = tmp_xb;
                ya = tmp_ya;
                yb = tmp_yb;
                za = tmp_za;
                zb = tmp_zb;
                
                Vector3 min = xa.Add(ya).Add(za).Add(t);
                Vector3 max = xb.Add(yb).Add(zb).Add(t);

                return new Box(min, max);
            }

            public Matrix Transpose(){
                return new Matrix(
                this.X00, this.X10, this.X20, this.X30,
                this.X01, this.X11, this.X21, this.X31,
                this.X02, this.X12, this.X22, this.X32,
                this.X03, this.X13, this.X23, this.X33);
            }
            

            public double Determinant(){

                return (this.X00*this.X11*this.X22*this.X33 - this.X00*this.X11*this.X23*this.X32 +
                        this.X00*this.X12*this.X23*this.X31 - this.X00*this.X12*this.X21*this.X33 +
                        this.X00*this.X13*this.X21*this.X32 - this.X00*this.X13*this.X22*this.X31 -
                        this.X01*this.X12*this.X23*this.X30 + this.X01*this.X12*this.X20*this.X33 -
                        this.X01*this.X13*this.X20*this.X32 + this.X01*this.X13*this.X22*this.X30 -
                        this.X01*this.X10*this.X22*this.X33 + this.X01*this.X10*this.X23*this.X32 +
                        this.X02*this.X13*this.X20*this.X31 - this.X02*this.X13*this.X21*this.X30 +
                        this.X02*this.X10*this.X21*this.X33 - this.X02*this.X10*this.X23*this.X31 +
                        this.X02*this.X11*this.X23*this.X30 - this.X02*this.X11*this.X20*this.X33 -
                        this.X03*this.X10*this.X21*this.X32 + this.X03*this.X10*this.X22*this.X31 -
                        this.X03*this.X11*this.X22*this.X30 + this.X03*this.X11*this.X20*this.X32 -
                        this.X03*this.X12*this.X20*this.X31 + this.X03*this.X12*this.X21*this.X30);
            }

            public Matrix Inverse(){

                Matrix m = new Matrix();
                double d = this.Determinant();
                m.X00 = (this.X12*this.X23*this.X31 - this.X13*this.X22*this.X31 + this.X13*this.X21*this.X32 - this.X11*this.X23*this.X32 - this.X12*this.X21*this.X33 + this.X11*this.X22*this.X33) / d;
                m.X01 = (this.X03*this.X22*this.X31 - this.X02*this.X23*this.X31 - this.X03*this.X21*this.X32 + this.X01*this.X23*this.X32 + this.X02*this.X21*this.X33 - this.X01*this.X22*this.X33) / d;
                m.X02 = (this.X02*this.X13*this.X31 - this.X03*this.X12*this.X31 + this.X03*this.X11*this.X32 - this.X01*this.X13*this.X32 - this.X02*this.X11*this.X33 + this.X01*this.X12*this.X33) / d;
                m.X03 = (this.X03*this.X12*this.X21 - this.X02*this.X13*this.X21 - this.X03*this.X11*this.X22 + this.X01*this.X13*this.X22 + this.X02*this.X11*this.X23 - this.X01*this.X12*this.X23) / d;
                m.X10 = (this.X13*this.X22*this.X30 - this.X12*this.X23*this.X30 - this.X13*this.X20*this.X32 + this.X10*this.X23*this.X32 + this.X12*this.X20*this.X33 - this.X10*this.X22*this.X33) / d;
                m.X11 = (this.X02*this.X23*this.X30 - this.X03*this.X22*this.X30 + this.X03*this.X20*this.X32 - this.X00*this.X23*this.X32 - this.X02*this.X20*this.X33 + this.X00*this.X22*this.X33) / d;
                m.X12 = (this.X03*this.X12*this.X30 - this.X02*this.X13*this.X30 - this.X03*this.X10*this.X32 + this.X00*this.X13*this.X32 + this.X02*this.X10*this.X33 - this.X00*this.X12*this.X33) / d;
                m.X13 = (this.X02*this.X13*this.X20 - this.X03*this.X12*this.X20 + this.X03*this.X10*this.X22 - this.X00*this.X13*this.X22 - this.X02*this.X10*this.X23 + this.X00*this.X12*this.X23) / d;
                m.X20 = (this.X11*this.X23*this.X30 - this.X13*this.X21*this.X30 + this.X13*this.X20*this.X31 - this.X10*this.X23*this.X31 - this.X11*this.X20*this.X33 + this.X10*this.X21*this.X33) / d;
                m.X21 = (this.X03*this.X21*this.X30 - this.X01*this.X23*this.X30 - this.X03*this.X20*this.X31 + this.X00*this.X23*this.X31 + this.X01*this.X20*this.X33 - this.X00*this.X21*this.X33) / d;
                m.X22 = (this.X01*this.X13*this.X30 - this.X03*this.X11*this.X30 + this.X03*this.X10*this.X31 - this.X00*this.X13*this.X31 - this.X01*this.X10*this.X33 + this.X00*this.X11*this.X33) / d;
                m.X23 = (this.X03*this.X11*this.X20 - this.X01*this.X13*this.X20 - this.X03*this.X10*this.X21 + this.X00*this.X13*this.X21 + this.X01*this.X10*this.X23 - this.X00*this.X11*this.X23) / d;
                m.X30 = (this.X12*this.X21*this.X30 - this.X11*this.X22*this.X30 - this.X12*this.X20*this.X31 + this.X10*this.X22*this.X31 + this.X11*this.X20*this.X32 - this.X10*this.X21*this.X32) / d;
                m.X31 = (this.X01*this.X22*this.X30 - this.X02*this.X21*this.X30 + this.X02*this.X20*this.X31 - this.X00*this.X22*this.X31 - this.X01*this.X20*this.X32 + this.X00*this.X21*this.X32) / d;
                m.X32 = (this.X02*this.X11*this.X30 - this.X01*this.X12*this.X30 - this.X02*this.X10*this.X31 + this.X00*this.X12*this.X31 + this.X01*this.X10*this.X32 - this.X00*this.X11*this.X32) / d;
                m.X33 = (this.X01*this.X12*this.X20 - this.X02*this.X11*this.X20 + this.X02*this.X10*this.X21 - this.X00*this.X12*this.X21 - this.X01*this.X10*this.X22 + this.X00*this.X11*this.X22) / d;
                return m;
            }

            public Matrix DeepCopy(){

                return new Matrix(
                this.X00, this.X01, this.X02, this.X03,
                this.X10, this.X11, this.X12, this.X13,
                this.X20, this.X21, this.X22, this.X23,
                this.X30, this.X31, this.X32, this.X33);

            }

            public string toString(){
                StringBuilder sb = new StringBuilder();
                
                sb.Append("{"+X00+",");
                sb.Append(X01+",");
                sb.Append(X02+",");
                sb.Append(X03+",");
                sb.Append(X10+",");
                sb.Append(X11+",");
                sb.Append(X12+",");
                sb.Append(X13+",");
                sb.Append(X20+",");
                sb.Append(X21+",");
                sb.Append(X22+",");
                sb.Append(X23+",");
                sb.Append(X30+",");
                sb.Append(X31+",");
                sb.Append(X32+",");
                sb.Append(X33+"}");

                return sb.ToString();
            }
        }

    public static Matrix Identity(){

            return new Matrix(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
    }

        public static Matrix Translate(Vector3 v){

            return new Matrix(
                1, 0, 0, v.x,
                0, 1, 0, v.y,
                0, 0, 1, v.z,
                0, 0, 0, 1);
        }


        public static Matrix Scale(Vector3 v){

            return new Matrix(
                v.x, 0, 0, 0,
                0, v.y, 0, 0,
                0, 0, v.z, 0,
                0, 0, 0, 1);
        }

        public static Matrix Rotate(Vector3 v, double a){

            v = v.normalized;
            double s = (double)Math.Sin(a);
            double c = (double)Math.Cos(a);
            double m = 1 - c;

            return new Matrix(
                m*v.x*v.x + c,
                m*v.x*v.y + v.z*s,
                m*v.z*v.x - v.y*s,
                0,
                m*v.x*v.y - v.z*s,
                m*v.y*v.y + c,
                m*v.y*v.z + v.x*s,
                0,
                m*v.z*v.x + v.y*s,
                m*v.y*v.z - v.x*s,
                m*v.z*v.z + c,
                0,
                0, 0, 0, 1);
        }

        public static Matrix RotateTo(Vector3 a , Vector3 b){

            double dot = Vector3.Dot(b,a);

            StringBuilder sb = new StringBuilder();
            
            if(dot == 1){
                return Identity();

            } else if( dot == -1) {
                return Rotate(a.Perpendicular(), Math.PI);

            } else {

                double angle = Math.Acos(dot);
                Vector3 v = Vector3.Cross(b, a).normalized; 

                return Rotate(v, angle);
            }
        }

        public static Matrix Orient(Vector3 position, Vector3 size, Vector3 up, double rotation){

            Matrix m = Rotate(new Vector3(0, 0, 1), rotation);
            m = m.Scale(size);
            m = m.RotateTo(new Vector3(0, 0, 1), up);
            m = m.Translate(position);
            return m;
        }

        public static Matrix Frustum(double l, double r, double b , double t, double n, double f){
            
            double t1 = 2 * n;
            double t2 = r - l;
            double t3 = t - b;
            double t4 = f - n;
            return new Matrix(
                t1 / t2, 0, (r + l) / t2, 0,
                0, t1 / t3, (t + b) / t3, 0,
                0, 0, (-f - n) / t4, (-t1 * f) / t4,
                0, 0, -1, 0);
        }

        public static Matrix Orthographic(double l, double r, double b , double t, double n, double f){

            return new Matrix(
                2 / (r - l), 0, 0, -(r + l) / (r - l),
                0, 2 / (t - b), 0, -(t + b) / (t - b),
                0, 0, -2 / (f - n), -(f + n) / (f - n),
                0, 0, 0, 1);
        }

        public static Matrix Perspective(double fovy, double aspect, double near, double far){
            double ymax = (double)(near * Math.Tan(fovy*Math.PI/360));
            double xmax = ymax * aspect;
            return Frustum(-xmax, xmax, -ymax, ymax, near, far);
        }

        public static Matrix LookAt(Vector3 eye, Vector3 center, Vector3 up ){
            Vector3 z = eye.Sub(center).normalized;
            Vector3 x = Vector3.Cross(up,z).normalized;
            Vector3 y = Vector3.Cross(z,x);
            
            return new Matrix(
                x.x, x.y, x.z, - Vector3.Dot(x,eye),
                y.x, y.y, y.z, - Vector3.Dot(y,eye),
                z.x, z.y, z.z, - Vector3.Dot(z,eye),
                0, 0, 0, 1
            );
        }

        public static Matrix LookAtDirection(Vector3 forward, Vector3 up){

            Vector3 z = forward.normalized;
            Vector3 x = Vector3.Cross(up,z).normalized;
            Vector3 y = Vector3.Cross(z, x);

            return new Matrix(
                x.x, x.y, x.z, 0,
                y.x, y.y, y.z, 0,
                z.x, z.y, z.z, 0,
                0, 0, 0, 1
            );
        }

        public static Matrix Screen(int w, int h){

            double w2 = (double)(w) / 2.0f;
            double h2 = (double)(h) / 2.0f;

            return new Matrix(
                w2, 0, 0, w2,
                0, -h2, 0, h2,
                0, 0, 0.5f, 0.5f,
                0, 0, 0, 1
            );
        }  

        public static Matrix Viewport(double x, double y , double w, double h){
            double l = x;
            double b = y;
            double r = x + w;
            double t = y + h;

            return new Matrix(
                (r - l) / 2, 0, 0, (r + l) / 2,
                0, (t - b) / 2, 0, (t + b) / 2,
                0, 0, 0.5f, 0.5f,
                0, 0, 0, 1
            );
        } 

    }

}
