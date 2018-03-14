using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace M_money.Models
{
    class M_moneyItem
    {
        public int id { get; set; }

        public double cost { get; set; }

        public string type { get; set; }

        public string description { get; set; }

        // 账单APP上填写的时间（由用户填写，即该笔消费产生的时间）
        public DateTime time { get; set; }

        public ImageSource typeImg { get; set; }

        public M_moneyItem(int id, double cost, string type, string description, DateTime time)
        {
            this.id = id;
            this.cost = cost;
            this.type = type;
            this.description = description;
            this.time = time;
            ImageSource source = new BitmapImage(new Uri("ms-appx:///Assets/"+type+".png"));
            this.typeImg = source;
        }

    }

    class MonthItem
    {
        public int month { get; set; }

        public int year { get; set; }

        public double cost { get; set; }

        public MonthItem(int year, int month, double cost)
        {
            this.year = year;
            this.month = month;
            this.cost = cost;
        }
    }

    class YearItem
    {
        public int year { get; set; }

        public double cost { get; set; }

        public YearItem(int year, double cost)
        {
            this.year = year;
            this.cost = cost;
        }
    }
}
