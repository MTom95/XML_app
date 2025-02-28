using System.Windows;
using XML_app.ViewModels;

namespace XML_app
{
	  public partial class MainWindow : Window
      {
            public MainWindow()
            {
                  InitializeComponent();
                  // Set the DataContext to an instance of the ViewModel.
                  DataContext = new MainWindowViewModel();
            }
      }
}