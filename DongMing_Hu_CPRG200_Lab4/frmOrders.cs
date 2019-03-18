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
    /*
     * Author: DongMing Hu
     * Date Created: 3/15/2019
     * Purpose: Form class. Read inputs and do outputs to the form.
     * 
     */

    public partial class frmOrders : Form
    {
        private List<Order> allOrders = OrderDB.GetOrders();
        private List<OrderDetail> allOrderDetails = OrderDetailDB.GetOrderDetails();

        private List<Order> listToUpdate = new List<Order>();
        private Order currentOrder = null;
        private const string shipDateFormat = "yyyy-MMM-d";

        public frmOrders()
        {
            InitializeComponent();
        }

        // Form Loaded: give orderBindingSource all the orders to display
        private void frmOrders_Load(object sender, EventArgs e)
        {
            orderBindingSource.DataSource = allOrders;
        }

        // Current Order Changed: display order details and total of current order, do some graphic adjustment corresponding to the current order
        private void orderBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            currentOrder = (Order)orderBindingSource.Current;
            DisplayCurrentOrderDetailsAndTotal();

            // DBNull dates from database were set to MinDateTime when retriving data 
            // change custom display format for these DBNull datetimes to " " 
            // so that nothing will show in datepicker (indicates ship date is not decided yet)
            shippedDateDateTimePicker.CustomFormat = currentOrder.ShippedDate == DateTimePicker.MinimumDateTime ? " " : shipDateFormat;

            // if current order is in update list, give some visual sign to indicate that shipping date has been chaged but not saved
            if (listToUpdate.Contains(currentOrder))
            {
                grpDateChangeVisualIndicator.Visible = true;
                lblDateChangeTextIndicator.Visible = true;
                lblDateChangeTextIndicator.Text = listToUpdate.Count + " change" + (listToUpdate.Count > 1 ? "s were " : " was ") + "made but not saved.";
            }
            else
            {   // if not in update list, hide indicators
                grpDateChangeVisualIndicator.Visible = false;
                lblDateChangeTextIndicator.Visible = false;
            }
        }

        // Datepicker Dropped Down and Closed Up (means user picked a new date): make sure user picked a different and valid date, add it to update list
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
                foreach (var failedID in failedOrderIDs)
                    failMsg += failedID + ", ";
                failMsg += "did not update due to concurrency issue, please try again later.";

                // use messagebox to give user feedback
                int howManySucceed = listToUpdate.Count - failedOrderIDs.Count;
                MessageBox.Show((howManySucceed == 0 ? "" : howManySucceed.ToString() + " order(s) updated.\n") +
                                (failedOrderIDs.Count == 0 ? "" : failedOrderIDs.Count + " order(s) failed to update. " + failMsg));

                // if there are failed updates, add them to listToUpdate again, fetch newest data from database, so user can try to update again
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
                    // fetch newest data, bind to orders, so that data on form is up to date
                    allOrders = OrderDB.GetOrders();
                    orderBindingSource.DataSource = allOrders;
                    // if current order is not in update list, hide date change indicators
                    if (!listToUpdate.Contains(currentOrder))
                    {
                        grpDateChangeVisualIndicator.Visible = false;
                        lblDateChangeTextIndicator.Visible = false;
                    }
                }
                else  // if all updated successfully, clear update list, disable save
                {
                    listToUpdate.Clear();
                    // save button is only enabled when there are unsaved changes in listToUpdate
                    orderBindingNavigatorSaveItem.Enabled = false;
                    grpDateChangeVisualIndicator.Visible = false;
                    lblDateChangeTextIndicator.Visible = false;
                }
                
            }
            catch (Exception ex)
            {   // if exception is thrown, show error message
                MessageBox.Show("Updates failed due to " + ex.Message, ex.GetType().ToString());
            }
        }

        // Change Shipping Date In The Form
        private void ChangeShippingDate()
        {
            // 1.   make sure user picked date is different & valid
            // 2.1  if not valid, show warning message, restore old shipping date
            // 2.11 if old shipping date in database was DBNull, set shipping date to order date
            // 2.12 if old shipping date not DBNull, use old shipping date
            // 2.2  if picked date is valid, save it to current order object
            // 3    add current order to update list (if not already in list)

            currentOrder = (Order)orderBindingSource.Current;

            // check if picked date is different from before (crazy user may just open and close datepicker for fun)
            if (shippedDateDateTimePicker.Value != currentOrder.ShippedDate)
            {
                // check if picked date is early than order date or later than required date
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

                    // if current order is not already in update list, add to list, enable saving
                    if (!listToUpdate.Contains(currentOrder))
                    {
                        listToUpdate.Add(currentOrder);
                        orderBindingNavigatorSaveItem.Enabled = true;

                        grpDateChangeVisualIndicator.Visible = true;
                        lblDateChangeTextIndicator.Visible = true;
                        lblDateChangeTextIndicator.Text = listToUpdate.Count + " change" + (listToUpdate.Count > 1 ? "s were " : " was ") + "made but not saved.";
                        // TEST
                        //Console.WriteLine(listToUpdate.Count + " items in the update list. The last item's OrderID is: " + listToUpdate.Last<Order>().OrderID + " (" + listToUpdate.Last<Order>().ShippedDate.ToShortDateString() + ")");
                    }
                }
            }
        }

        // Display order details under current order, also calculate and display current order's total
        private void DisplayCurrentOrderDetailsAndTotal()
        {
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
    }
}
