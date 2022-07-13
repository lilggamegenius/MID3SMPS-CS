using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MID3SMPS.Containers;

namespace MID3SMPS.Forms;

// Added "YM" to 2612Edit since classes can't start with numbers
public partial class Ym2612Edit{
	private readonly List<Line> _lines = new();

	private readonly MainWindow _mainWindow;
	private int _curXScale;
	private ushort _renderPosition;
	public WriteableBitmap? Bitmap;
	public bool IsClosed;

	public Ym2612Edit(){
		// ReSharper disable once AssignNullToNotNullAttribute
		_mainWindow = (MainWindow)Application.Current.MainWindow;
		InitializeComponent();
		IsClosed = false;
		Closed += OnClosed;
		Dispatcher.Invoke(()=>{
							  // Add an event handler to update canvas background color just before it is rendered.
							  CompositionTarget.Rendering += UpdateOsc;
						  },
						  DispatcherPriority.Render);
	}

	public bool IsHex{get;set;}

	public Mappings Mappings=>_mainWindow.Mappings;
	private void OnClosed(object? sender, EventArgs? args){IsClosed = true;}

	private void UpdateOsc(object? sender, EventArgs e){
		if(!Oscilloscope.IsLoaded) return;
		for(int i = 0; i < 100; i++){
			CreateLine(Math.Sin(_renderPosition / (Oscilloscope.ActualWidth / 25)));
		}
	}

	private Line CreateLine(double value){
		Line line;
		if(_renderPosition >= _lines.Count){
			line = new Line();
			_lines.Add(line);
			Oscilloscope.Children.Add(line);
		} else{
			line = _lines[_renderPosition];
		}

		const double offset = 1;
		double yScale = Oscilloscope.ActualHeight / 2;
		int xScale = (int)Oscilloscope.ActualWidth;
		if(_curXScale == 0) _curXScale = xScale;
		line.Stroke = Brushes.Lime;
		line.Visibility = Visibility.Visible;
		if(_renderPosition == 0){
			line.X1 = line.X2 = _renderPosition;
			line.Y1 = line.Y2 = (value + offset) * yScale;
		} else{
			line.X1 = _renderPosition - 1;
			line.Y1 = _lines[_renderPosition - 1].Y2;
			line.X2 = _renderPosition;
			line.Y2 = (value + offset) * yScale;
		}

		if(xScale >= _curXScale){
			if(_lines.Count > _curXScale){
				for(int i = _curXScale; i < xScale; i++){
					if(_lines.Count <= i) break;
					_lines[i].Visibility = Visibility.Hidden;
				}
			}
		}

		_curXScale = xScale;
		_renderPosition++;
		if(_renderPosition >= xScale){
			Oscilloscope.Children.RemoveRange(_renderPosition, _lines.Count - _renderPosition);
			_lines.RemoveRange(_renderPosition, _lines.Count                - _renderPosition);
			_renderPosition = 0;
		}

		return line;
	}
	private void NumericUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e){
		// This function only really needs to be called when the interval isn't 1
		var control = (NumericUpDown)sender;
		if(!e.NewValue.HasValue || (control.Interval % 1 != 0)) return;
		double testValue = e.NewValue.Value - control.Minimum;
		if((testValue % control.Interval) == 0) return; // New value is in line with interval
		double rounded = testValue;
		rounded /= control.Interval;
		rounded = Math.Round(rounded); // Divide then round instead of just truncating
		rounded *= control.Interval;
		control.Value = rounded + control.Minimum;
	}
}
