using System.Collections.Generic;
using M;

namespace MID3SMPS.Containers;

public class TrackFrame{
	public MidiMessageCC? BankMsb, BankLsb;
	public MidiMessageCC? DataEntryMsb, DataEntryLsb;
	public MidiMessageCC? ExpressionMsb, ExpressionLsb;
	public MidiMessagePatchChange? InsChange;
	public MidiMessageCC? Loop;
	public HashSet<MidiMessageMeta> MetaSet;

	public List<MidiMessageNoteOn> NoteOns;
	public List<MidiMessageNoteOff> NotesOffs;
	public HashSet<MidiMessageCC> OtherCCs;
	public MidiMessageCC? PanMsb, PanLsb;
	public MidiMessageChannelPitch? Pitch;

	public TrackFrame? PreviousFrame;
	public MidiMessageCC? RpnMsb, RpnLsb;
	public MidiMessageCC? VolumeMsb, VolumeLsb;
	public TrackFrame(){
		OtherCCs = new HashSet<MidiMessageCC>(new MidiMessageCcTypeComparer());
		MetaSet = new HashSet<MidiMessageMeta>(new MidiMessageMetaTypeComparer());
		NoteOns = new List<MidiMessageNoteOn>();
		NotesOffs = new List<MidiMessageNoteOff>();
	}

	public TrackFrame(TrackFrame previousFrame) : this(){
		PreviousFrame = previousFrame;
		if(previousFrame.PreviousFrame == null) return;

		// This should copy the previous frame's values so they contain the last value they were set to
		// if they were ever set to one
		previousFrame.BankMsb ??= previousFrame.PreviousFrame.BankMsb;
		previousFrame.BankLsb ??= previousFrame.PreviousFrame.BankLsb;
		previousFrame.InsChange ??= previousFrame.PreviousFrame.InsChange;
		previousFrame.VolumeMsb ??= previousFrame.PreviousFrame.VolumeMsb;
		previousFrame.VolumeLsb ??= previousFrame.PreviousFrame.VolumeLsb;
		previousFrame.PanMsb ??= previousFrame.PreviousFrame.PanMsb;
		previousFrame.PanLsb ??= previousFrame.PreviousFrame.PanLsb;
		previousFrame.ExpressionMsb ??= previousFrame.PreviousFrame.ExpressionMsb;
		previousFrame.ExpressionLsb ??= previousFrame.PreviousFrame.ExpressionLsb;
		previousFrame.RpnMsb ??= previousFrame.PreviousFrame.RpnMsb;
		previousFrame.RpnLsb ??= previousFrame.PreviousFrame.RpnLsb;
		previousFrame.DataEntryMsb ??= previousFrame.PreviousFrame.DataEntryMsb;
		previousFrame.DataEntryLsb ??= previousFrame.PreviousFrame.DataEntryLsb;
		previousFrame.Pitch ??= previousFrame.PreviousFrame.Pitch;
		previousFrame.Loop ??= previousFrame.PreviousFrame.Loop;
		previousFrame.OtherCCs.UnionWith(previousFrame.PreviousFrame.OtherCCs);
		previousFrame.MetaSet.UnionWith(previousFrame.PreviousFrame.MetaSet);
		previousFrame.PreviousFrame = null;
	}

	private class MidiMessageMetaTypeComparer : EqualityComparer<MidiMessageMeta>{
		public override bool Equals(MidiMessageMeta? x, MidiMessageMeta? y){
			if(x == null || y == null) return object.Equals(x, y);
			return x.Type == y.Type;
			//if(x.PayloadLength != y.PayloadLength) return false;
			//return x.Data.SequenceEqual(y.Data);
		}
		public override int GetHashCode(MidiMessageMeta obj)=>obj.Type;
	}

	private class MidiMessageCcTypeComparer : EqualityComparer<MidiMessageCC>{
		public override bool Equals(MidiMessageCC? x, MidiMessageCC? y){
			if(x == null || y == null) return object.Equals(x, y);
			return x.ControlId == y.ControlId;
		}
		public override int GetHashCode(MidiMessageCC obj)=>obj.ControlId;
	}
}
