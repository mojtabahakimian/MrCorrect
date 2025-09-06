using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System;
using Prg_UI;
using Prg_UI.Functions;
using System.Windows.Media;
using System.IO;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.Generaly;
using Functions;
using static Prg_Proccessy.SQLMODELS.CTABLES;
using static Functions.CL_MenuManager;
using Wins.WinMenus.KHARID_FORUSH.GOZARESHAT;
using Prg_UI.Wins.WinMenus.BARNAME_RIZI;

namespace Wins.WinOther
{
    public partial class SEARCHMENIU_WIN : Window
    {
        #region Header Window Begin
        //Header Window Begin
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            PackIcon packIcon = new PackIcon();
            switch (WindowState)
            {
                case WindowState.Maximized:
                    //🗖,🗗
                    WindowState = WindowState.Normal;
                    packIcon.Kind = PackIconKind.WindowMaximize;
                    Btn_Max.Content = packIcon;
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    packIcon.Kind = PackIconKind.WindowRestore;
                    Btn_Max.Content = packIcon;
                    break;
            }
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)
            {
                Btn_Max_Click(null, null);
            }
        }
        //Header Window End;
        #endregion
        public SEARCHMENIU_WIN()
        {
            InitializeComponent();
            this.Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            // Clear the static reference when the window is closed
            App.SEARCHMENIU_WIN_ST = null;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMenuItemModels();

            SEARCH_TEXT_MENU.Focus();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                if (IsFocusWithinListBox(LISTBOX_MENU) && LISTBOX_MENU.SelectedItem != null)
                {
                    LetsOpenTheWin();
                }
                else
                {
                    e.Handled = true;

                    CL_LMethods.SendKey_US(Key.Tab);
                }
            }
            else if (e.Key == Key.Down && Keyboard.Modifiers == ModifierKeys.None) // Detect Down Arrow key
            {
                if (SEARCH_TEXT_MENU.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    CL_LMethods.SendKey_US(Key.Tab);
                    if (LISTBOX_MENU.SelectedValue == null)
                    {
                        LISTBOX_MENU.SelectedIndex = 0;
                    }
                }

                //if (SEARCH_TEXT_MENU.IsFocused)
                //{
                //    if (LISTBOX_MENU.Items.Count > 0)
                //    {
                //        LISTBOX_MENU.SelectedIndex = 0;
                //    }
                //    CL_LMethods.SendKey_US(Key.Tab);
                //    LISTBOX_MENU.Focus();
                //}
            }
            else if (e.Key is Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
            }
        }

        public class MenuItemModel
        {
            public int ID { get; set; }
            public string CAPTION { get; set; }
            public CL_MenuManager.WinNameType WIN_NAME { get; set; }
            public string REMARKS { get; set; }
            public bool ISCONFIRMED { get; set; } = false;
            public string ShortName { get; set; }
            public bool IsAllowed { get; set; } = true;
            public object TheParam { get; set; }

            public int RowNumber { get; set; }
        }
        private void AssignRowNumbers(List<MenuItemModel> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].RowNumber = i + 1;
            }
        }
        private List<MenuItemModel> MenuItemModels = new List<MenuItemModel>();
        private void LoadMenuItemModels()
        {
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "اتوماسیون", WIN_NAME = CL_MenuManager.WinNameType.Automasion_MAIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "انتقال کالا از یک انبار به انبار دیگر (انتقال انبار به انبار)", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_ENTEGHAL_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "انبار گردانی", WIN_NAME = CL_MenuManager.WinNameType.ANBGRD_HEAD_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "پیش فاکتور", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_PISHFROOSH2 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "پیش فرض واحد و شیفت", WIN_NAME = CL_MenuManager.WinNameType.DEFAULT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف گروه کالا ها", WIN_NAME = CL_MenuManager.WinNameType.TCOD_STUFGROUP_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف واحد های کالا ها", WIN_NAME = CL_MenuManager.WinNameType.TCOD_VAHEDS_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف انبار ها", WIN_NAME = CL_MenuManager.WinNameType.TCOD_ANBAR_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف بانکها", WIN_NAME = CL_MenuManager.WinNameType.TCOD_BANKS_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف خدمات", WIN_NAME = CL_MenuManager.WinNameType.KHAD_DEF });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف شیفت", WIN_NAME = CL_MenuManager.WinNameType.SHIFT_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف کالا ها", WIN_NAME = CL_MenuManager.WinNameType.STUF_DEF_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف کد مپ شماره فنی", WIN_NAME = CL_MenuManager.WinNameType.TCOD_MAPF_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف گروه حساب ها", WIN_NAME = CL_MenuManager.WinNameType.TCOD_HESGROUP_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف مشتری جدید", WIN_NAME = CL_MenuManager.WinNameType.FCODE_CUSTOMER });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف نوع مشتری", WIN_NAME = CL_MenuManager.WinNameType.CUSTKIND_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف واحد های زیر مجموعه سازمان", WIN_NAME = CL_MenuManager.WinNameType.DEPART_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تنظیمات اتصال به پایگاه داده", WIN_NAME = CL_MenuManager.WinNameType.WinConnectionChoose });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تنظیمات تِم", WIN_NAME = CL_MenuManager.WinNameType.MaterialThemSettingy });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "جستجوگر موجودی کالا", WIN_NAME = CL_MenuManager.WinNameType.MOGUDI_SEARCH_MAIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "حساب های خودگردان", WIN_NAME = CL_MenuManager.WinNameType.AUTOMATIC });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "خزانه داری (دریافت پرداخت) F5", WIN_NAME = CL_MenuManager.WinNameType.PGET_HED, ShortName = "KHAZANEH" });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "سرفصل حسابهای کل", WIN_NAME = CL_MenuManager.WinNameType.TOTA_HES_SHEET_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "شروع بودجه", WIN_NAME = WinNameType.BUDGET0 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور و ویرایش اسناد (سند) F10", WIN_NAME = WinNameType.DEED_HEAD, ShortName = "SANAD" });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور فروش", WIN_NAME = WinNameType.HEAD_LST_FROOSH_AUTO_DETECT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده فاکتور های فروش", WIN_NAME = WinNameType.HEAD_LST_FROOSH_AUTO_DETECT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "راس گیری", WIN_NAME = WinNameType.RAAS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور خرید", WIN_NAME = WinNameType.HEAD_LST_KHAREED1_RASID });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف فرمت گرید بندی مشتریان (فرمت رده بندی مشتری)", WIN_NAME = WinNameType.GRADE_FORMAT_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = false, CAPTION = "فروش روی نقشه", WIN_NAME = CL_MenuManager.WinNameType.IRAN_SALES_MAP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "نبض فعالیت سازمان", WIN_NAME = CL_MenuManager.WinNameType.NABZEDARY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش کارت انبار یک کالا", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KART });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "کدینگ مپ", WIN_NAME = CL_MenuManager.WinNameType.TCOD_MAP_GRP_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست اسناد", WIN_NAME = CL_MenuManager.WinNameType.DEED_HEAD_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست بوجه های گرفته شده (چاپی)", WIN_NAME = CL_MenuManager.WinNameType.BUGET_MAIN_list2 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست پیش فاکتور ها", WIN_NAME = CL_MenuManager.WinNameType.FACTORS_LST, TheParam = Convert.ToByte(20) });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست حواله فروش ها", WIN_NAME = CL_MenuManager.WinNameType.FACTORS_LST, TheParam = Convert.ToByte(2) });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده فاکتور های فروش", WIN_NAME = CL_MenuManager.WinNameType.FACTORS_LST, TheParam = Convert.ToByte(13) });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "جستجو در شرح عملکرد خزانه", WIN_NAME = CL_MenuManager.WinNameType.PGET_LST_SEARCH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست خزانه", WIN_NAME = CL_MenuManager.WinNameType.PGET_HED_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست دفتر تفضیلی Ctrl + F8", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش چاپی دفتر تفضیلی F8", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_TAF });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست بدهکاران و بستانکاران F9", WIN_NAME = CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست بدهکاران و بستانکاران محدود شده Ctrl + F9", WIN_NAME = CL_MenuManager.WinNameType.BEDEHKARAN_BESTANKARAN_LIMITED });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت مغایرت های گرفته شده", WIN_NAME = CL_MenuManager.WinNameType.MOGHAYERAT });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست کالا ها", WIN_NAME = CL_MenuManager.WinNameType.STUF_DEF_LST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشخصات اعضا و اعتبارات", WIN_NAME = CL_MenuManager.WinNameType.AZAE_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "نبض فروش", WIN_NAME = CL_MenuManager.WinNameType.NABZEFROOSH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "نبض مالی", WIN_NAME = CL_MenuManager.WinNameType.NABZEMALI });


            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف مرکز مصرف", WIN_NAME = CL_MenuManager.WinNameType.MASRAF_CENTER });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف تخفیفات پیشرفته", WIN_NAME = CL_MenuManager.WinNameType.TAKHFIF_DEF_FORM });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "چاپ گروهی اسناد", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ASNAD_PRINT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست درخواست پرداخت‌ ها", WIN_NAME = CL_MenuManager.WinNameType.paymentformorder_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "درخواست پرداخت وجه", WIN_NAME = CL_MenuManager.WinNameType.paymentformorder });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "جستجو در شرح اسناد", WIN_NAME = CL_MenuManager.WinNameType.DEED_SEARCH_MAIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مغایرت گیری یک حساب", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_MOGHAYERAT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت مغایرت‌های گرفته شده", WIN_NAME = CL_MenuManager.WinNameType.MOGHAYERAT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور خدمات", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_KHADAMAT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور برگشت فروش (استاندارد - عادی)", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_FROOSH_BACK2 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور برگشت خرید (استاندارد - عادی)", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_KH_BACK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور برگه رسید انبار خرید کالا Ctrl + W", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_RASID });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور برگه حواله انبار فروش کالا Ctrl + E", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_HAVL });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور فروش صادراتی", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_FROOSH22_SADERATI });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور خرید صادراتی", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_KHAREED1_SADERATI });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور برگشت فروش آزاد (رسید شده)", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_BRFR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "فاکتور برگشت خرید آزاد", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_KH_BACK_AZAD });


            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "سایر رسید انبار ها", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_RASID_OTHER_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "سایر حواله انبار ها", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_HAV_OTHER_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست موجودی کل انبار", WIN_NAME = CL_MenuManager.WinNameType.MOGUDI_ANBARHA_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش موجودی کالاها به تفکیک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR /* یا مورد مشابه */ });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز موجودی کل انبار ها", WIN_NAME = CL_MenuManager.WinNameType.C_TARAZ_ANBAR_KHAS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز موجودی کل انبار ها", WIN_NAME = CL_MenuManager.WinNameType.R_TARAZ_ANBARHA });

            //MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش موجودی انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR });
            //MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت حساب هوشمند مشتریان جهت تسویه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_DATE_AI });
            //MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز انبارها به تفکیک گروه کالا", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ});

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش موجودی کالا ها به تفکیک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_R });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست موجودی کالا ها به تفکیک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_F_AK_MOGUDI_ANBAR_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست موجودی انبار ها تلفیق شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_MERG });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش کارت انبار بدون نرخ", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KART_KARTR2 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز موجودی یک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_F });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز موجودی یک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_R });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز انبار به تفکیک نوع برگه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_FD });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز انبار ها به تفکیک گروه کالا", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_TARAZ_TANBGRP });


            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش حواله انبار گروهی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_GRP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تایید و قطعی کردن اسناد تایید نشده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ASNAD });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "درخواست خرید", WIN_NAME = CL_MenuManager.WinNameType.HEAD_LST_REQUEST_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف گروه بندی کالا ها برای ویزیت و منو", WIN_NAME = CL_MenuManager.WinNameType.TCODE_MENUITEM_WIN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "خلاصه خرید روزانه به تفکیک اشخاص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FKHAREDAY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "خلاصه فروش روزانه به تفکیک اشخاص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FROOSHDAY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فروش به تفکیک واحد ها", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش ارزش افزوده فروش - گزارش فصلی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_FRCUST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فصلی برگشت فروش (برگشت فروش مشتریان)", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_FASLIBR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فصلی برگشت خرید", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_FASLIKHBR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش ارزش افزوده خرید - گزارش فصلی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_KHCUST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش خرید به تفکیک فاکتور", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_KHLS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فروش به تفکیک فاکتور", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_LFACT });


            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "دفتر چکهای دریافتی", WIN_NAME = CL_MenuManager.WinNameType.WIN_PAYGETD_LST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "دفتر چکهای پرداختی", WIN_NAME = CL_MenuManager.WinNameType.WIN_PAYGETP_LST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "اعلام وصول چکهای دریافتی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHREC_H });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "اعلام وصول چکهای پرداختی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHREC_HP });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی به تفکیک ماه | دفتر چکهای ماهانه دریافتی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHECK_MON });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی نزد بانک و صندوق وصول نشده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VLISTALL_NAZDEBANK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "جایگذاری حساب ها", WIN_NAME = CL_MenuManager.WinNameType.ZASESABBEESAB_REPLACE_HESAB });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست کل چکهای دریافتی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHKE_DLIST_KOLCHECKD });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی نزد بانک", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_VOSUL_LES });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "سوابق تغییر وضعیت و موقعیت چکهای دریافتی", WIN_NAME = CL_MenuManager.WinNameType.PAY_GETD_LOG_FORM });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش چکهای دریافتی سررسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHEK_DLISTS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای پرداختی به تفکیک ماه | دفتر چکهای ماهانه پرداختی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHECK_MONP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست کل چکهای پرداختی", WIN_NAME = CL_MenuManager.WinNameType.WIN_CHKE_PLIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای پرداختی اعلام وصول نشده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CVP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "Ctrl + 7 چاپ سریع چک", WIN_NAME = CL_MenuManager.WinNameType.CHAPCHEK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "ثبت و ویرایش مرخصی پرسنل", WIN_NAME = CL_MenuManager.WinNameType.PMORAKH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشخصات سیسیتم", WIN_NAME = CL_MenuManager.WinNameType.WIN_SAZMAN });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تنظیم قالب و رنگ", WIN_NAME = CL_MenuManager.WinNameType.MaterialThemSettingy });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور برگه خروج موارد اولیه از انبار", WIN_NAME = CL_MenuManager.WinNameType.HAVALAH_EXIT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور برگه ورود کالای ساخته شده به انبار", WIN_NAME = CL_MenuManager.WinNameType.HAVALAH_ENTER });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "دفتر روزنامه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_DFTROOS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "دفتر کل", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_DATE });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "دفتر معین", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_DATE });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "چاپ خلاصه دفتر کل", WIN_NAME = CL_MenuManager.WinNameType.R_DAFTAR_KOL_kh });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "حساب تفکیکی تفضیلی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_KOL_MOIN_TAFKIK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش عملکرد روزانه خزانه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_DPDAY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "چاپ سرفصل حسابها", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_PRINTHES });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست اسنادی که تراز نیستند", WIN_NAME = CL_MenuManager.WinNameType.ADAMTARAZ });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز های آزمایشی حسابهای معین", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست مراکز هزینه", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_MARKAZ });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "ترازنامه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_SND });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت منابع و مصارف وجوه نقد (گردش وجوه نقد)", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_MANABE });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعیین سطح دسترسی", WIN_NAME = CL_MenuManager.WinNameType.F_USER_PERMITION_FORMS_DASTRASI });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی کل", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_KOL_FT4 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی معین", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_MOIN_FT4M });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی تفضیلی", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_TAFZILI_FT4T });

            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تنظیم پیش فرض کاربران برای ارجاع", WIN_NAME = CL_MenuManager.WinNameType.UserPersonelOrderWin_PishfarzeErja });


            // case WinNameType.WIN_VISIT_ROUTE_FORM: //تعيين مسير وزيت براي مشتريان
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تنظیم لیست دستی توضیع", WIN_NAME = CL_MenuManager.WinNameType.WIN_TOZIE });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف ویزیت روزانه برای ویزیتور", WIN_NAME = CL_MenuManager.WinNameType.VISITOR_DAY_HEAD });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعيين مسير وزيت براي مشتريان", WIN_NAME = CL_MenuManager.WinNameType.WIN_VISIT_ROUTE_FORM });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "نمودار فروش کلی", WIN_NAME = CL_MenuManager.WinNameType.AMAR_FROOSH_KOL });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی سررسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای برگشتی (دریافتی)", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHKB });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف اعلامیه قیمت", WIN_NAME = CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_GHEYMAT });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف اعلامیه تخفیف", WIN_NAME = CL_MenuManager.WinNameType.PRICE_ELAMIE_FORM_ELAMIYEH_TAKHFIF });
            
            //گزارشات ویزیتوری
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش عملکرد ویزیتور ها به تفکیک کالا (برگشتی در ماه فروش)", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITDLV });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش عملکرد ویزیتور ها به تفکیک کالا (برگشتی در تاریخ برگشت)", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITKALMA });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست ویزیتور ها و پورسانت به تفکیک فاکتور", WIN_NAME = CL_MenuManager.WinNameType.flist_porsant_factors });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست فروش ویزیتور به تفکیک فاکتور", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_VISIT_VISITONE });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "چاپ لیست کالا ها جهت ویزیت", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_AJNAS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه", WIN_NAME = CL_MenuManager.WinNameType.VISITOR_GOL_REP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه بر اساس گروه", WIN_NAME = CL_MenuManager.WinNameType.VISITOR_GOL_GRP_REP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه بر اساس گروه (در تاریخ برگشت)", WIN_NAME = CL_MenuManager.WinNameType.VISITOR_GOL_GRP_REP_MAR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مشاهده عملکرد (اهداف) هر ویزیتور به تفکیک ماه (در تاریخ برگشت)", WIN_NAME = CL_MenuManager.WinNameType.VISITOR_GOL_REP_MAR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صدور برگه خروج سایر مواد از انبار", WIN_NAME = CL_MenuManager.WinNameType.HAVALE_EXIT_SAYER });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "مدیریت ارتباط با مشتری | CRM", WIN_NAME = CL_MenuManager.WinNameType.CRMMAIN });

            //گزارشات خرید فروش
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "خلاصه فروش روزانه به تفکیک اشخاص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FROOSHDAY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فروش روزانه کاربران", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فروش به تفکیک واحد ها", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست جمع فروش کالا ها تفکیک کالا", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_CFRALL });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست خرید به تفکیک کالا ....", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_CKRALL });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "امار فروش کالا ها به تفکیک روز", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_AMFDAY });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فاکتور های فروش به اشخاص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فاکتور های خرید به اشخاص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فاکتور های فروش به صورت گروهی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_FROOSH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش فروش کالا ها به تفکیک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش خرید کالا ها به تفکیک انبار", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_ANBAR_FRKH_RK });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت وضعیت معاملات اشخاص", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_VAZ });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "صورت وضعیت معاملات تاریخ چک", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_KOL_MOIN_TAFZIL_FRKMA4 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست برگه های بدون قبض باسکول", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_HBGHB });

            //سایر گزارشات خرید فروش
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست کالا های فروش نرفته اکد", WIN_NAME = CL_MenuManager.WinNameType.FROOSH_NARAFTAH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست کالا های فروش نرفته به شخص", WIN_NAME = CL_MenuManager.WinNameType.WIN_F_MENU_KHFR_FLIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست محدود شده مشتریانی که خرید نکرده اند", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_CUSTNP });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست خرید به تفکیک کالا و قبض باسکول", WIN_NAME = CL_MenuManager.WinNameType.LIST_KHARID });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست فروش به تفکیک کالا و قبض باسکول", WIN_NAME = CL_MenuManager.WinNameType.LIST_FROOSH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش خلاصه فروش روزانه", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_F });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش خلاصه فروش روزانه به تفکیک فاکتور", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_GOZARESH_FROOSH_FR });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست مشخصات کلیه کالا ها", WIN_NAME = CL_MenuManager.WinNameType.STUF_DEF_LIST });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست فروش کالا ها به تفکیک اشخاص", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_DATE_HES });

            //گزارشات چکها
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی سررسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی سر رسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای برگشتی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHKB });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای دریافتی با مبالغ", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_DCHSS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش به حساب گذاشتن چکها", WIN_NAME = CL_MenuManager.WinNameType.CHRE_LSPH });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش چکهای دریافتی سررسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_RCHEKD });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای پرداختی سررسید شده", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_PCHSS });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست چکهای موجود در صندوق", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_CHEK_CHKM });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "کنترل سریالی چکهای پرداختی", WIN_NAME = CL_MenuManager.WinNameType.F_MENU_SERILA });

            //حسابداری
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "کنترل اسناد و دفاتر چک", WIN_NAME = CL_MenuManager.WinNameType.ADAMTARAZ });


            //تعاریف
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "تعریف مرکز هزینه", WIN_NAME = CL_MenuManager.WinNameType.TCOD_MARKAZHAZ_WIN });


            //ترازها و مراکز هزینه
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی کل", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی معین", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4M });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "لیست تراز آزمایشی چهار ستونی تفضیلی", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_FT4T });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = " گزارش تراز کل ، معین ، تفضیلی و بالاتر", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_OTHER });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز آزمایشی چهار ستونی حسابهای کل", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_RT4 });
            MenuItemModels.Add(new MenuItemModel { ISCONFIRMED = true, CAPTION = "گزارش تراز آزمایشی چهار ستونی حسابهای کل ، معین و تفضیلی", WIN_NAME = CL_MenuManager.WinNameType.FMENU_TARAZ_4_RFT4T });

            AssignRowNumbers(MenuItemModels.Where(m => m.ISCONFIRMED).ToList());

            // Loading user-specific access level
            string userCode = Baseknow.USERCOD?.ToString();
            try
            {
                var lines = File.ReadAllLines(CL_Generaly.FILEACCESSPATH);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3)
                    {
                        var fileUserCode = parts[0].Trim();
                        var shortName = parts[1].Trim();
                        var isAllowed = Convert.ToBoolean(Convert.ToByte(parts[2].Trim()));

                        // Check if the current line pertains to the user we're interested in
                        if (fileUserCode == userCode)
                        {
                            var menuItem = MenuItemModels.FirstOrDefault(m => m.ShortName?.ToLower() == shortName?.ToLower() || m.WIN_NAME.ToString().ToLower() == shortName?.ToLower());

                            if (menuItem != null)
                            {
                                menuItem.IsAllowed = isAllowed; //igonored temprary
                            }
                        }
                    }
                }
            }
            catch { }

            LISTBOX_MENU.ItemsSource = MenuItemModels.Where(m => m.ISCONFIRMED).ToList(); //LISTBOX_MENU.ItemsSource = MenuItemModels.Where(m => m.IsAllowed && m.ISCONFIRMED).ToList();
        }
        private void LetsOpenTheWin()
        {
            var menuItem = LISTBOX_MENU.SelectedItem as MenuItemModel;
            if (menuItem != null)
            {
                CL_MenuManager.OpenWinMenu(menuItem.WIN_NAME, this, menuItem.TheParam ?? default);
            }
        }
        private void LISTBOX_MENU_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LetsOpenTheWin();
        }
        private bool IsFocusWithinListBox(ListBox listBox)
        {
            var focusedElement = Keyboard.FocusedElement as DependencyObject;
            // Check if the focused element is the ListBox itself
            if (focusedElement == listBox)
            {
                return true;
            }
            // Traverse the visual tree upwards from the focused element to see if we encounter the ListBox
            while (focusedElement != null)
            {
                if (focusedElement == listBox)
                {
                    return true;
                }
                // Move up the visual tree
                focusedElement = VisualTreeHelper.GetParent(focusedElement);
            }
            return false;
        }

        private void SEARCH_TEXT_MENU_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SEARCH_TEXT_MENU.Text.Trim().ToLower();

            if (CHB_STATIC_FIND.IsChecked == true)
            {
                // Static list mode - maintain the full list
                LISTBOX_MENU.ItemsSource = MenuItemModels.Where(m => m.ISCONFIRMED).ToList();

                if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrWhiteSpace(searchText))
                {
                    // Find the first item that matches the search text
                    var matchingItem = MenuItemModels.FirstOrDefault(item =>
                        item.CAPTION.ToLower().Contains(searchText) && item.ISCONFIRMED);

                    if (matchingItem != null)
                    {
                        // Select the item but don't change focus
                        LISTBOX_MENU.SelectedItem = matchingItem;
                        LISTBOX_MENU.ScrollIntoView(matchingItem);

                        // Important: Keep focus on the text box
                        SEARCH_TEXT_MENU.Focus();

                        // Maintain the cursor position
                        int caretPosition = SEARCH_TEXT_MENU.CaretIndex;
                        SEARCH_TEXT_MENU.CaretIndex = caretPosition;
                    }
                }
            }
            else
            {
                // Dynamic list mode - original behavior
                if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrWhiteSpace(searchText))
                {
                    LISTBOX_MENU.Visibility = Visibility.Visible;
                    var filteredItems = MenuItemModels.Where(item =>
                        item.CAPTION.ToLower().Contains(searchText) && item.ISCONFIRMED).ToList();
                    LISTBOX_MENU.ItemsSource = filteredItems;
                }
                else
                {
                    LISTBOX_MENU.ItemsSource = MenuItemModels.Where(m => m.ISCONFIRMED).ToList();
                }
            }
        }

        private void CHB_STATIC_FIND_Click(object sender, RoutedEventArgs e)
        {
            //SEARCH_TEXT_MENU_TextChanged(SEARCH_TEXT_MENU, null);
        }
    }
}
