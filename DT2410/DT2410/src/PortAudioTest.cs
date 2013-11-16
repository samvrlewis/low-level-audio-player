using System;
using System.Runtime.InteropServices;

using PortAudioSharp;
using System.IO;
using NAudio.Wave;
using System.Collections;

namespace PortAudioSharpTest
{

    public class PortAudioTest
    {
        private const int NUM_SAMPLES = 2048;
        private const int QUEUE_LENGTH = 20;

        short[] sampleBuffer = new short[NUM_SAMPLES];
        Queue sampleQueue = new Queue();

        PortAudio.PaStreamCallbackDelegate callbackDelegate; //kept as a class variable so it doesn't get garbage collected

        public PortAudioTest()
        { }

        
        public PortAudio.PaStreamCallbackResult myPaStreamCallback(
            IntPtr input,
            IntPtr output,
            uint frameCount,
            ref PortAudio.PaStreamCallbackTimeInfo timeInfo,
            PortAudio.PaStreamCallbackFlags statusFlags,
            IntPtr userData)
        {
            Console.WriteLine("Reading");
            short[] samplePacket;

            if (sampleQueue.Count == 0)
            {
                //this is likely a bad way of checking if the stream is complete as it could get in here if the read thread
                //falls behind but it works behind. 
                //todo: find a smarter way of knowing when the stream completes
                return PortAudio.PaStreamCallbackResult.paComplete;
            }

            samplePacket = (short[])sampleQueue.Dequeue();
            Marshal.Copy(samplePacket, 0, output, (int)frameCount * 2);

            return PortAudio.PaStreamCallbackResult.paContinue;
        }

        public void Run()
        {
            string rootPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
            WaveFileReader reader = new WaveFileReader(rootPath + "/wave_files/jazzblues.wav");
            callbackDelegate = new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback); 

            int readerPosition = 0;
            byte[] buffer = new byte[NUM_SAMPLES * 2]; //buffer to read the wav raw bytes into
            int read = 0;

            try
            {
                PortAudio.Pa_Initialize();

                IntPtr stream;
                IntPtr userdata = IntPtr.Zero; //intptr.zero is essentially just a null pointer

                //for some reason .wavs seem to have half the sample rate that they should, i'm not sure quite why
                //but we should find out
                PortAudio.Pa_OpenDefaultStream(out stream, 1, 2, 8,
                    reader.WaveFormat.SampleRate/2, NUM_SAMPLES/2, callbackDelegate , userdata);

                PortAudio.Pa_StartStream(stream);

                while (readerPosition < reader.Length)
                {
                    //This is essentially working as a circular/FIFO buffer, where new sample packets are only added to the queue
                    //if there's room in the queue. Once the packet is read out of the queue in the callback function it's 
                    //removed from the queue and there is room to add more info to the queue
                    if (sampleQueue.Count < QUEUE_LENGTH)
                    {
                        Console.WriteLine("Writing");
                        short[] qBuffer = new short[NUM_SAMPLES]; //this has to be declared here as objects as sent by reference to the queue
                        read = reader.Read(buffer, 0, NUM_SAMPLES * 2); //read a block of bytes out from the wav
                        Buffer.BlockCopy(buffer, 0, qBuffer, 0, read); //copy them to a buffer of samples
                        sampleQueue.Enqueue(qBuffer); //send the buffer to the queue
                        readerPosition += read;
                    }

                }

                while(PortAudio.Pa_IsStreamActive(stream) != 0)
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
    }

}