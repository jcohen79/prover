using reslab;
using reslab.test;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SimpleController
{
    public class Wti
    {
        public int nId;
        public Ici iciLoad;
        public Stopwatch watch;
        public BackgroundWorker sender;
        public Wts wts;

        static int msUpdateRate = 1000;
        long msNext;

        public void DoWork()
        {
            watch.Start();
            msNext = msUpdateRate;
            long cExprs = 0;
            long cExprsPrev = 0;

            while (true)
            {
                if (wts.fPaused)
                {
                    wts.tPause();
                }

                iciLoad.DoStep();
                cExprs++;

                long msNow = watch.ElapsedMilliseconds;

                if (msNow > msNext)
                {
                    msNext = msNext + msUpdateRate;
                    long cExprsSince = cExprs - cExprsPrev;
                    cExprsPrev = cExprs;

                    wts.tReport(this, cExprsSince);
                }
                // else
                //    (sender as BackgroundWorker).ReportProgress(progressPercentage);

            }
        }
    }

    public class Wts
    {
        public volatile bool fPaused = false;

        public delegate void TPause();
        public delegate void TReport(Wti wti, long nExprsSince);

        public TPause tPause;
        public TReport tReport;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool fStarted = false;
        Wts wts = new Wts();
        int nNumWorkers = 3;
        Wti[] rgWti;
        Icf ictFactory;
        int nMaxBufChars = 100000;
        long nExprsTotal = 0;
        long nExprsPrev = 0;
        long msPrev;
        public Stopwatch watch;

        static EventWaitHandle _waitHandle = new ManualResetEvent(false);

        void Pause()
        {
            _waitHandle.WaitOne();
            Dispatcher.BeginInvoke(
              (Action)(() =>
              {
                  buttonPause.IsEnabled = true;
              }));
        }

        void Report (Wti wti, long nExprsSince)
        {
            lock (this)
            {
                nExprsTotal += nExprsSince;
            }
            long msNow = watch.ElapsedMilliseconds;
            if (msNow - msPrev > 1000)
            {
                wti.sender.ReportProgress(0, wti);
                Thread.Sleep(50);
            }
        }

        public MainWindow()
        {

            ictFactory = new SatTestFactory(new Gid(3, false)); // , Gid.nTypical()));    // TODO: allow Gid to be controlled via gui

            InitializeComponent();
            wts.tPause = Pause;
            wts.tReport = Report;
            watch = new Stopwatch();

            rgWti = new Wti[nNumWorkers];
            for (int i = 0; i < nNumWorkers; i++)
            {
                Wti wti = new Wti();
                wti.nId = i;
                wti.iciLoad = ictFactory.iciMakeInstance(i);
                wti.watch = new Stopwatch();
                wti.wts = wts;
                rgWti[i] = wti;
            }

            //GraphMatching.WebServer._main();
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            buttonPause.IsEnabled = false;
            buttonGo.IsEnabled = true;
            _waitHandle.Reset();
            wts.fPaused = true;
        }
        // TODO: have worker call UI to enable GO

        private void buttonGo_Click(object sender, RoutedEventArgs e)
        {
            buttonGo.IsEnabled = false;
            if (fStarted)
            {
                _waitHandle.Set();
                wts.fPaused = false;
            }
            else
            {
                buttonPause.IsEnabled = true;
                fStarted = true;
                watch.Start();
                for (int i = 0; i < nNumWorkers; i++)
                {

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.WorkerReportsProgress = true;
                    worker.DoWork += worker_DoWork;
                    worker.ProgressChanged += worker_ProgressChanged;
                    worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                    worker.RunWorkerAsync(rgWti[i]);
                }
            }
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Wti wti = (Wti)e.Argument;
            wti.sender = sender as BackgroundWorker;
            wti.DoWork();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                Wti wti = (Wti)e.UserState;

                long msNow = watch.ElapsedMilliseconds;
                // double dRate = 1000.0 * ((double)(nExprsTotal - nExprsPrev)) / (double)(msNow - msPrev);  - need to acc time over threads
                double dRate = 1000.0 * ((double)(nExprsTotal)) / (double)(msNow);
                String stStatus = String.Format("elapsed={0:n0} exprs={1:n0} rate={2:n0}", msPrev / 1000, nExprsTotal, dRate);
                msPrev = msNow;
                nExprsPrev = nExprsTotal;

                textNumSequents.Text = stStatus;

                string stProgress = wti.iciLoad.stProgress();
                string stText = textBlock.Text;
                int nChars = stText.Length + stProgress.Length;
                if (nChars > nMaxBufChars)
                {
                    if (nChars > stText.Length)
                        stText = "";
                    else
                        stText = stText.Substring(nChars - nMaxBufChars);
                }
                textBlock.Text = stText + stProgress;
                textBlockScrollViewer.ScrollToEnd();
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
    }
}
