using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;


namespace CCA
{
    public partial class FormCCA : Form
    {
        #region Fields
        CCA cca;
        Bitmap bitMapPartciles;
        Color colorParticles = Color.Blue;
        int sizeRect = 2;
        int snapshots = 0;
        Color[] colorArr;
        #endregion

        #region Methods

        void DrawBitMapParticles()
        {
            Graphics g = Graphics.FromImage(bitMapPartciles);
            g.Clear(Color.White);
            int L = cca.L;

            if (sizeRect == 1)
            {
                for (int i = 0; i < L; i++)
                    for (int j = 0; j < L; j++)
                        if (cca.site[i, j] != 0)
                        {
                            int s = (cca.site[i, j]) % colorArr.Length;
                            colorParticles = colorArr[s];
                            bitMapPartciles.SetPixel(i, j, colorParticles);
                        }

            }
            else
            {
                Rectangle rect;
                for (int i = 0; i < L; i++)
                    for (int j = 0; j < L; j++)
                        if (cca.site[i, j] != 0)
                        {
                            int s = (cca.site[i, j]) % colorArr.Length;
                            colorParticles = colorArr[s];
                            rect = new Rectangle(i * sizeRect, j * sizeRect, sizeRect, sizeRect);
                            g.FillRectangle(new SolidBrush(colorParticles), rect);
                        }
            }
            g.Dispose();

            bitMapPartciles.MakeTransparent(Color.White);
            
            
        }

        void Reset()
        {
            int N, L;
            
            try
            {
                L = int.Parse(textBox_L.Text); //Math.Min(pictureBox1.Height, pictureBox1.Width) / sizeRect;
                N = int.Parse(textBox_N.Text);
            }
            catch(FormatException e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            if (N > L * L - 2)
            {
                MessageBox.Show("The number of particle are very large exceed than sites of latice");
                return;
            }
            if (L <= 60) sizeRect = 4;
            else if (L <= 100) sizeRect = 3;
            else sizeRect = 1;

            colorArr = new Color[N+1];
            for (int i = 1; i <= N; i++)
            {
                int s = (i + 5) * (i + 1) + i + 1000;
                Random r = new Random(s);
                colorArr[i] = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));
            }



            pictureBox1.Size = new Size(L * sizeRect, L * sizeRect);
            groupBoxStruct.Left = pictureBox1.Right + 10;

            cca = new CCA(N, L);
            labelNumClusters.Text = cca.nClusters.ToString();
            bitMapPartciles = new Bitmap(L * sizeRect, L * sizeRect);
            
            DrawBitMapParticles();

            GC.Collect();
        }

        int CalculateTimerInterval()
        {
            int maxInterval = 600, minInterval = 10,
                min = trackBar_Latency.Minimum, max = trackBar_Latency.Maximum, value = trackBar_Latency.Value;


            return ((minInterval - maxInterval) * (value - min)) / (max - min) + maxInterval;
        }

        void Initialize()
        {
            timer1.Interval = CalculateTimerInterval();
            textBox_L.Text = "100";
            textBox_N.Text = "1000";
        }

        #endregion


        //************************************************************************************************

        public FormCCA()
        {
            InitializeComponent();
            Initialize();
            Reset();
            
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (buttonStart.Text == "Move")
            {
                buttonStart.Text = "Pause";
                timer1.Start();
            }
            else
            {
                buttonStart.Text = "Move";
                timer1.Stop();
            }
            
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Reset();
            pictureBox1.Invalidate();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(bitMapPartciles, 0, 0);
        }

        private void FormCCA_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1 != null && timer1.Enabled)
                timer1.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int steps = (int)numericUpDownIt.Value;
            for (int i = 0; i < steps; i++)
                cca.Move();
            labelNumClusters.Text = cca.nClusters.ToString();
            DrawBitMapParticles();
            pictureBox1.Invalidate();

        }

        private void trackBar_Latency_Scroll(object sender, EventArgs e)
        {
            timer1.Interval = CalculateTimerInterval();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = "Designed by Hor Dashti (h.dashti2@gmail.com)";
            MessageBox.Show(text);
        }

        private void buttonSnapshot_Click(object sender, EventArgs e)
        {
            //string folder = "Images_DLA(L=" + cca.L.ToString() + "N=" + cca.N.ToString() + ")";
            //Directory.CreateDirectory(folder);
            //string namefile = snapshots.ToString() + ".png";
            //string path = Path.Combine(folder, namefile);
            string namefile = snapshots.ToString() + ".png";
            bitMapPartciles.Save(namefile);
            snapshots++;
        }
    }
}