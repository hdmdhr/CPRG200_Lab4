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

        private void orderBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            Console.WriteLine(orderIDComboBox.Text);

            // todo: filter right order details, show in orderDetailsDataGridView
            var currentDetails = from od in orderDetails
                                 where od.OrderID == Convert.ToInt32(orderIDComboBox.SelectedItem.ToString())
                                 orderby od.UnitPrice
                                 select od;

            //var currentDetails = new List<OrderDetail>();
            //foreach (var od in orderDetails)
            //{
            //    if (od.OrderID == Convert.ToInt32(orderIDComboBox.Text))
            //        currentDetails.Add(od);
            //}

            orderDetailsBindingSource.DataSource = currentDetails;
        }
    }
}
