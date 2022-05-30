using System.Data;
using System.Diagnostics;
using System.IO;
using MID3SMPS.Containers.Gyb;

namespace MID3SMPS.Containers;

public struct Mappings{
	public readonly GYB Gyb;
	public readonly DacMap DacMap;
	public readonly DacList DacList;
	public readonly PsgList PsgList;

	public readonly FileInfo gybPath;
	public readonly FileInfo dacMapPath;
	public readonly FileInfo dacListPath;
	public readonly FileInfo psgListPath;

	public readonly FileInfo path;

	public Mappings(FileInfo mappingFile){
		path = mappingFile;
		string[] lines = File.ReadAllLines(path.FullName);
		if(lines.Length <= 4 || !lines[0].Equals("-- mid2smps Configuration --")){
			throw new DataException("File is not a mappings file");
		}

		// Suppress null warning
		Debug.Assert(path.DirectoryName != null, "path.DirectoryName != null");
		gybPath = new FileInfo(Path.Combine(path.DirectoryName, lines[1])); // Get relative paths
		dacMapPath = new FileInfo(Path.Combine(path.DirectoryName, lines[2]));
		dacListPath = new FileInfo(Path.Combine(path.DirectoryName, lines[3]));
		psgListPath = new FileInfo(Path.Combine(path.DirectoryName, lines[4]));
		Gyb = GYB.LoadGYB(gybPath);
		DacMap = new DacMap(dacMapPath);
		DacList = new DacList(dacListPath);
		PsgList = new PsgList(psgListPath);
	}
}
