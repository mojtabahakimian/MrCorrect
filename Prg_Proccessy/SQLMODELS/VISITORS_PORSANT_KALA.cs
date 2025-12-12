using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class VISITORS_PORSANT_KALA : INotifyPropertyChanged, IEditableObject
    {
        public VISITORS_PORSANT_KALA()
        {
            PORSANT = 0;
        }

        // ==========================================
        // Fields (متغیرهای اصلی)
        // ==========================================
        private long _id; // <--- Added Field
        private int? _porId;
        private string? _code;
        private double? _porsant;

        // ==========================================
        // Backup Fields (برای ذخیره موقت جهت قابلیت Cancel)
        // ==========================================
        private long _backupId; // <--- Added Backup Field
        private int? _backupPorId;
        private string? _backupCode;
        private double? _backupPorsant;
        private bool _inEdit;

        // ==========================================
        // Properties (پراپرتی‌ها با اطلاع‌رسانی به UI)
        // ==========================================

        // <--- Added Property
        public long ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? PORID
        {
            get => _porId;
            set
            {
                if (_porId != value)
                {
                    _porId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CODE
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? PORSANT
        {
            get => _porsant;
            set
            {
                if (_porsant != value)
                {
                    _porsant = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _name_code;
        public string NAME_CODE
        {
            get => _name_code;
            set { if (_name_code == value) return; _name_code = value; OnPropertyChanged(nameof(NAME_CODE)); }
        }

        // ==========================================
        // INotifyPropertyChanged Implementation
        // (اطلاع به دیتاگرید جهت رفرش شدن)
        // ==========================================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ==========================================
        // IEditableObject Implementation
        // (پیاده‌سازی قابلیت Cancel و Edit)
        // ==========================================

        // 1. وقتی کاربر دوبار کلیک می‌کند تا ویرایش شروع شود
        public void BeginEdit()
        {
            if (!_inEdit)
            {
                // گرفتن بک‌آپ از مقادیر فعلی
                _backupId = _id; // <--- Backup ID
                _backupPorId = _porId;
                _backupCode = _code;
                _backupPorsant = _porsant;
                _inEdit = true;
            }
        }

        // 2. وقتی کاربر دکمه ESC را می‌زند (لغو تغییرات)
        public void CancelEdit()
        {
            if (_inEdit)
            {
                // بازگرداندن مقادیر از بک‌آپ
                _id = _backupId; // <--- Restore ID
                _porId = _backupPorId;
                _code = _backupCode;
                _porsant = _backupPorsant;
                _inEdit = false;

                // چون مقادیر را دستی برگرداندیم، باید به UI بگوییم رفرش شود
                OnPropertyChanged(nameof(ID)); // <--- Refresh ID UI
                OnPropertyChanged(nameof(PORID));
                OnPropertyChanged(nameof(CODE));
                OnPropertyChanged(nameof(PORSANT));
            }
        }

        // 3. وقتی کاربر اینتر می‌زند یا از سطر خارج می‌شود (تایید تغییرات)
        public void EndEdit()
        {
            if (_inEdit)
            {
                // تغییرات تایید شده است، نیازی به بک‌آپ نیست
                _backupPorId = null;
                _backupCode = null;
                _backupPorsant = null;
                // Note: _backupId is a value type (long), so we don't set it to null.

                _inEdit = false;
            }
        }
    }
}