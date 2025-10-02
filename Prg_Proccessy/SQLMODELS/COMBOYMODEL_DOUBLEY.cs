using System.ComponentModel;
using System;

namespace Prg_Proccessy.SQLMODELS
{
    public class COMBOYMODEL_DOUBLEY : INotifyPropertyChanged, ICloneable
    {
        public object Clone()
        {
            // Creates a shallow copy of the object
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double _ID;
        /// <summary>
        /// The unique identifier, converted to double.
        /// </summary>
        public double ID
        {
            get { return _ID; }
            set
            {
                if (_ID == value) return; // Optimization: only update if the value is different
                _ID = value;
                OnPropertyChanged(nameof(ID)); // Notify listeners that the property has changed
            }
        }

        private string _NAME = string.Empty; // Initializing to string.Empty is a common best practice
        /// <summary>
        /// The name associated with the model.
        /// </summary>
        public string NAME
        {
            get { return _NAME; }
            set
            {
                if (_NAME == value) return; // Optimization
                _NAME = value;
                OnPropertyChanged(nameof(NAME)); // Notify listeners
            }
        }
    }
}