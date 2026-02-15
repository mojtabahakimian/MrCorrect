using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class ORDR_HED : INotifyPropertyChanged
    {
        private int _id;
        public int id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged();
            }
        }

        private long _date;
        public long DATE
        {
            get => _date;
            set
            {
                if (_date == value) return;
                _date = value;
                OnPropertyChanged();
            }
        }

        private string _molah;
        public string MOLAH
        {
            get => _molah;
            set
            {
                if (_molah == value) return;
                _molah = value;
                OnPropertyChanged();
            }
        }

        private double? _n_s;
        public double? N_S
        {
            get => _n_s;
            set
            {
                if (_n_s == value) return;
                _n_s = value;
                OnPropertyChanged();
            }
        }

        private string _cust_no;
        public string CUST_NO
        {
            get => _cust_no;
            set
            {
                if (_cust_no == value) return;
                _cust_no = value;
                OnPropertyChanged();
            }
        }

        private string _user_name;
        public string USER_NAME
        {
            get => _user_name;
            set
            {
                if (_user_name == value) return;
                _user_name = value;
                OnPropertyChanged();
            }
        }

        private string _sharayet;
        public string SHARAYET
        {
            get => _sharayet;
            set
            {
                if (_sharayet == value) return;
                _sharayet = value;
                OnPropertyChanged();
            }
        }

        private int? _sgn1usid;
        public int? sgn1usid
        {
            get => _sgn1usid;
            set
            {
                if (_sgn1usid == value) return;
                _sgn1usid = value;
                OnPropertyChanged();
            }
        }

        private int? _sgn2usid;
        public int? sgn2usid
        {
            get => _sgn2usid;
            set
            {
                if (_sgn2usid == value) return;
                _sgn2usid = value;
                OnPropertyChanged();
            }
        }

        private int? _sgn3usid;
        public int? sgn3usid
        {
            get => _sgn3usid;
            set
            {
                if (_sgn3usid == value) return;
                _sgn3usid = value;
                OnPropertyChanged();
            }
        }

        private bool _sgn1;
        public bool SGN1
        {
            get => _sgn1;
            set
            {
                if (_sgn1 == value) return;
                _sgn1 = value;
                OnPropertyChanged();
            }
        }

        private bool _sgn2;
        public bool SGN2
        {
            get => _sgn2;
            set
            {
                if (_sgn2 == value) return;
                _sgn2 = value;
                OnPropertyChanged();
            }
        }

        private bool _sgn3;
        public bool SGN3
        {
            get => _sgn3;
            set
            {
                if (_sgn3 == value) return;
                _sgn3 = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _crt;
        public DateTime? CRT
        {
            get => _crt;
            set
            {
                if (_crt == value) return;
                _crt = value;
                OnPropertyChanged();
            }
        }

        private int? _uid;
        public int? UID
        {
            get => _uid;
            set
            {
                if (_uid == value) return;
                _uid = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
