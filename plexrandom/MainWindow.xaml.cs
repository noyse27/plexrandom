using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace plexrandom;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                // Synchronize PasswordBox with the loaded config token
                TokenPasswordBox.Password = vm.PlexToken;

                // Sync PasswordBox when ViewModel property changes (e.g. from TextBox)
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.PlexToken))
                    {
                        if (TokenPasswordBox.Password != vm.PlexToken)
                        {
                            TokenPasswordBox.Password = vm.PlexToken;
                        }
                    }
                };
            }
        };
    }

    private void TokenPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Avoid recursive updates
            if (vm.PlexToken != TokenPasswordBox.Password)
            {
                vm.PlexToken = TokenPasswordBox.Password;
            }
        }
    }
}