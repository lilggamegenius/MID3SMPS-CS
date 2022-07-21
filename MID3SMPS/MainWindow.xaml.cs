using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using M;
using Microsoft.Win32;
using MID3SMPS.Containers;
using MID3SMPS.Forms;

namespace MID3SMPS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow{
	public Ym2612Edit? EditWindow;
	public DateTime LastMidiWrite;
	public Mappings Mappings;
	public MidiFile Midi;
	public FileInfo? MidiPath;

	public MainWindow(){
		InitializeComponent();
		//DebuggingStuff();
		byte ret = OpnDll.OpenOPNDriver(1);
		Debug.WriteLine($"ChipOpen = 0x{ret:X2}");
	}

	private void DebuggingStuff(){
		OpnDll.OPN_Write(0, 0x2B, 0x80);
		OpnDll.OPN_Write(0x0, 0x30, 0x32);
		OpnDll.OPN_Write(0x0, 0x34, 0x00);
		OpnDll.OPN_Write(0x0, 0x38, 0x00);
		OpnDll.OPN_Write(0x0, 0x3C, 0x03);
		OpnDll.OPN_Write(0x0, 0x40, 0x11);
		OpnDll.OPN_Write(0x0, 0x44, 0x18);
		OpnDll.OPN_Write(0x0, 0x48, 0x1B);
		OpnDll.OPN_Write(0x0, 0x4C, 0x11);
		OpnDll.OPN_Write(0x0, 0x50, 0x1F);
		OpnDll.OPN_Write(0x0, 0x54, 0x1F);
		OpnDll.OPN_Write(0x0, 0x58, 0x1F);
		OpnDll.OPN_Write(0x0, 0x5C, 0x1F);
		OpnDll.OPN_Write(0x0, 0x60, 0x06);
		OpnDll.OPN_Write(0x0, 0x64, 0x07);
		OpnDll.OPN_Write(0x0, 0x68, 0x09);
		OpnDll.OPN_Write(0x0, 0x6C, 0x03);
		OpnDll.OPN_Write(0x0, 0x70, 0x00);
		OpnDll.OPN_Write(0x0, 0x74, 0x00);
		OpnDll.OPN_Write(0x0, 0x78, 0x00);
		OpnDll.OPN_Write(0x0, 0x7C, 0x00);
		OpnDll.OPN_Write(0x0, 0x80, 0x17);
		OpnDll.OPN_Write(0x0, 0x84, 0x16);
		OpnDll.OPN_Write(0x0, 0x88, 0x15);
		OpnDll.OPN_Write(0x0, 0x8C, 0x13);
		OpnDll.OPN_Write(0x0, 0xB0, 0x38);
		OpnDll.OPN_Write(0x0, 0xB4, 0x00 | 0xC0);
		OpnDll.OPN_Write(0, 0x28, 0xF0);
		const string DACPath = @"00_BassDrum.raw";
		byte[] sampleData =
			File.ReadAllBytes(DACPath);
		OpnDll.SetDACFrequency(0, 15625);
		OpnDll.SetDACVolume(0, 0x100); // 0x100 = 100%
		OpnDll.PlayDACSample(0, (uint)sampleData.Length, sampleData, 0);
	}

	private async Task LoadMappings(FileInfo path){
		await Task.Run(()=>{Mappings = new Mappings(path);});
		LoadedBankTextBox.Text = Mappings.Gyb.Path?.Name;
		LoadedBankTextBox.ToolTip = Mappings.Gyb.Path?.FullName;
		Status.Text = "Configuration Loaded";
		MainSettings.Default.LastMappingFile = Mappings.path.FullName;
	}

	private async Task LoadMidi(FileInfo fileInfo){
		MidiPath = fileInfo;
		await Task.Run(()=>{Midi = MidiFile.ReadFrom(MidiPath.OpenRead());});
		LoadedMidiTextBox.Text = MidiPath.Name;
		MidiNumber.Text = Midi.TimeBase.ToString();
		Status.Text = $"Loaded Midi {Midi.Name}{(AutoOptimizeMidiItem.IsChecked ? " - Optimizing..." : string.Empty)}";
		LastMidiWrite = MidiPath.LastWriteTime;
		await OptimizeMidi();
	}

	private async Task OptimizeMidi(){
		if(!AutoOptimizeMidiItem.IsChecked) return;
		var watch = new Stopwatch();
		watch.Start();
		List<Task> trackOptimizers = new();
		for(int i = 0; i < Midi.Tracks.Count; i++){
			trackOptimizers.Add(OptimizeTrack(i, Midi.Tracks[i]));
		}

		await Task.WhenAll(trackOptimizers);
		watch.Stop();
		Status.Text = $"Midi optimized in {watch.ElapsedMilliseconds} ms";
	}

