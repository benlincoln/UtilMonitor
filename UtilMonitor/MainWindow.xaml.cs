using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UtilMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer remainTimer = new System.Windows.Threading.DispatcherTimer();
        Getter g = new Getter();
        int x = 0;
        string graphView = "cpuUtil";
        PointCollection points = new PointCollection();
        public MainWindow()
        {
            InitializeComponent();
            remainTimer.Tick += timedEvent;
            //Creation of buttons and their events
            Button cpuUtilBtn = new Button();
            cpuUtilBtn.Name = "CPUUtil";
            cpuUtilBtn.Click += cpuUtilClick;
            Button cpuTempBtn = new Button();
            cpuTempBtn.Name = "CPUTemp";
            cpuTempBtn.Click += cpuTempClick;
            Button gpuTempBtn = new Button();
            gpuTempBtn.Name = "GPUTemp";
            gpuTempBtn.Click += gpuTempClick;
            Button ramBtn = new Button();
            ramBtn.Name = "RAM";
            ramBtn.Click += ramClick;
            Button settingsBtn = new Button();
            ramBtn.Name = "Settings";
            ramBtn.Click += settingsClick;
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
        
        //Button clicks, feels messy like this but unsure of a better way to organise them
        private void cpuTempClick(object sender, RoutedEventArgs e)
        {
            graphView = "cpuTemp";
            CurrentGraph.Text = "CPU Tempurature";
            //Resets the graph
            x = 0;
            points.Clear();
        }
        private void cpuUtilClick(object sender, RoutedEventArgs e)
        {
            graphView = "cpuUtil";
            CurrentGraph.Text = "CPU Utilisation";
            //Resets the graph
            x = 0;
            points.Clear();
        }
        private void gpuTempClick(object sender, RoutedEventArgs e)
        {
            graphView = "gpuTemp";
            CurrentGraph.Text = "GPU Tempurature";
            //Resets the graph
            x = 0;
            points.Clear();
        }
        private void ramClick(object sender, RoutedEventArgs e)
        {
            graphView = "ramUtil";
            CurrentGraph.Text = "RAM Utilisation";
            //Resets the graph
            x = 0;
            points.Clear();
        }

        private void settingsClick(object sender, RoutedEventArgs e)
        {
            Settings settingsWin = new Settings();
            settingsWin.Show();
        }

        private void Drives_Loaded(object sender, RoutedEventArgs e)
        {
            var drivesVar = sender as ComboBox;
            drivesVar.ItemsSource = g.getdriveList();
            drivesVar.SelectedIndex = 0; 
        }

        private void Drives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var drivesVar = sender as ComboBox;
            string selected = drivesVar.SelectedItem as string;
            MessageBox.Show(selected);
        }
    }

}


