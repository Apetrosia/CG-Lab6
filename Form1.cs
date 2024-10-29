using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.AxHost;

namespace CG_Lab
{
    public partial class Form1 : Form
    {
        private enum Operation { DrawCube = 0, Reflect_XY = 1, Reflect_YZ = 2, Reflect_XZ = 3, ScaleCenter = 4 }
        PolyHedron currPolyHedron = null;
        Graphics g;

        string currPlane = "XY";
        Pen defaultPen = new Pen(Color.Black, 3);

        public Form1()
        {
            InitializeComponent();

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            g = Graphics.FromImage(pictureBox1.Image);

            comboBox1.Items.Add("Отображение на XY");
            comboBox1.Items.Add("Отображение на YZ");
            comboBox1.Items.Add("Отображение на XZ");

            comboBox1.SelectedIndex = 0;
        }

        private PointF ProjectParallel(Vertex v)
        {
            return new PointF(v.X, v.Y);
        }
       
        private void DrawPolyhedron(PolyHedron polyhedron, string plane)
        {
            float centerY = pictureBox1.Height / 2;

            

            foreach (var face in polyhedron.Faces)
            {
                var points = new List<PointF>();

                foreach (var vertexIndex in face.Vertices)
                {
                    Vertex v = polyhedron.Vertices[vertexIndex];
                    //points.Add(ProjectParallel(v));
                    PointF projectedPoint;
                    switch (plane)
                    {
                        case "XY":
                            projectedPoint = new PointF(v.X , v.Y );
                            break;
                        case "YZ":
                            projectedPoint = new PointF(v.Y , v.Z  + centerY);
                            break;
                        case "XZ":
                            projectedPoint = new PointF(v.X , v.Z + centerY);
                            break;
                        default:
                            throw new ArgumentException("Invalid plane. Use 'XY', 'YZ', or 'XZ'.");
                    }
                    points.Add(projectedPoint);
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
            numericUpDown1.Value = 1;
            g.Clear(Color.White);
            PolyHedron cube = PolyHedron.GetCube().Rotated(20, 20, 0)
                                                .Scaled(100, 100, 100)
                                                .Moved(pictureBox1.Width / 2, pictureBox1.Height / 2, 0);
            currPolyHedron = cube;
            switch (comboBox1.SelectedIndex)
            {
                case (int)Operation.DrawCube:
                    DrawPolyhedron(cube, "XY");
                    currPlane = "XY";
                    break;
                case (int)Operation.Reflect_XY:
                    DrawPolyhedron(cube, "XY");
                    currPlane = "XY";
                    break;
                case (int)Operation.Reflect_YZ:
                    DrawPolyhedron(cube, "YZ");
                    currPlane = "YZ";
                    break;
                case (int)Operation.Reflect_XZ:
                    DrawPolyhedron(cube, "XZ");
                    currPlane = "XZ";
                    break;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            float scaleFactor = (float)numericUpDown1.Value;
            g.Clear(Color.White);


            PolyHedron cube = currPolyHedron;
            

            cube = cube.ScaledAroundCenter(scaleFactor, scaleFactor, scaleFactor);
            DrawPolyhedron(cube, currPlane);
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
        }

        public class Face
        {
            public List<int> Vertices { get; private set; }

            public Face(List<int> vertices)
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

                cube.Faces.Add(new Face(new List<int> { 0, 1, 2, 3 }));
                cube.Faces.Add(new Face(new List<int> { 4, 5, 6, 7 }));
                cube.Faces.Add(new Face(new List<int> { 0, 1, 5, 4 }));
                cube.Faces.Add(new Face(new List<int> { 3, 2, 6, 7 }));
                cube.Faces.Add(new Face(new List<int> { 1, 2, 6, 5 }));
                cube.Faces.Add(new Face(new List<int> { 0, 3, 7, 4 }));

                return cube;
            }

            public PolyHedron Clone()
            {
                var newPoly = new PolyHedron(this.Faces, new List<Vertex>(this.Vertices));

                return newPoly;
            }
            public void FindCenter(List<Vertex> vertices, ref double a, ref double b, ref double c)
            {
                a = 0;
                b = 0;
                c = 0;
                foreach (var vertex in vertices)
                {
                    a += vertex.X;
                    b += vertex.Y;
                    c += vertex.Z;
                }

                a /= vertices.Count;
                b /= vertices.Count;
                c /= vertices.Count;
            }
            public PolyHedron Reflected(string plane)
            {
                var newPoly = this.Clone();

                Matrix<float> reflectionMatrix;

                switch (plane.ToUpper())
                {
                    case "XY":
                        reflectionMatrix = new float[4, 4]
                        {
                { 1,  0,  0,  0 },
                { 0,  1,  0,  0 },
                { 0,  0, -1,  0 },
                { 0,  0,  0,  1 }
                        };
                        break;

                    case "YZ":
                        reflectionMatrix = new float[4, 4]
                        {
                { -1,  0,  0,  0 },
                { 0,  1,  0,  0 },
                { 0,  0,  1,  0 },
                { 0,  0,  0,  1 }
                        };
                        break;

                    case "XZ":
                        reflectionMatrix = new float[4, 4]
                        {
                { 1,  0,  0,  0 },
                { 0, -1,  0,  0 },
                { 0,  0,  1,  0 },
                { 0,  0,  0,  1 }
                        };
                        break;

                    default:
                        throw new ArgumentException("Invalid plane. Use 'XY', 'YZ', or 'XZ'.");
                }

                // Применяем матрицу отражения ко всем вершинам многогранника
                for (int i = 0; i < newPoly.Vertices.Count; i++)
                {
                    newPoly.Vertices[i] *= reflectionMatrix;
                }

                return newPoly;
            }

            public PolyHedron ScaledAroundCenter(float scaleX, float scaleY, float scaleZ)
            {
                var newPoly = this.Clone();

                // Шаг 1: Находим центр многогранника
                double centerX = 0, centerY = 0, centerZ = 0;
                FindCenter(newPoly.Vertices, ref centerX, ref centerY, ref centerZ);

                // Шаг 2: Перемещаем многогранник так, чтобы его центр оказался в начале координат
                Matrix<float> moveToOriginMatrix = new float[4, 4]
                {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { (float)-centerX, (float)-centerY, (float)-centerZ, 1 }
                };

                for (int i = 0; i < newPoly.Vertices.Count; i++)
                {
                    newPoly.Vertices[i] *= moveToOriginMatrix;
                }

                // Шаг 3: Выполняем масштабирование
                Matrix<float> scalingMatrix = new float[4, 4]
                {
        { scaleX, 0, 0, 0 },
        { 0, scaleY, 0, 0 },
        { 0, 0, scaleZ, 0 },
        { 0, 0, 0, 1 }
                };

                for (int i = 0; i < newPoly.Vertices.Count; i++)
                {
                    newPoly.Vertices[i] *= scalingMatrix;
                }

                // Шаг 4: Перемещаем многогранник обратно в исходное положение
                Matrix<float> moveBackMatrix = new float[4, 4]
                {
        { 1, 0, 0, 0 },
        { 0, 1, 0, 0 },
        { 0, 0, 1, 0 },
        { (float)centerX, (float)centerY, (float)centerZ, 1 }
                };

                for (int i = 0; i < newPoly.Vertices.Count; i++)
                {
                    newPoly.Vertices[i] *= moveBackMatrix;
                }

                return newPoly;
            }    
        }   
}