	/*
	 * Order for events in optimized midi
	 *
	 * Note Offs
	 * Bank Select MSB, LSB
	 * instrument change
	 * Volume
	 * Pan
	 * Expression
	 * other CCs
	 * Pitch Bend Range (RPN MSB, LSB, Data Entry)
	 * Pitch Bend
	 * Loop Control (111)
	 * Note Ons
	 */
	private static async Task OptimizeTrack(int trackId, MidiSequence track){
		if(track.Events.Count == 0) return;
		var watch = new Stopwatch();
		watch.Start();
		await Task.Run(()=>{
			List<MidiEvent> events = new(track.Events);
			track.Events.Clear();
			int currentPos = events[0].Position; // In case this track's first event isn't on 0
			var trackFrame = new TrackFrame();
			int delta = currentPos;
			foreach(MidiEvent msg in events){
				if(msg.Position > 0){ // Apply changes in the correct order
					MessagesToEvents(track.Events, trackFrame.NotesOffs, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.BankMsb, trackFrame.PreviousFrame?.BankMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.BankLsb, trackFrame.PreviousFrame?.BankLsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.InsChange, trackFrame.PreviousFrame?.InsChange, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.VolumeMsb, trackFrame.PreviousFrame?.VolumeMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.VolumeLsb, trackFrame.PreviousFrame?.VolumeLsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.PanMsb, trackFrame.PreviousFrame?.PanMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.PanLsb, trackFrame.PreviousFrame?.PanLsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.ExpressionMsb, trackFrame.PreviousFrame?.ExpressionMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.ExpressionLsb, trackFrame.PreviousFrame?.ExpressionLsb, ref delta);
					MessagesToEvents(track.Events, trackFrame.OtherCCs, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.RpnMsb, trackFrame.PreviousFrame?.RpnMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.RpnLsb, trackFrame.PreviousFrame?.RpnLsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.DataEntryMsb, trackFrame.PreviousFrame?.DataEntryMsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.DataEntryLsb, trackFrame.PreviousFrame?.DataEntryLsb, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.Pitch, trackFrame.PreviousFrame?.Pitch, ref delta);
					AddMessageIfNotChanged(track.Events, trackFrame.Loop, trackFrame.PreviousFrame?.Loop, ref delta);
					MessagesToEvents(track.Events, trackFrame.NoteOns, ref delta);
					delta += msg.Position;
					currentPos += msg.Position; // Apparently midi files use deltas instead of absolute position
					Debug.WriteLine($"Track({trackId}), Delta({delta}), Position({currentPos})");
					trackFrame = new TrackFrame(trackFrame);
				}

				switch(msg.Message){
					case MidiMessageMeta meta:
						if(trackFrame.MetaSet.Contains(meta)) trackFrame.MetaSet.Remove(meta);
						trackFrame.MetaSet.Add(meta);
						continue;
					case MidiMessagePatchChange patchChange:
						trackFrame.InsChange = patchChange;
						continue;
					case MidiMessageChannelPitch pitch:
						trackFrame.Pitch = pitch;
						continue;
					case MidiMessageNoteOn noteOn:
						trackFrame.NoteOns.Add(noteOn);
						continue;
					case MidiMessageNoteOff noteOff:
						trackFrame.NotesOffs.Add(noteOff);
						continue;
					case MidiMessageCC cc:
						const int lsbOffset = 32;
						switch(cc.ControlId){
							case 0:
								trackFrame.BankMsb = cc;
								continue;
							case 0 + lsbOffset:
								trackFrame.BankLsb = cc;
								continue;
							case 7:
								trackFrame.VolumeMsb = cc;
								continue;
							case 7 + lsbOffset:
								trackFrame.VolumeLsb = cc;
								continue;
							case 10:
								trackFrame.PanMsb = cc;
								continue;
							case 10 + lsbOffset:
								trackFrame.PanLsb = cc;
								continue;
							case 11:
								trackFrame.ExpressionMsb = cc;
								continue;
							case 11 + lsbOffset:
								trackFrame.ExpressionLsb = cc;
								continue;
							default: // Any other CC
								trackFrame.OtherCCs.Add(cc);
								continue;
							case 101:
								trackFrame.RpnMsb = cc;
								continue;
							case 100:
								trackFrame.RpnLsb = cc;
								continue;
							case 6:
								trackFrame.DataEntryMsb = cc;
								continue;
							case 6 + lsbOffset:
								trackFrame.DataEntryLsb = cc;
								continue;
							case 111:
								trackFrame.Loop = cc;
								continue;
						}
				}
			}
		});
		watch.Stop();
		Debug.WriteLine($"Track {trackId} took {watch.ElapsedMilliseconds} ms to optimize");
	}

