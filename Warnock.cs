/* Steshenko Alexander */

/*
This is my implementation Warnock algorithm.
Warnock algorithm is a hidden surface algorithm 
used in computer graphics. 

It solves the problem of rendering a complicated image 
by recursive subdivision of a scene until areas 
are obtained that are trivial to compute.

Complex: O(n*p), where n is the number of polygons 
and p is the number of pixels in the viewport 

For more information:
http://www.cs.sun.ac.za/~lvzijl/courses/rw778/grafika/OpenGLtuts/Big/graphicsnotes009.html
*/

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
    class Warnock
    {
        List<Line> lines;
        List<Verge> verges;
        List<RectangleF> rectWin;
        Stack<Win> stacktWin;
        Position pos;

        public Warnock(ref List<Line> _lines, ref Position _pos) 
        {
            pos = _pos;
            lines = new List<Line>(_lines);
            verges = new List<Verge>();
            rectWin = new List<RectangleF>();
            stacktWin = new Stack<Win>();
        }

        public List<RectangleF> See(ref List<int> colors, ref float x_move, ref float y_move)
        {
            //TestFillLine(lines);

            verges = GetVerges(ref lines);
            colors.Clear();

            Win win = GetWin(ref lines, ref x_move, ref y_move);
            stacktWin.Push(win);

            while (stacktWin.Count > 0)
            {
                Win tmp = new Win();
                tmp = stacktWin.Peek();

                List<int> list_cover = new List<int>();
                List<int> list_out = new List<int>();
                List<int> list_part = new List<int>();

                if (tmp.X_R - tmp.X_L < 0.01 && tmp.Y_U - tmp.Y_D < 0.01) 
                { 
                    stacktWin.Pop();
                    continue; 
                }

                stacktWin.Pop();

				// check all rectangles on outside to window (borderwindow)
                for (int i = 0; i < verges.Count; ++i) 
                {
                    double x_max = 0.0, x_min = 0.0, y_max = 0.0, y_min = 0.0;
                    GetBorderVerge(ref x_max, ref x_min, ref y_max, ref y_min, verges[i]);

                    if (x_max >= tmp.X_R && x_min <= tmp.X_L && y_max >= tmp.Y_U && y_min <= tmp.Y_D)
                    {
                        list_cover.Add(i); 
                    }
                    else if (x_min >= tmp.X_R || x_max <= tmp.X_L || y_min >= tmp.Y_U || y_max <= tmp.Y_D)
                    { 
                        list_out.Add(i);
                    }
                    else 
                    { 
                        list_part.Add(i); 
                    }
                }

                if (list_part.Count > 0) 
                {
                    stacktWin.Push(new Win(tmp.X_L + (tmp.X_R - tmp.X_L) / 2, tmp.X_R, tmp.Y_U, tmp.Y_D + (tmp.Y_U - tmp.Y_D) / 2));
                    stacktWin.Push(new Win(tmp.X_L, tmp.X_L + (tmp.X_R - tmp.X_L) / 2, tmp.Y_U, tmp.Y_D + (tmp.Y_U - tmp.Y_D) / 2));
                    stacktWin.Push(new Win(tmp.X_L, tmp.X_L + (tmp.X_R - tmp.X_L) / 2, tmp.Y_D + (tmp.Y_U - tmp.Y_D) / 2, tmp.Y_D));
                    stacktWin.Push(new Win(tmp.X_L + (tmp.X_R - tmp.X_L) / 2, tmp.X_R, tmp.Y_D + (tmp.Y_U - tmp.Y_D) / 2, tmp.Y_D));
                }
                else if (list_cover.Count > 1 && list_part.Count == 0)
                {
                    // sort
                    double Zmax = 0.0;

                    if (pos == Position.front) { Zmax = 10000.0; }
                    else if (pos == Position.back) { Zmax = -10000.0; }
                    int index = -1;

                    for (int i = 0; i < list_cover.Count; ++i)
                    {
                        if (pos == Position.front) 
                        { 
                            if (verges[list_cover[i]].lines[0].p_a.z < Zmax) 
                            { 
                                Zmax = verges[list_cover[i]].lines[0].p_a.z;
                                index = i;
                            } 
                        }
                        else if (pos == Position.back)
                        { 
                            if (verges[list_cover[i]].lines[0].p_a.z > Zmax)
                            { 
                                Zmax = verges[list_cover[i]].lines[0].p_a.z;
                                index = i; 
                            } 
                        }
                    }

                    if (index == -1) { continue; }

                    rectWin.Add(new RectangleF((float)tmp.X_L, (float)tmp.Y_D, (float)(tmp.X_R - tmp.X_L), (float)(tmp.Y_U - tmp.Y_D)));
                    colors.Add(list_cover[index]);
                }
                else if (list_cover.Count == 1)
                {
                    rectWin.Add(new RectangleF((float)tmp.X_L, (float)tmp.Y_D, (float)(tmp.X_R - tmp.X_L), (float)(tmp.Y_U - tmp.Y_D)));
                    colors.Add(list_cover[0]);
                }
            }

            return rectWin;
        }

        private List<Verge> GetVerges(ref List<Line> lines) 
        {
            List<Verge> verges = new List<Verge>();
            List<bool> fg = new List<bool>(lines.Count);

            for (int i = 0; i < lines.Count; ++i) 
            { 
                fg.Add(false); 
            }

            Point prev_p = new Point();
            Point p = new Point();
            prev_p = lines[0].p_a;
            p = lines[0].p_b;
            fg[0] = true;

            verges.Add(new Verge());
            verges[verges.Count - 1].lines.Add(lines[0]);

            while (true)
            {
                do
                {
                    //bool set = false;
                    for (int i = 0; i < lines.Count; ++i)
                    {
                        if (!fg[i] && (p.x == lines[i].p_a.x && p.y == lines[i].p_a.y && p.z == lines[i].p_a.z))
                        {
                            verges[verges.Count - 1].lines.Add(lines[i]);
                            p = lines[i].p_b;
                            fg[i] = true;
                            //set = true;
                        }
                        else if (!fg[i] && (p.x == lines[i].p_b.x && p.y == lines[i].p_b.y && p.z == lines[i].p_b.z))
                        {
                            verges[verges.Count - 1].lines.Add(lines[i]);
                            p = lines[i].p_a;
                            fg[i] = true;
                            //set = true;
                        }
                    }

                    if (prev_p.x == p.x && prev_p.y == p.y && prev_p.z == p.z /*|| !set*/) { break; }
                }
                while (true);

                bool controlle = true;
                int index = 0;
                for (int i = 0; i < fg.Count; ++i) 
                { 
                    if (!fg[i]) 
                    { 
                        controlle = false; 
                        index = i;
                        break; 
                    }
                }

                if (controlle) { break; }

                verges.Add(new Verge());
                verges[verges.Count - 1].lines.Add(lines[index]);

                p = lines[index].p_b;
                prev_p = lines[index].p_a;
                fg[index] = true;
            }

            return verges;
        }

        private Win GetWin(ref List<Line> lines, ref float x_move, ref float y_move)
        {
            double x_l = 100000.0, x_r = -100000.0, y_u = -100000.0, y_d = 100000.0;
            foreach (var line in lines) 
            { 
                if (line.p_a.x > x_r) { x_r = line.p_a.x; }
                if (line.p_a.x < x_l) { x_l = line.p_a.x; }
                if (line.p_b.x > x_r) { x_r = line.p_b.x; }
                if (line.p_b.x < x_l) { x_l = line.p_b.x; }

                if (line.p_a.y > y_u) { y_u = line.p_a.y; }
                if (line.p_a.y < y_d) { y_d = line.p_a.y; }
                if (line.p_b.y > y_u) { y_u = line.p_b.y; }
                if (line.p_b.y < y_d) { y_d = line.p_b.y; }
            }

            if (x_r - x_l > y_u - y_d)
            {
                y_u += Math.Abs(x_r - x_l - y_u + y_d); 
            }
            else if (x_r - x_l < y_u - y_d)
            {
                x_r += Math.Abs(y_u - y_d - x_r + x_l);
            }

            if (x_l < 0) { x_move = Math.Abs((float)x_l); }
            if (y_d < 0) { y_move = Math.Abs((float)y_d); }
            
            return new Win(x_l, x_r, y_u, y_d);
        }

        private void GetBorderVerge(ref double x_max, ref double x_min, ref double y_max, ref double y_min, Verge verge)
        {
            x_max = -100000.0; x_min = 100000.0; y_max = -100000.0; y_min = 100000.0;
            foreach (var line in verge.lines) 
            { 
                if (line.p_a.x > x_max) { x_max = line.p_a.x; }
                if (line.p_a.x < x_min) { x_min = line.p_a.x; }
                if (line.p_b.x > x_max) { x_max = line.p_b.x; }
                if (line.p_b.x < x_min) { x_min = line.p_b.x; }

                if (line.p_a.y > y_max) { y_max = line.p_a.y; }
                if (line.p_a.y < y_min) { y_min = line.p_a.y; }
                if (line.p_b.y > y_max) { y_max = line.p_b.y; }
                if (line.p_b.y < y_min) { y_min = line.p_b.y; }
            }
        }
        private void TestFillLine(List<Line> _lines)
        {
            lines.Clear();

            // first seria
            lines.Add(new Line(new Point(0.0, 0.0, 5.0), new Point(0.0, 100.0, 5.0)));
            lines.Add(new Line(new Point(0.0, 100.0, 5.0), new Point(100.0, 100.0, 5.0)));
            lines.Add(new Line(new Point(100.0, 100.0, 5.0), new Point(100.0, 0.0, 5.0)));
            lines.Add(new Line(new Point(100.0, 0.0, 5.0), new Point(0.0, 0.0, 5.0)));

            lines.Add(new Line(new Point(5.0, 5.0, -10.0), new Point(5.0, 25.0, -10.0)));
            lines.Add(new Line(new Point(5.0, 25.0, -10.0), new Point(25.0, 25.0, -10.0)));
            lines.Add(new Line(new Point(25.0, 25.0, -10.0), new Point(25.0, 5.0, -10.0)));
            lines.Add(new Line(new Point(25.0, 5.0, -10.0), new Point(5.0, 5.0, -10.0)));

            lines.Add(new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 25.0, 0.0)));
            lines.Add(new Line(new Point(0.0, 25.0, 0.0), new Point(25.0, 25.0, 0.0)));
            lines.Add(new Line(new Point(25.0, 25.0, 0.0), new Point(25.0, 0.0, 0.0)));
            lines.Add(new Line(new Point(25.0, 0.0, 0.0), new Point(0.0, 0.0, 0.0)));

            lines.Add(new Line(new Point(-10.0, 40.0, 0.0), new Point(-10, 60, 0.0)));
            lines.Add(new Line(new Point(-10, 60, 0.0), new Point(10, 60, 0.0)));
            lines.Add(new Line(new Point(10, 60, 0.0), new Point(10.0, 40.0, 0.0)));
            lines.Add(new Line(new Point(10.0, 40.0, 0.0), new Point(-10.0, 40.0, 0.0)));

            lines.Add(new Line(new Point(40.0, 40.0, 0.0), new Point(160.0, 40.0, 0.0)));
            lines.Add(new Line(new Point(160.0, 40, 0.0), new Point(160.0, 20.0, 0.0)));
            lines.Add(new Line(new Point(160.0, 20.0, 0.0), new Point(40.0, 20.0, 0.0)));
            lines.Add(new Line(new Point(40.0, 20.0, 0.0), new Point(40.0, 40.0, 0.0)));

            // second seria
            /*lines.Add(new Line(new Point(0.0, 0.0, 5.0), new Point(0.0, 50.0, 5.0)));
            lines.Add(new Line(new Point(0.0, 50.0, 5.0), new Point(60.0, 50.0, 5.0)));
            lines.Add(new Line(new Point(60.0, 50.0, 5.0), new Point(60.0, 0.0, 5.0)));
            lines.Add(new Line(new Point(60.0, 0.0, 5.0), new Point(0.0, 0.0, 5.0)));

            lines.Add(new Line(new Point(20.0, 20.0, 10.0), new Point(20.0, 40.0, 10.0)));
            lines.Add(new Line(new Point(20.0, 40.0, 10.0), new Point(40.0, 40.0, 10.0)));
            lines.Add(new Line(new Point(40.0, 40.0, 10.0), new Point(40.0, 20.0, 10.0)));
            lines.Add(new Line(new Point(40.0, 20.0, 10.0), new Point(20.0, 20.0, 10.0)));

            lines.Add(new Line(new Point(10.0, 10.0, 0.0), new Point(10.0, 60.0, 0.0)));
            lines.Add(new Line(new Point(10.0, 60.0, 0.0), new Point(20.0, 60.0, 0.0)));
            lines.Add(new Line(new Point(20.0, 60.0, 0.0), new Point(20.0, 40.0, 0.0)));
            lines.Add(new Line(new Point(20.0, 40.0, 0.0), new Point(10.0, 10.0, 0.0)));*/

            // third seria
            /*lines.Add(new Line(new Point(0.0, 0.0, 5.0), new Point(0.0, -100.0, 5.0)));
            lines.Add(new Line(new Point(0.0, -100.0, 5.0), new Point(-100.0, -100.0, 5.0)));
            lines.Add(new Line(new Point(-100.0, -100.0, 5.0), new Point(-100.0, 0.0, 5.0)));
            lines.Add(new Line(new Point(-100.0, 0.0, 5.0), new Point(0.0, 0.0, 5.0)));

            lines.Add(new Line(new Point(0.0, 0.0, 0.0), new Point(0.0, 100.0, 0.0)));
            lines.Add(new Line(new Point(0.0, 100.0, 0.0), new Point(100.0, 100.0, 0.0)));
            lines.Add(new Line(new Point(100.0, 100.0, 0.0), new Point(100.0, 0.0, 0.0)));
            lines.Add(new Line(new Point(100.0, 0.0, 0.0), new Point(0.0, 0.0, 0.0)));*/
        }
    }
}
