using Microsoft.VisualBasic;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Functions;
using Prg_UI.Wins.WinMenus.ANBAR;
using Prg_UI.Wins.WinMenus.HESABDARI;
using Prg_UI.Wins.WinMenus.KHARID_FORUSH;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using Prg_UI.Wins.WinMenus.WinAutomasion;
using Prg_UI.Wins.WinMenus.WinDEFAULT;
using Prg_UI.Wins.WinSetting;
using System;
using System.Linq;
using System.Windows;
using Wins.WinMenus.ANBAR;
using Wins.WinMenus.ANBAR.ANBAR_REPORTS;
using Wins.WinMenus.Checkha;
using Wins.WinMenus.HESABDARI;
using Wins.WinMenus.HESABDARI.GOZARESHAT;
using Wins.WinMenus.KHARID_FORUSH;
using Wins.WinMenus.KHARID_FORUSH.GOZARESHAT;
using Wins.WinMenus.SALARY;
using Wins.WinMenus.Taarif;
using Wins.WinMenus.CONFIGS;
using Wins.WinMenus.SANATI;
using Prg_Proccessy.Generaly;
using static Functions.InventoryManager;
using Stimulsoft.Base;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report;
using System.Reflection;
using Wins.WinSetting;
using Rpts;
using Prg_Proccessy.FUNCTIONS;
using Prg_UI.Wins.WinMenus.CONFIGS;

