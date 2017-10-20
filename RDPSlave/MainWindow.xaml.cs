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

namespace RDPSlave
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AddButtons();
        }
        
        void AddButtons()
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Classes.RDPFunctions.StartRDPSession("192.168.15.2", "administrator@com-four.local", "Heute.2014!");
        }
    }
}
