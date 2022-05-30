using System;
using System.Collections.Generic;
using System.IO;

namespace MID3SMPS.Containers.Gyb{
	public class GYB{
		private sbyte defaultLFOSpeed;
		private List<Instrument> Instruments = new();

		public static GYB LoadGYB(FileInfo path){
			Span<byte> gybData = File.ReadAllBytes(path.FullName);
			if(gybData[0] != 26 || gybData[1] != 12) throw new FormatException("Not a valid GYB formatted file");
			GYB gyb = new();
			byte version = gybData[2];
			return version switch{
				1=>LoadV1(gyb, gybData),
				2=>LoadV2(gyb, gybData),
				3=>LoadV3(gyb, gybData),
				var _=>throw new FormatException("Not a valid GYB formatted file")
			};
		}

		private static GYB LoadV1(GYB gyb, Span<byte> gybData){return gyb;}

		private static GYB LoadV2(GYB gyb, Span<byte> gybData){
			gyb.defaultLFOSpeed = (sbyte)gybData[105];
			return gyb;
		}

		private static GYB LoadV3(GYB gyb, Span<byte> gybData){
			gyb.defaultLFOSpeed = (sbyte)gybData[3];
			uint filesize = BitConverter.ToUInt32(gybData[0x4..]);
			if(filesize != gybData.Length) throw new FormatException("Not a valid GYB formatted file");
			uint bankOffset = BitConverter.ToUInt32(gybData[0x8..]);
			uint mapsOffset = BitConverter.ToUInt32(gybData[0xC..]);
			ushort instrumentCount = BitConverter.ToUInt16(gybData[(int)bankOffset..]);
			uint currentOffset = bankOffset; // First 2 bytes are the count
			for(int currentInstrument = 0; currentInstrument < instrumentCount; currentInstrument++){
				ushort instrumentSize = BitConverter.ToUInt16(gybData[((int)currentOffset + 2)..]);
				var ins = new Instrument(3,
										 gybData.Slice((int)currentOffset + 2,
													   instrumentSize));
				gyb.Instruments.Add(ins);
				currentOffset += instrumentSize;
			}

			return gyb;
		}
	}
}

/*
GYB Version 1/2
===============

Header
------
Pos	Len	Description
--------------------------------
00	02	Signature: 26 12 (both decimal)
02	01	Version
03	01	Instrument Count: Melody Bank
04	01	Instrument Count: Drum Bank
05	100	Instrument Map (Melody
		-> FM instrument IDs for GM instruments/drums
		-> Melody/Drum Bank interleaved (melody 0, drum 0, melody 1, drum 1, ...)
105	01	[GYB v2 only] default LFO Speed
??	??	Instrument Data [Melody Bank]
??	??	Instrument Data [Drum Bank]
??	??	Instrument Names [Melody Bank]
??	??	Instrument Names [Drum Bank]
??	04	Checksum (calculated over whole file minus the checksum itself)
EOF

Instrument Data [v1]
---------------
00	1D	YM2612 Registers
		Order:	30 34 38 3C 40 44 48 4C 50 54 58 5C 60 64 68 6C 70 74 78 7C 80 84 88 8C 90 94 98 9C B0
1D	01	Instrument Transposition (signed 8-bit)
		Drum Bank: default drum note (unsigned 8-bit)

Instrument Data [v2]
---------------
00	1E	YM2612 Registers
		Order:	30 34 38 3C 40 44 48 4C 50 54 58 5C 60 64 68 6C 70 74 78 7C 80 84 88 8C 90 94 98 9C B0 B4
1E	01	Instrument Transposition (same as v1)
1F	01	Padding (set to 0)

Instrument Name
---------------
00	01	String Length n
01	n	String



GYB Version 3
=============

Header
------
Pos	Len	Description
--------------------------------
00	02	Signature: 26 12 (both decimal)
02	01	Version
03	01	default LFO Speed
04	04	File Size
08	04	File Offset: Instrument Banks
0C	04	File Offset: Instrument Maps


Instrument Bank
---------------
00	02	Instrument Count
02	??	Instrument Data

Instrument Data
---------------
00	02	Bytes used by this instrument (includes this value)
02	1E	YM2612 Registers
		Order:	30 34 38 3C 40 44 48 4C 50 54 58 5C 60 64 68 6C 70 74 78 7C 80 84 88 8C 90 94 98 9C B0 B4
20	01	Instrument Transposition (signed 8-bit)
		Drum Bank: default drum note (unsigned 8-bit)
21	01	Bitfield: Additional Data
		Bit 0: Chord notes (additional notes that sound by playing this instrument)
22	??	additional data
??	01	String Length n
??	n	String


Chord notes
-----------
00	01	Number of Notes n
01	01*n	Notes (8-bit signed, relative to initial note)



Instrument Maps
---------------
00	??	Instrument Map: Melody [GM instrument -> FM instrument]
??	??	Instrument Map: Drum [GM drum kit sound -> FM instrument]

Instrument Map
--------------
00	??*80	128 (dec) Map Entries
		[Melody] each entry corresponds to one GM instrument
		[Drum] each entry corresponds to one GM instrument

Instrument Map Entry
--------------------
00	02	Map Sub-Entry Count n
02	04*n	Map Sub-Entries

00	01	Bank MSB (00 = default, FF = all) [Melody] / Drum Kit [Drum]
01	01	Bank LSB (FF = all) [Melody] / unused [Drum]
02	02	FM Instrument (Bit 15 = Drum Bank)

*/
