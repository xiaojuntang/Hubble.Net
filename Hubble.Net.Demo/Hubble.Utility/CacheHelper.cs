using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;

namespace Hubble.Utility
{
    public sealed class CacheHelper
    {
        private int cacheTime = 15;//缓存时间默认为 15秒
        private Cache dataCache;// 存放数据

        public int CacheTime
        {
            get { return cacheTime; }
            set { cacheTime = value; }
        }

        public Cache DataCache
        {
            get { return this.dataCache; }
            set { this.dataCache = value; }
        }
        //构造函数初始化数据
        public  CacheHelper(int _time)
        {
            this.cacheTime = _time;
            this.dataCache = HttpRuntime.Cache;
        }

        public void AddCache(string key ,object obj)
        {
            TimeSpan tsp=TimeSpan.FromSeconds (this.cacheTime );
            if (this.dataCache [key] == null)
            {
                this.dataCache.Insert(key, obj, null, DateTime.Now.AddSeconds(this.cacheTime), Cache .NoSlidingExpiration );
            }
            else
            {
                //nothing is here
            }
        }

        public void RemoveCaChe(string key)
        {
            if (dataCache[key] != null)
            {
                dataCache.Remove(key);
            }
        }

       

        public object GetCacheObj(string key )
        {
            return this.dataCache[key];   
        }


    }
}
