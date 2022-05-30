using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace MID3SMPS.Containers.Gyb;

public struct Instrument{
	public const byte InstrumentRegistersSize = 0x1E;

	public string Name;
	public ushort TotalSize;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = InstrumentRegistersSize)]
	public byte[] Registers;
	public sbyte InstrumentTransposition;
	public BitFieldOptions Options;
	public Chords ChordNotes;

	// Version of Instrument, and Instrument data
	public Instrument(int version, in Span<byte> data){
		Name = null!;
		TotalSize = 0;
		Registers = new byte[InstrumentRegistersSize];
		InstrumentTransposition = 0;
		Options = BitFieldOptions.None;
		ChordNotes = new Chords();
		switch(version){
			case 1:
				LoadV1(data);
				break;
			case 2:
				LoadV2(data);
				break;
			case 3:
				LoadV3(data);
				break;
			default: throw new InvalidDataException();
		}
	}

	private void LoadV1(in Span<byte> data){}

	private void LoadV2(in Span<byte> data){}

	private void LoadV3(in Span<byte> data){
		TotalSize = BitConverter.ToUInt16(data);
		if(data.Length != TotalSize) throw new InvalidDataException($"Instrument data size does not match included size: Length:0x{data.Length:x4} != TotalSize:0x{TotalSize:x4}");
		Registers = new byte[InstrumentRegistersSize];
		int currentOffset = 0x02;
		data.Slice(currentOffset, InstrumentRegistersSize).CopyTo(Registers);
		currentOffset = 0x20;
		InstrumentTransposition = unchecked((sbyte)data[currentOffset++]);
		Options = (BitFieldOptions)data[currentOffset++];
		if(Options.HasFlag(BitFieldOptions.ChordNotes)){
			ChordNotes = new Chords(data[currentOffset..], ref currentOffset);
		}

		byte nameLength = data[currentOffset++];
		Name = System.Text.Encoding.ASCII.GetString(data.Slice(currentOffset, nameLength).ToArray());
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "UnusedType.Local")]
	[SuppressMessage("ReSharper", "UnusedMember.Local")]
	enum RegisterIds : byte{
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

	[Flags] public enum BitFieldOptions : byte{ None = 0, ChordNotes = 1 }

	public struct Chords{
		public byte NoteCount;
		public sbyte[] RelativeNotes;

		public Chords(in Span<byte> additionalData, ref int currentOffset){
			NoteCount = additionalData[0];
			RelativeNotes = (sbyte[])(object)additionalData.Slice(1, NoteCount).ToArray(); // Convert to object first to bypass compiler error
			currentOffset += NoteCount + 1;                                                // Increase offset to after additional data
		}
	}
}
