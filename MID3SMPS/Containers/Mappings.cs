using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using MID3SMPS.Containers.Gyb;

namespace MID3SMPS.Containers;

public struct Mappings{
	[NonSerialized] public bool Modified;
	public GYB Gyb{get;}
	public DacMap DacMap{get;}
	public DacList DacList{get;}
	public PsgList PsgList{get;}

	public readonly FileInfo path;

	public Mappings(FileInfo mappingFile){
		path = mappingFile;
		string[] lines = File.ReadAllLines(path.FullName);
		if(lines.Length <= 4 || !lines[0].Equals("-- mid2smps Configuration --")){
			throw new DataException("File is not a mappings file");
		}

		// Suppress null warning
		Debug.Assert(path.DirectoryName != null, "path.DirectoryName != null");
		// Get relative paths
		Gyb = new GYB(new FileInfo(Path.Combine(path.DirectoryName, lines[1])));
		DacMap = new DacMap(new FileInfo(Path.Combine(path.DirectoryName, lines[2])));
		DacList = new DacList(new FileInfo(Path.Combine(path.DirectoryName, lines[3])));
		PsgList = new PsgList(new FileInfo(Path.Combine(path.DirectoryName, lines[4])));
		Modified = false;
	}
}
