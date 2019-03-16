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
        private List<OrderDetail> allOrderDetails = OrderDetailDB.GetOrderDetails();

        private List<Order> listToUpdate = new List<Order>();
        private Order currentOrder = null;

        private const string shipDateFormat = "MMM-d-yyyy";

        public frmOrders()
        {
            InitializeComponent();
        }

        private void frmOrders_Load(object sender, EventArgs e)
        {
            orderBindingSource.DataSource = orders;
        }

        // Current Order Changed: collect current order's details, bind to DataGridView, calculate total
        private void orderBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            // DBNull dates from database were set to min DateTime, change display format for these datetimes, so that nothing will show in datepicker
            currentOrder = (Order)orderBindingSource.Current;
            shippedDateDateTimePicker.CustomFormat = currentOrder.ShippedDate == DateTimePicker.MinimumDateTime ? " " : shipDateFormat;

            // iterate through all order details, collect those whose OrderID match current order ID
            var currentOrderDetails = new List<OrderDetail>();
            decimal currentOrderTotal = 0;
            foreach (var od in allOrderDetails)
            {
                if (od.OrderID == currentOrder.OrderID)
                {
                    // if OrderID matches the current order, add to new list
                    currentOrderDetails.Add(od);
                    // do sum calculation
                    currentOrderTotal += od.UnitPrice * (1 - od.Discount) * od.Quantity;
                }
            }
            // give DataGridView current order detail data to display
            orderDetailsBindingSource.DataSource = currentOrderDetails;
            // output current order's total
            txtOrderTotal.Text = currentOrderTotal.ToString("c");
        }

        // Picker Value Changed: doesn't necessarily mean it's user's pick, can be programmatic changes
        private void shippedDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
        }

        // Datepicker Dropped Down and Closed Up: make sure user picked a different and valid date, 
        private void shippedDateDateTimePicker_CloseUp(object sender, EventArgs e)
        {
            currentOrder = (Order)orderBindingSource.Current;

            // check if picked date is different from before (crazy user may just open and close datepicker for fun)
            if (shippedDateDateTimePicker.Value != currentOrder.ShippedDate)
            {
                // if picked date is not between order date and required date
                if (shippedDateDateTimePicker.Value < currentOrder.OrderDate ||
                    shippedDateDateTimePicker.Value > currentOrder.RequiredDate)
                {
                    MessageBox.Show("Ship date must be between order date and required date, please choose a valid date.", "Invalid Date");
                    // invalid, rollback to old value (if old value derive from DBNull, set to order date)
                    shippedDateDateTimePicker.Value = currentOrder.ShippedDate == DateTimePicker.MinimumDateTime ? currentOrder.OrderDate : currentOrder.ShippedDate;
                }
                else
                {
                    // valid new date, unset the " " custom format, so date can show in picker
                    shippedDateDateTimePicker.CustomFormat = shipDateFormat;  
                    // set current order's date to picked value
                    currentOrder.ShippedDate = shippedDateDateTimePicker.Value;

                    // if current order is not already in update list, add to list
                    if (!listToUpdate.Contains(currentOrder))
                    {
                        listToUpdate.Add(currentOrder);
                        orderBindingNavigatorSaveItem.Enabled = true;
                        // TEST
                        //Console.WriteLine(listToUpdate.Count + " items in the update list. Last item's OrderID is: " + listToUpdate.Last<Order>().OrderID + " (" + listToUpdate.Last<Order>().ShippedDate.ToShortDateString() + ")");
                    }
                }
            }

        }


        // Save Button Clicked: save changes to database
        private void orderBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            // send update list to OrderDB, get back result in List<int> format (collection of OrderIDs)
            var failedOrderIDs = new List<int>();
            try
            {
                failedOrderIDs = OrderDB.UpdateOrders(listToUpdate);

                MessageBox.Show(listToUpdate.Count - failedOrderIDs.Count + " orders were successfully updated. " + (failedOrderIDs.Count == 0 ? "" : failedOrderIDs.Count+" order(s) failed."));

                // if all updated successfully, clear update list, disable save
                if (failedOrderIDs.Count == 0)
                {
                    listToUpdate.Clear();
                    orderBindingNavigatorSaveItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                // create error message shows which orders failed update
                string failMsg = "Order ID: ";
                foreach (var fail in failedOrderIDs)
                    failMsg += fail + ", ";
                failMsg += "did not update, please try again later.";

                MessageBox.Show("Updates failed due to " + ex.Message + (failedOrderIDs.Count == 0 ? "" : failMsg), 
                    ex.GetType().ToString());
            }

        }

        // Before Close Form: unset autocomplete mode, otherwise got a funny error
        private void frmOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderIDComboBox.AutoCompleteMode = AutoCompleteMode.None;
        }



        // testing code -----------------------------------

    }
}
