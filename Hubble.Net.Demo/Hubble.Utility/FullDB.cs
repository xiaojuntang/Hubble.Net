using Hubble.Analyzer;
using Hubble.Core.Analysis;
using Hubble.Core.Analysis.HighLight;
using Hubble.SQLClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hubble.Utility
{
    /// <summary>
    /// 枚举分词的方式
    /// </summary>
    public enum AnalyzerEnum
    {
        SimpleAnalyzer,
        EnglishAnalyzer,
        PanGuSegment
    }
    /// <summary>
    /// 搜索方式
    /// </summary>
    public enum SearchEnum
    {
        like,
        match,
        contains
    }

    /// <summary>
    /// 全文索引服务类
    /// </summary>
    public sealed class FullDB
    {
        #region 私有数据成员
        private static SortedList<string, string> _conStrs = null;   //所有的连接字符串
        private int _cacheTimeout = -1;//缓存时间 为毫秒  -1 标示不缓存
        #endregion

        #region 静态构造函数初始化私有数据
        /// <summary>
        /// 静态构造函数
        /// 提供数据库列表集合
        /// </summary>
        static FullDB()
        {
            if (_conStrs == null)
            {
                _conStrs = new SortedList<string, string>();
                for (int i = 1; i < ConfigurationManager.ConnectionStrings.Count; i++)
                {
                    _conStrs.Add(ConfigurationManager.ConnectionStrings[i].Name, ConfigurationManager.ConnectionStrings[i].ConnectionString);
                }
            }
        }
        #endregion

        #region  构造函数 初始化数据
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cacheTime">是否缓存 0为缓存-1为不缓存</param>
        public FullDB(int cacheTime)
        {
            this._cacheTimeout = cacheTime;
        }
        #endregion

        #region 静态方法
        /// <summary>
        /// 获取全文索引异步链接对象
        /// </summary>
        /// created：mxk  date：2012-01-16
        /// <param name="conStr">访问的数据库相关字符串比默认库传入空</param>
        /// <returns>全文索引异步链接对象</returns>
        public static HubbleAsyncConnection getAsyncConnect(string conStr)
        {
            string hubConstr = conStr;
            if (conStr == null || conStr == string.Empty)
            {
                hubConstr = "constr";
            }
            return new HubbleAsyncConnection(_conStrs[conStr]);
            //return new HubbleAsyncConnection(_conStrs["f" + hubConstr]);
        }

        /// <summary>指定字段的搜索方式 返回搜索的字符串
        /// 指定字段的搜索方式 并且可以给字段加权
        /// </summary>
        /// <param name="searchEnum">搜索方式</param>
        /// <param name="filed">搜索的字段</param>
        /// <param name="FiledWeight">如果给字段加权 那么权重范围是0~65535之间数字 默认为1</param>
        /// <returns></returns>
        public static string GetFiledSearch(SearchEnum searchEnum, string filed, int FiledWeight)
        {
            StringBuilder searchFiledSb = new StringBuilder();
            searchFiledSb.Append(filed);
            if (FiledWeight != 1)
            {
                searchFiledSb.Append("^");
                searchFiledSb.Append(FiledWeight);
            }
            searchFiledSb.Append(" ");
            searchFiledSb.Append(searchEnum);
            return searchFiledSb.ToString();
        }

        /// <summary>
        /// 文本语法高亮的方法
        /// </summary>
        /// <param name="text">输入的文本</param>
        /// <param name="keys">搜索关键字 需要高亮的文本</param>
        /// <param name="analyEnum">选择分词方式</param>
        /// <returns>成功返回高亮的文本 失败返回空字符串</returns>
        public static string HightLightText(string text, string keys, AnalyzerEnum analyEnum)
        {
            string _hightText = string.Empty;
            SimpleHTMLFormatter simpleHTMLFormatter = new SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = null;
            switch (analyEnum)
            {
                case AnalyzerEnum.SimpleAnalyzer:
                    highlighter = new Highlighter(simpleHTMLFormatter, new SimpleAnalyzer());
                    break;
                case AnalyzerEnum.EnglishAnalyzer:
                    EnglishAnalyzer engEa = new EnglishAnalyzer();
                    engEa.Init();
                    highlighter = new Highlighter(simpleHTMLFormatter, engEa);
                    break;
                case AnalyzerEnum.PanGuSegment:
                    highlighter = new Highlighter(simpleHTMLFormatter, new PanGuAnalyzer());
                    break;
                default:
                    break;
            }
            if (highlighter != null)
            {
                highlighter.FragmentSize = 500000;
                _hightText = highlighter.GetBestFragment(keys, text);
            }
            return _hightText;
        }
        #endregion

        #region 实力方法
        /// <summary>获取数据
        /// 全文搜索方法 获取数据
        /// </summary>
        /// <param name="conStr">hubble服务连接字符串</param>
        /// <param name="sql">查询sql语句 注意这里是T-SFQL语句 不是T-SQL语句</param>
        /// <param name="_paras">参数列表</param>
        /// <returns>返回数据集</returns>
        public DataTable GetDataTable(string conStr, string sql, params object[] _paras)
        {
            DataTable table = null;
            DataSet set = null;
            //获取使用分词的名称
            using (HubbleConnection hubbleCon = getAsyncConnect(conStr))
            {
                //打开链接
                try
                {
                    hubbleCon.Open();
                    HubbleCommand hubbleCommand = new HubbleCommand(hubbleCon);
                    //适配器对象
                    HubbleDataAdapter adapter = new HubbleDataAdapter();
                    /*添加分页参数*/
                    adapter.SelectCommand = new HubbleCommand(sql, hubbleCon);
                    /*添加用户传递的参数*/
                    if (_paras != null)
                    {
                        for (int i = 0; i < _paras.Length; i += 2)
                        {
                            adapter.SelectCommand.Parameters.Add(_paras[i].ToString(), _paras[i + 1]);
                        }
                    }
                    hubbleCommand = adapter.SelectCommand;
                    set = hubbleCommand.Query(this._cacheTimeout);
                }
                catch (Exception e)
                {

                    throw;
                }

                hubbleCon.Close();
            }
            if (set != null && set.Tables.Count == 1)
            {
                table = set.Tables[0];
            }

            return table;

        }
        /// <summary>
        /// 
        /// </summary>获取数据
        /// <param name="conStr">hubble服务连接字符串</param>
        /// <param name="sql">查询sql语句 注意这里是T-SFQL语句 不是T-SQL语句</param>
        /// <param name="count">返回总条数</param>
        /// <param name="_paras">参数列表</param>
        /// <returns>返回数据集</returns>
        public DataTable GetDataTable(string conStr, string sql, out int count, params object[] _paras)
        {
            DataTable table = GetDataTable(conStr, sql, _paras);
            if (table != null && table.Rows.Count > 0)
            {
                count = table.MinimumCapacity;//获取数据表的总条数
            }
            else
            {
                count = 0;
            }
            return table;
        }
        #endregion
    }
}
