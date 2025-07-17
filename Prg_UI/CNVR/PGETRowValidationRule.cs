using Prg_Proccessy.SQLMODELS;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Prg_UI.CNVR
{
    public class PGETRowValidationRule : ValidationRule
    {
        /*
         *        <DataGrid.RowValidationRules>
                    <VERT:PGETRowValidationRule ValidationStep="UpdatedValue"/>
                </DataGrid.RowValidationRules>
         */
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // value در واقع یک BindingGroup است که شامل آیتم مرتبط با سطر است
            var rowBindingGroup = value as BindingGroup;
            if (rowBindingGroup?.Items == null || rowBindingGroup.Items.Count == 0)
                return ValidationResult.ValidResult;

            // گرفتن شیء PGET_LST مربوط به این سطر
            var rowItem = rowBindingGroup.Items[0] as PGET_LST;
            if (rowItem == null)
                return ValidationResult.ValidResult;

            // اینجا چک می‌کنیم اگر مبلغ null بود خطا بده
            if (rowItem.MABL == null || rowItem.MABL < 1)
            {
                return new ValidationResult(false, "مبلغ نباید خالی باشد.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
