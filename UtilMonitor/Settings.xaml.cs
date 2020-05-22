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
using System.Windows.Shapes;
using UtilMonitor.Non_WPF_Code;

namespace UtilMonitor
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        Getter g = new Getter();
        public Settings()
        {
            InitializeComponent();
            totalRam.Text = $"{g.getSysRAM()} MB";
            gpuName.Text = g.GPUName;
            cpuName.Text = g.CPUName;
            GPUTemp.Text = configReadWriter.readConfig("tempMaxGPU");
            CPUTemp.Text = configReadWriter.readConfig("tempMaxCPU");
        }

        private void CPUTemp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RegexChecker.tempRegex.IsMatch(e.Text);

        }

        private void GPUTemp_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RegexChecker.tempRegex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            configReadWriter.addUpdateKey("tempMaxCPU", CPUTemp.Text);
            configReadWriter.addUpdateKey("tempMaxCPU", CPUTemp.Text);
            System.Windows.MessageBox.Show("Settings Applied");
        }
    }
}
