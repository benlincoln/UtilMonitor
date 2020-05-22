using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UtilMonitor.Non_WPF_Code;

namespace UtilMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer remainTimer = new System.Windows.Threading.DispatcherTimer();
        public Getter g = new Getter();
        int x = 0;
        string graphView = "cpuUtil";
        PointCollection points = new PointCollection();
        public MainWindow()
        {
            InitializeComponent();
            remainTimer.Tick += timedEvent;
            //How frequently the timedEvent function is ran, in seconds
            remainTimer.Interval = TimeSpan.FromSeconds(0.25);
            remainTimer.Start();

        }
        private void timedEvent(object sender, EventArgs e)
        {
            //WPF requires dispatcher to be invoked to allow for code to be ran from a different thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Runs update to update the current values
                g.update();
                //Updates the text values, could likely move these into a seperate function for code cleanliness
                CPUUtil.Text = $"{g.getCPUUtil()}%";
                GPUTemp.Text = $"{g.getGPUTemp()}ºC";
                RAM.Text = $"{g.getRAM()}MB";
                //Open HW Monitor cannot support Ryzen 3000 series at time of writing
                if (g.getCPUTemp() == 0)
                {
                    CPUTemp.Text = "Cannot Detect";
                }
                CPUTemp.Text = $"{g.getCPUTemp()}ºC";
                GPULoad.Text = $"{g.getGPULoad()}%";
                //For drawing the graph, could likely move into it's own function as well which would return a Polyline object 
                double xMax = Graph.Width;
                double yMax = Graph.Height;
                double currentY = 0;
                //The canvas uses the top left as (0,0)
                currentY = g.graphCalc(Convert.ToInt32(yMax), graphView);
                points.Add(new Point(x, currentY));
                //Increments the x value along the graph, smaller the x value, the tighter the points on the graph and thus the more points shown before the graph resets
                x += 8;
                Polyline polyline = new Polyline();
                polyline.StrokeThickness = 1;
                polyline.Stroke = Brushes.White;
                polyline.Points = points;
                Graph.Children.Add(polyline);
                //Current implementation results in losing the final value for the cycle
                if (x >= xMax)
                {
                    x = 0;
                    points.Clear();
                }
            });

        }
        
        private void settingsClick(object sender, RoutedEventArgs e)
        {
            Settings settingsWin = new Settings();
            settingsWin.Show();
        }

        /*private void Drives_Loaded(object sender, RoutedEventArgs e)
        {
            var drivesVar = sender as ComboBox;
            drivesVar.ItemsSource = g.getdriveList();
            drivesVar.SelectedIndex = 0; 
        }*/

        private void Graph_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            string selected = GraphCombo.SelectedItem.ToString();
            // The above contains extra stuff we don't want with a colon at the end so we split using said colon
            var SplitSelected = selected.Split(':');
            x = 0;
            points.Clear();
            switch (SplitSelected[1])
            {
                //There is a space after the colon
                case " CPU Utilisation":
                    graphView = "cpuUtil";
                    CurrentGraph.Text = "CPU Utilisation";
                    YAxisTop.Text = "100%";
                    break;
                case " CPU Tempurature":
                    graphView = "cpuTemp";
                    CurrentGraph.Text = "CPU Tempurature";
                    YAxisTop.Text = $"{configReadWriter.readConfig("tempMaxCPU")}ºC";
                    break;
                case " Free RAM":
                    graphView = "ramUtil";
                    CurrentGraph.Text = "RAM Utilisation";
                    YAxisTop.Text = "100%";
                    break;
                case " GPU Tempurature":
                    graphView = "gpuTemp";
                    CurrentGraph.Text = "GPU Tempurature";
                    YAxisTop.Text = $"{configReadWriter.readConfig("tempMaxGPU")}ºC";
                    break;
                case " GPU Load":
                    graphView = "gpuLoad";
                    CurrentGraph.Text = "GPU Load";
                    YAxisTop.Text = "100%";
                    break;
            }
        }
    }

}


