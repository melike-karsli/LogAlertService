using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class TrayContext : ApplicationContext
{
    NotifyIcon _notifyIcon;
    Timer _timer;
    bool _blink;

    string _basePath;

    // Small invisible form used so the app gets a taskbar button with an icon
    Form _taskbarForm;

    public TrayContext()
    {
        _basePath = AppDomain.CurrentDomain.BaseDirectory;

        // Keep NotifyIcon for compatibility (context menu, balloon, etc.)
        // but do not rely on it being visible in the overflow area.
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon(Path.Combine(_basePath, "green.ico")),
            Visible = false,
            Text = "RestoPOS Internet Monitor"
        };

        // Create a tiny invisible form that will produce a taskbar button with an icon.
        _taskbarForm = new Form
        {
            Size = new Size(1, 1),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-100, -100), // keep off-screen
            ShowInTaskbar = true,
            ShowIcon = true,
            Text = "RestoPOS Internet Monitor",
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            Opacity = 0.0 // fully transparent
        };

        // Set initial icon for the taskbar button
        try
        {
            _taskbarForm.Icon = new Icon(Path.Combine(_basePath, "green.ico"));
        }
        catch
        {
            // ignore icon load errors
        }

        // Must call Show so Windows creates the taskbar button
        _taskbarForm.Show();

        _timer = new Timer();
        _timer.Interval = 2000;
        _timer.Tick += CheckStatus;
        _timer.Start();
    }

    void CheckStatus(object sender, EventArgs e)
    {
        string path = @"C:\RestoPOS\status.txt";
        if (!File.Exists(path))
            return;

        string status = File.ReadAllText(path).Trim();

        Icon nextIcon;
        if (status == "ERROR")
        {
            nextIcon = _blink
                ? new Icon(Path.Combine(_basePath, "red.ico"))
                : new Icon(Path.Combine(_basePath, "green.ico"));

            _blink = !_blink;
        }
        else
        {
            nextIcon = new Icon(Path.Combine(_basePath, "green.ico"));
        }

        // Update NotifyIcon (if visible) and the taskbar form icon
        try
        {
            _notifyIcon.Icon = nextIcon;
        }
        catch
        {
            // ignore icon set errors
        }

        try
        {
            _taskbarForm.Icon = (Icon)nextIcon.Clone();
        }
        catch
        {
            // ignore icon set errors
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= CheckStatus;
                _timer.Dispose();
                _timer = null;
            }

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_taskbarForm != null)
            {
                try
                {
                  

                    _taskbarForm.Close();
                }
                catch { }
                _taskbarForm.Dispose();
                _taskbarForm = null;
            }
        }

        base.Dispose(disposing);
    }
}