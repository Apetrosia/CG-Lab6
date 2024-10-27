using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CG_Lab
{
    public partial class Form1 : Form
    {
        private enum RenderingOp { DrawCube = 0, DrawTetrahedron, DrawOctahedron, DrawIcosahedron, DrawDodecahedron }

        private enum AffineOp { Move = 0, Scaling, Rotation }

        Graphics g;

        Pen defaultPen = new Pen(Color.Black, 3);

        PolyHedron currentPolyhedron;

        public Form1()
        {
            InitializeComponent();

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            g = Graphics.FromImage(pictureBox1.Image);

            comboBox1.SelectedIndex = 0;
        }

        private PointF ProjectParallel(Vertex v)
        {
            return new PointF(v.X, v.Y);
        }

        private void DrawPolyhedron(PolyHedron polyhedron)
        {
            foreach (var face in polyhedron.Faces)
            {
                var points = new List<PointF>();

                foreach (var vertexIndex in face.Vertices)
                {
                    Vertex v = polyhedron.Vertices[vertexIndex];
                    points.Add(ProjectParallel(v));
                }

                // g.FillPolygon(Brushes.Red, points.ToArray());

                g.DrawPolygon(defaultPen, points.ToArray());
                
            }

            pictureBox1.Invalidate();
        }

        ~Form1()
        {
            g.Dispose();
        }

        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            g.Clear(pictureBox1.BackColor);

            switch (comboBox1.SelectedIndex)
            {
                case (int)RenderingOp.DrawCube:
                    DrawPolyhedron(currentPolyhedron = PolyHedron.GetCube()
                                             .Rotated(20, 20, 0)
                                             .Scaled(100, 100, 100)
                                             .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0)
                                  );
                    break;
                case (int)RenderingOp.DrawTetrahedron:
                    DrawPolyhedron(currentPolyhedron = PolyHedron.GetTetrahedron()
                                             .Rotated(10, 10, 0)
                                             .Scaled(100, 100, 100)
                                             .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0)
                                  );
                    break;
                case (int)RenderingOp.DrawOctahedron:
                    DrawPolyhedron(currentPolyhedron = PolyHedron.GetOctahedron()
                                             .Rotated(20, 20, 0)
                                             .Scaled(100, 100, 100)
                                             .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0)
                                  );
                    break;
                case (int)RenderingOp.DrawIcosahedron:
                    DrawPolyhedron(currentPolyhedron = PolyHedron.GetIcosahedron()
                                             .Rotated(10, 10, 0)
                                             .Scaled(150, 150, 150)
                                             .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0)
                                  );
                    break;
                case (int)RenderingOp.DrawDodecahedron:
                    DrawPolyhedron(currentPolyhedron = PolyHedron.GetDodecahedron()
                                             .Rotated(10, 10, 0)
                                             .Scaled(200, 200, 200)
                                             .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0)
                                  );
                    break;                   
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            g.Clear(pictureBox1.BackColor);

            Vertex anchor;

            switch (comboBox2.SelectedIndex)
            {
                case (int)AffineOp.Move:
                    DrawPolyhedron(currentPolyhedron = currentPolyhedron
                                                       .Moved((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value));
                    break;
                case (int)AffineOp.Scaling:
                    anchor = new Vertex((float)numericUpDown4.Value, (float)numericUpDown5.Value, (float)numericUpDown6.Value);

                    DrawPolyhedron(currentPolyhedron = currentPolyhedron
                                                       .Moved(-anchor.X, -anchor.Y, -anchor.Z)
                                                       .Scaled((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value)
                                                       .Moved(anchor.X, anchor.Y, anchor.Z));

                    break;

                case (int)AffineOp.Rotation:
                    anchor = new Vertex((float)numericUpDown4.Value, (float)numericUpDown5.Value, (float)numericUpDown6.Value);

                    DrawPolyhedron(currentPolyhedron = currentPolyhedron
                                                       .Moved(-anchor.X, -anchor.Y, -anchor.Z)
                                                       .Rotated((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value)
                                                       .Moved(anchor.X, anchor.Y, anchor.Z));
                    break;
            }
        }


        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            numericUpDown4.Value = e.X;
            numericUpDown5.Value = e.Y;
        }
    }

    public class Matrix<T> where T : struct, IConvertible
    {
        public T[,] Values { get; }

        public Matrix(T[,] values)
        {
            Values = values;
        }

        public static implicit operator Matrix<T>(T[,] values)
        {
            return new Matrix<T>(values);
        }

        public static implicit operator Vertex(Matrix<T> m)
        {
            return new Vertex(Convert.ToSingle(m[0, 0]), Convert.ToSingle(m[0, 1]), Convert.ToSingle(m[0, 2]));
        }

        public Matrix(Vertex vertex)
        {
            Values = new T[1, 4] { 
                  { 
                    (T)Convert.ChangeType(vertex.X, typeof(T)),
                    (T)Convert.ChangeType(vertex.Y, typeof(T)),
                    (T)Convert.ChangeType(vertex.Z, typeof(T)),
                    (T)Convert.ChangeType(1, typeof(T)) 
                  } 
            };
        }

        public static implicit operator Matrix<T>(Vertex vertex)
        {
            return new Matrix<T>(vertex);
        }

        public static Matrix<T> operator *(Matrix<T> A, Matrix<T> B)
        {
            int rowsA = A.Values.GetLength(0);
            int colsA = A.Values.GetLength(1);
            int rowsB = B.Values.GetLength(0);
            int colsB = B.Values.GetLength(1);

            T[,] result = new T[rowsA, colsB];

            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    result[i, j] = default;
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i, j] += (dynamic)A.Values[i, k] * (dynamic)B.Values[k, j];
                    }
                }
            }

            return new Matrix<T>(result);
        }

        public T this[int row, int column]
        {
            get
            {
                return Values[row, column];
            }
            set
            {
                Values[row, column] = value;
            }
        }


    }

    public struct Vertex
    {
        public float X { get; private set; }
        
        public float Y { get; private set; }

        public float Z { get; private set; }

        public Vertex(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceTo(in Vertex other)
        {
            float xDelta = X - other.X;
            float yDelta = Y - other.Y;
            float zDelta = Z - other.Z;

            return (float)Math.Sqrt(xDelta * xDelta + yDelta * yDelta + zDelta * zDelta);
        }
    }

    public class Face
    {
        public int[] Vertices { get; private set; }

        public Face(params int[] vertices)
        {
            Vertices = vertices;
        }
    }

    public class PolyHedron
    {
        public List<Face> Faces { get; private set; }

        public List<Vertex> Vertices { get; private set; }

        public PolyHedron()
        {
            Faces = new List<Face>();
            Vertices = new List<Vertex>();
        }

        public PolyHedron(List<Face> faces, List<Vertex> vertices)
        {
            Faces = faces;
            Vertices = vertices;
        }

        public PolyHedron Scaled(float c1, float c2, float c3)
        {
            var newPoly = this.Clone();

            Matrix<float> translationMatrix = new float[4, 4]
            {
                { c1, 0,  0,  0 },
                { 0,  c2, 0,  0 },
                { 0,  0,  c3, 0 },
                { 0,  0,  0,  1 }
            };

            for (int i = 0; i < newPoly.Vertices.Count; i++)
            {
                newPoly.Vertices[i] *= translationMatrix;
            }

            return newPoly;
        }

        public PolyHedron Moved(float a, float b, float c)
        {
            var newPoly = this.Clone();

            Matrix<float> translationMatrix = new float[4, 4]
            {
                { 1,  0,  0,  0 },
                { 0,  1,  0,  0 },
                { 0,  0,  1,  0 },
                { a,  b,  c,  1 }
            };

            for (int i = 0; i < newPoly.Vertices.Count; i++)
            {
                newPoly.Vertices[i] *= translationMatrix;
            }

            return newPoly;
        }

        public PolyHedron RotatedXAxis(float alpha)
        {
            var newPoly = this.Clone();

            double angleRadians = (double)alpha * (Math.PI / 180);

            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);

            Matrix<float> translationMatrix = new float[4, 4]
            {
                { 1,  0,  0,  0 },
                { 0,  cos, sin,  0 },
                { 0,  -sin,  cos,  0 },
                { 0,  0,  0,  1 }
            };

            for (int i = 0; i < newPoly.Vertices.Count; i++)
            {
                newPoly.Vertices[i] *= translationMatrix;
            }

            return newPoly;
        }

        public PolyHedron RotatedYAxis(float alpha)
        {
            var newPoly = this.Clone();

            double angleRadians = (double)alpha * (Math.PI / 180);

            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);

            Matrix<float> translationMatrix = new float[4, 4]
            {
                { cos,  0,  -sin,  0 },
                { 0,  1, 0,  0 },
                { sin,  0,  cos,  0 },
                { 0,  0,  0,  1 }
            };

            for (int i = 0; i < newPoly.Vertices.Count; i++)
            {
                newPoly.Vertices[i] *= translationMatrix;
            }

            return newPoly;
        }

        public PolyHedron RotatedZAxis(float alpha)
        {
            var newPoly = this.Clone();

            double angleRadians = (double)alpha * (Math.PI / 180);

            float cos = (float)Math.Cos(angleRadians);
            float sin = (float)Math.Sin(angleRadians);

            Matrix<float> translationMatrix = new float[4, 4]
            {
                { cos,  sin,  0,  0 },
                { -sin, cos, 0,  0 },
                { 0,  0,  1,  0 },
                { 0,  0,  0,  1 }
            };

            for (int i = 0; i < newPoly.Vertices.Count; i++)
            {
                newPoly.Vertices[i] *= translationMatrix;
            }

            return newPoly;
        }

        public PolyHedron Rotated(float xAngle, float yAngle, float zAngle)
        {
            return this.Clone()
                .RotatedXAxis(xAngle)
                .RotatedYAxis(yAngle)
                .RotatedZAxis(zAngle);
        }

        public static PolyHedron GetCube()
        {
            var cube = new PolyHedron();

            cube.Vertices.Add(new Vertex(-1, -1, -1));
            cube.Vertices.Add(new Vertex(1, -1, -1));
            cube.Vertices.Add(new Vertex(1, 1, -1));
            cube.Vertices.Add(new Vertex(-1, 1, -1)); 
            cube.Vertices.Add(new Vertex(-1, -1, 1));  
            cube.Vertices.Add(new Vertex(1, -1, 1));  
            cube.Vertices.Add(new Vertex(1, 1, 1));   
            cube.Vertices.Add(new Vertex(-1, 1, 1)); 

            cube.Faces.Add(new Face( 0, 1, 2, 3 ));
            cube.Faces.Add(new Face( 4, 5, 6, 7 )); 
            cube.Faces.Add(new Face( 0, 1, 5, 4 ));
            cube.Faces.Add(new Face( 3, 2, 6, 7 )); 
            cube.Faces.Add(new Face( 1, 2, 6, 5 )); 
            cube.Faces.Add(new Face( 0, 3, 7, 4 ));

            return cube;
        }

        public static PolyHedron GetTetrahedron()
        {
            var tetra = new PolyHedron();

            tetra.Vertices.Add(new Vertex(-1, 1, -1));
            tetra.Vertices.Add(new Vertex(1, -1, -1));
            tetra.Vertices.Add(new Vertex(1, 1, 1));
            tetra.Vertices.Add(new Vertex(-1, -1, 1));

            tetra.Faces.Add(new Face(0, 1, 2));
            tetra.Faces.Add(new Face(0, 1, 3));
            tetra.Faces.Add(new Face(0, 2, 3));
            tetra.Faces.Add(new Face(1, 2, 3));

            return tetra;
        }

        public static PolyHedron GetOctahedron()
        {
            var cube = GetCube();

            var octa = new PolyHedron();

            foreach (Face face in cube.Faces)
            {
                octa.Vertices.Add(cube.GetFaceCenter(face));
            }

            var octaCenters = cube.Scaled(1 / 3f, 1 / 3f, 1 / 3f).Vertices;

            for (int i = 0; i < 8; i++)
            {
                var faceVertices = octa.Vertices.Select((v, ind) => (v, ind))
                                                .OrderBy(p => octaCenters[i].DistanceTo(in p.v))
                                                .Select(p => p.ind)
                                                .Take(3);

                octa.Faces.Add(new Face(faceVertices.ToArray()));
            }

            return octa;
        }

        public static PolyHedron GetIcosahedron()
        {
            var icosa = new PolyHedron();

            var verticesBottom = new List<(Vertex v, int number)>(5);

            var verticesTop = new List<(Vertex v, int number)>(5);

            double angle = -90;

            int number = 1;

            for (int i = 0; i < 5; i++)
            {
                var angleRadians = angle * (Math.PI / 180);

                verticesBottom.Add((new Vertex((float)Math.Cos(angleRadians), -0.5f, (float)Math.Sin(angleRadians)), number));

                angle += 72;

                number += 2;
            }

            angle = -54;

            number = 2;

            for (int i = 0; i < 5; i++)
            {
                var angleRadians = angle * (Math.PI / 180);

                verticesTop.Add((new Vertex((float)Math.Cos(angleRadians), 0.5f, (float)Math.Sin(angleRadians)), number));

                angle += 72;

                number += 2;
            }

            icosa.Vertices = verticesBottom.Concat(verticesTop).OrderBy(p => p.number).Select(p => p.v).ToList();

            for (int i = 1; i <= 8; i++)
            {
                icosa.Faces.Add(new Face(i - 1, i, i + 1));
            }

            icosa.Faces.Add(new Face(8, 9, 0));
            icosa.Faces.Add(new Face(9, 0, 1));

            icosa.Vertices.Add(new Vertex(0, -(float)Math.Sqrt(5) / 2, 0));
            icosa.Vertices.Add(new Vertex(0, (float)Math.Sqrt(5) / 2, 0));

            number = 1;

            for (int i = 0; i < 4; i++)
            {
                icosa.Faces.Add(new Face(10, number - 1, number + 1));

                number += 2;
            }

            icosa.Faces.Add(new Face(10, 8, 0));

            number = 2;

            for (int i = 0; i < 4; i++)
            {
                icosa.Faces.Add(new Face(11, number - 1, number + 1));

                number += 2;
            }

            icosa.Faces.Add(new Face(11, 9, 1));

            return icosa;
        }

        public static PolyHedron GetDodecahedron()
        {
            var icosa = GetIcosahedron();

            var dodeca = new PolyHedron();

            foreach (Face face in icosa.Faces)
            {
                dodeca.Vertices.Add(icosa.GetFaceCenter(face));
            }

            for (int i = 0; i < 12; i++)
            {
                var faceVertices = dodeca.Vertices.Select((v, ind) => (v, ind))
                                                .OrderBy(p => icosa.Vertices[i].DistanceTo(in p.v))
                                                .Select(p => p.ind)
                                                .Take(5);

                int first = faceVertices.First();

                var rest = faceVertices.Skip(1).Select(ind => (dodeca.Vertices[ind], ind)).OrderBy(p => dodeca.Vertices[first].DistanceTo(in p.Item1));

                var next = rest.First().ind;

                var lastTwo = rest.Skip(2).OrderBy(p => dodeca.Vertices[next].DistanceTo(in p.Item1));

                dodeca.Faces.Add(new Face(faceVertices.Take(1)
                                          .Concat(new int[1] { next })
                                          .Concat(lastTwo.Select(p => p.ind))
                                          .Concat(rest.Select(p => p.ind).Skip(1).Take(1))
                                          .ToArray()
                                 ));
            }

            return dodeca;
        }

        public Vertex GetFaceCenter(Face face)
        {
            float x = 0, y = 0, z = 0;
            
            foreach (int vertexIndex in face.Vertices)
            {
                x += Vertices[vertexIndex].X;
                y += Vertices[vertexIndex].Y;
                z += Vertices[vertexIndex].Z;
            }
            
            return new Vertex(x / face.Vertices.Length, y / face.Vertices.Length, z / face.Vertices.Length);
        }

        public PolyHedron Clone()
        {
            var newPoly = new PolyHedron(this.Faces, new List<Vertex>(this.Vertices));

            return newPoly;
        }
    }
}
