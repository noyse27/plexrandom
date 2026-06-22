using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace plexrandom;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                TokenPasswordBox.Password = vm.PlexToken;

                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.PlexToken))
                    {
                        if (TokenPasswordBox.Password != vm.PlexToken)
                            TokenPasswordBox.Password = vm.PlexToken;
                    }
                };
            }
        };
    }

    private void TokenPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            if (vm.PlexToken != TokenPasswordBox.Password)
                vm.PlexToken = TokenPasswordBox.Password;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