namespace Functions
{
    public static class CL_MenuManager
    {
        #region TYPES
        public enum WinNameType
        {
            /// <summary>
            /// اتوماسیون اداری
            /// </summary>
            Automasion_MAIN,
            /// <summary>
            /// اطلاعات بیشتر تعریف مشتری
            /// </summary>
            FCODE_CUSTOMER_MORE,
            /// <summary>
            /// انتقال کالا از انبار به انبار
            /// </summary>
            HEAD_LST_ENTEGHAL_WIN,
            LOK,
            /// <summary>
            /// انبار گردانی
            /// </summary>
            ANBGRD_HEAD_WIN,
            BAKCHEKP,
            ZF_IVO_EXTENDED,
            PAYCHEK,
            SPAYCHEK,
            /// <summary>
            /// پیش فرض واحد و شیفت
            /// </summary>
            DEFAULT,
            /// <summary>
            /// پیش فاکتور
            /// </summary>
            HEAD_LST_PISHFROOSH2,
            TAKHFIF,
            /// <summary>
            /// تعریف گروه کالا ها
            /// </summary>
            TCOD_STUFGROUP_WIN,
            /// <summary>
            /// تعریف واحد های کالا
            /// </summary>
            TCOD_VAHEDS_WIN,
            /// <summary>
            /// تعریف انبار های کالا
            /// </summary>
            TCOD_ANBAR_WIN,
            //تعاریف نام بانکها
            TCOD_BANKS_WIN,
            /// <summary>
            /// تعریف خدمات
            /// </summary>
            KHAD_DEF,
            /// <summary>
            /// تعریف شیفت
            /// </summary>
            SHIFT_WIN,
            /// <summary>
            /// تعریف کالا
            /// </summary>
            STUF_DEF_WIN,
            /// <summary>
            /// تعریف کد مپ شماره فنی
            /// </summary>
            TCOD_MAPF_WIN,
            /// <summary>
            /// تعریف  گروه حسابها
            /// </summary>
            TCOD_HESGROUP_WIN,
            /// <summary>
            /// تعریف مشتری جدید
            /// </summary>
            FCODE_CUSTOMER,
            /// <summary>
            /// تعریف نوع مشتری
            /// </summary>
            CUSTKIND_WIN,
            /// <summary>
            /// تغریف واحد های زیر مجموعه سازمان
            /// </summary>
            DEPART_WIN,
            /// <summary>
            /// تنظیمات پایگاه داده
            /// </summary>
            WinConnectionChoose,
            /// <summary>
            /// تنظیم تم قالب
            /// </summary>
            MaterialThemSettingy,
            BACK_CHK_SERCH,
            FOR_CHK_SERCH,
            /// <summary>
            /// جستجو گر موجودی
            /// </summary>
            MOGUDI_SEARCH_MAIN,
            BAKCHEK,
            CREATE_CHEKPDP,
            CREATE_CHEKDP,
            /// <summary>
            /// حساب های خودگردان
            /// </summary>
            AUTOMATIC,
            /// <summary>
            /// تعریف سرفصل حسابهای معین
            /// </summary>
            DETA_HES_SHEET,
            /// <summary>
            /// خزانه داری
            /// </summary>
            PGET_HED,
            lockok,
            GETCHEK,
            SGETCHEK,
            mesagL,
            /// <summary>
            /// + اتوماسیون
            /// </summary>
            WinEVENTS,
            OTHER_DTL,
            /// <summary>
            /// سرفصل حسابهای تفضیلی
            /// </summary>
            TDETA_HES_SHEET,
            TDETA_HES_SHEET2,
            TDETA_HES_SHEET3,
            TDETA_HES_SHEET4,
            /// <summary>
            /// سرفصل حسابهای کل
            /// </summary>
            TOTA_HES_SHEET_WIN,
            BUDGET0,
            /// <summary>
            /// صدور و ویرایش اسناد
            /// </summary>
            DEED_HEAD,
            /// <summary>
            /// فاکتور فروش مستقیم
            /// </summary>
            HEAD_LST_FROOSH22_DIRECT,
            /// <summary>
            /// فاکتور فروش از انتخاب حواله
            /// </summary>
            HEAD_LST_FROOSH22_HAVALEHEE,
            /// <summary>
            /// فاکتور فروش صادراتی از انتخاب حواله
            /// </summary>
            HEAD_LST_FROOSH22_SADERATI,
            /// <summary>
            /// فرمت رده بندی مشتری
            /// </summary>
            GRADE_FORMAT_WIN,
            /// <summary>
            /// سابقه فروش
            /// </summary>
            froosh_customer,
            IRAN_SALES_MAP,
            /// <summary>
            /// فعالیت های سازمان
            /// </summary>
            NABZEDARY,
            /// <summary>
            /// کارت انبار کالا
            /// </summary>
            F_MENU_KART,
            KARTBANK,
            /// <summary>
            /// کدینگ مپ
            /// </summary>
            TCOD_MAP_GRP_WIN,
            /// <summary>
            /// لیست اسناد
            /// </summary>
            DEED_HEAD_LIST,
            BUGET_MAIN_list2,
            HINVOLST,
            chek_pass_nashodah,
            HAVALE_LIST,
            PGET_HED_LIST,
            /// <summary>
            /// لیست دفتر تفضیلی
            /// </summary>
            F_MENU_KOL_MOIN_TAFZIL,
            LIST_KALA_TAKHPERS,
            /// <summary>
            /// لیست جستجویی کالا ها
            /// </summary>
            STUF_DEF_LST,
            NAGHDF,
            /// <summary>
            /// مشخصات مشتریان - اعضاء  و اعتبارات
            /// </summary>
            AZAE_WIN,
            BUDGET_DEFAULT,
            /// <summary>
            /// نبض فروش
            /// </summary>
            NABZEFROOSH,
            /// <summary>
            /// نبض مالی
            /// </summary>
            NABZEMALI,
            /// <summary>
            /// نتیجه لیست دفتر تفضیلی
            /// </summary>
            R_DAFTAR_MOIN_LIST,
            /// <summary>
            /// واگذاری چک
            /// </summary>
            FORCHEK,
            /// <summary>
            /// تایید و قطعی کردن اسناد تایید نشده
            /// </summary>
            F_MENU_ASNAD,
            /// <summary>
            /// جستجو در شرح سند
            /// </summary>
            DEED_SEARCH_MAIN,
            /// <summary>
            /// حواله انبار فروش
            /// </summary>
            HEAD_LST_HAVL,
            /// <summary>
            /// درخواست پرداخت
            /// </summary>
            paymentformorder,
            /// <summary>
            /// رسید انبار خرید
            /// </summary>
            HEAD_LST_RASID,
            /// <summary>
            /// نتیجخ جستجو در شرح اسناد
            /// </summary>
            DEED_SERCH_CREATE,
            /// <summary>
            /// سایر حواله انبار ها
            /// </summary>
            HEAD_LST_HAV_OTHER_WIN,
            /// <summary>
            /// سایر رسید انبار ها
            /// </summary>
            HEAD_LST_RASID_OTHER_WIN,
            /// <summary>
            /// صورت مغایرت
            /// </summary>
            F_MENU_MOGHAYERAT,
            /// <summary>
            /// صورت مغایرت های گرفته شده (int?)
            /// </summary>
            MOGHAYERAT,
            /// <summary>
            /// فاکتور برگشت خرید - عادی 
            /// </summary>
            HEAD_LST_KH_BACK,
            /// <summary>
            /// فاکتور برگشت فروش - عادی
            /// </summary>
            HEAD_LST_FROOSH_BACK2,
            /// <summary>
            /// فاکتور خدمات فروش
            /// </summary>
            HEAD_LST_KHADAMAT,
            /// <summary>
            /// فاکتور خرید مستقیم
            /// </summary>
            HEAD_LST_KHAREED1_DIRECT,
            /// <summary>
            /// فاکتور خرید از انتخاب رسید انبار
            /// </summary>
            HEAD_LST_KHAREED1_RASID,
            /// <summary>
            /// فاکتور خرید صادراتی با انتخاب رسید انبار
            /// </summary>
            HEAD_LST_KHAREED1_SADERATI,
            /// <summary>
            /// لیست کلیه انواع فاکتور ها
            /// </summary>
            FACTORS_LST,
            /// <summary>
            /// فاکتور برگشت فروش (آزاد) رسید شده
            /// </summary>
            HEAD_LST_BRFR,
            /// <summary>
            /// فاکتور برگشت خرید آزاد
            /// </summary>
            HEAD_LST_KH_BACK_AZAD,
            /// <summary>
            /// جستجو در شرح خزانه
            /// </summary>
            PGET_LST_SEARCH,
            /// <summary>
            /// ترعیف مراکز مصرف
            /// </summary>
            MASRAF_CENTER,
            /// <summary>
            /// تعریف تخفیفات پیشرفته
            /// </summary>
            TAKHFIF_DEF_FORM,
            /// <summary>
            /// تعریف مراکز هزینه
            /// </summary>
            TCOD_MARKAZHAZ_WIN,
            /// <summary>
            /// تعریف گروه بندی برای ویزیتور
            /// </summary>
            TCODE_MENUITEM_WIN,
            /// <summary>
            /// باز کردن فاکتور فروش بر اساس دسترسی A: NUMBER1 , B: NUMBER
            /// </summary>
            HEAD_LST_FROOSH_AUTO_DETECT,
            /// <summary>
            /// گزارشات > گزارشات دارایی
            /// </summary>
            F_MENU_DATE_TAX_REPORT,
            /// <summary>
            /// F9 لیست بدهکاران و بستانکاران
            /// </summary>
            BEDEHKARAN_BESTANKARAN,
            /// <summary>
            /// لیست بدهکاران و بستانکاران محدود شده
            /// </summary>
            BEDEHKARAN_BESTANKARAN_LIMITED,
            /// <summary>
            /// زارش حواله انبار گروهی
            /// </summary>
            F_MENU_ANBAR_FRKH_GRP,
            /// <summary>
            /// لیست موجودی کالای انبار های تلفیق شده
            /// </summary>
            F_MENU_ANBAR_MERG,
            /// <summary>
            /// لیست موجودی کالا ها به تفکیک انبار
            /// </summary>
            F_MENU_ANBAR,
            /// <summary>
            /// درخواست خرید
            /// </summary>
            HEAD_LST_REQUEST_WIN,
            /// <summary>
            /// خلاصه فروش روزانه به تفکیک اشخاص
            /// </summary>
            FROOSHDAY,
            /// <summary>
            /// خلاصه خـرید  روزانه به تفکیک اشخاص
            /// </summary>
            FKHAREDAY,
            /// <summary>
            /// لیست درخواست پرداخت وجه
            /// </summary>
            paymentformorder_LIST,
            /// <summary>
            /// ارسال به مودیان در پنجره جدا
            /// </summary>
            WIN_MOADIAN_SINGLE,
            /// <summary>
            /// گزارش ارزش افزوده فروش - گزارش فصلی
            /// </summary>
            F_MENU_DATE_FRCUST,

            /// <summary>
            /// گزارش فصلی (برگشت فروش مشتریان) برگشت فروش
            /// </summary>
            F_MENU_DATE_FASLIBR,

            /// <summary>
            /// گزارش فصلی برگشت خرید
            /// </summary>
            F_MENU_DATE_FASLIKHBR,

            /// <summary>
            /// گزارش ارزش افزوده خرید - گزارش فصلی
            /// </summary>
            F_MENU_DATE_KHCUST,

            /// <summary>
            /// گزارش خرید به تفکیک فاکتور
            /// </summary>
            F_MENU_DATE_KHLS,

