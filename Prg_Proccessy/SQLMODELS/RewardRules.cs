using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_Proccessy.SQLMODELS
{
    public class RewardRules : INotifyPropertyChanged, ICloneable
    {
        public object Clone() { return this.MemberwiseClone(); }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        public RewardRules()
        {
            IsActive = true;
        }

        private int? _ruleid;
        public int? RuleID { get => _ruleid; set { if (_ruleid == value) return; _ruleid = value; OnPropertyChanged("RuleID"); } }
        private string? _productid_target;
        public string? ProductID_Target { get => _productid_target; set { if (_productid_target == value) return; _productid_target = value; OnPropertyChanged("ProductID_Target"); } }
        private int? _quantity_threshold;
        public int? Quantity_Threshold { get => _quantity_threshold; set { if (_quantity_threshold == value) return; _quantity_threshold = value; OnPropertyChanged("Quantity_Threshold"); } }
        private string? _reward_type;
        public string? Reward_Type { get => _reward_type; set { if (_reward_type == value) return; _reward_type = value; OnPropertyChanged("Reward_Type"); } }
        private string? _reward_productid;
        public string? Reward_ProductID { get => _reward_productid; set { if (_reward_productid == value) return; _reward_productid = value; OnPropertyChanged("Reward_ProductID"); } }
        private int? _reward_quantity;
        public int? Reward_Quantity { get => _reward_quantity; set { if (_reward_quantity == value) return; _reward_quantity = value; OnPropertyChanged("Reward_Quantity"); } }
        private decimal? _reward_discount_percentage;
        public decimal? Reward_Discount_Percentage { get => _reward_discount_percentage; set { if (_reward_discount_percentage == value) return; _reward_discount_percentage = value; OnPropertyChanged("Reward_Discount_Percentage"); } }
        private bool? _isactive;
        public bool? IsActive { get => _isactive; set { if (_isactive == value) return; _isactive = value; OnPropertyChanged("IsActive"); } }
        private long? _startdate;
        public long? StartDate { get => _startdate; set { if (_startdate == value) return; _startdate = value; OnPropertyChanged("StartDate"); } }
        private long? _enddate;
        public long? EndDate { get => _enddate; set { if (_enddate == value) return; _enddate = value; OnPropertyChanged("EndDate"); } }
        private string? _description;
        public string? Description { get => _description; set { if (_description == value) return; _description = value; OnPropertyChanged("Description"); } }
        private DateTime? _crt;
        public DateTime? CRT { get => _crt; set { if (_crt == value) return; _crt = value; OnPropertyChanged("CRT"); } }
        private int? _uid;
        public int? UID { get => _uid; set { if (_uid == value) return; _uid = value; OnPropertyChanged("UID"); } }
    }
}
