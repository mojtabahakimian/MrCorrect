namespace Prg_Proccessy.SQLMODELS
{
    /// <summary>
    /// برای بخش اعلام وصول چک گروهی
    /// </summary>
    public class COME_FROM_VOSULDIALOG2
    {
        /// <summary>
        /// درتاريخ سررسيد وصول شود
        /// </summary>
        public bool RD_SARRESID_DT_1 { get; set; } //درتاريخ سررسيد وصول شود

        /// <summary>
        /// روز پس از تاريخ سررسيد وصول شود
        /// </summary>
        public bool RD_AFTER_SARRESID_DT_2 { get; set; } //روز پس از تاريخ سررسيد وصول شود
        public int NUMBERIC_DTC_DAY_2 { get; set; }

        /// <summary>
        /// درتاريخ ذيل وصول شود
        /// </summary>
        public bool RD_ZEIL_DT_3 { get; set; } //درتاريخ ذيل وصول شود

        /// <summary>
        /// تاریخ وصول
        /// </summary>
        public string DTS_DATE { get; set; } //تاریخ وصول

        /// <summary>
        /// وصول به حساب
        /// </summary>
        public string HHMOIN_VOSUL { get; set; } //وصول به حساب


        /// <summary>
        /// پنل وصولی گروهی بین Radio Buttons Returns the active selection method (the first true property among RD_SARRESID_DT_1, RD_AFTER_SARRESID_DT_2, and RD_ZEIL_DT_3)
        /// </summary>
        public int VCLC
        {
            get
            {
                if (RD_SARRESID_DT_1) return 1;
                if (RD_AFTER_SARRESID_DT_2) return 2;
                if (RD_ZEIL_DT_3) return 3;
                return 0; // No method selected
            }
        }
    }
}
