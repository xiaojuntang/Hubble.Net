using Hubble.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hubble.Net.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string searchKey = "天安门";
            string mastr = FullDB.GetFiledSearch(SearchEnum.match, "Name1", 1);
            string sql = "select * from HubbleTable where " + mastr + "  @Name1";
            FullDB searchDb = new FullDB(10);
            int allcount = 0;
            object[] o = new object[2];
            o[0] = "@Name1";
            //o[1] = searchKey;


            List<string> _prarmsList = new List<string>();

            WrodAnalyzerServer word = null;
            //if (conStr == "03")
            //{
            //    word = new WrodAnalyzerServer(searchKey, AnalyzerEnum.EnglishAnalyzer);
            //}
            //else
            //{
            word = new WrodAnalyzerServer(searchKey, AnalyzerEnum.PanGuSegment);
            //}
            o[1] = word.GetMacheStr(10);





            string conStr = "ZYTConnString";
            // table = FullText.GetFullDataTable(conStr, sql, "ddt_t_taoshijuan", matchDic, 1, 6, out allcount, out millsecond, o);
            var table = searchDb.GetDataTable(conStr, sql, out allcount, o);
        }
    }
}
