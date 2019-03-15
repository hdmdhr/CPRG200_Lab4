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

        public static List<Order> GetOrders()
        {
            List<Order> orders = new List<Order>();

            var connection = NorthwindDB.getConnection();
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
                
                if (dr["OrderDate"] == DBNull.Value)
                    o.OrderDate = null;
                else
                    o.OrderDate = (DateTime)dr["OrderDate"];

                if (dr["RequiredDate"] == DBNull.Value)
                    o.RequiredDate = null;
                else
                    o.RequiredDate = (DateTime)dr["RequiredDate"];


                if (dr["ShippedDate"] == DBNull.Value)
                    o.ShippedDate = null;
                else
                    o.ShippedDate = (DateTime)dr["ShippedDate"];

                orders.Add(o);
            }
            dr.Close();


            return orders;
        }
    }
}