	private static void MessagesToEvents(ICollection<MidiEvent> events, IEnumerable<MidiMessage> messages, ref int delta){
		foreach(MidiMessage message in messages){
			events.Add(new MidiEvent(delta, message));
			delta = 0;
		}
	}

	private static void AddMessageIfNotChanged(ICollection<MidiEvent> events, MidiMessage? newMessage, MidiMessage? oldMessage, ref int delta){
		if(newMessage == null) return;
		if(Equals(newMessage, oldMessage)) return; // Make sure the new message isn't simply a duplicate of the old value
		events.Add(new MidiEvent(delta, newMessage));
		delta = 0;
	}

	private void Unimplemented(object sender, EventArgs e){
		MessageBox.Show("Sorry, That action hasn't been implemented yet",
						"Unimplemented Action",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
	}
	private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void OpenMappingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void SaveMappingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void OpenInstrumentEditorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void OpenMappingsEditorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void TempoCalculatorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void LoadInsLibCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private void QuickConvertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
	private async void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e){
		OpenFileDialog fileDialog = new(){
			AddExtension = true,
			CheckFileExists = true,
			CheckPathExists = true,
			Filter = "MIDI files (*.mid)|*.mid;*.midi",
			DefaultExt = "mid",
		};
		if(MidiPath                != null) fileDialog.FileName = MidiPath.Name;
		if(fileDialog.ShowDialog() != true) return;
		await LoadMidi(new FileInfo(fileDialog.FileName));
	}
	private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private async void OpenMappingsCommand_Executed(object sender, ExecutedRoutedEventArgs e){
		OpenFileDialog fileDialog = new(){
			AddExtension = true,
			CheckFileExists = true,
			CheckPathExists = true,
			Filter = "Configuration files (*.cfg)|*.cfg",
			DefaultExt = "cfg"
		};
		if(fileDialog.ShowDialog() != true) return;
		await LoadMappings(new FileInfo(fileDialog.FileName));
	}
	private void SaveMappingsCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private void OpenInstrumentEditorCommand_Executed(object sender, ExecutedRoutedEventArgs e){
		if(EditWindow == null || EditWindow.IsClosed){
			EditWindow = new Ym2612Edit();
		}

		if(!EditWindow.IsVisible){
			EditWindow.Show();
		}

		if(EditWindow.WindowState == WindowState.Minimized){
			EditWindow.WindowState = WindowState.Normal;
		}

		EditWindow.Activate();
		EditWindow.Topmost = true;
		EditWindow.Topmost = false; // Makes sure window gets put on top but doesn't stay there
	}
	private void OpenMappingsEditorCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private void TempoCalculatorCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private void LoadInsLibCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private void QuickConvertCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	private void OnClosing(object sender, CancelEventArgs e){MainSettings.Default.Save();}
	private async void MainWindowLoaded(object sender, RoutedEventArgs e){
		if(string.IsNullOrWhiteSpace(MainSettings.Default.LastMappingFile)) return;
		await LoadMappings(new FileInfo(MainSettings.Default.LastMappingFile));
		ConvertSongTitleItem.IsChecked = MainSettings.Default.ConvertSongTitle;
		PerFileInstrumentsItem.IsChecked = MainSettings.Default.PerFileInstruments;
		AutoReloadMidiItem.IsChecked = MainSettings.Default.AutoReloadMidi;
		AutoOptimizeMidiItem.IsChecked = MainSettings.Default.AutoOptimizeMidi;
	}
	private void ConvertSongTitleItem_OnChecked(object sender, RoutedEventArgs e){
		MainSettings.Default.ConvertSongTitle = ConvertSongTitleItem.IsChecked;
		e.Handled = true;
	}
	private void PerFileInstrumentsItem_OnChecked(object sender, RoutedEventArgs e){
		MainSettings.Default.PerFileInstruments = PerFileInstrumentsItem.IsChecked;
		e.Handled = true;
	}
	private void AutoReloadMidiItem_OnChecked(object sender, RoutedEventArgs e){
		MainSettings.Default.AutoReloadMidi = AutoReloadMidiItem.IsChecked;
		e.Handled = true;
	}
	private void AutoOptimizeMidi_OnChecked(object sender, RoutedEventArgs e){
		MainSettings.Default.AutoOptimizeMidi = AutoOptimizeMidiItem.IsChecked;
		e.Handled = true;
	}
}
