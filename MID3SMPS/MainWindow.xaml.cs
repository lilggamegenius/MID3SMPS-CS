﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using MID3SMPS.Containers;

namespace MID3SMPS{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow{
		public Mappings Mappings;
		public MainWindow(){
			InitializeComponent();
			//DebuggingStuff();
		}

		private void DebuggingStuff(){
			byte ret = OpnDll.OpenOPNDriver(1);
			Debug.WriteLine($"ChipOpen = 0x{ret:X2}");
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

			//const string DACPath = @"00_BassDrum.raw";
			//byte[] sampleData =
			//	File.ReadAllBytes(DACPath);
			//OpnDll.SetDACFrequency(0, 15625);
			//OpnDll.SetDACVolume(0, 0x100/2); // 0x100 = 100%
			//OpnDll.PlayDACSample(0, (uint)sampleData.Length, sampleData, 0);
		}

		private void Unimplemented(object sender, EventArgs e){
			MessageBox.Show("Sorry, That action hasn't been implemented yet",
							"Unimplemented Action",
							MessageBoxButton.OK,
							MessageBoxImage.Error);
		}
		private void Unimplemented_Checked(object sender, RoutedEventArgs e){}
		private void Unimplemented_Unchecked(object sender, RoutedEventArgs e){}
		private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void OpenMappingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void SaveMappingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void OpenInstrumentEditorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void OpenMappingsEditorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void TempoCalculatorCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void ConvertSongTitleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void LoadInsLibCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void QuickConvertCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)=>e.CanExecute = true;
		private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void OpenMappingsCommand_Executed(object sender, ExecutedRoutedEventArgs e){
			OpenFileDialog fileDialog = new(){
				AddExtension = true,
				CheckFileExists = true,
				CheckPathExists = true,
				Filter = "Configuration files (*.cfg)|*.cfg",
				DefaultExt = "cfg"
			};
			if(fileDialog.ShowDialog() == true){
				Mappings = new Mappings(new FileInfo(fileDialog.FileName));
			}

			LoadedBankTextBox.Text = Mappings.gybPath.Name;
			LoadedBankTextBox.ToolTip = Mappings.gybPath.FullName;
			Status.Text = "Configuration Loaded";
		}
		private void SaveMappingsCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void OpenInstrumentEditorCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void OpenMappingsEditorCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void TempoCalculatorCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void ConvertSongTitleCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void LoadInsLibCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
		private void QuickConvertCommand_Executed(object sender, ExecutedRoutedEventArgs e){}
	}
}