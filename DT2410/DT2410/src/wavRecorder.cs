﻿using System;
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
        private Form2 form; //we'll have to make a new form

        private IntPtr stream;


        PortAudio.PaStreamCallbackDelegate callbackDelegate; //kept as a class variable so it doesn't get garbage collected

        /// <summary>
        /// - A public int to count the current sample we're up to so that we're able to 'fast forward' the audio by increasing the current sample count.
        /// - A sample queue, same as what I'm using now.
        /// </summary>

        //private file

        public wavRecorder(string filename, Form2 form)
        {
            this.filename = filename;
            this.sampleRate = 44100;
            this.bitDepth = 16;
            this.inputChannels = 1;
            this.outputChannels = 1;
            this.frameSize = 2;
            this.form = form;
            
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

                    byte[] packet = (byte[])sampleQueue.Dequeue();  
      
                    SoundPacket p = new SoundPacket(packet);
                    form.Invoke(form.myDelegate, new object[] {p.averageDB});
                    
                    writer.Write(packet, 0, packet.Length);
                    //writer.Write()
                   
                }

            }

            writer.Close();
            
           
            PortAudio.Pa_StopStream(stream);

        }

        public void Record()
        {
            IntPtr userdata = IntPtr.Zero; //intptr.zero is essentially just a null pointer
            callbackDelegate = new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback);
            PortAudio.Pa_Initialize();

            uint sampleFormat = 8;
            
            //not sure why framesPerBuffer is so strange.

            //PortAudio.PaError error = PortAudio.Pa_OpenDefaultStream(out stream, inputChannels, outputChannels, sampleFormat,
                //sampleRate / outputChannels, (uint)(NUM_SAMPLES / (frameSize * 2)), callbackDelegate, userdata);

            //Console.WriteLine(error.ToString());

       

            PortAudio.PaStreamParameters outputparams = new PortAudio.PaStreamParameters();

            outputparams.channelCount = 1;
            outputparams.sampleFormat = PortAudio.PaSampleFormat.paInt16;
            outputparams.device = PortAudio.Pa_GetDefaultInputDevice();
            outputparams.suggestedLatency = PortAudio.Pa_GetDeviceInfo(outputparams.device).defaultLowInputLatency;
            outputparams.hostApiSpecificStreamInfo = IntPtr.Zero;


            PortAudio.PaStreamParameters a = new PortAudio.PaStreamParameters();

            a.channelCount = 1;
            a.sampleFormat = PortAudio.PaSampleFormat.paInt16;
            a.device = PortAudio.Pa_GetDefaultOutputDevice();
            a.suggestedLatency = PortAudio.Pa_GetDeviceInfo(a.device).defaultLowOutputLatency;
            a.hostApiSpecificStreamInfo = IntPtr.Zero;

           
            PortAudio.PaError error =  PortAudio.Pa_OpenStream(out stream, ref outputparams, ref a, 44100, 
                (uint)(NUM_SAMPLES / (frameSize)), PortAudio.PaStreamFlags.paClipOff, callbackDelegate, IntPtr.Zero );

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
            Marshal.Copy(input, buffer, 0, (int)(NUM_SAMPLES / (frameSize))); //this might be something else, depending on mic?
            //SoundPacket packet = new SoundPacket(buffer);

            //form.Invoke(form.myDelegate, new object[] { packet.averageDB });

            //Console.WriteLine("in the callback");

            sampleQueue.Enqueue(buffer); //send the buffer to the queue
            
            return PortAudio.PaStreamCallbackResult.paContinue;
        }

    }
}


