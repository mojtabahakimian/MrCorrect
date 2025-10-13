using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Prg_Proccessy.SQLMODELS
{
    public class GRADE_TAB_FT : INotifyPropertyChanged, IEditableObject
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string strCaller = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strCaller));
        }
        public GRADE_TAB_FT()
        {
            GFGZARIB = 0;

            GRADEGRPFT ??= new ObservableCollection<GRADE_GRP_FT?>();
        }
        private ObservableCollection<GRADE_GRP_FT?> _GRADEGRPFT;
        public ObservableCollection<GRADE_GRP_FT?> GRADEGRPFT
        {
            get => _GRADEGRPFT;
            set
            {
                if (_GRADEGRPFT == value) return;
                _GRADEGRPFT = value;
                OnPropertyChanged(nameof(GRADEGRPFT));
            }
        }

        private int? _gfid;
        public int? GFID { get => _gfid; set { if (_gfid == value) return; _gfid = value; OnPropertyChanged("GFID"); } }

        private int? _gftid;
        public int? GFTID { get => _gftid; set { if (_gftid == value) return; _gftid = value; OnPropertyChanged("GFTID"); } }

        private string? _gfnameft;
        public string? GFNAMEFT { get => _gfnameft; set { if (_gfnameft == value) return; _gfnameft = value; OnPropertyChanged("GFNAMEFT"); } }

        private Single? _gfgzarib;
        public Single? GFGZARIB { get => _gfgzarib; set { if (_gfgzarib == value) return; _gfgzarib = value; OnPropertyChanged("GFGZARIB"); } }

        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }

        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }

        private GRADE_TAB_FT _backup;
        private bool _inEdit;

        public void BeginEdit()
        {
            if (_inEdit) return;
            _backup = (GRADE_TAB_FT)this.Clone();
            _inEdit = true;
        }

        public void CancelEdit()
        {
            if (!_inEdit || _backup is null) return;

            GFID = _backup.GFID;
            GFTID = _backup.GFTID;
            GFNAMEFT = _backup.GFNAMEFT;
            GFGZARIB = _backup.GFGZARIB;
            CRT = _backup.CRT;
            UID = _backup.UID;

            _inEdit = false;
            _backup = null;
        }

        public void EndEdit()
        {
            if (!_inEdit) return;
            _inEdit = false;
            _backup = null;
        }
    }
}
