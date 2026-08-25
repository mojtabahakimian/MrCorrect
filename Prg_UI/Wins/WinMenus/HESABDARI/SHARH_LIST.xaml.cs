using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prg_UI.Wins.WinMenus.HESABDARI
{
    public partial class SHARH_LIST : Window, INotifyPropertyChanged
    {
        public string SelectedSharh { get; private set; } = null;

        private List<SharhModel> _sharhList;
        public List<SharhModel> SharhList
        {
            get { return _sharhList; }
            set
            {
                _sharhList = value;
                OnPropertyChanged();
            }
        }

        public SHARH_LIST()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // وقتی این پنجره از داخل CellEditEnding یک گرید دیگر (مثلا PGET_LST_SUB_CellEditEnding) و به‌صورت
            // deferred باز می‌شود، فوکوس پیش‌فرض ویندوز روی MasterDataGrid نمی‌نشیند و کیبورد (بالا/پایین/Enter)
            // کار نمی‌کند تا کاربر اول با ماوس داخل گرید کلیک کند؛ اینجا صریحاً فوکوس را روی گرید می‌گذاریم.
            if (MasterDataGrid.Items.Count > 0)
            {
                MasterDataGrid.SelectedIndex = 0;
            }

            Keyboard.Focus(MasterDataGrid);
            MasterDataGrid.Focus();
        }

        private void LoadData()
        {
            try
            {
                var dbms = new Prg_SendInvoice.CNNMANAGER.CL_CCNNMANAGER();
                SharhList = dbms.DoGetDataSQL<SharhModel>("SELECT ID, SHARH FROM dbo.SHARH ORDER BY ID").ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا در بارگذاری شرح آماده: " + ex.Message);
            }
        }

        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MasterDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectAndClose();
        }

        private void MasterDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SelectAndClose();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void SelectAndClose()
        {
            if (MasterDataGrid.SelectedItem is SharhModel selectedItem)
            {
                SelectedSharh = selectedItem.SHARH;
                this.DialogResult = true;
                this.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class SharhModel
    {
        public int ID { get; set; }
        public string SHARH { get; set; }
    }
}
