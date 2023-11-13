using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IMS.Models
{
    public class common
    {
        //public static string MyConnection()
        //{
        //    return @"Server=BPDWEBSERVER\SQLEXPRESS;Database=IMS;Uid=sa;pwd=1;";
        //}
        public static string MyConnection()
        {
            return @"Data Source=192.168.101.52;initial catalog=spms;Password=pimo@123;Persist Security Info=True;User ID=sa;";
            //return @"Data Source=192.168.2.1\PGAS;initial catalog=spms;Password=(@/51u0#2@3n8D0e1L1#0u1R;Persist Security Info=True;User ID=pgasIS;";
           // return @"Data Source=SMUGGLER-PC\SA;initial catalog=spms;Password=12345;Persist Security Info=True;User ID=sa;";
           // return @"Data Source=10.0.0.123;initial catalog=SPMS;Password=123123;Persist Security Info=True;User ID=sa;";
           // <ConName>Server=.;Database=SPMS;Uid=sa;pwd=123123;Pooling=False;</ConName>
        }
        public static string livecon()
        {
            return @"Data Source=192.168.101.52;initial catalog=spms;Password=pimo@123;Persist Security Info=True;User ID=sa;";
           //return @"Data Source=192.168.2.1\PGAS;initial catalog=spms;Password=(@/51u0#2@3n8D0e1L1#0u1R;Persist Security Info=True;User ID=pgasIS;";
           // return @"Data Source=SMUGGLER-PC\SA;initial catalog=spms;Password=12345;Persist Security Info=True;User ID=sa;";
           // return @"Data Source=10.0.0.123;initial catalog=SPMS;Password=123123;Persist Security Info=True;User ID=sa;";
        }

        public static string pmis()
        {
            return @"Data Source=192.168.101.52;initial catalog=pmis;Password=pimo@123;Persist Security Info=True;User ID=sa;";
            //return @"Data Source=192.168.2.1\PGAS;initial catalog=spms;Password=(@/51u0#2@3n8D0e1L1#0u1R;Persist Security Info=True;User ID=pgasIS;";
            // return @"Data Source=SMUGGLER-PC\SA;initial catalog=spms;Password=12345;Persist Security Info=True;User ID=sa;";
            // return @"Data Source=10.0.0.123;initial catalog=SPMS;Password=123123;Persist Security Info=True;User ID=sa;";
        }

        public static string memis()
        {
            return @"Data Source=192.168.101.52;initial catalog=memis;Password=pimo@123;Persist Security Info=True;User ID=sa;";
 
        }

    }
}