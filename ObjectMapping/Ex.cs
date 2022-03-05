﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMapping
{
    /// <summary>
    /// Json序列化Extension
    /// </summary>
    public static class JsonTransform
    {
        /// <summary>
        /// 轉換為指定Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T ToModelOrDefault<T>(string jsonString)
        {
            return jsonString == null ? Activator.CreateInstance<T>() : JsonConvert.DeserializeObject<T>(jsonString);
        }
    }

    /// <summary>
    /// Dictionary 擴充方法
    /// </summary>
    public static class DictionaryEx
    {
        /// <summary>
        /// ToDictionary
        /// </summary>
        /// <typeparam name="TValue">T</typeparam>
        /// <param name="obj">obj</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        /// <summary>
        /// 兩dictionary相加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static Dictionary<string, T> AddEach<T>(Dictionary<string, T> d1, Dictionary<string, T> d2)
        {
            Dictionary<string, T> ans = new Dictionary<string, T>();

            foreach (var i in d1)
            {
                //TODO是否要做重複ｋｅｙ值判斷
                ans.Add(i.Key, i.Value);
            }

            foreach (var i in d2)
            {
                //TODO是否要做重複ｋｅｙ值判斷
                ans.Add(i.Key, i.Value);
            }

            return ans;
        }
    }
}
