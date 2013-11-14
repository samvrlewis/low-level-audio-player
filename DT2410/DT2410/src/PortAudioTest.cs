using System;
using System.Runtime.InteropServices;

using PortAudioSharp;
using System.IO;
using NAudio.Wave;

namespace PortAudioSharpTest
{
	
	public class PortAudioTest
	{
        private const int NUM_SAMPLES = 10000;

        private float[] callbackBuffer = new float[NUM_SAMPLES];
        short[][] sampleBuffer = new short[2][];
        private int writeIndex = 1;

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
	 		try {
                
                int readIndex = (writeIndex == 1) ? 0 : 1;
                Console.WriteLine("Reading from " + readIndex.ToString());
		 		if (callbackBuffer.Length < frameCount*2) 
		 			callbackBuffer = new float[frameCount*2];
		 	
		 		for (int j = 0; j < frameCount*2; j++)
					callbackBuffer[j] = Convert.ToSingle(sampleBuffer[readIndex][j++])/32767.0f;

                writeIndex = readIndex;

				Marshal.Copy(callbackBuffer, 0, output, (int)frameCount*2); 
               
	 		} catch (Exception e) { 
	 			Console.WriteLine(e.ToString());
	 		}
	 		 
	 		return PortAudio.PaStreamCallbackResult.paContinue;
	 	}
		
		public void Run() {
            //declare the sizes of the 2d array
            sampleBuffer[0] = new short[NUM_SAMPLES];
            sampleBuffer[1] = new short[NUM_SAMPLES];
            
            Audio audio = null;
            string path = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
            WaveFileReader reader = new WaveFileReader(path + "/wave_files/test.wav");

            int readerPosition = 0;
            int lastWritten = 0;

            byte[] buffer = new byte[NUM_SAMPLES*2]; //buffer to read the wav raw bytes into
            
            //read out two buffers worth of samples
            int read = reader.Read(buffer, 0, NUM_SAMPLES*2); 
            Buffer.BlockCopy(buffer, 0, sampleBuffer[0], 0, read); 
            readerPosition += read;
           
			try {
			 
				Audio.LoggingEnabled = true;
                audio = new Audio(1, 2, 44100, NUM_SAMPLES/2, new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback));
                audio.Start();

                while (readerPosition < reader.Length)
                {
                   
                    //wait for a buffer to be ready to be written into
                    if (lastWritten != writeIndex)
                    {
                        Console.WriteLine("Writing to " + writeIndex.ToString());
                        read = reader.Read(buffer, 0, NUM_SAMPLES*2); //read a block out

                        Buffer.BlockCopy(buffer, 0, sampleBuffer[writeIndex], 0, read); //copy them to the short buffer

                        readerPosition += read;
                        lastWritten = writeIndex;
                    }
                }

                while (lastWritten == writeIndex)
                {
                    
                    //wait for it to finish reading the final buffer
                }

                System.Threading.Thread.Sleep(5000);
                
				audio.Stop();
				
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			} finally {
				if (audio != null) audio.Dispose();
			}
		}
	}
	
}
