using Hubble.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hubble.Net.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string searchKey = "北京";
            string mastr = FullDB.GetFiledSearch(SearchEnum.match, "Name1", 1);
            string sql = "select * from HubbleTable where " + mastr + "  @Name1";
            //string sql = "select * from HubbleTable where " + mastr + "  '天安门'";
            FullDB searchDb = new FullDB(600);
            int allcount = 0;
            List<string> _prarmsList = new List<string>();
            _prarmsList.Add("@Name1");
            WrodAnalyzerServer word = new WrodAnalyzerServer(searchKey, AnalyzerEnum.PanGuSegment);
            _prarmsList.Add(word.GetMacheStr(600));

            string conStr = "ZYTConnString";
            // table = FullText.GetFullDataTable(conStr, sql, "ddt_t_taoshijuan", matchDic, 1, 6, out allcount, out millsecond, o);
            var table = searchDb.GetDataTable(conStr, sql, out allcount, _prarmsList.ToArray());
            if(table!=null && table.Rows.Count > 0)
            {
                foreach (DataRow item in table.Rows)
                {
                   string constr= FullDB.HightLightText(item["Name1"].ToString(), searchKey, AnalyzerEnum.PanGuSegment);
                    Console.WriteLine(constr);
                }
            }
            Console.Read();
        }
    }
}
