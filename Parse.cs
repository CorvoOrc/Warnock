/* Steshenko Alexander */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HiddenLineRemoval
{
    class Line { public Point p_a, p_b; public Line(Point a, Point b) { p_a = a; p_b = b; } public Line() { p_a = new Point(); p_b = new Point(); } }
    class Point { public double x, y, z; public Point(double _x, double _y, double _z) { x = _x; y = _y; z = _z; } public Point() { x = 0.0; y = 0.0; z = 0.0; } }
    class Verge { public List<Line> lines; public Verge() { lines = new List<Line>(); } }
    class Win { public double X_L, X_R, Y_U, Y_D; public Win() { } public Win(double x_l, double x_r, double y_u, double y_d) { X_L = x_l; X_R = x_r; Y_U = y_u; Y_D = y_d;} }
    public enum Position { front, back, right, left, top, bottom }

    class Parse
    {
        const int MAX_BUFFER_SIZE = 2048;
        static Encoding enc;

        List<Line> lines;
        List<Point> points;

        string filename;

        public Parse() {
            enc = Encoding.Default;

            lines = new List<Line>();
            points = new List<Point>();
        }

        private string OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Application.StartupPath;
            dialog.Filter = "DXF файлы (*.dxf)|*.dxf";
            dialog.FilterIndex = 2;
            dialog.RestoreDirectory = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filename = dialog.FileName;
                return filename;
            }
            else return null;
        }

        private static string ReadFromBuffer(FileStream fStream)
        {
            Byte[] bytes = new Byte[MAX_BUFFER_SIZE];
            string output = String.Empty;
            Decoder decoder = enc.GetDecoder();

            while (fStream.Position < fStream.Length)
            {
                int nBytes = fStream.Read(bytes, 0, bytes.Length);
                int nChars = decoder.GetCharCount(bytes, 0, nBytes);
                char[] chars = new char[nChars];
                nChars = decoder.GetChars(bytes, 0, nBytes, chars, 0);
                output += new String(chars, 0, nChars);
            }

            fStream.Close();
            return output;
        }

        public string RunParse() {
            filename = OpenFile();

            if (filename == null) return "Error/incorrect filename";

            try
            {
                FileStream input = new FileStream(filename, FileMode.OpenOrCreate);
                string text = ReadFromBuffer(input);

                int pos_entities = text.IndexOf("ENTITIES");
                List<string> sym = text.Split('\n').ToList<string>();
                bool flag1 = false, flag2 = false, flag3 = false,
                flag4 = false, flag5 = false, flag6 = false;

                for (int i = 0; i < sym.Count; i++)
                {
                    for (int j = 0; j < sym[i].Length; j++)
                    {
                        if (sym[i][j] == '\r')
                            sym[i] = sym[i].Remove(j, 1);
                    }
                }

                for (int i = 0; i < sym.Count; i++)
                {
                    if (sym[i] == "LINE")
                    {

                        flag1 = false; flag2 = false; flag3 = false; flag4 = false; flag5 = false; flag6 = false;
                        Line tmp_line = new Line();
                        for (int k = i; k < i + 23; k++)
                        {
                            if (sym[k] == " 10")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_a.x = Convert.ToDouble(sym[k + 1]) + 100;
                                flag1 = true;
                            }
                            if (sym[k] == " 20")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_a.y = Convert.ToDouble(sym[k + 1]) + 100;
                                flag2 = true;
                            }
                            if (sym[k] == " 30")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_a.z = Convert.ToDouble(sym[k + 1]) + 100;
                                flag3 = true;
                            }
                            if (sym[k] == " 11")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_b.x = Convert.ToDouble(sym[k + 1]) + 100;
                                flag4 = true;
                            }
                            if (sym[k] == " 21")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_b.y = Convert.ToDouble(sym[k + 1]) + 100;
                                flag5 = true;
                            }
                            if (sym[k] == " 31")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_line.p_b.z = Convert.ToDouble(sym[k + 1]) + 100;
                                flag6 = true;
                            }
                        }
                        if (flag1 && flag2 && flag3 && flag4 && flag5 && flag6)
                        {
                            lines.Insert(0, tmp_line);
                        }
                    }

                    if (sym[i] == "AcDbPoint")
                    {

                        flag1 = false; flag2 = false; flag3 = false;
                        Point tmp_point = new Point();
                        for (int k = i; k < i + 23; k++)
                        {
                            if (sym[k] == " 10")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_point.x = Convert.ToDouble(sym[k + 1]);
                                flag1 = true;
                            }
                            if (sym[k] == " 20")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_point.y = Convert.ToDouble(sym[k + 1]);
                                flag2 = true;
                            }
                            if (sym[k] == " 30")
                            {
                                sym[k + 1] = sym[k + 1].Replace('.', ',');
                                tmp_point.z = Convert.ToDouble(sym[k + 1]);
                                flag3 = true;
                            }
                        }
                        if (flag1 && flag2 && flag3)
                        {
                            points.Insert(0, tmp_point);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("RunParse exception");
            }

            return "Ok";
        }

        public List<Line> GetLines() 
        {
            return lines;
        }
        public List<Point> GetPoints() 
        {
            return points;
        }
    }
}
