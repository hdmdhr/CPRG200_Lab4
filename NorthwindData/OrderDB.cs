using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NorthwindData
{
    /*
     * Author: DongMing Hu
     * Date Created: 3/15/2019
     * Purpose: Data access class, contains two methods. One is to get all orders from database, the other one is to update several modified records at one time.
     * 
     */

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
            try
            {
                connection.Open();
                SqlDataReader dr = selectCmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    var o = new Order();
                    o.OrderID = (int)dr["OrderID"];
                    o.CustomerID = dr["CustomerID"] == DBNull.Value ? "" : dr["CustomerID"].ToString();
                    // if a datetime from database is DBNull, convert it to min DateTime can be picked in datetimepicker
                    o.OrderDate = dr["OrderDate"] == DBNull.Value ? DateTimePicker.MinimumDateTime : (DateTime)dr["OrderDate"];
                    o.RequiredDate = dr["RequiredDate"] == DBNull.Value ? DateTimePicker.MinimumDateTime : (DateTime)dr["RequiredDate"];
                    o.ShippedDate = dr["ShippedDate"] == DBNull.Value ? DateTimePicker.MinimumDateTime : (DateTime)dr["ShippedDate"];

                    orders.Add(o);
                }
                dr.Close();

                return orders;
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update multiple records' ShippedDate columns.
        /// </summary>
        /// <param name="ordersToUpdate">A list of orders to update</param>
        /// <returns>A list of int that indicates orderID failed updating.</returns>
        public static List<int> UpdateOrders(List<Order> ordersToUpdate)
        {
            List<int> idsOfFailedUpdates = new List<int>();
            string updateSQL = "UPDATE Orders " +
                               "SET ShippedDate=@NewShipDate " +
                               "WHERE OrderID=@OldOrderID " +  // identity record
                               "AND CustomerID=@OldCustomerID " +  // rest: optimistic concurrency
                               "AND OrderDate=@OldOrderDate " +
                               "AND RequiredDate=@OldRequiredDate";
            // todo: to optimize concurrency, it's ideal to also check if other columns have been modified by other users
            foreach (var order in ordersToUpdate)
            {
                SqlCommand updateCmd = new SqlCommand(updateSQL, connection);
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
                    var rowsAffected = updateCmd.ExecuteNonQuery();
                    // if update failed, collect failed OrderID to return to user
                    if (rowsAffected == 0)
                        idsOfFailedUpdates.Add(order.OrderID);
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }

            return idsOfFailedUpdates;
        }
    }
}
