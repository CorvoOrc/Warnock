/* Steshenko Alexander */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HiddenLineRemoval
{
    public partial class form : Form
    {
        List<Color> colors;
        int scale;

        public form()
        {
            InitializeComponent();
            colors = new List<Color>();
            scale = 1;
            CrateColorList();
        }

        void CrateColorList()
        {
            colors.Add(Color.Green);
            colors.Add(Color.DarkRed);
            colors.Add(Color.Blue);
            colors.Add(Color.Black);
            colors.Add(Color.Firebrick);
            colors.Add(Color.Brown);
            colors.Add(Color.DarkOrange);
        }

        private void build_Click(object sender, EventArgs e)
        {
            Position pos;

            if (radioButton1.Checked) { pos = Position.front; }
            else { pos = Position.back; }
            scale = Convert.ToInt32(ScalenumericUpDown.Value);

            // Parsing dfx file
            Parse myParse = new Parse();
            string result;
            try
            {
                result = myParse.RunParse();
            }
            catch(Exception err)
            {
                return;
            }
            List<Line> lines = myParse.GetLines();

            //Run algo
            Warnock warnock = new Warnock(ref lines, ref pos);

            List<int> _colors = new List<int>();
            float x_move = 0.0f, y_move = 0.0f;
            List<RectangleF> plot = warnock.See(ref _colors, ref x_move, ref y_move);

            //Draw rect
            Graphics graphics = panelDraw.CreateGraphics();
            for (int i = 0; i < plot.Count; ++i)
            {
                graphics.DrawRectangle(new Pen(new LinearGradientBrush(plot[i], colors[_colors[i] % colors.Count], colors[_colors[i] % colors.Count], LinearGradientMode.BackwardDiagonal)), (plot[i].X + x_move) * scale, (plot[i].Y + y_move) * scale, plot[i].Width * scale, plot[i].Height * scale);
                // another various of filling
                //graphics.FillRectangle(Brushes.Tomato, (plot[i].X + x_move), (plot[i].Y + y_move), plot[i].Width + x_move, plot[i].Height + y_move);
                //graphics.FillRectangle(new LinearGradientBrush(plot[i], colors[_colors[i] % colors.Count], colors[_colors[i] % colors.Count], LinearGradientMode.BackwardDiagonal), (plot[i].X + x_move) * scale, (plot[i].Y + y_move) * scale, plot[i].Width * scale, plot[i].Height * scale);
            }
            graphics.Dispose();
        }
    }
}
