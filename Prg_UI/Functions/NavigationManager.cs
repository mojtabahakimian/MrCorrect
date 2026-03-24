using AUTO_BAZ.Functions;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.HelperWins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static Interfaces.INavigator;

namespace Functions
{
    public interface INavigationManager
    {
        int CurrentRecordIndex { get; }
        int RecordsCount { get; }
        bool IsNewRecord { get; }
        void MoveFirst();
        void MoveBack();
        void MoveNext();
        void MoveLast();
        void NewRecord();
        void ReloadData();

        event Action<int> CurrentRecordIndexChanged;
        event Action<int> RecordsCountChanged;
    }

    public class NavigationManager<T> : IDisposable, INotifyPropertyChanged, INavigationManager where T : class
    {
        private ObservableCollection<T> _recordsData = new ObservableCollection<T>();
        public ObservableCollection<T> RecordsData
        {
            get => _recordsData;
            set
            {
                if (_recordsData != value)
                {
                    _recordsData = value;
                    OnPropertyChanged(nameof(RecordsData));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private T _currentRecord;
        public T CurrentRecord
        {
            get => _currentRecord;
            set
            {
                // || _currentRecord == null) این خط اضافه شد چون اگر این نباشه زمانی که رکوردئ جدید هست و کاربر مجددا این دکمهخ جدید رو میزنه که فاکتور خالی بشه دیگه , وارد این رویداد برای ClearAll نمیشه !

                if (!EqualityComparer<T>.Default.Equals(_currentRecord, value) || _currentRecord == null)
                {
                    _currentRecord = value;
                    OnCurrentRecordChanged(_currentRecord);
                }
            }
        }

        private int _currentRecordIndex;
        public int CurrentRecordIndex
        {
            get => _currentRecordIndex;
            set
            {
                if (_currentRecordIndex != value)
                {
                    _currentRecordIndex = value;
                    OnCurrentRecordIndexChanged(_currentRecordIndex);
                }
            }
        }

        public bool IsNewRecord { get; set; }
        public string RecCount { get; private set; }
        public string Current_Rec { get; private set; }
        public string MasterQuery { get; private set; }
        public object? NUMBER_TO_OPEN { get; private set; } = null;

        private readonly CL_CCNNMANAGER dbms;
        private readonly Func<T, string> _propertySelector;
        private readonly Func<T, string> _queryReloadWhere;

        public event Func<T, bool> OnInsertRecord; //Insert as add New data
        public event Func<T, bool> OnDeleteRecord; //Delete Current

        /// <summary>
        /// A callback to check if the current record can be changed (e.g., checking for unsaved changes).
        /// Returns true if navigation is allowed, false to cancel.
        /// </summary>
        public Func<bool> CanChangeRecord { get; set; } = () => true;

        // Use a single public event for CurrentRecordIndexChanged.
        public event Action<T> CurrentRecordChanged;
        public event Action<int> CurrentRecordIndexChanged;
        public event Action<int> RecordsDataCountChanged;

        protected virtual void OnCurrentRecordChanged(T record)
        {
            CurrentRecordChanged?.Invoke(record);
        }
        protected virtual void OnCurrentRecordIndexChanged(int index)
        {
            CurrentRecordIndexChanged?.Invoke(index);
        }
        protected virtual void OnRecordsDataCountChanged(int count)
        {
            RecordsDataCountChanged?.Invoke(count);
        }

        #region DATE_2025
        private string GenerateInsertQuery(T record)
        {
            var properties = typeof(T).GetProperties();
            var columnNames = properties.Select(p => p.Name).ToArray();
            var values = properties.Select(p =>
                $"'{p.GetValue(record) ?? "NULL"}'").ToArray(); // Handling null values gracefully

            // Assuming a table with the same name as the class (T) for simplicity.
            string tableName = typeof(T).Name;

            // Generate SQL INSERT INTO query
            return $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", values)})";
        }
        #endregion

        #region Insert / Delete Logic
        /// <summary>
        /// Before Add to Recods Data
        /// </summary>
        /// <param name="newRecord"></param>
        /// <returns></returns>
        public bool InsertCurrentRecord(T newRecord)
        {
            // If no event is subscribed, there's no insertion logic
            if (OnInsertRecord == null)
                return false;

            // Fire OnInsertRecord event to let consumer do the actual DB INSERT
            bool success = OnInsertRecord.Invoke(newRecord); //Fire event
            if (success)
            {
                // Add to memory collection
                RecordsData.Add(newRecord);
                IsNewRecord = false;

                // Move to newly added item
                CurrentRecordIndex = RecordsData.IndexOf(newRecord);

                var Before = CurrentRecordChanged; //To prevent fire this event to let proccess complete to avoid some null error
                CurrentRecordChanged = null;

                CurrentRecord = newRecord;

                CurrentRecordChanged += Before; //Bring it back
                OnRecordsDataCountChanged(RecordsData.Count);
            }
            return success;
        }

        public bool DeleteCurrentRecord()
        {
            if (CurrentRecord == null || RecordsData.Count == 0)
                return false;

            if (OnDeleteRecord != null)
            {
                // Fire event so user can do DELETE in DB
                bool success = OnDeleteRecord.Invoke(CurrentRecord);
                if (success)
                {
                    RecordsData.RemoveAt(CurrentRecordIndex);

                    RefreshNavigationState();

                    return success;
                    //OnCurrentRecordChanged(CurrentRecord);
                }
            }
            else
            {
                RecordsData.RemoveAt(CurrentRecordIndex);
                RefreshNavigationState();

                return true;
            }

            return false;
        }

        private void RefreshNavigationState()
        {
            // Update the record count
            OnRecordsDataCountChanged(RecordsData.Count);

            // Update the current record index and current record
            if (RecordsData.Count > 0)
            {
                CurrentRecordIndex = Math.Min(CurrentRecordIndex, RecordsData.Count - 1);
                CurrentRecord = RecordsData[CurrentRecordIndex];

                // Trigger the UI update for current record and record count
                OnCurrentRecordIndexChanged(CurrentRecordIndex);
                //OnCurrentRecordChanged(CurrentRecord); //UiDataUpdate // no needed because with CurrentRecord change it will automaticlly fire that
            }
            else
            {
                IsNewRecord = true; //if there is nothing
                CurrentRecord = null; //it will automaticlly raise this : OnCurrentRecordChanged
                CurrentRecordIndex = 0;
            }

        }
        #endregion

        public NavigationManager(
            CL_CCNNMANAGER _dbms_,
            Func<T, string> propertySelector,
            string masterquery,
            Func<T, string> queryReloadWhere,
            object? numberToOpen = null)
        {
            dbms = _dbms_;
            RecordsData = new ObservableCollection<T>();
            _propertySelector = propertySelector;
            MasterQuery = masterquery;
            _queryReloadWhere = queryReloadWhere;

            if (TryGetOpenKey(numberToOpen, out var key))
                NUMBER_TO_OPEN = key;

            ReGetMasterData();
        }

        private static bool TryGetOpenKey(object? value, out object openKey)
        {
            openKey = default!;

            if (value is null)
                return false;

            return value switch
            {
                string s => TryParseStringKey(s, out openKey),
                int i => TrySetNumericKey(i, out openKey),
                long l => TrySetNumericKey(l, out openKey),
                double d => TrySetNumericKey(d, out openKey),
                float f => TrySetNumericKey(f, out openKey),
                decimal dec => TrySetNumericKey(dec, out openKey),
                _ => TryParseFallbackKey(value, out openKey)
            };
        }

        private static bool TryParseStringKey(string s, out object openKey)
        {
            openKey = default!;

            s = NormalizeDigits(s).Trim();

            if (string.IsNullOrWhiteSpace(s))
                return false;

            // اگر رشته عددی است
            if (decimal.TryParse(s,
                                System.Globalization.NumberStyles.Number,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var numericValue))
            {
                if (numericValue <= 0)
                    return false;

                openKey = numericValue;
                return true;
            }

            // اگر رشته غیرعددی است (مثل "115-1-1")
            openKey = s;
            return true;
        }
        private static bool TrySetNumericKey<T>(T value, out object openKey) where T : struct, IComparable<T>
        {
            openKey = default!;

            // تبدیل به decimal برای مقایسه یکپارچه
            decimal decimalValue = value switch
            {
                int i => i,
                long l => l,
                double d => d > 0 && !double.IsNaN(d) && !double.IsInfinity(d) ? (decimal)d : -1m,
                float f => f > 0 && !float.IsNaN(f) && !float.IsInfinity(f) ? (decimal)f : -1m,
                decimal dec => dec,
                _ => -1m
            };

            if (decimalValue <= 0)
                return false;

            openKey = value;
            return true;
        }
        private static bool TryParseFallbackKey(object value, out object openKey)
        {
            openKey = default!;

            var str = NormalizeDigits(value.ToString() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(str))
                return false;

            // تلاش برای تبدیل به عدد
            if (decimal.TryParse(str,
                                System.Globalization.NumberStyles.Number,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out var numericValue))
            {
                if (numericValue <= 0)
                    return false;

                openKey = numericValue;
                return true;
            }
            // اگر عددی نیست ولی رشته معتبری است
            openKey = str;
            return true;
        }
        private static string NormalizeDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                // اعداد فارسی
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2')
                .Replace('۳', '3').Replace('۴', '4').Replace('۵', '5')
                .Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
                // اعداد عربی
                .Replace('٠', '0').Replace('١', '1').Replace('٢', '2')
                .Replace('٣', '3').Replace('٤', '4').Replace('٥', '5')
                .Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');
        }

        public void SubscribeToEvents(
            Action<T> currentRecordChanged,
            Action<int> currentRecordIndexChanged,
            Action<int> recordsDataCountChanged)
        {
            CurrentRecordChanged += currentRecordChanged;
            CurrentRecordIndexChanged += currentRecordIndexChanged;
            RecordsDataCountChanged += recordsDataCountChanged;
            RaiseInitializationEvents();
        }

        public void RaiseInitializationEvents()
        {
            OnCurrentRecordChanged(CurrentRecord);
            OnCurrentRecordIndexChanged(CurrentRecordIndex);
            OnRecordsDataCountChanged(RecordsData.Count);
        }

        public void ReGetMasterData()
        {
            var masterHead = dbms.DoGetDataSQL<T>(MasterQuery).ToList();
            RecordsData.Clear();
            foreach (var record in masterHead)
            {
                RecordsData.Add(record);
            }

            if (!string.IsNullOrWhiteSpace(NUMBER_TO_OPEN?.ToString()))
            {
                var item = RecordsData.FirstOrDefault(x => _propertySelector(x).Equals(NUMBER_TO_OPEN.ToString()));
                if (item != null)
                {
                    NUMBER_TO_OPEN = null;
                    CurrentRecordIndex = RecordsData.IndexOf(item);
                    CurrentRecord = item;
                    MoveReGetData(Jahat.CustomPosition, CurrentRecordIndex);
                    return;
                }
                else
                {
                    CurrentRecordIndex = -1;
                    CurrentRecord = null;
                    OnCurrentRecordIndexChanged(CurrentRecordIndex);
                    OnRecordsDataCountChanged(RecordsData.Count);
                    MoveReGetData(Jahat.NewItem);
                    return;
                }
            }
            MoveReGetData(Jahat.LastItem);
        }

        public void MoveReGetData(
            Jahat jahat,
            int? customPosition = null,
            IEnumerable<System.Windows.Controls.TextBox> textBoxes = null,
            IEnumerable<KeyValuePair<System.Windows.Controls.ComboBox, System.Windows.Controls.SelectionChangedEventHandler>> comboBoxes = null,
            IEnumerable<System.Windows.Controls.CheckBox> checkBoxes = null,
            params IEnumerable[] observableCollections)
        {
            // Check if navigation is allowed by the host (e.g. unsaved changes check)
            if (CanChangeRecord != null && !CanChangeRecord())
            {
                return;
            }

            int recordCount = RecordsData.Count;

            if (IsNewRecord && !ConfirmExitWithoutSaving())
            {
                return;
            }

            switch (jahat)
            {
                case Jahat.FirstItem:
                    IsNewRecord = false;
                    CurrentRecordIndex = 0;
                    break;
                case Jahat.BackItem:
                    if (CurrentRecordIndex > 0)
                    {
                        IsNewRecord = false;
                        CurrentRecordIndex--;
                    }
                    break;
                case Jahat.NextItem:
                    if (CurrentRecordIndex < recordCount - 1)
                    {
                        IsNewRecord = false;
                        CurrentRecordIndex++;
                    }
                    break;
                case Jahat.LastItem:
                    IsNewRecord = false;
                    CurrentRecordIndex = recordCount - 1;
                    break;
                case Jahat.CustomPosition:
                    if (customPosition >= 0 && customPosition < recordCount)
                    {
                        IsNewRecord = false;
                        CurrentRecordIndex = customPosition.Value;
                    }
                    break;
                case Jahat.NewItem:
                    IsNewRecord = true;
                    ClearFreshNew(textBoxes, comboBoxes, checkBoxes, observableCollections);
                    CurrentRecordIndex = recordCount;
                    //OnCurrentRecordChanged(CurrentRecord);
                    break;
            }

            if (recordCount == 0)
            {
                IsNewRecord = true;
            }

            GetDbUpdateRecord();
            OnCurrentRecordIndexChanged(CurrentRecordIndex);
            OnRecordsDataCountChanged(RecordsData.Count);

            //if (CurrentRecord != null)
            //{
            //    UiDataUpdate();
            //}
        }

        public bool ConfirmExitWithoutSaving()
        {
            Msgwin msgwin = new Msgwin(true, "آیتم جدید را ذخیره نکرده اید , آیا از خروج از این آیتم اطمینان دارید ؟");
            msgwin.ShowDialog();
            return msgwin.DialogResult == true;
        }

        private void GetDbUpdateRecord()
        {
            var RowRequestRecord = RecordsData.ElementAtOrDefault(CurrentRecordIndex);

            if (RowRequestRecord != null)
            {
                var queryString = _queryReloadWhere(RowRequestRecord);
                CurrentRecord = dbms.DoGetDataSQL<T>(queryString).FirstOrDefault();
            }
            else
            {
                CurrentRecord = RecordsData.ElementAtOrDefault(CurrentRecordIndex);
            }

        }
        private void GetDbUpdateRecord11111()
        {
            // Get the record from the collection based on the current index.
            CurrentRecord = RecordsData.ElementAtOrDefault(CurrentRecordIndex);

            if (CurrentRecord != null)
            {
                // Check if the current record is "new" (i.e. its key property is "0")
                if (_propertySelector(CurrentRecord) == "0")
                {
                    // Mark as new and exit without reloading from the database.
                    IsNewRecord = true;
                    return;
                }

                // Otherwise, build the query to refresh this record.
                var queryString = _queryReloadWhere(CurrentRecord);
                var updatedRecord = dbms.DoGetDataSQL<T>(queryString).FirstOrDefault();
                if (updatedRecord != null)
                {
                    CurrentRecord = updatedRecord;
                }
            }
            else
            {
                CurrentRecord = null;
            }
        }
        public void ClearNumberToOpen()
        {
            NUMBER_TO_OPEN = null;
        }

        private void UiDataUpdate()
        {
            OnCurrentRecordChanged(CurrentRecord);
        }

        public void ClearFreshNew(
            IEnumerable<System.Windows.Controls.TextBox> textBoxes,
            IEnumerable<KeyValuePair<System.Windows.Controls.ComboBox, System.Windows.Controls.SelectionChangedEventHandler>> comboBoxes,
            IEnumerable<System.Windows.Controls.CheckBox> checkBoxes,
            params IEnumerable[] observableCollections)
        {
            if (textBoxes != null)
            {
                foreach (var textBox in textBoxes)
                {
                    textBox.Text = null;
                    textBox.Tag = null;
                }
            }
            if (comboBoxes != null)
            {
                foreach (var comboBoxInfo in comboBoxes)
                {
                    var comboBox = comboBoxInfo.Key;
                    var selectionChangedHandler = comboBoxInfo.Value;
                    comboBox.SelectionChanged -= selectionChangedHandler;
                    comboBox.SelectedIndex = -1;
                    comboBox.Items.Refresh();
                    comboBox.SelectionChanged += selectionChangedHandler;
                }
            }
            if (checkBoxes != null)
            {
                foreach (var checkBox in checkBoxes)
                {
                    checkBox.IsChecked = false;
                }
            }
            foreach (var observableCollection in observableCollections)
            {
                ((dynamic)observableCollection).Clear();
            }
            CurrentRecordIndex = RecordsData.Count - 1;
            IsNewRecord = true;
        }

        public void UnsubscribeFromEvents()
        {
            CurrentRecordChanged = null;
            CurrentRecordIndexChanged = null;
            RecordsDataCountChanged = null;

            OnInsertRecord = null;
            OnDeleteRecord = null;
        }

        #region INavigationManager Implementation

        public int RecordsCount => RecordsData.Count;

        public void MoveFirst() => MoveReGetData(Jahat.FirstItem);
        public void MoveBack() => MoveReGetData(Jahat.BackItem);
        public void MoveNext() => MoveReGetData(Jahat.NextItem);
        public void MoveLast() => MoveReGetData(Jahat.LastItem);
        public void NewRecord() => MoveReGetData(Jahat.NewItem);
        public void ReloadData() => ReGetMasterData();

        public event Action<int> RecordsCountChanged
        {
            add { RecordsDataCountChanged += value; }
            remove { RecordsDataCountChanged -= value; }
        }

        // The public event CurrentRecordIndexChanged declared above satisfies the interface.

        #endregion
        #region IDisposable Implementation

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    RecordsData?.Clear();
                    // free managed resources
                    UnsubscribeFromEvents();
                }
                // free unmanaged resources if any
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NavigationManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
