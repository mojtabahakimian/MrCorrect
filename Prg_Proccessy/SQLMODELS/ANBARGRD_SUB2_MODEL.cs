using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// این کلاس اکنون IEditableObject را پیاده‌سازی می‌کند
    /// تا عملیات BeginEdit, EndEdit, و CancelEdit را پشتیبانی کند.
    /// این کار به DataGrid اجازه می‌دهد تا تغییرات سطر را (Undo) کند.
    /// </summary>
    public class ANBARGRD_SUB2_MODEL : INotifyPropertyChanged, ICloneable, IEditableObject
    {
        // 1. متغیرهایی برای نگهداری نسخه پشتیبان و وضعیت ویرایش
        private ANBARGRD_SUB2_MODEL _backupData;
        private bool _isEditing = false;

        public object Clone()
        {
            // MemberwiseClone یک کپی سطحی (Shallow Copy) ایجاد می‌کند
            // که برای این کلاس (که فقط شامل انواع داده ساده است) کافی می‌باشد.
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Properties
        private double? _ekh;
        public double? EKH { get => _ekh; set { if (_ekh == value) return; _ekh = value; OnPropertyChanged("EKH"); } }
        private int? _grd_num;
        public int? GRD_NUM { get => _grd_num; set { if (_grd_num == value) return; _grd_num = value; OnPropertyChanged("GRD_NUM"); } }
        private string? _code;
        public string? CODE { get => _code; set { if (_code == value) return; _code = value; OnPropertyChanged("CODE"); } }
        private string? _nam;
        public string? nam { get => _nam; set { if (_nam == value) return; _nam = value; OnPropertyChanged("nam"); } }
        private decimal? _mog;
        public decimal? MOG { get => _mog; set { if (_mog == value) return; _mog = value; OnPropertyChanged("MOG"); } }
        private double? _num1;
        public double? NUM1 { get => _num1; set { if (_num1 == value) return; _num1 = value; OnPropertyChanged("NUM1"); } }
        private double? _num2;
        public double? NUM2 { get => _num2; set { if (_num2 == value) return; _num2 = value; OnPropertyChanged("NUM2"); } }
        private double? _num3;
        public double? NUM3 { get => _num3; set { if (_num3 == value) return; _num3 = value; OnPropertyChanged("NUM3"); } }
        private double? _mabl;
        public double? MABL { get => _mabl; set { if (_mabl == value) return; _mabl = value; OnPropertyChanged("MABL"); } }
        private string? _names;
        public string? NAMES { get => _names; set { if (_names == value) return; _names = value; OnPropertyChanged("NAMES"); } }
        private string? _n_fani;
        public string? N_FANI { get => _n_fani; set { if (_n_fani == value) return; _n_fani = value; OnPropertyChanged("N_FANI"); } }
        private string? _grp;
        public string? grp { get => _grp; set { if (_grp == value) return; _grp = value; OnPropertyChanged("grp"); } }
        #endregion

        #region IEditableObject Implementation

        /// <summary>
        /// 2. این متد توسط DataGrid فراخوانی می‌شود
        /// زمانی که کاربر شروع به ویرایش یک سطر می‌کند.
        /// </summary>
        public void BeginEdit()
        {
            // اگر در حال ویرایش نیستیم، یک کپی پشتیبان تهیه کن
            if (!_isEditing)
            {
                _backupData = (ANBARGRD_SUB2_MODEL)this.MemberwiseClone();
                _isEditing = true;
            }
        }

        /// <summary>
        /// 3. این متد توسط DataGrid فراخوانی می‌شود
        /// زمانی که کاربر ویرایش را تایید می‌کند (مثلاً به سطر دیگری می‌رود).
        /// </summary>
        public void EndEdit()
        {
            if (_isEditing)
            {
                // ویرایش موفق بود، داده پشتیبان را پاک کن
                _backupData = null;
                _isEditing = false;
            }
        }

        /// <summary>
        /// 4. این متد توسط DataGrid فراخوانی می‌شود
        /// زمانی که کاربر کلید Escape را فشار می‌دهد. (عملیات Undo)
        /// </summary>
        public void CancelEdit()
        {
            if (_isEditing && _backupData != null)
            {
                // تمام مقادیر را از پشتیبان به آبجکت فعلی بازگردان
                // فراخوانی تک تک Property ها باعث می‌شود INotifyPropertyChanged
                // فعال شده و UI دیتاگرید به‌روزرسانی شود.
                this.EKH = _backupData.EKH;
                this.GRD_NUM = _backupData.GRD_NUM;
                this.CODE = _backupData.CODE;
                this.nam = _backupData.nam;
                this.MOG = _backupData.MOG;
                this.NUM1 = _backupData.NUM1;
                this.NUM2 = _backupData.NUM2;
                this.NUM3 = _backupData.NUM3;
                this.MABL = _backupData.MABL;
                this.NAMES = _backupData.NAMES;
                this.N_FANI = _backupData.N_FANI;
                this.grp = _backupData.grp;

                // از حالت ویرایش خارج شو و پشتیبان را پاک کن
                _isEditing = false;
                _backupData = null;
            }
        }

        #endregion
    }
}