            /// <summary>
            /// گزارش فروش به تفکیک فاکتور
            /// </summary>
            F_MENU_DATE_LFACT,
            /// <summary>
            /// گزارش فروش روزانه واحد ها
            /// </summary>
            F_MENU_GOZARESH_FROOSH,
            /// <summary>
            /// خلاصه فروش روزانه به تفکیک اشخاص
            /// </summary>
            WIN_F_MENU_KHFR_FROOSHDAY,
            /// <summary>
            /// خلاصه خـرید روزانه به تفکیک اشخاص
            /// </summary>
            WIN_F_MENU_KHFR_FKHAREDAY,
            /// <summary>
            /// گزارش روزانه
            /// </summary>
            C_TARAZ_ANBAR_KHAS,
            /// <summary>
            /// دفتر چک های دریافتی
            /// </summary>
            WIN_PAYGETD_LST,
            /// <summary>
            /// دفتر چک های پرداختی
            /// </summary>
            WIN_PAYGETP_LST,
            /// <summary>
            /// اعلام وصول چک های دریافتی
            /// </summary>
            WIN_CHREC_H,
            /// <summary>
            /// اعلام وصول چک های پرداختی
            /// </summary>
            WIN_CHREC_HP,
            /// <summary>
            /// لیست چکهای دریافتی به تفکیک ماه | دفتر چکهای ماهانه دریافتی
            /// </summary>
            WIN_CHECK_MON,
            /// <summary>
            /// لیست چکهای دریافتی نزد بانک و صندوق وصول نشده
            /// </summary>
            F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK,
            /// <summary>
            /// جایگذاری حساب ها
            /// </summary>
            ZASESABBEESAB_REPLACE_HESAB,
            /// <summary>
            /// لیست کل چکهای دریافتی
            /// </summary>
            WIN_CHKE_DLIST_KOLCHECKD,
            /// <summary>
            /// لیست چکهای دریافتی نزد بانک
            /// </summary>
            F_MENU_CHEK_CHEK_VOSUL_LES,
            /// <summary>
            /// سوابق تغیر وضعیت چک
            /// </summary>
            PAY_GETD_LOG_FORM,
            /// <summary>
            /// گزارش چاپی چک های دریافتی سررسید شده
            /// </summary>
            F_MENU_CHEK_CHEK_DLISTS,
            /// <summary>
            /// لیست چکهای پرداختی به تفکیک ماه | دفتر چکهای ماهانه پرداختی
            /// </summary>
            WIN_CHECK_MONP,
            /// <summary>
            /// لیست کل چکهای پرداختی
            /// </summary>
            WIN_CHKE_PLIST,
            /// <summary>
            /// چاپ سریع چک
            /// </summary>
            CHAPCHEK,
            /// <summary>
            /// ثبت و ویرایش مرخصی پرسنل
            /// </summary>
            PMORAKH,

            /// <summary>
            /// گزارش فاکتور های فروش به صورت گروهی
            /// </summary>
            F_MENU_FROOSH,
            /// <summary>
            /// گزارش خرید فروش از انبار ها
            /// </summary>
            F_MENU_ANBAR_FRKH,
            /// <summary>
            /// لیست برگه های بدون قبض باسکول
            /// </summary>
            F_MENU_DATE_HBGHB,
            /// <summary>
            /// گزارش فروش روزانه کاربران
            /// </summary>
            F_MENU_DATE_CROS,
            /// <summary>
            /// لیست فروش روزانه به تفکیک نوع کالا
            /// </summary>
            F_MENU_DATE_CFRALL,
            /// <summary>
            /// لیست فروش روزانه به تفکیک نوع کالا
            /// </summary>
            FROOSH_COUNTALL,
            /// <summary>
            /// لیست خرید روزانه به تفکیک نوع کالا
            /// </summary>
            F_MENU_DATE_CKRALL,
            /// <summary>
            /// آمار فروش کالا ها به تفکیک روز
            /// </summary>
            F_MENU_DATE_AMFDAY,
            /// <summary>
            /// لیست کالای های فروش نرفته
            /// </summary>
            FROOSH_NARAFTAH,
            /// <summary>
            /// لیست کالا های فروش نرفته به شخص
            /// </summary>
            WIN_F_MENU_KHFR_FONARP,
            /// <summary>
            ///  لیست محدود شده مشتریانی که خرید نکرده اند
            /// </summary>
            F_MENU_DATE_CUSTNP,
            /// <summary>
            /// لیست خرید به تفکیک کالا و قبض  باسکول
            /// </summary>
            LIST_KHARID,
            /// <summary>
            /// لیست فروش به تفکیک کالا و قبض  باسکول
            /// </summary>
            LIST_FROOSH,
            /// <summary>
            /// لیست مشخصات کلیه کالاها
            /// </summary>
            STUF_DEF_LIST,
            /// <summary>
            /// گزارش خلاصه فروش روزانه
            /// </summary>
            F_MENU_GOZARESH_FROOSH_F,
            /// <summary>
            /// چاپ گروهی اسناد
            /// </summary>
            F_MENU_ASNAD_PRINT,
            /// <summary>
            /// گزارش چاپی دفتر تفضیلی
            /// </summary>
            F_MENU_KOL_MOIN_TAFZIL_TAF,
            /// <summary>
            /// لیست موجودی کالا ها به تفکیک انبار
            /// </summary>
            F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST,
            /// <summary>
            /// لیست موجودی کل انبار
            /// </summary>
            MOGUDI_ANBARHA_LIST,
            /// <summary>
            /// گزارش موجودی کالا ها به تفکیک انبار
            /// </summary>
            F_MENU_ANBAR_R,
            /// <summary>
            /// گزارش کارت انبار بدون نرخ
            /// </summary>
            F_MENU_KART_KARTR2,
            /// <summary>
            /// لیست تراز موجودی یک انبار
            /// </summary>
            F_MENU_ANBAR_TARAZ_F,
            /// <summary>
            /// گزارش تراز موجودی یک انبار
            /// </summary>
            F_MENU_ANBAR_TARAZ_R,
            /// <summary>
            /// گزارش تراز انبار ها به تفکیک گروه کالا
            /// </summary>
            F_MENU_ANBAR_TARAZ_TANBGRP,
            /// <summary>
            /// لیست تراز انبار به تفکیک نوع برگه
            /// </summary>
            F_MENU_ANBAR_TARAZ_FD,

            /// <summary>
            /// لیست چکهای پرداختی اعلام وصول نشده
            /// </summary>
            F_MENU_CHEK_CVP,

            /// <summary>
            /// سازمان
            /// </summary>
            WIN_SAZMAN,
            /// <summary>
            /// برگه ورود کالای ساخته شده
            /// </summary>
            HAVALAH_ENTER,
            /// <summary>
            /// برگه خروج مواد اولیه جهت تولید
            /// </summary>
            HAVALAH_EXIT,
            /// <summary>
            /// دفتر روزنامه
            /// </summary>
            F_MENU_DATE_DFTROOS,
            /// <summary>
            /// دفتر کل
            /// </summary>
            F_MENU_KOL_DATE,
            /// <summary>
            /// دفتر معین
            /// </summary>
            F_MENU_KOL_MOIN_DATE,
            /// <summary>
            /// حساب تفکیکی تفضیلی
            /// </summary>
            F_MENU_DATE_KOL_MOIN_TAFKIK,
            /// <summary>
            /// چاپ سرفصل حسابها
            /// </summary>
            F_MENU_PRINTHES,
            /// <summary>
            /// لیست اسنادی که تراز نیستند
            /// </summary>
            ADAMTARAZ,
            /// <summary>
            /// گزارش تراز های آزمایشی حسابهای معین
            /// </summary>
            FMENU_TARAZ_4,
            /// <summary>
            /// لیست مراکز هزینه
            /// </summary>
            FMENU_TARAZ_4_MARKAZ,
            /// <summary>
            /// صورت منابع و مصارف وجوه گردش وجوه نقد
            /// </summary>
            FMENU_TARAZ_4_MANABE,
            /// <summary>
            /// ترازنامه
            /// </summary>
            F_MENU_SND,

