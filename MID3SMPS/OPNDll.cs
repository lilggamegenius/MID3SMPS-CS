using System.Runtime.InteropServices;

namespace MID3SMPS{
	public class OpnDll{
		private const string OPN_DLL = @"OPN_DLL.dll"; // Full path to DLL
		[DllImport(OPN_DLL)] public static extern void SetOPNOptions(uint outSmplRate, byte resmplMode, byte chipSmplMode, uint chipSmplRate);
		[DllImport(OPN_DLL)] public static extern byte OpenOPNDriver(byte chips);
		[DllImport(OPN_DLL)] public static extern void CloseOPNDriver();

		[DllImport(OPN_DLL)] public static extern void OPN_Write(byte chipId, ushort register, byte data);
		[DllImport(OPN_DLL)] public static extern void OPN_Mute(byte chipId, byte muteMask);

		[DllImport(OPN_DLL)] public static extern void PlayDACSample(byte chipId, in uint dataSize, in byte[] data, uint smplFreq);
		[DllImport(OPN_DLL)] public static extern void SetDACFrequency(byte chipId, uint smplFreq);
		[DllImport(OPN_DLL)] public static extern void SetDACVolume(byte chipId, ushort volume); // 0x100 = 100%
	}
}
