using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace PortAudioSharpTest
{
    public partial class Form2 : Form
    {
        public string tmpFilename;
        
        public delegate void updateMeter(int value);
        public updateMeter myDelegate;
        private wavRecorder rec = null;


        
        public Form2()
        {
            InitializeComponent();
            myDelegate = new updateMeter(updateDbMeter);
            progressBar1.Maximum = 100;
            progressBar1.Minimum = 0;
            tmpFilename = "tmp";
            tb_filename.Text = "";
        }

        private void ThreadFunction()
        {
            rec = new wavRecorder(tb_filename.Text + ".wav", this);
            rec.Record();
        }

        private void updateDbMeter(int value)
        {
            progressBar1.Value = value;
        }

        private void tb_filename_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }
        private void btn_record_Click(object sender, EventArgs e)
        {
            if (rec == null)
                ThreadFunction();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            if (rec != null)
                rec.Stop();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rec != null)
                rec.Stop();

            Thread.Sleep(50); //let the audio thread have a bit of time to die
        }

    }
}
