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

        private const string shipDateFormat = "yyyy-MMM-d";

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
            // TODO: if current order is in update list, give some visual sign to indicate the shipping date has been chaged

        }

        // Picker Value Changed: doesn't necessarily mean it's user's pick, can be programmatic changes
        private void shippedDateDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
        }

        // Datepicker Dropped Down and Closed Up: make sure user picked a different and valid date, add it to update list
        private void shippedDateDateTimePicker_CloseUp(object sender, EventArgs e)
        {
            ChangeShippingDate();
        }

        


        // Save Button Clicked: save changes to database, give user feedback
        private void orderBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            TryUpdate();
        }


        // Before Close Form: unset autocomplete mode (which I set in property panel), otherwise get an error
        private void frmOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            orderIDComboBox.AutoCompleteMode = AutoCompleteMode.None;
        }




        // ----------------- Methods ------------------

        // Do Updates Against Database
        private void TryUpdate()
        {
            // 1.   pass listToUpdate to OrderDB update method
            // 2.1  if thrown, show exception message
            // 2.2  if no exception, return a List<int> (collection of update failed OrderIDs)
            // 3.   show user how many updates succeed, how many failed, and OrderID of failed ones
            // 4.1  if all updates succeed, clear listToUpdate, disable save btn (only enabled when there are unupdated changes)
            // 4.2  if there are failed updates, add failed orders to listToUpdate again, reload orders table
            var failedOrderIDs = new List<int>();
            try
            {
                // try to update changes
                failedOrderIDs = OrderDB.UpdateOrders(listToUpdate);

                // create error message shows which orders failed update
                string failMsg = "Order ID: ";
                foreach (var fail in failedOrderIDs)
                    failMsg += fail + ", ";
                failMsg += "did not update due to concurrency issue, please try again later.";

                // use messagebox to give user feedback
                int howManySucceed = listToUpdate.Count - failedOrderIDs.Count;
                MessageBox.Show((howManySucceed == 0 ? "" : howManySucceed.ToString() + " orders were updated.\n") +
                                (failedOrderIDs.Count == 0 ? "" : failedOrderIDs.Count + " order(s) failed to update. " + failMsg));

                // if there are failed updates, add them to listToUpdate again, fetch newest data from database
                if (failedOrderIDs.Count > 0)
                {
                    var updateFailedOrders = new List<Order>();
                    foreach (var failedOrderID in failedOrderIDs)
                    {
                        foreach (var order in listToUpdate)
                        {
                            if (order.OrderID == failedOrderID)
                                updateFailedOrders.Add(order);
                        }
                    }
                    listToUpdate = updateFailedOrders;
                    // fetch newest data, bind to orders
                    orders = OrderDB.GetOrders();
                    orderBindingSource.DataSource = orders;
                }
                else  // if all updated successfully, clear update list, disable save
                {
                    listToUpdate.Clear();
                    orderBindingNavigatorSaveItem.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Updates failed due to " + ex.Message, ex.GetType().ToString());
            }
        }

        // Change Shipping Date In The Form
        private void ChangeShippingDate()
        {
            // 1.   make sure user picked date is different & valid
            // 2.1  if not valid, show warning message, restore old shipping date
            // 2.11 if old shipping date in database was DBNull, set shipping date equal to order date
            // 2.12 if old shipping date not DBNull, use old shipping date
            // 2.2  if picked date is valid, save it to current displaying order object
            // 3    add current order to update list (if not in list yet)

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
                        Console.WriteLine(listToUpdate.Count + " items in the update list. Last item's OrderID is: " + listToUpdate.Last<Order>().OrderID + " (" + listToUpdate.Last<Order>().ShippedDate.ToShortDateString() + ")");
                    }
                }
            }
        }
    }
}
