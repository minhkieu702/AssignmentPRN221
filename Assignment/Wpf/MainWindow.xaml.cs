using Data.Models;
using Microsoft.Extensions.Configuration;
using Services;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpListener _httpListener;
        private Thread _thread;
        private Service _service;

        public MainWindow()
        {
            _service ??= new();
            InitializeComponent();
            StartHttpListener();
            LoadGrdOrders();
        }

        #region CRUD
        private void LoadGrdOrders()
        {
            var selectedComboBoxItem = cboSource.SelectedItem as ComboBoxItem;
            var f = selectedComboBoxItem?.Content.ToString();

            if (string.IsNullOrEmpty(f))
            {
                cboSource.SelectedIndex = 0;
                f = "Json";
            }
            txtDate.Text = DateTime.Now.ToString();
            txtType.SelectedIndex = 0;
            _service.ChangeSource(f);
            grdOrder.ItemsSource = _service.GetAllOrders();

            string fileContent = _service.LoadTextFromFile(f);
            txtJsonXML.Text = fileContent;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orderId = int.Parse(txtOrderId.Text);
                if (orderId == 0)
                {
                    _service.Insert(GetOrder());
                }
                else { _service.Update(GetOrder()); }
                var check = _service.SaveChange(cboSource.Text);
                if (check)
                {
                    MessageBox.Show("Successfully");
                }
                else MessageBox.Show("Nothing changes!");
                LoadGrdOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private Order GetOrder()
        {
            try
            {
                var result = _service.GetCustomer(int.Parse(txtCustomerId.Text));
                if (result == null)
                {
                    throw new Exception("This customer Id is not found");
                }
                return new Order
                {
                    Customer = result,
                    CustomerId = int.Parse(txtCustomerId.Text),
                    OrderDate = DateTime.Parse(txtDate.Text),
                    OrderId = int.Parse(txtOrderId.Text),
                    OrderNotes = txtNote.Text,
                    TotalAmount = double.Parse(txtTotalAmount.Text),
                    Type = txtType.Text
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            // Wait for the listener thread to finish
            if (_thread != null)
            {
                _thread.Join();
            }

            if (_httpListener != null)
            {
                _httpListener.Stop();
            }
            Close();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshAllField();
        }

        private void RefreshAllField()
        {
            txtCustomerId.Text = string.Empty;
            txtDate.Text = DateTime.Now.ToString();
            txtKey.Text = string.Empty;
            txtNote.Text = string.Empty;
            txtOrderId.Text = _service.GetAllOrders().Count().ToString();
            lbCustomerInfo.Content = string.Empty;
            txtTotalAmount.Text = string.Empty;
        }

        private void cboSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAllField();
            LoadGrdOrders();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = txtKey.Text;
            if (string.IsNullOrEmpty(s)) s = "";
            var list = _service.Search(s);
            grdOrder.ItemsSource = list;
        }

        private void grdOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //MessageBox.Show("Double Click on Grid");
                DataGrid grd = sender as DataGrid;
                if (grd != null && grd.SelectedItems != null && grd.SelectedItems.Count == 1)
                {
                    var row = grd.ItemContainerGenerator.ContainerFromItem(grd.SelectedItem) as DataGridRow;
                    if (row != null)
                    {
                        var item = row.Item as Order;
                        if (item != null)
                        {
                            var result = _service.GetById(item.OrderId);

                            if (result != null)
                            {
                                txtOrderId.Text = result.OrderId.ToString();
                                txtCustomerId.Text = result.CustomerId.ToString();
                                txtTotalAmount.Text = result.TotalAmount.ToString();
                                txtNote.Text = result.OrderNotes;
                                txtDate.SelectedDate = result.OrderDate;

                                // Select the appropriate item in the ComboBox
                                foreach (ComboBoxItem comboBoxItem in txtType.Items)
                                {
                                    if (comboBoxItem.Content.ToString() == result.Type)
                                    {
                                        txtType.SelectedItem = comboBoxItem;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void txtCustomerId_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = txtCustomerId.Text;
            if (string.IsNullOrEmpty(s))
            {
                return;
            }
            if (!int.TryParse(s, out int customerid))
            {
                s = "0";
            }
            var result = _service.GetCustomer(customerid);
            if (result == null)
            {
                lbCustomerInfo.Content = "Not Found";
                return;
            }
            lbCustomerInfo.Content = result.Email + " " + result.Name + " " + result.Phone;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            {
                try
                {
                    Button btn = (Button)sender;

                    string orderCode = btn.CommandParameter.ToString();

                    //MessageBox.Show(orderCode);

                    if (!string.IsNullOrEmpty(orderCode))
                    {
                        if (MessageBox.Show("Do you want to delete this item?", "Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            _service.Delete(int.Parse(orderCode));
                            var check = _service.SaveChange(cboSource.Text);
                            if (check)
                            {
                                MessageBox.Show("Successfully");
                            }
                            else MessageBox.Show("Nothing changes!");
                            this.LoadGrdOrders();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            RefreshAllField();
            var check = _service.SaveFile(cboSource.Text, txtJsonXML.Text);
            if (check)
            {
                MessageBox.Show("Successfully");
            }
            else MessageBox.Show("Nothing changes!");
            LoadGrdOrders();
        }
        #endregion

        #region Payment
        private void StartHttpListener()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8080/vnpay_return/");
            _httpListener.Start();
            _thread = new Thread(new ThreadStart(HandleHttpRequests));
            _thread.Start();
        }

        private void HandleHttpRequests()
        {
            try
            {
                while (_httpListener.IsListening)
                {
                    var context = _httpListener.GetContext();
                    var response = context.Response;
                    var request = context.Request;

                    if (request.HttpMethod == "GET")
                    {
                        var queryParams = request.QueryString;
                        string responseString = "<html><body>Payment status received</body></html>";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        var responseOutput = response.OutputStream;
                        responseOutput.Write(buffer, 0, buffer.Length);
                        response.Close();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string CreatePaymentUrl(Order model)
        {
            try
            {
                var tick = DateTime.Now.Ticks.ToString();
                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "Pay");
                vnpay.AddRequestData("vnp_TmnCode", "NJJ0R8FS");
                vnpay.AddRequestData("vnp_Amount", (model.TotalAmount * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Pay Order:" + model.OrderId);
                vnpay.AddRequestData("vnp_OrderType", "other"); // default value: other
                vnpay.AddRequestData("vnp_ReturnUrl", "http://localhost:8080/vnpay_return/");
                vnpay.AddRequestData("vnp_TxnRef", tick);

                var paymentUrl = vnpay.CreateRequestUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", "BYKJBHPPZKQMKBIBGGXIYKWYFAYSJXCW");
                return paymentUrl;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private void ButtonPay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string orderCode = btn.CommandParameter.ToString();

                if (int.TryParse(orderCode, out int orderId))
                {
                    var order = _service.GetById(orderId);
                    if (order != null)
                    {
                        string vnpUrl = CreatePaymentUrl(order);
                        var paymentWindow = new wPayment(vnpUrl);
                        paymentWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Stop the HttpListener
            if (_httpListener != null)
            {
                _httpListener.Stop();
            }

            // Wait for the listener thread to finish
            if (_thread != null)
            {
                _thread.Join();
            }

            base.OnClosing(e);
        }
        #endregion
    }
}