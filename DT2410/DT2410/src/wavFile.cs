using System;
using System.Collections.Generic;
using System.Text;

namespace PortAudioSharpTest
{
    /// <summary>
    /// Wave file class
    /// 
    /// I'm thinking that maybe this can encapsulate all file IO and buffering that we may need to do
    /// so that all that we need to do to get more samples is call wavFile.read(n) to get n samples.
    /// </summary>
    class wavFile
    {
        public string filename;
        public int channels;
        public int bitDepth;
        public int chunkSize;
        public int sampleRate;

        private float[] buffer = new float[512];
        private int readHead = 0;
        //private file

        public wavFile(string filename)
        {

        }

        public float[] read(int samples)
        {
            List<float> output = new List<float>();

            return output.ToArray();

        }
    }
}
