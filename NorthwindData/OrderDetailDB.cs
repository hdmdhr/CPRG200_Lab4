using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindData
{
    public static class OrderDetailDB
    {
        public static List<OrderDetail> GetOrderDetails()
        {
            List<OrderDetail> details = new List<OrderDetail>();
            var connection = NorthwindDB.getConnection();
            string sql = "SELECT OrderID,ProductID,UnitPrice,Quantity,Discount FROM [Order Details]";

            SqlCommand selectCmd = new SqlCommand(sql, connection);

            // execute
            connection.Open();
            SqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (dr.Read())
            {
                var od = new OrderDetail();
                od.OrderID = (int) dr["OrderID"];
                od.ProductID = (int) dr["ProductID"];
                od.UnitPrice = (decimal) dr["UnitPrice"];
                od.Quantity = Convert.ToInt32(dr["Quantity"]);
                od.Discount = Convert.ToDecimal(dr["Discount"]);

                details.Add(od);
            }
            dr.Close();

            return details;
        }
    }
}
