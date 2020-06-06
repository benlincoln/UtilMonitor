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
            gpuName.Text = g.getGPUName();
            cpuName.Text = g.getCPUName();
            GPUTemp.Text = configReadWriter.readConfig("tempMaxGPU");
            CPUTemp.Text = configReadWriter.readConfig("tempMaxCPU");
            CPUTemp_Noti.Text = configReadWriter.readConfig("tempNotiCPU");
            GPUTemp_Noti.Text = configReadWriter.readConfig("tempNotiGPU");
            CPUUtil_Noti.Text = configReadWriter.readConfig("utilNotiCPU");
            RamUtil_Noti.Text = configReadWriter.readConfig("utilNotiRAM");
            GPULoad_Noti.Text = configReadWriter.readConfig("loadNotiGPU");
        }

        
        //Previously had seperate checkers for the graph max values despite using the same regex so condensed into one
        private void nonpercentPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RegexChecker.tempRegex.IsMatch(e.Text);
        }

        private void percentPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RegexChecker.percentRegex.IsMatch(e.Text); 
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            configReadWriter.addUpdateKey("tempMaxCPU", CPUTemp.Text);
            configReadWriter.addUpdateKey("tempMaxGPU", GPUTemp.Text);
            configReadWriter.addUpdateKey("tempNotiGPU", GPUTemp_Noti.Text);
            configReadWriter.addUpdateKey("tempNotiCPU", CPUTemp_Noti.Text);
            configReadWriter.addUpdateKey("loadNotiGPU", GPULoad_Noti.Text);
            configReadWriter.addUpdateKey("utilNotiCPU", CPUUtil_Noti.Text);
            configReadWriter.addUpdateKey("utilNotiRAM", RamUtil_Noti.Text);
            MessageBox.Show("Settings Applied");
            
        }
    }
}
