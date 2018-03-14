using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace M_money
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class YearPage : Page
    {
        public YearPage()
        {
            this.InitializeComponent();
        }
        private ViewModels.M_moneyItemViewModel ViewModel;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel = ((ViewModels.M_moneyItemViewModel)e.Parameter);
            if (ViewModel.SelectedYear == null)
            {
                //createButton.Content = "Create";
                //var i = new MessageDialog("Welcome!").ShowAsync();
            }
            else
            {
                //
                year.Text = "Bill of " + ViewModel.SelectedYear.year.ToString();
                // todo:选出选中年的各类型的消费记录的金额。
                double Func = 0, Foodc = 0, Otherc = 0, Studyc = 0, Transportc = 0, Clothesc = 0, sum = 1;
                //  筛选Fun类
                var target = Convert.ToString(ViewModel.SelectedYear.year) + "/%";

                double Funtotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Fun");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Funtotal += (double)monthsearch[1];
                        }
                    }
                }
                Func = Funtotal;

                // 筛选Food类
                double Foodtotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Food");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Foodtotal += (double)monthsearch[1];
                        }
                    }
                }
                Foodc = Foodtotal;

                // 筛选Other类
                double Othertotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Other");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Othertotal += (double)monthsearch[1];
                        }
                    }
                }
                Otherc = Othertotal;

                // 筛选Study类
                double Studytotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Study");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Studytotal += (double)monthsearch[1];
                        }
                    }
                }
                Studyc = Studytotal;

                // 筛选Transport类
                double Transporttotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Transport");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Transporttotal += (double)monthsearch[1];
                        }
                    }
                }
                Transportc = Transporttotal;

                // 筛选Clothes类
                double Clothestotal = 0;
                using (var monthsearch = App.conn.Prepare("SELECT* FROM Message WHERE Time LIKE ? AND Type = ?"))
                {
                    monthsearch.Bind(1, target);
                    monthsearch.Bind(2, "Clothes");
                    if (monthsearch.ColumnCount != 0)
                    {
                        while (monthsearch.Step() == SQLitePCL.SQLiteResult.ROW)
                        {
                            Clothestotal += (double)monthsearch[1];
                        }
                    }
                }   
                Clothesc = Clothestotal;

                sum = Func + Foodc + Otherc + Studyc + Transportc + Clothesc;
                food.Height = Foodc / sum * 200;
                fun.Height = Func / sum * 200;
                other.Height = Otherc / sum * 200;
                transport.Height = Transportc / sum * 200;
                clothes.Height = Clothesc / sum * 200;
                study.Height = Studyc / sum * 200;
                //设置百分比的值
                foodtext.Text = String.Format("{0:F}", Foodc / sum * 100) + "%";
                funtext.Text = String.Format("{0:F}", Func / sum * 100) + "%";
                othertext.Text = String.Format("{0:F}", Otherc / sum * 100) + "%";
                transporttext.Text = String.Format("{0:F}", Transportc / sum * 100) + "%";
                clothestext.Text = String.Format("{0:F}", Clothesc / sum * 100) + "%";
                studytext.Text = String.Format("{0:F}", Studyc / sum * 100) + "%";
                //设置各消费金额
                foodcost.Text = "￥" + Foodc.ToString();
                funcost.Text = "￥" + Func.ToString();
                othercost.Text = "￥" + Otherc.ToString();
                transportcost.Text = "￥" + Transportc.ToString();
                clothescost.Text = "￥" + Clothesc.ToString();
                studycost.Text = "￥" + Studyc.ToString();
                total.Text = sum.ToString();
            }
        }
        private void Changedollar_click(object sender, RoutedEventArgs e)
        {
            dollarcost.Text = "";
            queryAsync_dollar(total.Text);
        }

        async void queryAsync_dollar(string rmb)
        {
            string url = " http://api.k780.com:88/?app=finance.rate&scur=USD&tcur=CNY&appkey=10003&sign=b59bc3ef6191eb9f747dd4e83c99f2a4&format=xml";
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(url);
            //解析xml
            XmlDocument document = new XmlDocument();
            document.LoadXml(result);
            XmlNodeList list = document.GetElementsByTagName("rate");
            IXmlNode node = list.Item(0);
            double rate = Convert.ToDouble(node.InnerText);
            double dollar = Convert.ToDouble(rmb) / rate;
            dollarcost.Text = "$" + String.Format("{0:F}", dollar);
        }
    }
}
