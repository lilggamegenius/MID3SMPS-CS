using System.Windows.Input;

namespace MID3SMPS{
	public static class WindowCommands{
		public static readonly RoutedUICommand OpenMidi = new("_Open MIDI File",
															  "OpenMidi",
															  typeof(WindowCommands),
															  new InputGestureCollection{
																  new KeyGesture(Key.O, ModifierKeys.Control)
															  });
		public static readonly RoutedUICommand SaveSmps = new("_Save SMPS File",
															  "SaveSmps",
															  typeof(WindowCommands),
															  new InputGestureCollection{
																  new KeyGesture(Key.S, ModifierKeys.Control)
															  });
		public static readonly RoutedUICommand OpenMappings = new("_Open Mapping Configuration",
																  "OpenMappings",
																  typeof(WindowCommands),
																  new InputGestureCollection{
																	  new KeyGesture(Key.F5)
																  });
		public static readonly RoutedUICommand SaveMappings = new("_Save Mapping Configuration",
																  "SaveMappings",
																  typeof(WindowCommands),
																  new InputGestureCollection{
																	  new KeyGesture(Key.F6)
																  });
		public static readonly RoutedUICommand OpenInstrumentEditor = new("Open Instruments _Editor",
																		  "OpenInstrumentEditor",
																		  typeof(WindowCommands),
																		  new InputGestureCollection{
																			  new KeyGesture(Key.I, ModifierKeys.Control)
																		  });
		public static readonly RoutedUICommand OpenMappingsEditor = new("Open _Mappings Editor",
																		"OpenMappingsEditor",
																		typeof(WindowCommands),
																		new InputGestureCollection{
																			new KeyGesture(Key.M, ModifierKeys.Control)
																		});
		public static readonly RoutedUICommand TempoCalculator = new("Tempo _Calculator",
																	 "TempoCalculator",
																	 typeof(WindowCommands),
																	 new InputGestureCollection{
																		 new KeyGesture(Key.T, ModifierKeys.Control)
																	 });
		public static readonly RoutedUICommand ConvertSongTitle = new("Convert Song _Title", "ConvertSongTitle", typeof(WindowCommands));
		public static readonly RoutedUICommand LoadInsLib = new("Load Ins _Lib",
																"LoadInsLib",
																typeof(WindowCommands),
																new InputGestureCollection{
																	new KeyGesture(Key.L, ModifierKeys.Control)
																});
		public static readonly RoutedUICommand QuickConvert = new("_Quick Convert",
																  "QuickConvert",
																  typeof(WindowCommands),
																  new InputGestureCollection{
																	  new KeyGesture(Key.Q, ModifierKeys.Control)
																  });
	}
}
