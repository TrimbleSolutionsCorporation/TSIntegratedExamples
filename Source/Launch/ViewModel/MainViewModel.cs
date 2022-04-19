namespace Listener.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Timers;
    using System.Windows.Input;
    using Tekla.Structures.Model;

    public class MainViewModel : SmartBindingBase
    {
        private int _timerInterval = 3000;
        private Timer _timer;

        public bool IsConnected
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public bool IsModelOpen
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public bool IsBusy
        {
            get { return GetDynamicValue<bool>(); }
            set { SetDynamicValue(value); }
        }

        public ModelInfo ModelInfo
        {
            get { return GetDynamicValue<ModelInfo>(); }
            set { SetDynamicValue(value); }
        }

        public TimeSpan ListenTime
        {
            get { return GetDynamicValue<TimeSpan>(); }
            set { SetDynamicValue(value); }
        }

        public DateTime StartTime
        {
            get { return GetDynamicValue<DateTime>(); }
            set { SetDynamicValue(value); }
        }

        public MainViewModel()
        {
            this.IsBusy = false;
            this.StartTime = DateTime.Now;
        }

        public ICommand OnLoadedCommand => new CommandWrapper(this.OnLoaded);

        private void OnLoaded(object parameter)
        {
            this._timer = new Timer(this._timerInterval);
            this._timer.Elapsed += this.CheckUpdateStatus;
            this._timer.Start();
        }

        public ICommand OnClosingCommand => new CommandWrapper(this.OnClosing);

        private void OnClosing(object parameter)
        {
            if (this._timer == null) return;
            this._timer.Elapsed -= this.CheckUpdateStatus;
            this._timer.Stop();
            this._timer.Dispose();
            this._timer = null;
        }

        private void CheckUpdateStatus(object sender, ElapsedEventArgs e)
        {
            IsBusy = true;
            try
            {
                this._timer?.Stop();

                var TsIsConnectable = MainLogic.CheckTeklaStructuresStatus();

                //Check if Tekla Structures is running first
                //var tsProcess = MainLogic.GetTeklaProcess();
                //if (!tsProcess.Any())
                //{
                //    this.SetStatusNotConnected();
                //    return;
                //}

                //foreach (var p in tsProcess)
                //{
                //    if (p.Modules.Count < 360)
                //    {
                //        return;
                //    }
                //}
                if (TsIsConnectable == 0 )
                {
                    var tModel = new Model();
                    this.IsConnected = tModel.GetConnectionStatus();
                }
                //If Tekla Structures is running, try connection via API
                //var tModel = new Model();
                //this.IsConnected = tModel.GetConnectionStatus();
                if (this.IsConnected )
                {
                    var tModel = new Model();
                    //the connection breaks here sometimes
                    this.ModelInfo = tModel.GetInfo(); 
                    this.IsModelOpen = !string.IsNullOrEmpty(this.ModelInfo.ModelPath);
                }
                else
                {
                    this.IsModelOpen = false;
                    this.ModelInfo = null;
                }
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                if (this.IsConnected)
                {
                    this.IsModelOpen = false;
                    this.ModelInfo = null;
                }
                else
                {
                    this.SetStatusNotConnected();
                }
                //Expected - not connected yet
                //todo: find if can re-activate connection once failed?

                // ends up here if there is issue getting ModelInfo even if IsConnected is true
                // need to restart the app?
            }
            catch (Exception ex)
            {
                this.SetStatusNotConnected();
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                this.ListenTime = this.StartTime.Subtract(DateTime.Now);
                this._timer?.Start();
                IsBusy = false;
            }
        }

        public ICommand StartTeklaCommand => new CommandWrapper(this.StartTekla);
        private void StartTekla(object parameter)
        {
            IsBusy = true;
            MainLogic.StartTeklaStructures();
            IsBusy = false;
        }

        private void SetStatusNotConnected()
        {
            this.IsConnected = false;
            this.IsModelOpen = false;
            this.ModelInfo = null;
        }
    }
}
