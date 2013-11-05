using System;
using System.Reflection;

using PortAudioSharp;

namespace PortAudioSharpTest
{

	class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			Console.WriteLine("PortAudioSharp Test");
			Console.WriteLine("*******************");
			Console.WriteLine();
			Console.WriteLine("PortAudioSharpTest version: "
				+ Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Console.WriteLine("PortAudioSharp version: "
				+ Assembly.GetAssembly(typeof(PortAudio)).GetName().Version.ToString());
			Console.WriteLine("PortAudio version: " 
				+ PortAudio.Pa_GetVersionText() 
				+ " (" + PortAudio.Pa_GetVersion() + ")");
			Console.WriteLine(); 
			
			new PortAudioTest().Run();
		}
	}

}
