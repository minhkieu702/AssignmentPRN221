using Data.Models;
using Services;
using System.Text;
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
        private Service _service;

        public MainWindow()
        {
            _service ??= new();
            InitializeComponent();
            LoadGrdOrders();
        }

        private void LoadGrdOrders()
        {
            var selectedComboBoxItem = cboSource.SelectedItem as ComboBoxItem;
            var f = selectedComboBoxItem?.Content.ToString();

            if (string.IsNullOrEmpty(f))
            {
                cboSource.SelectedIndex = 0;
                f = "Json";
            }
            txtType.SelectedIndex = 0;
            _service.ChangeSource(f);
            grdOrder.ItemsSource = _service.GetAll();
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
                else MessageBox.Show("Failed");
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
                    throw new Exception("Not Found");
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
            Close();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtCustomerId.Text = string.Empty;
            txtDate.Text = DateTime.Now.ToString();
            txtKey.Text = string.Empty;
            txtNote.Text = string.Empty;
            txtOrderId.Text = _service.GetAll().Count().ToString();
            lbCustomerInfo.Content = string.Empty;
            txtTotalAmount.Text = string.Empty;
        }

        private void cboSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            var result = _service.GetCustomer(int.Parse(s));
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
                            else MessageBox.Show("Failed");
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
    }
}