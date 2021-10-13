using System;
using System.Runtime.InteropServices;

namespace MID3SMPS.Containers.Gyb{
	public class Instrument{
		public Instrument(int version){}

		[StructLayout(LayoutKind.Sequential)]
		struct V1{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1D)]
			private byte[] registers;
			private sbyte InstrumentTransposition;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct V2{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1E)]
			private byte[] registers;
			private sbyte InstrumentTransposition;
			private readonly byte _; // Padding
		}

		[StructLayout(LayoutKind.Sequential)]
		struct V3{
			private ushort totalSize;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1E)]
			private byte[] registers;
			private sbyte InstrumentTransposition;
			private BitFieldOptions options;
		}

		enum Registers : byte{
			R30,
			R34,
			R38,
			R3C,
			R40,
			R44,
			R48,
			R4C,
			R50,
			R54,
			R58,
			R5C,
			R60,
			R64,
			R68,
			R6C,
			R70,
			R74,
			R78,
			R7C,
			R80,
			R84,
			R88,
			R8C,
			R90,
			R94,
			R98,
			R9C,
			RB0,
			RB4
		}

		[Flags] enum BitFieldOptions : byte{ None = 0, ChordNotes = 1 }
	}
}
