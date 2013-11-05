using System;
using System.Runtime.InteropServices;

using PortAudioSharp;

namespace PortAudioSharpTest
{
	
	public class PortAudioTest
	{
	 	private float[] callbackBuffer = new float[512];
	 	private int callbackPos = 0;
	 	
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
	 			// log("Callback called");
		 		// log("time: " + timeInfo.currentTime 
		 		// 		+ " " + timeInfo.inputBufferAdcTime
		 		//		+ " " + timeInfo.outputBufferDacTime);
		 		// log("statusFlags: "+statusFlags);
		 		
		 		if (callbackBuffer.Length < frameCount*2) 
		 			callbackBuffer = new float[frameCount*2];
		 	
		 		for (int j = 0; j < frameCount*2; j++)
					callbackBuffer[j] = (float) Math.Sin((double)(callbackPos++)/20.0);
				
				Marshal.Copy(callbackBuffer, 0, output, (int)frameCount*2);
				
	 		} catch (Exception e) { 
	 			Console.WriteLine(e.ToString());
	 		}
	 		 
	 		return PortAudio.PaStreamCallbackResult.paContinue;
	 	}
		
		public void Run() {
			Audio audio = null;

			try {
			 
				Audio.LoggingEnabled = true;
				audio = new Audio(1, 2, 44100, 2048, new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback));
				
				audio.Start();
                
                System.Threading.Thread.Sleep(3000);

				audio.Stop();
				
			} catch(Exception e) {
				Console.WriteLine(e.ToString());
			} finally {
				if (audio != null) audio.Dispose();
			}
		}
	}
	
}
