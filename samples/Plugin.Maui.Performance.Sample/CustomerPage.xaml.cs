namespace Plugin.Maui.Performance.Sample;

public partial class CustomerPage : ContentPage
{
    public CustomerPage()
    {
        InitializeComponent();
        Avatar.Source = ImageSource.FromUri(new Uri("https://i.pravatar.cc/192?u=maui-performance"));
        NameLabel.Text = "Ada Lovelace";
        DetailLabel.Text = "First programmer · London";
    }

    async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
