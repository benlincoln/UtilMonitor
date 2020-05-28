using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtilMonitor.Non_WPF_Code
{
    class Notification
    {
        private readonly NotifyIcon _notifyIcon;

        public Notification()
        {
            _notifyIcon = new NotifyIcon();
            // Extracts app's icon and uses it as notify icon
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            // Hides the icon when the notification is closed
            _notifyIcon.BalloonTipClosed += (s, e) => _notifyIcon.Visible = false;
        }

        public void ShowNotification(string message)
        {
            _notifyIcon.Visible = true;
            // Shows notification
            _notifyIcon.ShowBalloonTip(3000, "Simple Hardware Monitor", message, ToolTipIcon.Info);
        }

    }
}
