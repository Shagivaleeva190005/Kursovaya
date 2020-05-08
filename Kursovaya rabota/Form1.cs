using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursovaya_rabota
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        class Point
        {
            public double m;
            public double l;
            public double fi;
            public double T;
            public double dt;
            public double dT;

            static public Point Parse(string str)
            {
                Point point = new Point();

                string[] st = str.Split(' ');

                point.m = Double.Parse(st[0]);
                point.l = Double.Parse(st[1]);
                point.fi = Double.Parse(st[2]);
                point.T = Double.Parse(st[3]);
                point.dt = Double.Parse(st[4]); 
                point.dT = Double.Parse(st[5]);

                return point;
            }
        }

        private void button_russchitat_Click(object sender, EventArgs e)
        {
           rasschitat(Double.Parse(textBoxm.Text),
                   Double.Parse(textBoxl.Text),
                   Double.Parse(textBoxfi.Text),
                   Double.Parse(textBoxT.Text),
                   Double.Parse(textBoxdt.Text),
                   Double.Parse(textBoxdT1.Text));
        }
    }
}
