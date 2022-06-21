using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace MID3SMPS.Forms;

// Added "YM" to 2612Edit since classes can't start with numbers
public partial class Ym2612Edit{
	private readonly MainWindow _mainWindow;
	private readonly List<Line> lines = new();
	public WriteableBitmap? Bitmap;
	private int curXScale = 0;
	public bool IsClosed;
	private ushort renderPosition;
	public Matrix transformToDevice;

	public Ym2612Edit(){
		InitializeComponent();
		IsClosed = false;
		Closed += OnClosed;
		Dispatcher.Invoke(()=>{
							  // Add an event handler to update canvas background color just before it is rendered.
							  CompositionTarget.Rendering += UpdateOsc;
						  },
						  DispatcherPriority.Render);
		// ReSharper disable once AssignNullToNotNullAttribute
		_mainWindow = (MainWindow)Application.Current.MainWindow;
	}
	private void OnClosed(object? sender, EventArgs? args){IsClosed = true;}

	private void UpdateOsc(object? sender, EventArgs e){
		for(int i = 0; i < 100; i++){
			CreateLine(MathF.Sin(renderPosition));
		}
	}

	private Line CreateLine(float value){
		Line line;
		if(renderPosition >= lines.Count){
			line = new Line();
			lines.Add(line);
			Oscilloscope.Children.Add(line);
		} else{
			line = lines[renderPosition];
		}

		const double offset = 1;
		double yScale = Oscilloscope.ActualHeight / 2;
		int xScale = (int)Oscilloscope.ActualWidth;
		if(curXScale == 0) curXScale = xScale;
		line.Stroke = Brushes.Lime;
		line.Visibility = Visibility.Visible;
		if(renderPosition == 0){
			line.X1 = line.X2 = renderPosition;
			line.Y1 = line.Y2 = (value + offset) * yScale;
		} else{
			line.X1 = renderPosition - 1;
			line.Y1 = lines[renderPosition - 1].Y2;
			line.X2 = renderPosition;
			line.Y2 = (value + offset) * yScale;
		}

		if(xScale >= curXScale){
			if(lines.Count > curXScale){
				for(int i = curXScale; i < xScale; i++){
					if(lines.Count <= i) break;
					lines[i].Visibility = Visibility.Hidden;
				}
				//lines.RemoveRange(renderPosition, lines.Count - renderPosition);
			}
		}

		curXScale = xScale;
		renderPosition++;
		if(renderPosition >= xScale) renderPosition = 0;
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
