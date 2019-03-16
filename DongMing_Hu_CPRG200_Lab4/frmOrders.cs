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

            // set null date to " "
            shippedDateDateTimePicker.CustomFormat = currentOrder.ShippedDate == null ? " " : "MMM-dd-yyyy";
        }

        // User Picked a Date: 
        private void shippedDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            shippedDateDateTimePicker.CustomFormat = "MMM-dd-yyyy";

            var currentOrder = (Order)orderBindingSource.Current;
            currentOrder.ShippedDate = shippedDateDateTimePicker.Value;
        }

        // Save Button Clicked: save changes to database
        private void orderBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            // todo: validate if date after orderdate && before required date
            //OrderDB.UpdateDatabase
            var currentOrder = (Order)orderBindingSource.Current;
            var listToUpdate = new List<Order>();
            listToUpdate.Add(currentOrder);
            MessageBox.Show(OrderDB.UpdateShipDate(listToUpdate).ToString() + " orders are updated.") ;
        }

        // Before Close Form: unset autocomplete mode, otherwise got a funny error
        private void frmOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderIDComboBox.AutoCompleteMode = AutoCompleteMode.None;
        }


        // testing code -----------------------------------

    }
}
