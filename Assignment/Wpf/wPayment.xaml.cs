using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Wpf
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
    }
}
