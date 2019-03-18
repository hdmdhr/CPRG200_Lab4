using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindData
{
    /*
     * Author: DongMing Hu
     * Date Created: 3/15/2019
     * Purpose: Data access class, use connection string to build connection and return it.
     * 
     */

    public static class NorthwindDB
    {
        public static SqlConnection getConnection()
        {
            string str = @"Data Source=localhost\sqlexpress;Initial Catalog=Northwind;Integrated Security=True";
            SqlConnection connection = new SqlConnection(str);

            return connection;
        }
    }
}
