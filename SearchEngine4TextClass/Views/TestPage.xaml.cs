using SearchEngine4TextClass.Model;

namespace SearchEngine4TextClass.Views;

public partial class TestPage : ContentPage
{
	WrapperClass4Test WrapperClass4Test1;
	public TestPage()
	{
		InitializeComponent();
		WrapperClass4Test1 = new WrapperClass4Test();
		BindingContext = WrapperClass4Test1;
	}

	async private void testBtn1_Clicked(object sender, EventArgs e)
	{
		await Task.Run(() => WrapperClass4Test1.callDeserialization());
	}
}