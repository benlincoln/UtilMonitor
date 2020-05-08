using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Diagnostics;
using System.Management;
using System.Windows.Threading;

namespace UtilMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer remainTimer = new System.Windows.Threading.DispatcherTimer();
        public Getter g = new Getter();
        public MainWindow()
        {
            InitializeComponent();
            remainTimer.Tick += timedEvent;
            remainTimer.Interval = TimeSpan.FromSeconds(1);
            remainTimer.Start();

        }

        private void timedEvent(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                
                g.update();
                CPUUtil.Text = g.getCPUUtil();
                CPUTemp.Text = g.getGPUTemp();
                RAM.Text = g.getRAM();

            });

            
            

        }
        void Update()
        {
           
                
            
        }

    }

}


