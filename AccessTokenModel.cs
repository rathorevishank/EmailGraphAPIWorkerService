using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DIYWorkerService;

namespace EmailGraphAPIWorkerService
{
    public  class AccessTokenModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        //  public string refresh_token { get; set; }

    }
}
