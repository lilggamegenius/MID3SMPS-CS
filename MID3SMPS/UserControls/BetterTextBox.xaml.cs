using System.Windows.Controls;

namespace MID3SMPS.UserControls;

public partial class BetterTextBox : UserControl{
	public BetterTextBox(){InitializeComponent();}
	public string Text{
		get=>TextBox.Text;
		set=>TextBox.Text = value;
	}

	public bool IsReadOnly{
		get=>TextBox.IsReadOnly;
		set=>TextBox.IsReadOnly = value;
	}
}
