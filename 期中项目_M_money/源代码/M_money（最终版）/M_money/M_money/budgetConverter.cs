using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace M_money
{
    public class budgetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ViewModel = new M_money.ViewModels.M_moneyItemViewModel();
            double remain = 0;
            remain = (double)value - ViewModel.MonthItems[DateTime.Now.Month-1].cost;
            return remain.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Collapsed) return false;
            return true;
        }
    }
}
