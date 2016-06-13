using Hubble.Analyzer;
using Hubble.Core.Analysis;
using Hubble.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hubble.Utility
{
    /// <summary>
    /// 分词服务类
    /// </summary>
    public class WrodAnalyzerServer
    {
        #region 私有成员
        private IEnumerable<WordInfo> _wordList = null;//分词容器存放 分词用
        private IAnalyzer _iAnalyzer = null;//分词接口
        private string _text;//处理关键字
        #endregion

        #region 构造函数
        /// <summary>
        /// 分词服务
        /// </summary>
        /// <param name="text">需要分词得字符串</param>
        /// <param name="analyEnu">分词类型</param>
        public WrodAnalyzerServer(string text, AnalyzerEnum analyEnu)
        {
            switch (analyEnu)
            {
                case AnalyzerEnum.SimpleAnalyzer:
                    _iAnalyzer = new SimpleAnalyzer();
                    break;
                case AnalyzerEnum.EnglishAnalyzer:
                    _iAnalyzer = new EnglishAnalyzer();
                    break;
                case AnalyzerEnum.PanGuSegment:
                    _iAnalyzer = new PanGuAnalyzer();
                    break;
                default:
                    break;
            }
            _wordList = _iAnalyzer.Tokenize(text);
            _text = text;
        }


        #endregion

        #region 分词方法
        /// <summary>
        /// 公用方法 获取分词之后字符串
        /// </summary>
        /// <param name="CacheTime">缓存时间</param>
        /// <returns>返回分词之后字符串</returns>
        public string GetMacheStr(int CacheTime)
        {
            string macheStr = null;
            //先从缓存中读取
            CacheHelper dataChache = new CacheHelper(CacheTime);
            object obj = dataChache.GetCacheObj(this._text);
            if (obj != null)
            {
                macheStr = obj.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(this._text))
                {
                    StringBuilder result = new StringBuilder();
                    foreach (WordInfo word in _wordList)
                    {
                        result.AppendFormat("{0}^{1}^{2} ", word.Word, word.Rank, word.Position);
                    }
                    macheStr = result.ToString();
                }
                if (CacheTime >= 0)
                {
                    dataChache.AddCache(this._text, macheStr);
                }
                else
                {
                    //nothing is here
                }
            }

            return macheStr;
        }
        #endregion
    }
}
