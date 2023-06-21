using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PasswordSaver.MVVM.View
{
    /// <summary>
    /// Interaction logic for LogersPage.xaml
    /// </summary>
    public partial class LogersPage : UserControl
    {
        public LogersPage()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.LogersPageViewModel();
        }
    }
}
