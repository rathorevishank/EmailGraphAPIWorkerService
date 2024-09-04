using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIYWorkerService;

namespace DIYWorkerService
{
    public  class MailMessageInfo
    {

        public DateTime CreatedDateTime { get; set; }
        public DateTime SentDateTime { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string MessageId { get; set; }
    }
}
