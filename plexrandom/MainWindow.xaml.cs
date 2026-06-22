using System.Windows;
using System.Windows.Controls;

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
}
