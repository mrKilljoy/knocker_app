using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using KnockerLib;

namespace KnockerWPF.ViewModels
{
    class PingTabVM : ViewModelBase
    {
        #region Fields
        private Commands.RelayCommand _pingCheckCmd;
        private Commands.RelayCommand _openDialogCmd;
        private Commands.RelayCommand _removePointCmd;
        private Commands.RelayCommand _clearListCmd;

        private int _pingCheckTimeout = 5000;
        private bool _isPingCheckRunning;
        private ObservableCollection<DestinationPoint> _points;
        private int _selectedRow = -1;
        private bool _canDeleteItem;
        private bool _isAvailable;
        #endregion

        public PingTabVM()
        {
            _points = new ObservableCollection<DestinationPoint>();
        }

        #region Props
        public bool IsPingCheckRunning
        {
            get { return _isPingCheckRunning; }
            set
            {
                _isPingCheckRunning = value;
                OnPropertyChanged();
            }
        }

        public int PingCheckTimeout
        {
            get { return _pingCheckTimeout; }
            set
            {
                _pingCheckTimeout = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DestinationPoint> Points
        {
            get { return _points; }
            set
            {
                _points = value;
                OnPropertyChanged();
            }
        }

        public int SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                _selectedRow = value;
                OnPropertyChanged();

                CanDeleteItem = _selectedRow > -1;
            }
        }

        public bool CanDeleteItem
        {
            get { return _canDeleteItem; }
            set
            {
                _canDeleteItem = value;
                OnPropertyChanged();
            }
        }

        public bool IsOperationAvailable
        {
            get { return _isAvailable; }
            set
            {
                _isAvailable = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods
        private void OpenDialogWindow(object temp)
        {
            if (IsPingCheckRunning)
            {
                System.Windows.MessageBox.Show("Please wait until the end of current operation.", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var wnd = new Views.modal_newroom();
            var vm = new ModalVM();

            wnd.DataContext = vm;
            wnd.ShowDialog();

            if (vm.CurrentPoint == null)
                return;

            if(Points.FirstOrDefault(p=>p.Address == vm.CurrentPoint.Address) == null)
                Points.Add(vm.CurrentPoint);
            else
            {
                System.Windows.MessageBox.Show("Point with this address is already exists!", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            IsOperationAvailable = _points.Count > 0;
        }

        private void PingCheck(object temp)
        {
            if (IsPingCheckRunning)
                return;

            if (Points.Count == 0)
            {
                System.Windows.MessageBox.Show("Checklist is empty!", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            PointCheckSettings.PingCheckTimeout = _pingCheckTimeout;

            IsPingCheckRunning = true;

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < Points.Count; i++)
            {
                var current_point = Points[i];

                Task t = new Task(() =>
                 {
                     PointTester.PointPingCheckAction(current_point);
                 });
                tasks.Add(t);
                t.Start();
            }

            Task.WhenAll(tasks).ContinueWith(a =>
            {
                IsPingCheckRunning = false;
            });
            //Task t = Task.Run(() =>
            //{
            //    PointTester.PointPingCheckAction(null);
            //});

            //t.ContinueWith((a) =>
            //{
            //    IsPingCheckRunning = false;
            //});
        }

        private void RemoveSelectedPoint(object param)
        {
            if (IsPingCheckRunning)
            {
                System.Windows.MessageBox.Show("Please wait until end of current operation.", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (SelectedRow < 0)
                return;

            int index = SelectedRow;
            Points.RemoveAt(index);

            IsOperationAvailable = _points.Count > 0;
        }

        private void ClearPointList(object param)
        {
            if (IsPingCheckRunning)
            {
                System.Windows.MessageBox.Show("Please wait until end of current operation.", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            Points.Clear();

            IsOperationAvailable = _points.Count > 0;
        }
        #endregion

        #region Commands
        public ICommand PingCheckCommand
        {
            get { return _pingCheckCmd ?? (_pingCheckCmd = new Commands.RelayCommand(PingCheck)); }
        }

        public ICommand OpenDialogCommand
        {
            get { return _openDialogCmd ?? (_openDialogCmd = new Commands.RelayCommand(OpenDialogWindow)); }
        }
        
        public ICommand RemovePointCommand
        {
            get { return _removePointCmd ?? (_removePointCmd = new Commands.RelayCommand(RemoveSelectedPoint)); }
        }

        public ICommand ClearListCommand
        {
            get { return _clearListCmd ?? (_clearListCmd = new Commands.RelayCommand(ClearPointList)); }
        }
        #endregion
    }
}
