using Data.Models;
using Microsoft.Extensions.Configuration;
using Services;
using System.Collections.Specialized;
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
    /*
     NCB
9704198526191432198
NGUYEN VAN A
07/15
123456
     */
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpListener _httpListener;
        private Thread _thread;
        private Service _service;
        private wPayment _paymentWindow;
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
            try
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
                LoadCboCustomer();
                string fileContent = _service.LoadTextFromFile(f);
                txtJsonXML.Text = fileContent;
            }
            catch (Exception)
            {
                MessageBox.Show("Payment is successfully");
            }
        }
        private List<KeyValuePair<int, string>> GetCustomers()
        {
            var customers = _service.GetAllCustomers();
            var cboCust = new List<KeyValuePair<int, string>>();
            customers.ForEach(c => cboCust.Add(new KeyValuePair<int, string>(c.CustomerId, $"{c.Name} - {c.Email} - {c.DateOfBirth} - {c.Address}")));
            return cboCust;
        }
        private void LoadCboCustomer()
        {
            try
            {
                cboCustomer.ItemsSource = GetCustomers();
                cboCustomer.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("Payment is successfully");
            }
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
                RefreshAllField();
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
                int customerId = (int)cboCustomer.SelectedValue;
                var customer = _service.GetCustomer(customerId);
                if (customer == null)
                {
                    throw new Exception("This customer Id is not found");
                }
                return new Order
                {
                    Customer = customer,
                    CustomerId = customerId,
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
            cboCustomer.SelectedItem = 0;
            txtDate.Text = DateTime.Now.ToString();
            txtKey.Text = string.Empty;
            txtNote.Text = string.Empty;
            txtOrderId.Text = _service.GetAllOrders().Count().ToString();
            lbCustomerInfo.Content = string.Empty;
            txtTotalAmount.Text = string.Empty;
            LoadGrdOrders();
        }

        private void cboSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshAllField();
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
                                var customers = GetCustomers();
                                txtOrderId.Text = result.OrderId.ToString();
                                cboCustomer.Text = result.CustomerId.ToString();
                                txtTotalAmount.Text = result.TotalAmount.ToString();
                                txtNote.Text = result.OrderNotes;
                                txtDate.SelectedDate = result.OrderDate;
                                cboCustomer.ItemsSource = customers;

                                cboCustomer.SelectedValue = result.CustomerId;

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
            string s = cboCustomer.Text;
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
                        var isSuccess = UpdateOrder(queryParams);
                        string responseString = $"<html><body>Payment status received: {isSuccess}</body></html>";
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

        public DateTime ConvertStringToDateTime(string dateTimeString)
        {
            if (DateTime.TryParseExact(dateTimeString, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                throw new FormatException("The provided string is not in the correct format.");
            }
        }

        private string UpdateOrder(NameValueCollection queryParams)
        {
            var responseCode = queryParams.GetValues("vnp_ResponseCode");
            if (responseCode[0].Equals("00"))
            {
                var orderInfo = queryParams.GetValues("vnp_OrderInfo")[0];
                var orderId = orderInfo.Substring(orderInfo.IndexOf(":") + 1);
                var order = _service.GetById(int.Parse(orderId));
                var amount = queryParams.GetValues("vnp_Amount")[0];
                var payDate = queryParams.GetValues("vnp_PayDate")[0];
                order.OrderNotes = orderId + " is paid to " + amount + " at " + ConvertStringToDateTime(payDate);
                _service.Update(order);
                var check = _service.SaveChange("Json");
                if (check)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (_paymentWindow != null)
                        {
                            _paymentWindow.ClosePaymentWindow();
                        }
                        LoadGrdOrders();
                    });
                    return "Payment is successfully";
                }
            }
            return "Payment failed";
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
                        _paymentWindow = new wPayment(vnpUrl);
                        _paymentWindow.Show();
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