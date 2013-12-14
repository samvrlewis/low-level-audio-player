using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Collections;
using PortAudioSharp;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace PortAudioSharpTest
{
    /// <summary>
    /// Wave file class
    /// 
    /// I'm thinking that maybe this can encapsulate all file IO and buffering that we may need to do
    /// so that all that we need to do to get more samples is call wavFile.read(n) to get n samples.
    /// </summary>
    class wavRecorder
    {


        Random newRand = new Random();
        private const int NUM_SAMPLES = 4096/8;
        private const int QUEUE_LENGTH = 40;
        private bool stop_flag = false;

        public string filename;
        public int inputChannels;
        public int outputChannels;
        public int bitDepth;
        public int frameSize; //for 16 bit stereo, it would be 2 bytes * 2 channels = 4 bytes
        public int sampleRate;
        public int cSamplePos; // in bytes
        public int mSamplePos;
        public int mNumSeconds;

        private WaveFileWriter writer;
        private Queue sampleQueue;
        private Form1 form; //we'll have to make a new form

        private IntPtr stream;


        PortAudio.PaStreamCallbackDelegate callbackDelegate; //kept as a class variable so it doesn't get garbage collected

        /// <summary>
        /// - A public int to count the current sample we're up to so that we're able to 'fast forward' the audio by increasing the current sample count.
        /// - A sample queue, same as what I'm using now.
        /// </summary>

        //private file

        public wavRecorder(string filename, Form1 form)
        {
            this.filename = filename;
            this.sampleRate = 44100;
            this.bitDepth = 16;
            this.inputChannels = 1;
            this.outputChannels = 2;
            
            this.cSamplePos = 0;
            this.mNumSeconds = 10; //won't record longer than this
            this.mSamplePos = sampleRate * mNumSeconds;

            writer = new WaveFileWriter(filename, new WaveFormat(sampleRate, bitDepth, outputChannels));
            sampleQueue = new Queue();
        }

        public void Stop()
        {
            stop_flag = true;
        }
        
        private void record_loop()
        {
            while (cSamplePos < mSamplePos)
            {
                if (stop_flag)
                {
                    break;
                }

                if (sampleQueue.Count > 0)
                {

                    SoundPacket packet = (SoundPacket)sampleQueue.Dequeue();        
                    form.Invoke(form.myDelegate, new object[] {packet.averageDB});
                    
                    writer.Write(packet.samples, cSamplePos, NUM_SAMPLES);

                    cSamplePos += NUM_SAMPLES; // i think?
                }

            }

            PortAudio.Pa_StopStream(stream);

        }

        public void Record()
        {
            IntPtr userdata = IntPtr.Zero; //intptr.zero is essentially just a null pointer
            callbackDelegate = new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback);
            PortAudio.Pa_Initialize();

            uint sampleFormat = 0;
            
            //not sure why framesPerBuffer is so strange.
            PortAudio.Pa_OpenDefaultStream(out stream, inputChannels, outputChannels, sampleFormat,
                sampleRate / outputChannels, (uint)(NUM_SAMPLES / (frameSize * 2)), callbackDelegate, userdata);

            PortAudio.Pa_StartStream(stream);

            Thread myThread = new Thread(new ThreadStart(record_loop));
            myThread.Start();
        }

        public PortAudio.PaStreamCallbackResult myPaStreamCallback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref PortAudio.PaStreamCallbackTimeInfo timeInfo,
            PortAudio.PaStreamCallbackFlags statusFlags,
            IntPtr userData)
        {

            if (stop_flag)
            {
                return PortAudio.PaStreamCallbackResult.paComplete;
            }

            byte[] buffer = new byte[NUM_SAMPLES]; //buffer to read the raw bytes into
            Marshal.Copy(input, buffer, 0, (int)(frameCount * (bitDepth / 8) * 2)); //this might be something else, depending on mic?
            SoundPacket packet = new SoundPacket(buffer);
            sampleQueue.Enqueue(packet); //send the buffer to the queue
            
            return PortAudio.PaStreamCallbackResult.paContinue;
        }

    }
}


