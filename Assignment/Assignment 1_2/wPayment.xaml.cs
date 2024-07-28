using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Assignment_1_2
{
    public partial class wPayment : Window
    {
        public wPayment(string paymentUrl)
        {
            InitializeComponent();
            InitializeWebView(paymentUrl);
        }

        private async void InitializeWebView(string paymentUrl)
        {
            try
            {
                // Ensure that CoreWebView2 is initialized before navigating
                await PaymentWebView.EnsureCoreWebView2Async(null);
                PaymentWebView.CoreWebView2.Navigate(paymentUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}");
            }
        }
        public void ClosePaymentWindow()
        {
            if (Dispatcher.CheckAccess())
            {
                this.Close();
            }
            else
            {
                Dispatcher.Invoke(() => this.Close());
            }
        }
    }

}
