using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MID3SMPS.Annotations;
using MID3SMPS.Utils;

namespace MID3SMPS.Containers.Gyb;

[DebuggerDisplay("{Name}: 0x{TotalSize.ToString(\"X4\")}")]
[Serializable]
public struct Patch : INotifyPropertyChanged{
	public const byte InstrumentRegistersSize = 0x1E;

	private string _name;
	private ushort _totalSize;
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = InstrumentRegistersSize)]
	private readonly byte[] _registers;
	private readonly Operator[] _operators;
	private sbyte _instrumentTransposition;
	private BitFieldOptions _options;
	private Chords _chordNotes;

	// Version of Instrument, and Instrument data
	public Patch(int version, in Span<byte> data) : this(){
		_name = null!;
		_totalSize = 0;
		_registers = new byte[InstrumentRegistersSize];
		_operators = new[]{
			new Operator(_registers, (byte)RegisterIds.R30),
			new Operator(_registers, (byte)RegisterIds.R38),
			new Operator(_registers, (byte)RegisterIds.R34),
			new Operator(_registers, (byte)RegisterIds.R3C)
		};
		_instrumentTransposition = 0;
		_options = BitFieldOptions.None;
		_chordNotes = new Chords();
		switch(version){
			case 1: break;
			case 2: break;
			case 3:
				TotalSize = BitConverter.ToUInt16(data);
				if(data.Length != TotalSize)
					throw new InvalidDataException($"Instrument data size does not match included size: Length:0x{data.Length:x4} != TotalSize:0x{TotalSize:x4}");
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
				break;
			default: throw new InvalidDataException();
		}
	}

	public string Name{
		get=>_name;
		set{
			_name = value;
			OnPropertyChanged(nameof(Name));
		}
	}
	public ushort TotalSize{
		get=>_totalSize;
		set{
			_totalSize = value;
			OnPropertyChanged(nameof(TotalSize));
		}
	}
	public byte[] Registers=>_registers;
	public Operator[] Operators=>_operators;
	public sbyte InstrumentTransposition{
		get=>_instrumentTransposition;
		set{
			_instrumentTransposition = value;
			OnPropertyChanged(nameof(InstrumentTransposition));
		}
	}
	public BitFieldOptions Options{
		get=>_options;
		set{
			_options = value;
			OnPropertyChanged(nameof(Options));
		}
	}
	public Chords ChordNotes{
		get=>_chordNotes;
		set{
			_chordNotes = value;
			OnPropertyChanged(nameof(ChordNotes));
		}
	}

	private byte this[RegisterIds idx]{
		get=>_registers[(byte)idx];
		set=>_registers[(byte)idx] = value;
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
	public event PropertyChangedEventHandler? PropertyChanged;
	[NotifyPropertyChangedInvocator]
	private void OnPropertyChanged([CallerMemberName] string? propertyName = null){PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));}
}
public struct Operator : INotifyPropertyChanged{
	[NonSerialized] private readonly byte[] _registers;
	[NonSerialized] private readonly byte _offset;

	internal Operator(byte[] registers, byte offset) : this(){
		_registers = registers;
		_offset = offset;
	}

