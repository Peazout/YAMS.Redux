using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using YAMS.Redux.Core.Helpers;
using YAMS.Redux.Data;

namespace YAMS.Redux.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static IEnumerable<LogRow> LogRows;
        
        #region NLOG

        /// <summary>
        /// Get our logger, if null get the current one.
        /// </summary>
        private static NLog.Logger MyLog
        {
            get
            {
                if (_MyLog == null) { _MyLog = NLog.LogManager.GetCurrentClassLogger(); }
                return _MyLog;
            }

        }
        private static NLog.Logger _MyLog;
        /// <summary>
        /// Return a logevent for nlog.
        /// </summary>
        /// <param name="Lvl"></param>
        /// <param name="Message"></param>
        /// <param name="ServerId"></param>
        /// <returns></returns>
        private static NLog.LogEventInfo GetLogEvent(NLog.LogLevel Lvl, string Message, int ServerId = -1)
        {
            NLog.LogEventInfo theEvent = new NLog.LogEventInfo(Lvl, MyLog.Name, Message);
            if (ServerId != -1) theEvent.Properties["ServerId"] = ServerId;
            // theEvent.LoggerName = MyLog.Name;

            return theEvent;
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Servers.ItemsSource = MinecraftServerHelper.Servers;
            ServerLog.ItemsSource = LogRows;

            Thread work = new Thread(new ThreadStart(YAMS.Redux.Core.AppCore.Execute));
            work.Start();

        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<int, MinecraftServer> mc = new KeyValuePair<int, MinecraftServer>();
            BackgroundWorker bw = sender as BackgroundWorker;

            do
            {
                Servers.Dispatcher.Invoke(new System.Action(() => { if (Servers.SelectedItem != null) mc = (KeyValuePair<int, MinecraftServer>)Servers.SelectedItem; }));
                if (mc.Value != null) LogRows = DBHelper.GetLog(serverid: mc.Value.Id);
                ServerLog.Dispatcher.Invoke(new System.Action(() => { ServerLog.ItemsSource = LogRows; }));
                Thread.Sleep(5000);

            } while (!e.Cancel);


        }

        private void OpenAdmin_Click(object sender, RoutedEventArgs e)
        {
            MyLog.Debug("Opening admin webpage: {adminpage}", FilesAndFoldersHelper.HttpWebAdmin);
            System.Diagnostics.Process.Start(FilesAndFoldersHelper.HttpWebAdmin);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // pbStatus.Value = e.ProgressPercentage;
        }

        private void Servers_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            KeyValuePair<int, MinecraftServer> mc = new KeyValuePair<int, MinecraftServer>();
            if (Servers.SelectedItem != null) mc = (KeyValuePair<int, MinecraftServer>)Servers.SelectedItem;
            if (mc.Value != null) LogRows = DBHelper.GetLog(serverid: mc.Value.Id);
            ServerLog.ItemsSource = LogRows;

        }

    }
}
