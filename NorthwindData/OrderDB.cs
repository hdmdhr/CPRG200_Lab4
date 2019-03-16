using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindData
{
    public static class OrderDB
    {
        static SqlConnection connection = NorthwindDB.getConnection();

        /// <summary>
        /// Retrive all orders from Northwind Orders table
        /// </summary>
        /// <returns>A list of all orders</returns>
        public static List<Order> GetOrders()
        {
            List<Order> orders = new List<Order>();

            var sqlText = "SELECT OrderID,CustomerID,OrderDate,RequiredDate,ShippedDate FROM Orders";
            SqlCommand selectCmd = new SqlCommand(sqlText, connection);
            // execute
            connection.Open();
            SqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (dr.Read())
            {
                var o = new Order();
                o.OrderID = (int)dr["OrderID"];
                o.CustomerID = dr["CustomerID"] == DBNull.Value ? "" : dr["CustomerID"].ToString();
                o.OrderDate = dr["OrderDate"]==DBNull.Value ? null : (DateTime?)dr["OrderDate"];
                o.RequiredDate = dr["RequiredDate"] == DBNull.Value ? null : (DateTime?)dr["RequiredDate"];
                o.ShippedDate = dr["ShippedDate"] == DBNull.Value ? null : (DateTime?)dr["ShippedDate"];

                orders.Add(o);
            }
            dr.Close();


            return orders;
        }

        /// <summary>
        /// Update multiple records' ShippedDate column
        /// </summary>
        /// <param name="ordersToUpdate"></param>
        /// <returns>Row affected</returns>
        public static int UpdateShipDate(List<Order> ordersToUpdate)
        {
            int rowsAffected = 0;
            string updateSQL = "UPDATE Orders " +
                               "SET ShippedDate=@NewShipDate " +
                               "WHERE OrderID=@OldOrderID " +  // identity record
                               "AND CustomerID=@OldCustomerID " +  // rest: optimistic concurrency
                               "AND OrderDate=@OldOrderDate " +
                               "AND RequiredDate=@OldRequiredDate";
            SqlCommand updateCmd = new SqlCommand(updateSQL, connection);
            foreach (var order in ordersToUpdate)
            {
                // bind parameters
                updateCmd.Parameters.AddWithValue("@NewShipDate", order.ShippedDate);
                updateCmd.Parameters.AddWithValue("@OldOrderID", order.OrderID);
                updateCmd.Parameters.AddWithValue("@OldCustomerID", order.CustomerID);
                updateCmd.Parameters.AddWithValue("@OldOrderDate", order.OrderDate);
                updateCmd.Parameters.AddWithValue("@OldRequiredDate", order.RequiredDate);
                // execute
                try
                {
                    connection.Open();
                    rowsAffected += updateCmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    connection.Close();
                }

            }


            return rowsAffected;
        }
    }
}
