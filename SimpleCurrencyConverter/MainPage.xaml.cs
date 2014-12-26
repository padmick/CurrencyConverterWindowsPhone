using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace SimpleCurrencyConverter
{
    public class LocationUnit
    {
        public string Location { get; set; }

        public string URL { get; set; }
    }

    public static class EnumHelper
    {
        public static string[] GetNames(Type enumType)
        {
            FieldInfo[] fieldInfo = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            return fieldInfo.Select(f => f.Name).ToArray();
        }

        public static string[] GetNames(Enum e)
        {
            List<string> enumNames = new List<string>();

            foreach (FieldInfo fi in e.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumNames.Add(fi.Name);
            }

            return enumNames.ToArray<string>();
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        public static readonly DependencyProperty _locations = DependencyProperty.Register("Locations", typeof(ObservableCollection<LocationUnit>), typeof(PhoneApplicationPage), new PropertyMetadata(null));

        public ObservableCollection<LocationUnit> Locations
        {
            get { return (ObservableCollection<LocationUnit>)GetValue(_locations); }
            set { SetValue(_locations, value); }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Locations = new ObservableCollection<LocationUnit>();
            GetData();
        }

        public void GetData()
        {
            foreach (string s in EnumHelper.GetNames(typeof(ConverterService.Currency)))
            {
                LocationUnit unit = new LocationUnit()
                {
                    Location = s,
                    URL = s
                };
                Locations.Add(unit);

            }
        }

        public void txtConvertedFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool bNeedToUpdate = false;
            StringBuilder szNumbersOnly = new StringBuilder();
            TextBox textSource = sender as TextBox;

            if (null == textSource)
                return;

            foreach (char ch in textSource.Text)
            {
                if (("0123456789").Contains(ch.ToString()))
                {
                    szNumbersOnly.Append(ch);
                }
                else
                {
                    bNeedToUpdate = true;
                }
            }
            if (bNeedToUpdate)
            {
                textSource.Text = szNumbersOnly.ToString();
                textSource.SelectionStart = szNumbersOnly.Length;
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            ConverterService.CurrencyConvertorSoapClient aobClient = new ConverterService.CurrencyConvertorSoapClient();
            aobClient.ConversionRateCompleted += new EventHandler<ConverterService.ConversionRateCompletedEventArgs>(aobClient_ConversionRateCompleted);

            LocationUnit selectedLocationFrom = (from c in Locations where c.Location == ((LocationUnit)cmbCurrFrom.SelectedItem).Location select c).First();
            LocationUnit selectedLocationTo = (from c in Locations where c.Location == ((LocationUnit)cmbCurrTo.SelectedItem).Location select c).First();

            ConverterService.Currency currFrom = (ConverterService.Currency)Enum.Parse(typeof(ConverterService.Currency), selectedLocationFrom.Location, true);
            ConverterService.Currency currTo = (ConverterService.Currency)Enum.Parse(typeof(ConverterService.Currency), selectedLocationTo.Location, true);

            aobClient.ConversionRateAsync(currFrom, currTo);
        }

        private void aobClient_ConversionRateCompleted(object sender, ConverterService.ConversionRateCompletedEventArgs e)
        {
            double adblResult = Convert.ToDouble(txtConvertedFrom.Text) * e.Result;
            txtConvertedTo.Text = adblResult.ToString();
        }
    }
}