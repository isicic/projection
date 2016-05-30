using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace crtanje_tijela
{
    class Program
    {

        struct poligon
        {
            public int a, b, c;
        }

        sealed class Window : GameWindow
        {
            List<poligon> poligoni;
            List<Vector4d> tocke;
            List<Vector4d> tockeN;
            Vector4d G, O;
            List<Vector4d> Bez;
            double A, B, C, D;
            int b = 0;
            Matrix4d T;
            Matrix4d P;
            bool skrivanje = false;
            public Window(List<Vector4d> tockeN, List<poligon> poligoni, List<Vector4d> Bez, Vector4d G)
            {
                this.poligoni = poligoni;
                this.tockeN = tockeN;
                this.G = G;
                this.Bez = Bez;
                tocke = new List<Vector4d>();

                O = Bez[0];
                T = TMatrix(O, G);
                P = PMatrix(O, G);
                Vector4d temp;
                foreach (Vector4d tocka in tockeN)
                {
                    temp = Vector4d.Transform(tocka, T);
                    temp = Vector4d.Transform(temp, P);
                    temp.X /= temp.W;
                    temp.Y /= temp.W;
                    temp.Z /= temp.W;
                    tocke.Add(temp);
                }
            }

            private Matrix4d TMatrix(Vector4d O, Vector4d G)
            {
                Matrix4d T1 = new Matrix4d(1, 0, 0, 0,
                                           0, 1, 0, 0,
                                           0, 0, 1, 0,
                                           -O.X, -O.Y, -O.Z, 1);

                Vector4d G1 = Vector4d.Transform(G, T1);

                double salfa = G1.Y / Math.Sqrt(Math.Pow(G1.X, 2) + Math.Pow(G1.Y, 2));
                double calfa = G1.X / Math.Sqrt(Math.Pow(G1.X, 2) + Math.Pow(G1.Y, 2));

                Matrix4d T2 = new Matrix4d(calfa, -salfa, 0, 0,
                                           salfa, calfa, 0, 0,
                                           0, 0, 1, 0,
                                           0, 0, 0, 1);

                Vector4d G2 = Vector4d.Transform(G1, T2);

                double sbeta = G2.X / Math.Sqrt(Math.Pow(G2.X, 2) + Math.Pow(G2.Z, 2));
                double cbeta = G2.Z / Math.Sqrt(Math.Pow(G2.X, 2) + Math.Pow(G2.Z, 2));

                Matrix4d T3 = new Matrix4d(cbeta, 0, sbeta, 0,
                                           0, 1, 0, 0,
                                           -sbeta, 0, cbeta, 0,
                                           0, 0, 0, 1);

                Vector4d G3 = Vector4d.Transform(G2, T3);

                Matrix4d T4 = new Matrix4d(0, -1, 0, 0,
                                           1, 0, 0, 0,
                                           0, 0, 1, 0,
                                           0, 0, 0, 1);

                Matrix4d T5 = new Matrix4d(-1, 0, 0, 0,
                                           0, 1, 0, 0,
                                           0, 0, 1, 0,
                                           0, 0, 0, 1);

                Matrix4d T = T1 * T2 * T3 * T4 * T5;
                return T;
            }

            private Matrix4d PMatrix(Vector4d O, Vector4d G)
            {
                double H = Math.Sqrt(Math.Pow(O.X - G.X, 2) + Math.Pow(O.Y - G.Y, 2) + Math.Pow(O.Z - G.Z, 2));

                Matrix4d P = new Matrix4d(1, 0, 0, 0,
                                          0, 1, 0, 0,
                                          0, 0, 0, 1 / H,
                                          0, 0, 0, 0);
                return P;
            }

            protected override void OnResize(EventArgs e)
            {
            }
            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                if (Keyboard[OpenTK.Input.Key.Escape])
                {
                    Exit();
                }

                if (Keyboard[OpenTK.Input.Key.S])
                {
                    skrivanje = !skrivanje;
                }
                b = (b + 1) % 500;
                O = Bez[b];
                T = TMatrix(O, G);
                P = PMatrix(O, G);
                tocke.Clear();
                Vector4d temp;
                foreach (Vector4d tocka in tockeN)
                {
                    temp = Vector4d.Transform(tocka, T);
                    temp = Vector4d.Transform(temp, P);
                    temp.X /= temp.W;
                    temp.Y /= temp.W;
                    temp.Z /= temp.W;
                    tocke.Add(temp);
                }

            }

            protected override void OnKeyDown(KeyboardKeyEventArgs e)
            {
                if (e.Key == Key.R)
                {
                }
            }
            protected override void OnMouseDown(MouseButtonEventArgs e)
            {
                if (e.Button == MouseButton.Left)
                {
                    Point p = e.Position;
                    p.Y = Height - p.Y;
                }
            }
            protected override void OnMouseMove(MouseMoveEventArgs e)
            {
            }
            protected override void OnRenderFrame(FrameEventArgs e)
            {
                GL.ClearColor(Color.Cyan);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.Color3(Color.Black);


                foreach (poligon p in poligoni)
                {
                    Vector4d a = tocke[p.a];
                    Vector4d b = tocke[p.b];
                    Vector4d c = tocke[p.c];

                    A = (b.Y - a.Y) * (c.Z - a.Z)
                    - (b.Z - a.Z) * (c.Y - a.Y);

                    B = -(b.X - a.X) * (c.Z - a.Z)
                        + (b.Z - a.Z) * (c.X - a.X);

                    C = (b.X - a.X) * (c.Y - a.Y)
                        - (b.Y - a.Y) * (c.X - a.X);

                    D = -a.X * A - a.Y * B - a.Z * C;

                    Vector3d V = new Vector3d();

                    V.X = (a.X + b.X + c.X) / 3;
                    V.Y = (a.Y + b.Y + c.Y) / 3;
                    V.Z = (a.Z + b.Z + c.Z) / 3;

                    if ((O.X - V.X ) * A + (O.Y - V.Y) * B + (O.Y - V.Z) * C > 0 || !skrivanje)
                    {
                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(a.X, a.Y, 0);
                        GL.Vertex3(b.X, b.Y, 0);
                        GL.End();

                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(b.X, b.Y, 0);
                        GL.Vertex3(c.X, c.Y, 0);
                        GL.End();

                        GL.Begin(PrimitiveType.Lines);
                        GL.Vertex3(c.X, c.Y, 0);
                        GL.Vertex3(a.X, a.Y, 0);
                        GL.End();
                    }
                }

                SwapBuffers();
                return;
            }
        }

        static void Main(string[] args)
        {
            Vector4d G;
            List<Vector3d> putanja = new List<Vector3d>();
            putanja.Add(new Vector3d(2, 4, 6));
            putanja.Add(new Vector3d(5, 8, 10));
            putanja.Add(new Vector3d(10, 10, 0));
            putanja.Add(new Vector3d(10, 10, -10));
            putanja.Add(new Vector3d(-5, -8, -6));
            putanja.Add(new Vector3d(2, 4, 6));

            try
            {
                Console.WriteLine("Objekti: \n");
                string[] files = Directory.GetFiles(@"C:\Users\Ivan\Downloads\Objekti");
                foreach (string s in files)
                    Console.WriteLine(s.Split('\\')[5]);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("no such file!");
                return;
            }

            Console.WriteLine("Unesi koordinate gledista: ");
            String[] glediste = Console.ReadLine().Split(' ');
            G = new Vector4d(double.Parse(glediste[0]),
                             double.Parse(glediste[1]),
                             double.Parse(glediste[2]),
                             1);

            Console.Write("\nUnesi ime željenog objekta: ");
            String ime = Console.ReadLine();
            string[] lines;
            List<poligon> poligoni = new List<poligon>();
            List<Vector4d> tocke = new List<Vector4d>();


            try
            {
                lines = File.ReadAllLines(@"C:\Users\Ivan\Downloads\Objekti\" + ime + ".obj");
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Ne postoji!");
                return;
            }
            Vector4d t;
            foreach (string s in lines)
            {
                if (s.Length > 0)
                    if (s[0] == 'v')
                    {
                        string[] temp = s.Split(' ');
                        t = new Vector4d(
                        double.Parse(temp[1], CultureInfo.InvariantCulture.NumberFormat),
                        double.Parse(temp[2], CultureInfo.InvariantCulture.NumberFormat),
                        double.Parse(temp[3], CultureInfo.InvariantCulture.NumberFormat),
                        1);
                        tocke.Add(t);
                    }
                    else
                    if (s[0] == 'f')
                    {
                        string[] temp = s.Split(' ');
                        poligon p = new poligon();
                        p.a = int.Parse(temp[1]) - 1;
                        p.b = int.Parse(temp[2]) - 1;
                        p.c = int.Parse(temp[3]) - 1;
                        poligoni.Add(p);
                    }
            }


            Window window = new Window(tocke, poligoni, Bezier(putanja), G);
            window.Run(60);
        }

        private static List<Vector4d> Bezier(List<Vector3d> tocke)
        {
            List<Vector4d> btocke = new List<Vector4d>();
            int n = tocke.Count - 1;

            for (double t = 0; t <= 1; t += 0.002)
            {
                double b = 0, x = 0, y = 0, z = 0;
                for (int i = 0; i < n + 1; i++)
                {
                    b = Binomial(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
                    x += tocke[i].X * b;
                    y += tocke[i].Y * b;
                    z += tocke[i].Z * b;
                }
                btocke.Add(new Vector4d(x, y, z, 1));
            }
            return btocke;
        }

        private static int Factorial(int n)
        {
            if (n < 2)
                return 1;
            return n * Factorial(n - 1);
        }

        private static int Binomial(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }
    }
}
