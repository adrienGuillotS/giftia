using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Models
{
    public class Order
    {
        public string OrderId { get; set; }

        public List<CertificateInfo> CertificateInfos { get; set; }
    }
}
