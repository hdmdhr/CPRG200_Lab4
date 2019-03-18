using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthwindData
{
    /*
     * Author: DongMing Hu
     * Date Created: 3/15/2019
     * Purpose: Business class, properties match the columns of Order Detail table from database.
     * 
     */

    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }

    }
}
