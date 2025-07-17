using System.Windows.Data;

namespace Interfaces
{
    public interface INavigator
    {
        public enum Jahat
        {
            FirstItem,
            BackItem,
            NextItem,
            LastItem,
            NewItem,
            CustomPosition
        }
        public CollectionViewSource RecordsData { get; set; }// = new CollectionViewSource();
        public double? NUMBER_TO_OPEN { get; set; }
        public bool NewRecord { get; set; }// = false;
        public void MoveReGetData(Jahat jahat, int? custom_postiion = null);
        public void ClearFreshNew();
        public void UiDataUpdate();
        public void Form_Current();
        public void ReGetData();
        public bool ConfirmExitWithoutSaving();
        public void ReGetMasterData();
    }
}
