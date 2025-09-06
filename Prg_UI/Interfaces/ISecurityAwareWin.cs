namespace Prg_UI.Interfaces
{
    /// <summary>
    /// فعلا از این استفاده نمیشه تا بعدا
    /// </summary>
    public sealed class WinPermissionType
    {
        /// <summary>
        /// RUN : آیا میتوان فرم را اجرا کنید | فرم را باز کند
        /// </summary>
        public bool CanRun { get; init; }
        /// <summary>
        /// SEE : آیا میتوان فرم باز شده را ببیند | مشاهد اطلاعات فرم
        /// </summary>
        public bool CanSee { get; init; }
        /// <summary>
        /// INP : آیا اجازه اضافه کردن داده جدید دارد | ورود داده جدید
        /// </summary>
        public bool CanInsertInp { get; init; }
        /// <summary>
        /// UPD : آیا اجازه ویرایش یا اصلاح اطلاعات فرم را دارد | بهنگام سازی داده ها
        /// </summary>
        public bool CanUpdateUpd { get; init; }
        /// <summary>
        /// DELETE : آیا اجازه حذف اطلاعات فرم یا حذف سطر داده را دارد
        /// </summary>
        public bool CanDeleteDel { get; init; }

        public WinPermissionType(bool canRun = true, bool canSee = true, bool canInsert = true, bool canUpdate = true, bool canDelete = true)
        {
            CanRun = canRun;
            CanSee = canSee;
            CanInsertInp = canInsert;
            CanUpdateUpd = canUpdate;
            CanDeleteDel = canDelete;
        }

        public bool HasAnyPermission() => CanRun || CanSee || CanInsertInp || CanUpdateUpd || CanDeleteDel;

        public bool HasFullPermission() => CanRun && CanSee && CanInsertInp && CanUpdateUpd && CanDeleteDel;

        // Permission های از پیش تعریف شده
        public static WinPermissionType FullAccess => new(true, true, true, true, true);
        public static WinPermissionType ReadOnly => new(true, true, canInsert: false, canUpdate: false, canDelete: false);
        public static WinPermissionType NoAccess => new();
    }
    public interface ISecurityAwareWin
    {
        WinPermissionType Permission { get; set; }
    }
}