	public DetuneModes Detune{ // 0xF0
		get{
			byte val = _registers[_offset + (0 * 4)];
			//val &= 0xF0;
			val >>= 4;
			return (DetuneModes)val;
		}
		set{
			byte val = (byte)value;
			val <<= 4;
			byte oldVal = _registers[_offset + (0 * 4)];
			oldVal &= 0x0F;
			val |= oldVal;
			_registers[_offset + (0 * 4)] = val;
			OnPropertyChanged(nameof(Detune));
		}
	}
	public byte Multiple{ // 0x0F
		get{
			byte val = _registers[_offset + (0 * 4)];
			val &= 0x0F;
			return val;
		}
		set{
			byte val = value;
			val &= 0x0F;
			byte oldVal = _registers[_offset + (0 * 4)];
			oldVal &= 0xF0;
			val |= oldVal;
			_registers[_offset + (0 * 4)] = val;
			OnPropertyChanged(nameof(Multiple));
		}
	}
	public sbyte TotalLevel{ // 0x7F
		get=>(sbyte)(_registers[_offset + (1 * 4)] & 0x7F);
		set{
			_registers[_offset + (1 * 4)] = (byte)(value & 0x7F);
			OnPropertyChanged(nameof(TotalLevel));
		}
	}
	public RateScalingModes RateScaling{ // 0xC0
		get{
			byte val = _registers[_offset + (2 * 4)];
			val >>= 6;
			return (RateScalingModes)val;
		}
		set{
			byte val = (byte)value;
			val <<= 6;
			byte oldVal = _registers[_offset + (2 * 4)];
			oldVal &= 0x3F;
			val |= oldVal;
			_registers[_offset + (2 * 4)] = val;
			OnPropertyChanged(nameof(RateScaling));
		}
	}
	public byte AttackRate{ // 0x3F
		get{
			byte val = _registers[_offset + (2 * 4)];
			val &= 0x3F;
			return val;
		}
		set{
			byte val = value;
			val &= 0x3F;
			byte oldVal = _registers[_offset + (2 * 4)];
			oldVal &= 0xC0;
			val |= oldVal;
			_registers[_offset + (2 * 4)] = val;
			OnPropertyChanged(nameof(AttackRate));
		}
	}
	public bool AmplitudeModulation{ // 0x80
		get{
			byte val = _registers[_offset + (3 * 4)];
			val >>= 7;
			return val == 1;
		}
		set{
			byte val = _registers[_offset + (3 * 4)];
			if(value){
				val |= 0x80;
			} else{
				val &= 0x1F; // ANDing off the last 3 bits instead of only the last 1 because 2 bits go unused
			}

			_registers[_offset + (3 * 4)] = val;
			OnPropertyChanged(nameof(AmplitudeModulation));
		}
	}
	public byte DecayRate{ // 0x1F
		get=>(byte)(_registers[_offset + (3 * 4)] & 0x1F);
		set{
			byte val = value;
			val &= 0x1F;
			byte oldVal = _registers[_offset + (3 * 4)];
			oldVal &= 0xE0;
			val |= oldVal;
			_registers[_offset + (3 * 4)] = val;
			OnPropertyChanged(nameof(DecayRate));
		}
	}
	public byte SustainRate{ // 0x1F
		get=>(byte)(_registers[_offset + (4 * 4)] & 0x1F);
		set{
			_registers[_offset + (4 * 4)] = (byte)(value & 0x1F);
			OnPropertyChanged(nameof(SustainRate));
		}
	}
	public byte SustainLevel{ // 0xF0
		get=>(byte)(_registers[_offset + (5 * 4)] >> 4);
		set{
			byte val = value;
			val <<= 4;
			byte oldVal = _registers[_offset + (5 * 4)];
			oldVal &= 0x0F;
			val |= oldVal;
			_registers[_offset + (5 * 4)] = val;
			OnPropertyChanged(nameof(SustainLevel));
		}
	}
	public byte ReleaseRate{ // 0x0F
		get=>(byte)(_registers[_offset + (5 * 4)] & 0x0F);
		set{
			byte val = value;
			val &= 0x0F;
			byte oldVal = _registers[_offset + (5 * 4)];
			oldVal &= 0xF0;
			val |= oldVal;
			_registers[_offset + (5 * 4)] = val;
			OnPropertyChanged(nameof(ReleaseRate));
		}
	}
	public SsgegModes SSGEG{ // 0x0F
		get=>(SsgegModes)(_registers[_offset + (6 * 4)] & 0x0F);
		set{
			_registers[_offset + (6 * 4)] = (byte)((byte)value & 0x0F);
			OnPropertyChanged(nameof(SSGEG));
		}
	}
	public event PropertyChangedEventHandler? PropertyChanged;
	[NotifyPropertyChangedInvocator]
	private void OnPropertyChanged([CallerMemberName] string? propertyName = null){PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));}
}
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum DetuneModes : byte{
	[Description("No Change")] NoChange1,
	[Description("× (1+e)")] PlusE,
	[Description("× (1+2e)")] Plus2E,
	[Description("× (1+3e)")] Plus3E,
	[Description("No Change")] NoChange2,
	[Description("× (1-e)")] MinusE,
	[Description("× (1-2e)")] Minus2E,
	[Description("× (1-3e)")] Minus3E
}
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum RateScalingModes : byte{
	[Description("2 × Rate + (KC/8)")] Kc8, [Description("2 × Rate + (KC/4)")] Kc4, [Description("2 × Rate + (KC/2)")] Kc2, [Description("2 × Rate + (KC/1)")] Kc1
}
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum SsgegModes : byte{
	[Description("Disabled")] Disabled,
	[Description(@"\\\\")] Mode0 = 0b1000,
	[Description(@"\___")] Mode1,
	[Description(@"\/\/")] Mode2,
	[Description(@"\‾‾‾")] Mode3,
	[Description(@"////")] Mode4,
	[Description(@"/‾‾‾")] Mode5,
	[Description(@"/\/\")] Mode6,
	[Description(@"/___")] Mode7
}
