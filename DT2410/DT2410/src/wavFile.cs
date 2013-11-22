using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;
using System.Collections;
using PortAudioSharp;
using System.Runtime.InteropServices;


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

        private const int NUM_SAMPLES = 4096;
        private const int QUEUE_LENGTH = 20;

        public string filename;
        public int inputChannels;
        public int outputChannels;
        public int bitDepth;
        public int frameSize; //for 16 bit stereo, it would be 2 bytes * 2 channels = 4 bytes
        public int sampleRate;
        public long numOfSamples;
        public long numOfFrames;
        public long cSamplePos; // in bytes

        private WaveFileReader reader;
        private Queue sampleQueue;


        PortAudio.PaStreamCallbackDelegate callbackDelegate; //kept as a class variable so it doesn't get garbage collected

        /// <summary>
        /// - A public int to count the current sample we're up to so that we're able to 'fast forward' the audio by increasing the current sample count.
        /// - A sample queue, same as what I'm using now.
        /// </summary>

        //private file

        public wavFile(string filename)
        {
            this.filename = filename;
            reader = new WaveFileReader(this.filename);
            sampleQueue = new Queue();

            this.inputChannels = reader.WaveFormat.Channels;
            this.outputChannels = 2;
            this.bitDepth = reader.WaveFormat.BitsPerSample;
            this.frameSize = reader.WaveFormat.BlockAlign;
            this.sampleRate = reader.WaveFormat.SampleRate;
            this.numOfSamples = reader.SampleCount;
            this.numOfFrames = this.numOfSamples / this.frameSize;
            this.cSamplePos = 0;

            //Console.WriteLine("InputChannels: " + inputChannels);
            //Console.WriteLine("bitDepth: " + bitDepth);
            //Console.WriteLine("frameSize: " + frameSize);
            //Console.WriteLine("sampleRate: " + sampleRate);
            //Console.WriteLine("numOfSamples: " + numOfSamples);
            //Console.WriteLine("numOfFrames: " + numOfFrames);
            //System.Threading.Thread.Sleep(100000);
        }

        public void Play()
        {
            callbackDelegate = new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback);

            try
            {
                PortAudio.Pa_Initialize();

                IntPtr stream;
                IntPtr userdata = IntPtr.Zero; //intptr.zero is essentially just a null pointer

                uint sampleFormat = 0;
                switch(bitDepth)
                {
                    case 8:
                        sampleFormat = 16;
                        break;
                    case 16:
                        sampleFormat = 8;
                        break;
                    case 24:
                        sampleFormat = 4;
                        break;
                    case 32:
                        sampleFormat = 2;
                        break;
                    default:
                        Console.WriteLine("broken WAV");
                        break;
                        
                }
                //not sure why framesPerBuffer is so strange.
                PortAudio.Pa_OpenDefaultStream(out stream, inputChannels, outputChannels, sampleFormat,
                    sampleRate/outputChannels, (uint)(NUM_SAMPLES / (frameSize*2)), callbackDelegate, userdata);

                PortAudio.Pa_StartStream(stream);

                while (cSamplePos < reader.Length)
                {
                    
                    //This is essentially working as a circular/FIFO buffer, where new sample packets are only added to the queue
                    //if there's room in the queue. Once the packet is read out of the queue in the callback function it's 
                    //removed from the queue and there is room to add more info to the queue
                    if (sampleQueue.Count < QUEUE_LENGTH)
                    {
                        Console.WriteLine("Writing");

                        byte[] buffer = new byte[NUM_SAMPLES]; //buffer to read the wav raw bytes into
                        
                        int bytesRead = reader.Read(buffer, 0, NUM_SAMPLES); //read a block of bytes out from the wav

                        cSamplePos += bytesRead;
                        sampleQueue.Enqueue(buffer); //send the buffer to the queue
                    }

                }

                while (PortAudio.Pa_IsStreamActive(stream) != 0)
                {
                    Console.WriteLine("waiting"); //this loop never seems to gets entered, think my comparison may be the problem
                }

                PortAudio.Pa_StopStream(stream);

                Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {

            }
        }

        public PortAudio.PaStreamCallbackResult myPaStreamCallback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref PortAudio.PaStreamCallbackTimeInfo timeInfo,
            PortAudio.PaStreamCallbackFlags statusFlags,
            IntPtr userData)
        {
            Console.WriteLine("Reading");
            byte[] samplePacket;

            if (sampleQueue.Count == 0)
            {
                //this is likely a bad way of checking if the stream is complete as it could get in here if the read thread
                //falls behind but it works behind. 
                //todo: find a smarter way of knowing when the stream completes
                return PortAudio.PaStreamCallbackResult.paComplete;
            }

            samplePacket = (byte[])sampleQueue.Dequeue();
            
            //not sure about why the x2 at the end
            Marshal.Copy(samplePacket, 0, output, (int)(frameCount * (bitDepth / 8) * 2));

            return PortAudio.PaStreamCallbackResult.paContinue;
        }
    }
}


