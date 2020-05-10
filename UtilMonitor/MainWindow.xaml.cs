using System;
using System.Windows;
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
                GPUTemp.Text = g.getGPUTemp();
                RAM.Text = g.getRAM();
                CPUTemp.Text = g.getCPUTemp();
                //For drawing the graph, could likely move into it's own function as well which would return a Polyline object 
                double xMax = Graph.Width;
                double yMax = Graph.Height;
                int currentY = 0;
                //The canvas uses the top left as (0,0)
                currentY = Convert.ToInt32(yMax) - g.getCPUUtil();
                points.Add(new Point(x, currentY));
                //Increments the x value along the graph, smaller the x value, the tighter the points on the graph and thus the more points shown before the graph resets
                x += 5;
                Polyline polyline = new Polyline();
                polyline.StrokeThickness = 1;
                polyline.Stroke = Brushes.White;
                polyline.Points = points;
                Graph.Children.Add(polyline);
                if (x == xMax)
                {
                    x = 0;
                    points.Clear();
                }
            });
        }

    }

}