            /// <summary>
            /// گزارش عملکرد خزانه
            /// </summary>
            F_MENU_DATE_DPDAY,
            /// <summary>
            /// چاپ خلاصه دفتر کل
            /// </summary>
            R_DAFTAR_KOL_kh,

            /// <summary>
            /// تعیین سطح دسترسی
            /// </summary>
            F_USER_PERMITION_FORMS_DASTRASI,
            /// <summary>
            /// گزارش تراز موجودی کل انبار ها
            /// </summary>
            R_TARAZ_ANBARHA,
            /// <summary>
            /// لیست تراز چهار ستونی کل
            /// </summary>
            FMENU_TARAZ_4_KOL_FT4,
            /// <summary>
            /// لیست تراز آزمایشی چهار ستونی معین
            /// </summary>
            FMENU_TARAZ_4_MOIN_FT4M,
            /// <summary>
            /// لیست تراز آزمایشی چهار ستونی تفضیلی
            /// </summary>
            FMENU_TARAZ_4_TAFZILI_FT4T,

            /// <summary>
            /// پیش فرض ارجاعات کاربران
            /// </summary>
            UserPersonelOrderWin_PishfarzeErja
        }
        #endregion


        //, bool _IsModalDialog_ = false, bool _AlloMultipleInstances_ = true)
        public static void OpenWinMenu(WinNameType _TYPE_, Window? OWNERWIN, params object[] _PARAMETERS_)
        {
            switch (_TYPE_)
            {
                case WinNameType.UserPersonelOrderWin_PishfarzeErja: //پیش فرض ارجاعات کاربران
                    CL_LMethods.OpenWindow(OWNERWIN, new UserPersonelOrderWin(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.FMENU_TARAZ_4_TAFZILI_FT4T: //لیست تراز آزمایشی چهار ستونی تفضیلی
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("FT4T"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.FMENU_TARAZ_4_MOIN_FT4M: //لیست تراز آزمایشی چهار ستونی معین
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("FT4M"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.FMENU_TARAZ_4_KOL_FT4: //لیست تراز چهار ستونی کل
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("FT4"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.R_TARAZ_ANBARHA: //گزارش تراز موجودی کل انبار ها
                    {
                        var report = new StiReport();
                        var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"Prg_UI.Rpts.ANBAR.R_TARAZ_ANBARHA.mrt");
                        report.Load(pathreport);
                        string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                        report.Dictionary.Databases.Clear();
                        report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
                        new WINRPT(report, "گزارش تراز موجودی کل انبار ها").Show();
                    }
                    break;

                case WinNameType.F_USER_PERMITION_FORMS_DASTRASI:
                    CL_LMethods.OpenWindow(OWNERWIN, new F_USER_PERMITION_FORMS(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.R_DAFTAR_KOL_kh: //چاپ خلاصه دفتر کل
                    {
                        var report = new StiReport();
                        var pathreport = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"Prg_UI.Rpts.HESABDARI.R_DAFTAR_KOL_kh.mrt");
                        report.Load(pathreport);
                        string connstr = CL_CCNNMANAGER.CONNECTION_STR + "Connect Timeout=300";
                        report.Dictionary.Databases.Clear();
                        report.Dictionary.Databases.Add(new StiSqlDatabase("MS SQL", connstr));
                        //((StiSqlSource)report.Dictionary.DataSources["KART_KALA"]).CommandTimeout = 300;
                        new WINRPT(report, "چاپ خلاصه دفتر کل").Show();

                        //report.Render();
                        //report.ShowWithWpf();
                        //pathreport?.Dispose();
                    }
                    break;

                case WinNameType.F_MENU_DATE_DFTROOS: // دفتر روزنامه
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("DFTROOS"), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_KOL_DATE: //دفتر کل
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KOL_DATE("default"), isModalDialog: false, allowMultipleInstances: false);
                    break;



                case WinNameType.F_MENU_KOL_MOIN_DATE: //دفتر معین
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KOL_MOIN_DATE(), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_DATE_KOL_MOIN_TAFKIK: //حساب تفکیکی تفضیلی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE_KOL_MOIN_TAFKIK(), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_PRINTHES: // چاپ سرفصل حسابها
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_PRINTHES(), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.ADAMTARAZ: //لیست اسنادی که تراز نیستند
                    CL_LMethods.OpenWindow(OWNERWIN, new ADAMTARAZ(), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.FMENU_TARAZ_4: //گزارش تراز های آزمایشی حسابهای معین
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("FT4M2"), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.FMENU_TARAZ_4_MARKAZ: //لیست مراکز هزینه
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("MARKAZ"), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.FMENU_TARAZ_4_MANABE: // صورت منابع و مصارف وجوه گردش وجوه نقد
                    CL_LMethods.OpenWindow(OWNERWIN, new FMENU_TARAZ_4("MANABE"), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_SND: // ترازنامه
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_SND(), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_DATE_DPDAY: //گزارش عملکرد خزانه
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("DPDAY"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.HAVALAH_EXIT: //برگه خروج مواد اولیه جهت تولید
                    CL_LMethods.OpenWindow(OWNERWIN, new HAVALAH_EXIT((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HAVALAH_ENTER: //برگه ورود کالای ساخته شده
                    CL_LMethods.OpenWindow(OWNERWIN, new HAVALAH_ENTER((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.WIN_SAZMAN: //سازمان
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_SAZMAN(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_CHEK_CVP: //لیست چکهای پرداختی اعلام وصول نشده
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_CHEK("cvp", "لیست چکهای پرداختی اعلام وصول نشده"));
                    break;

                case WinNameType.F_MENU_ANBAR_TARAZ_FD: //لیست تراز انبار به تفکیک نوع برگه
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_TARAZ("FD"));
                    break;

                case WinNameType.F_MENU_ANBAR_TARAZ_TANBGRP: //گزارش تراز انبار ها به تفکیک گروه کالا
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_TARAZ("TANBGRP"));
                    break;

                case WinNameType.F_MENU_ANBAR_TARAZ_R: //گزارش تراز موجودی یک انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_TARAZ("R"));
                    break;

                case WinNameType.F_MENU_ANBAR_TARAZ_F: //لیست تراز موجودی یک انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_TARAZ("F"));
                    break;

                case WinNameType.F_MENU_KART_KARTR2: //گزارش کارت انبار بدون نرخ
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KART("KARTR2"));
                    break;

                case WinNameType.F_MENU_ANBAR_R: //گزارش موجودی کالا ها به تفکیک انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR("R"));
                    break;

                case WinNameType.MOGUDI_ANBARHA_LIST: //لیست موجودی کل انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new MOGUDI_ANBARHA_LIST());
                    break;

                case WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST: //لیست موجودی کالا ها به تفکیک انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR("F"));
                    break;

                #region MyRegion
                case WinNameType.F_MENU_FROOSH: //گزارش فاکتور های فروش به صورت گروهی 
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_FROOSH("R"));
                    break;

                case WinNameType.F_MENU_ANBAR_FRKH: //گزارش فاکتور های فروش به صورت گروهی 
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_FRKH("RF"));
                    break;

                case WinNameType.F_MENU_DATE_HBGHB: //لیست برگه های بدون قبض باسکول
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("HBGHB"), isModalDialog: false, allowMultipleInstances: false);
                    break;


                case WinNameType.F_MENU_DATE_CROS: //لیست برگه های بدون قبض باسکول
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("CROS"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_CFRALL: //لیست فروش روزانه به تفکیک نوع کالا
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("CFRALL"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.FROOSH_COUNTALL: //لیست فروش روزانه به تفکیک نوع کالا
                    CL_LMethods.OpenWindow(OWNERWIN, new FROOSH_COUNTALL("", ""), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_CKRALL: //لیست خرید روزانه به تفکیک نوع کالا
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("CKRALL"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_AMFDAY: //آمار فروش کالا ها به تفکیک روز
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("AMFDAY"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.FROOSH_NARAFTAH: //لیست کالا های فروش نرفته
                    CL_LMethods.OpenWindow(OWNERWIN, new FROOSH_NARAFTAH(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.WIN_F_MENU_KHFR_FONARP: //لیست کالا های فروش نرفته به شخص
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_F_MENU_KHFR("FONARP"));
                    break;

                case WinNameType.F_MENU_DATE_CUSTNP: // لیست محدود شده مشتریانی که خرید نکرده اند
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("CUSTNP"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.LIST_KHARID: // لیست خرید به تفکیک کالا و قبض  باسکول
                    CL_LMethods.OpenWindow(OWNERWIN, new LIST_KHARID(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.LIST_FROOSH: // لیست فروش به تفکیک کالا و قبض  باسکول
                    CL_LMethods.OpenWindow(OWNERWIN, new LIST_FROOSH(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.STUF_DEF_LIST: // لیست مشخصات کلیه کالاها
                    CL_LMethods.OpenWindow(OWNERWIN, new STUF_DEF_LIST(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_GOZARESH_FROOSH_F: //گزارش خلاصه فروش روزانه
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_GOZARESH_FROOSH("F"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_ASNAD_PRINT: //چاپ گروهی اسناد
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ASNAD_PRINT(), isModalDialog: false, allowMultipleInstances: false);
                    break;
                #endregion

                case WinNameType.PMORAKH: //ثبت و ویرایش مرخصی پرسنل
                    CL_LMethods.OpenWindow(OWNERWIN, new PMORAKH((int?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.CHAPCHEK: //چاپ سریع چک
                    CL_LMethods.OpenWindow(OWNERWIN, new CHAPCHEK());
                    break;

                case WinNameType.WIN_CHKE_PLIST: //لیست کل چکهای پرداختی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHKE_PLIST());
                    break;

                case WinNameType.WIN_CHECK_MONP: //لیست چکهای پرداختی به تفکیک ماه | دفتر چکهای ماهانه پرداختی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHECK_MONP());
                    break;

                case WinNameType.F_MENU_CHEK_CHEK_DLISTS: //گزارش چاپی چک های دریافتی سررسید شده
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_CHEK("rchekd", "گزارش چک های دریافتی سررسید شده"));
                    break;

                case WinNameType.PAY_GETD_LOG_FORM: //سوابق تغیر وضعیت چک
                    CL_LMethods.OpenWindow(OWNERWIN, new PAY_GETD_LOG_FORM(null, null, null));
                    break;

                case WinNameType.F_MENU_CHEK_CHEK_VOSUL_LES: //لیست چکهای دریافتی نزد بانک
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_CHEK("chkv", "لیست چکهای دریافتی نزد بانک"));
                    break;

                case WinNameType.WIN_CHKE_DLIST_KOLCHECKD: //لیست کل چکهای دریافتی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHKE_DLIST(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.ZASESABBEESAB_REPLACE_HESAB: //جایگذاری حساب ها
                    CL_LMethods.OpenWindow(OWNERWIN, new ZASESABBEESAB(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK: //لیست چکهای دریافتی نزد بانک و صندوق وصول نشده
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_CHEK("chkva", "لیست چکهای دریافتی نزد بانک و صندوق وصول نشده"));
                    break;

                case WinNameType.WIN_CHECK_MON: //لیست چکهای دریافتی به تفکیک ماه | دفتر چکهای ماهانه دریافتی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHECK_MON());
                    break;

                case WinNameType.WIN_CHREC_HP: //اعلام وصول چک های پرداختی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHREC_HP());
                    break;

                case WinNameType.WIN_CHREC_H: //اعلام وصول چک های دریافتی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_CHREC_H());
                    break;

                case WinNameType.WIN_PAYGETP_LST: //دفتر چک های پرداختی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_PAYGETP_LST());
                    break;

                case WinNameType.WIN_PAYGETD_LST: //دفتر چک های دریافتی
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_PAYGETD_LST());
                    break;

                case WinNameType.C_TARAZ_ANBAR_KHAS: //گزارش روزانه
                    CL_LMethods.OpenWindow(OWNERWIN, new C_TARAZ_ANBAR_KHAS((string?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.MaterialThemSettingy: //تنظیم تم قالب
                    CL_LMethods.OpenWindow(OWNERWIN, new MaterialThemSettingy(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.WinConnectionChoose: //تنظیمات پایگاه داده
                    CL_LMethods.OpenWindow(OWNERWIN, new WinConnectionChoose(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.TCOD_MAPF_WIN: //تعریف کد مپ شماره فنی
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_MAPF_WIN(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.DEFAULT: //پیش فرض واحد و شیفت
                    CL_LMethods.OpenWindow(OWNERWIN, new DEFAULT(), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.WIN_F_MENU_KHFR_FKHAREDAY: //خلاصه خـرید روزانه به تفکیک اشخاص
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_F_MENU_KHFR("FKHAREDAY"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.WIN_F_MENU_KHFR_FROOSHDAY: //خلاصه فروش روزانه به تفکیک اشخاص
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_F_MENU_KHFR("FROOSHDAY"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_GOZARESH_FROOSH: //گزارش فروش روزانه واحد ها
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_GOZARESH_FROOSH(default), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_FRCUST: //گزارش ارزش افزوده فروش - گزارش فصلی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("FRCUST"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_FASLIBR: //گزارش فصلی (برگشت فروش مشتریان) برگشت فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("FASLIBR"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_FASLIKHBR: //گزارش فصلی برگشت خرید
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("FASLIKHBR"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_KHCUST: //گزارش ارزش افزوده خرید - گزارش فصلی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("KHCUST"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_KHLS: //گزارش خرید به تفکیک فاکتور
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("KHLS"), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.F_MENU_DATE_LFACT: //گزارش فروش به تفکیک فاکتور
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE("LFACT"), isModalDialog: false, allowMultipleInstances: false);
                    break; //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                case WinNameType.ANBGRD_HEAD_WIN: //انبار گردانی
                    CL_LMethods.OpenWindow(OWNERWIN, new ANBGRD_HEAD_WIN((int?)_PARAMETERS_.FirstOrDefault()), isModalDialog: false, allowMultipleInstances: true);
                    break;

                case WinNameType.F_MENU_KART: //کارت انبار

                    if (string.IsNullOrEmpty(_PARAMETERS_?.FirstOrDefault()?.ToStringNullSafe()))
                    {
                        CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KART("R"));
                    }
                    else
                    {
                        CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KART((string?)_PARAMETERS_.FirstOrDefault()));
                    }
                    break;

                case WinNameType.HAVALE_LIST: //مشاهده حواله های فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new HAVALE_LIST());
                    break;

                case WinNameType.HEAD_LST_ENTEGHAL_WIN: //انتقال از انبار به انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_ENTEGHAL_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_HAV_OTHER_WIN: //سایر حواله انبار ها
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_HAV_OTHER_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_HAVL: //حواله انبار فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_HAVL((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_RASID: //رسید انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_RASID((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_RASID_OTHER_WIN: //سایر رسید انبار ها
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_RASID_OTHER_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.MOGUDI_SEARCH_MAIN: //جستجو گر موجودی
                    CL_LMethods.OpenWindow(OWNERWIN, new MOGUDI_SEARCH_MAIN());
                    break;

                case WinNameType.DEED_HEAD: //صدور و ویرایش اسناد
                    CL_LMethods.OpenWindow(OWNERWIN, new DEED_HEAD((double?)_PARAMETERS_.FirstOrDefault()), isModalDialog: false, allowMultipleInstances: true);
                    break;

                case WinNameType.DEED_HEAD_LIST: //لیست اسناد
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_ALLSANAD()); //CL_LMethods.OpenWindow(OWNERWIN, new DEED_HEAD_LIST());
                    break;

                case WinNameType.DEED_SEARCH_MAIN: //جستجو در شرح سند
                    CL_LMethods.OpenWindow(OWNERWIN, new DEED_SEARCH_MAIN());
                    break;

                case WinNameType.F_MENU_ASNAD: //تایید و قطعی کردن اسناد تایید نشده
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ASNAD());
                    break;

                case WinNameType.F_MENU_KOL_MOIN_TAFZIL: //لیست دفتر تفضیلی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KOL_MOIN_TAFZIL(_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.F_MENU_KOL_MOIN_TAFZIL_TAF: //گزارش چاپی دفتر تفضیلی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_KOL_MOIN_TAFZIL("TAF"));
                    break;

                case WinNameType.MOGHAYERAT: //صورت مغایرت های گرفته شده
                    CL_LMethods.OpenWindow(OWNERWIN, new MOGHAYERAT((int?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.F_MENU_MOGHAYERAT: //صورت مغایرت
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_MOGHAYERAT());
                    break;

                case WinNameType.paymentformorder: //درخواست پرداخت وجه
                    CL_LMethods.OpenWindow(OWNERWIN, new paymentformorder((double?)_PARAMETERS_.FirstOrDefault(), _PARAMETERS_.Length > 1 ? (bool)_PARAMETERS_[1] : false));
                    break;

                case WinNameType.paymentformorder_LIST: //لیست درخواست پرداخت وجه
                    CL_LMethods.OpenWindow(OWNERWIN, new paymentformorder_LIST());
                    break;

                case WinNameType.PGET_HED: //خزانه داری
                    CL_LMethods.OpenWindow(OWNERWIN, new Prg_UI.Wins.WinMenus.HESABDARI.PGET_HED((double?)_PARAMETERS_.FirstOrDefault()), isModalDialog: false, allowMultipleInstances: false);
                    break;

                case WinNameType.PGET_HED_LIST: //لیست خزانه ها
                    CL_LMethods.OpenWindow(OWNERWIN, new PGET_HED_LIST());
                    break;

                case WinNameType.FACTORS_LST: //کلیه انواع فاکتور
                    CL_LMethods.OpenWindow(OWNERWIN, new FACTORS_LST(Convert.ToByte(_PARAMETERS_.FirstOrDefault())));
                    break;

                case WinNameType.HEAD_LST_BRFR: //فاکتور برگشت فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_BRFR((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_FROOSH_BACK2: //فاکتور برگشت فروش - عادی
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_FROOSH_BACK2((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_FROOSH_AUTO_DETECT: //فاکتور فروش 
                    {
                        var MostaghimDastrasi = CL_HESABDARI.LETSGO("FRMOST"); //فاکتور فروش مستقیم عمل شود از تعیین سطح دسترسی
                        var MostaghimAmalShavad = Strings.Mid(Baseknow.OPTIONSS, 53, 1) == "5"; //فاکتور فروش مستقیم تیک داره از تنظیمات بیشتر

                        if (MostaghimAmalShavad && MostaghimDastrasi) //مستقیم
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_DIRECT, OWNERWIN, (string?)_PARAMETERS_.FirstOrDefault());
                        }
                        else if (Strings.Mid(Baseknow.OPTIONSS, 18, 1) != "5") //غیر مستقیم
                        {
                            if (Baseknow.TKHF < 2) { }
                            else
                            {
                                CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_HAVALEHEE, OWNERWIN, (string?)_PARAMETERS_.FirstOrDefault());
                            }
                        }
                        else
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_DIRECT, OWNERWIN, (string?)_PARAMETERS_.FirstOrDefault());
                        }

                    }
                    break;

                case WinNameType.HEAD_LST_FROOSH22_DIRECT: //فاکتور فروش مستقیم
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_FROOSH22((string?)_PARAMETERS_.FirstOrDefault(), true, false));
                    break;

                case WinNameType.HEAD_LST_FROOSH22_HAVALEHEE: //فاکتور فروش با انتخاب حواله
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_FROOSH22((string?)_PARAMETERS_.FirstOrDefault(), false, false));
                    break;

                case WinNameType.HEAD_LST_FROOSH22_SADERATI: //فاکتور فروش (با انتخاب حواله) صادراتی
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_FROOSH22((string?)_PARAMETERS_.FirstOrDefault(), false, true));
                    break;

                case WinNameType.HEAD_LST_KH_BACK: //فاکتور برگشت خرید - عادی
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KH_BACK((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_KH_BACK_AZAD: //فاکتور برگشت خرید آزاد
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KH_BACK_AZAD((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_KHADAMAT: //فاکتور (فروش) خدمات
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KHADAMAT((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_KHAREED1_DIRECT: //فاکتور خرید مستقیم
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KHAREED1((double?)_PARAMETERS_.FirstOrDefault(), true, false));
                    break;

                case WinNameType.HEAD_LST_KHAREED1_RASID: //فاکتور خرید با انتخاب رسید
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KHAREED1((double?)_PARAMETERS_.FirstOrDefault(), false, false));
                    break;

                case WinNameType.HEAD_LST_KHAREED1_SADERATI: //فاکتور خرید (با انتخاب رسید) صادراتی
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_KHAREED1((double?)_PARAMETERS_.FirstOrDefault(), false, true));
                    break;

                case WinNameType.HEAD_LST_PISHFROOSH2: //پیش فاکتور فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_PISHFROOSH2((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.IRAN_SALES_MAP: //گزارش فروش روی نقشه
                    CL_LMethods.OpenWindow(OWNERWIN, new IRAN_SALES_MAP());
                    break;

                case WinNameType.PGET_LST_SEARCH: //لیست کلیه خزانه ها
                    CL_LMethods.OpenWindow(OWNERWIN, new PGET_LST_SEARCH());
                    break;

                case WinNameType.BUDGET0:
                    CL_LMethods.OpenWindow(OWNERWIN, new BUDGET0(), false, false);
                    break;

                case WinNameType.BUGET_MAIN_list2: //لیست بودجه ها
                    CL_LMethods.OpenWindow(OWNERWIN, new BUGET_MAIN_list2());
                    break;

                case WinNameType.NABZEDARY: //فعالیت سازمان
                    CL_LMethods.OpenWindow(OWNERWIN, new NABZEDARY());
                    break;

                case WinNameType.NABZEFROOSH: //نبض فروش
                    CL_LMethods.OpenWindow(OWNERWIN, new NABZEFROOSH());
                    break;

                case WinNameType.NABZEMALI: //نبض مالی
                    CL_LMethods.OpenWindow(OWNERWIN, new NABZEMALI());
                    break;

                case WinNameType.AUTOMATIC: //حساب های خودگردان
                    CL_LMethods.OpenWindow(OWNERWIN, new AUTOMATIC());
                    break;

                case WinNameType.AZAE_WIN: //مشخصات مشتریان - اعضاء  و اعتبارات
                    CL_LMethods.OpenWindow(OWNERWIN, new AZAE_WIN());
                    break;

                case WinNameType.CUSTKIND_WIN: //تعریف نوع مشتری
                    CL_LMethods.OpenWindow(OWNERWIN, new CUSTKIND_WIN());
                    break;

                case WinNameType.DEPART_WIN: //تعریف واحد های زیر مجموعه سازمان
                    CL_LMethods.OpenWindow(OWNERWIN, new DEPART_WIN());
                    break;

                case WinNameType.FCODE_CUSTOMER: //تعریف مشتری
                    CL_LMethods.OpenWindow(OWNERWIN, new FCODE_CUSTOMER((string?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.GRADE_FORMAT_WIN: //فرمت رده بندی مشتری
                    CL_LMethods.OpenWindow(OWNERWIN, new GRADE_FORMAT_WIN());
                    break;

                case WinNameType.KHAD_DEF: //تعریف خدمات
                    CL_LMethods.OpenWindow(OWNERWIN, new KHAD_DEF());
                    break;

                case WinNameType.MASRAF_CENTER: //مراکز مصرف
                    CL_LMethods.OpenWindow(OWNERWIN, new MASRAF_CENTER());
                    break;


                case WinNameType.SHIFT_WIN: //تعریف شیفت
                    CL_LMethods.OpenWindow(OWNERWIN, new SHIFT_WIN());
                    break;

                case WinNameType.STUF_DEF_LST: //جستجو گر کالا ها | لیست کالا ها
                    CL_LMethods.OpenWindow(OWNERWIN, new STUF_DEF_LST());
                    break;

                case WinNameType.STUF_DEF_WIN: //تعریف کالا
                    CL_LMethods.OpenWindow(OWNERWIN, new STUF_DEF_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.TAKHFIF_DEF_FORM: //تعریف تخفیفات پیشرفته تر
                    CL_LMethods.OpenWindow(OWNERWIN, new TAKHFIF_DEF_FORM());
                    break;

                case WinNameType.TCOD_ANBAR_WIN: //تعریف انبار ها
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_ANBAR_WIN());
                    break;

                case WinNameType.TCOD_BANKS_WIN: //تعریف بانکها
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_BANKS_WIN());
                    break;

                case WinNameType.TCOD_HESGROUP_WIN: //تعریف گروه حساب ها
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_HESGROUP_WIN());
                    break;

                case WinNameType.TCOD_MARKAZHAZ_WIN: //تعریف مراکز هزینه
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_MARKAZHAZ_WIN());
                    break;

                case WinNameType.TCOD_STUFGROUP_WIN: //تعریف گروه کالا ها
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_STUFGROUP_WIN());
                    break;

                case WinNameType.TCOD_VAHEDS_WIN: //تعریف واحد ها
                    CL_LMethods.OpenWindow(OWNERWIN, new TCOD_VAHEDS_WIN());
                    break;

                case WinNameType.TCODE_MENUITEM_WIN: //تعریف گروه بندی برای ویزیتور
                    CL_LMethods.OpenWindow(OWNERWIN, new TCODE_MENUITEM_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.TOTA_HES_SHEET_WIN: //تعریف سرفصل حسابهای کل
                    CL_LMethods.OpenWindow(OWNERWIN, new TOTA_HES_SHEET_WIN((string?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.Automasion_MAIN: //اتوماسیون اداری
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MAIN.MAIN_INST.Owner = PublicVRB.WINBASE;
                        MAIN.MAIN_INST.Show();
                        MAIN.MAIN_INST.Activate();
                    });
                    break;

                case WinNameType.F_MENU_DATE_TAX_REPORT:
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_DATE((string)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.BEDEHKARAN_BESTANKARAN: //لیست کل بدهکاران و بستانکاران
                    CL_LMethods.OpenWindow(OWNERWIN, new BEDEHKARAN_BESTANKARAN(), false, false);
                    break;

                case WinNameType.BEDEHKARAN_BESTANKARAN_LIMITED: //لیست کل بدهکاران و بستانکاران محدود شده
                    BEDEHKARAN_BESTANKARAN bEDEHKARAN_BESTANKARAN = new BEDEHKARAN_BESTANKARAN(); bEDEHKARAN_BESTANKARAN.IsCTRLF9 = true;
                    CL_LMethods.OpenWindow(OWNERWIN, bEDEHKARAN_BESTANKARAN, false, false);
                    break;

                case WinNameType.F_MENU_ANBAR_FRKH_GRP:  //گزارش حواله انبار گروهی
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_FRKH_GRP());
                    break;

                case WinNameType.F_MENU_ANBAR_MERG: //لیست موجودی کالای انبار های تلفیق شده
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR_MERG());
                    break;

                case WinNameType.F_MENU_ANBAR: //لیست موجودی کالا ها به تفکیک انبار
                    CL_LMethods.OpenWindow(OWNERWIN, new F_MENU_ANBAR((string)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.HEAD_LST_REQUEST_WIN: //درخواست خرید
                    CL_LMethods.OpenWindow(OWNERWIN, new HEAD_LST_REQUEST_WIN((double?)_PARAMETERS_.FirstOrDefault()));
                    break;

                case WinNameType.FROOSHDAY: //خلاصه فروش روزانه به تفکیک اشخاص
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_F_MENU_KHFR("FROOSHDAY"));
                    break;

                case WinNameType.FKHAREDAY: //خلاصه خـرید  روزانه به تفکیک اشخاص
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_F_MENU_KHFR("FKHAREDAY"));
                    break;

                case WinNameType.WIN_MOADIAN_SINGLE: //ارسال به مودیان در پنجره جدا
                    CL_LMethods.OpenWindow(OWNERWIN, new WIN_MOADIAN_SINGLE(Convert.ToDouble(_PARAMETERS_.FirstOrDefault())), default, false);
                    break;


                default: throw new ArgumentException("Invalid WinNameType");
            }
        }

        public static void MenuBaseOnKindOpen(Window Thewindowthis, CL_CCNNMANAGER dbms, int kind, object _NUM_, bool SanadMabnaee = true)
        {
            switch (kind)
            {
                case 0:
                    if (SanadMabnaee)
                    {
                        //شماره مبنا درج شده num base
                        var _N_Sb_ = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 N_S FROM dbo.DEED_HED WHERE base = {_NUM_}").FirstOrDefault();
                        if (_N_Sb_ != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, Thewindowthis, (double)_N_Sb_);
                        }
                    }
                    else
                    {
                        //شماره سند درج شده num N_S
                        var _N_S2_ = dbms.DoGetDataSQL<double?>($"SELECT TOP 1 N_S FROM dbo.DEED_HED WHERE N_S = {_NUM_}").FirstOrDefault();
                        if (_N_S2_ != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.DEED_HEAD, Thewindowthis, (double)_N_S2_);
                        }
                    }
                    break;

                case 1:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_RASID, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 2:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_HAVL, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 3:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 4:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 5:
                case 34:
                    var _N_S_KHAZANEH_ = dbms.DoGetDataSQL<double?>($"SELECT ID FROM dbo.PGET_HED WHERE N_S = {_NUM_}").FirstOrDefault();
                    if (_N_S_KHAZANEH_ != null)
                    {
                        CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, Thewindowthis, _N_S_KHAZANEH_);
                    }
                    else
                    {
                        double _KHAZANEH_ = Convert.ToDouble(_NUM_);

                        _N_S_KHAZANEH_ = dbms.DoGetDataSQL<double?>($"SELECT ID FROM dbo.PGET_HED WHERE ID = {_NUM_}").FirstOrDefault();
                        if (_N_S_KHAZANEH_ != null)
                        {
                            CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PGET_HED, Thewindowthis, _KHAZANEH_);
                        }
                    }
                    break;

                case 6:
                    //new HEAD_LST_ENTEGHAL_WIN(_NUM_).Show();
                    break;

                case 12:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_RASID, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 13:
                    //گرفتن شماره فاکتور و شماره حواله از فاکتور های فروش
                    var RST = dbms.DoGetDataSQL<HEAD_LST>($"SELECT NUMBER, NUMBER1 FROM dbo.HEAD_LST WHERE NUMBER={_NUM_} AND TAG=13").FirstOrDefault();
                    if (RST != null && RST?.NUMBER1 != null)
                    {
                        var NUMBERTOOPEMN = $"{RST.NUMBER1},{RST.NUMBER}";
                        CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_FROOSH_AUTO_DETECT, Thewindowthis, NUMBERTOOPEMN);
                    }

                    //HEAD_LST_FROOSH22? newWindow = null;
                    //if (Strings.Mid(Baseknow.OPTIONSS, 53, 1) == "5")
                    //{
                    //    newWindow = new HEAD_LST_FROOSH22($"{RST.NUMBER1},{RST.NUMBER}", false); //حواله ای
                    //}
                    //else if (Strings.Mid(Baseknow.OPTIONSS, 18, 1) != "5")
                    //{
                    //    if (Baseknow.TKHF < 2) { }
                    //    else
                    //    {
                    //        newWindow = new HEAD_LST_FROOSH22($"{RST.NUMBER1},{RST.NUMBER}", false); //حواله ای
                    //    }
                    //}
                    //else
                    //{
                    //    newWindow = new HEAD_LST_FROOSH22($"{RST.NUMBER1},{RST.NUMBER}", true);
                    //}
                    //CL_LMethods.OpenWindow(Thewindowthis, newWindow, isModalDialog: false, allowMultipleInstances: true);
                    break;

                case 20:
                    //new HEAD_LST_PISHFROOSH2(_NUM_).Show();
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 24:
                    //new HEAD_LST_RASID_OTHER_WIN(_NUM_).Show();
                    break;

                case 25:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_BRFR, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 26:
                    //new HEAD_LST_HAV_OTHER_WIN(_NUM_).Show();
                    break;

                case 27:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 32:
                    //new HEAD_LST_KHAREED1_SADER(_NUM_).Show();
                    break;

                case 33:
                    //new HEAD_LST_FROOSH2_SADER(_NUM_).Show();
                    break;

                case 35:
                    //new ORDR_HED(_NUM_).Show();
                    break;

                case 36:
                    //new HEAD_LST_REQUEST(_NUM_).Show();
                    break;

                case 37:
                    //new PMORAKH(_NUM_).Show();
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.PMORAKH, default, Convert.ToInt32(_NUM_));
                    break;

                case 38:
                    //new HAVALAH_EXIT_SAYER(_NUM_).Show();
                    break;

                case 39:
                    //new HAVALAH_EXIT(_NUM_).Show();
                    break;

                case 40:
                    //new PM_ASSETS_FR(_NUM_).Show();
                    break;

                case 41:
                    //new PM_HEAD_LST_REQUEST_ANBAR(_NUM_).Show();
                    break;

                case 42:
                    //new PM_EM_FAILURE_EVENTS_FR(_NUM_).Show();
                    break;

                case 100:
                    CL_MenuManager.OpenWinMenu(CL_MenuManager.WinNameType.paymentformorder, Thewindowthis, Convert.ToDouble(_NUM_));
                    break;

                case 90:
                    //new PTAMIR(_NUM_).Show();
                    break;




                default: break;
            }
        }
    }
}