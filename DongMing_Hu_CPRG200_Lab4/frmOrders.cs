using NorthwindData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DongMing_Hu_CPRG200_Lab4
{
    public partial class frmOrders : Form
    {
        private List<Order> orders = OrderDB.GetOrders();
        private List<OrderDetail> orderDetails = OrderDetailDB.GetOrderDetails();

        public frmOrders()
        {
            InitializeComponent();
        }

        private void frmOrders_Load(object sender, EventArgs e)
        {

            orderBindingSource.DataSource = orders;
        }

        // Current Order Changed: filter corresponding order details, bind to DataGridView, calculate total for current order
        private void orderBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            var currentOrder = (Order)orderBindingSource.Current;
            var currentDetails = new List<OrderDetail>();
            decimal currentOrderTotal = 0;
            foreach (var od in orderDetails)
            {
                if (od.OrderID == currentOrder.OrderID)
                {
                    // if OrderID matches the current order, add to new list
                    currentDetails.Add(od);
                    // do calculation
                    currentOrderTotal += od.UnitPrice * (1 - od.Discount) * od.Quantity;
                }
            }

            orderDetailsBindingSource.DataSource = currentDetails;
            txtOrderTotal.Text = currentOrderTotal.ToString("c");
        }

        // Save Button Clicked: save changes to database
        private void orderBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            //OrderDB.UpdateDatabase
        }

        // testing code -----------------------------------
        private void shippedDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            Console.WriteLine(shippedDateDateTimePicker.Value);
        }

        private void frmOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderIDComboBox.AutoCompleteMode = AutoCompleteMode.None;
            //this.Close();
        }
    }
}
