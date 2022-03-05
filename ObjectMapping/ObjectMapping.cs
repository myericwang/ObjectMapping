using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectMapping
{
    #region OAO Using Class

    /// <summary>
    /// 映射轉換方式
    /// </summary>
    public enum EnumTransWay
    {
        /// <summary>
        /// 預設指派指標
        /// </summary>
        Default = 0,

        /// <summary>
        /// 設定NULL
        /// </summary>
        SetNull = 101,

        /// <summary>
        /// 新Guid
        /// </summary>
        NewGuidToString = 111,

        /// <summary>
        /// List<object> to List<object>
        /// </summary>
        ListObjectToListObject = 201,

        /// <summary>
        /// List to List<object>
        /// </summary>
        ListToListObject = 205,

        /// <summary>
        /// List to Dictionary
        /// </summary>
        TwoListToDictionary = 211,
    }

    #endregion OAO Using Class

    /// <summary>
    /// 動態物件映射邏輯
    /// </summary>
    public class ObjectMapping
    {
        /// <summary>
        /// 委派時做邏輯，可自行客製修改TransWay邏輯
        /// </summary>
        private Dictionary<EnumTransWay, transMethod> ITrans;

        private object _inModel;

        /// <summary>
        /// 建構值
        /// </summary>
        public ObjectMapping()
        {
            ITrans = new Dictionary<EnumTransWay, transMethod>()
            {
                { EnumTransWay.Default, new transMethod(Default) },
                { EnumTransWay.SetNull, new transMethod(setNull) },
                { EnumTransWay.TwoListToDictionary, new transMethod(TwoListToDictionary) },
            };
        }

        private delegate object transMethod(TreeMappingModel obj);

        /// <summary>
        /// 樹狀設定結構取的結果
        /// </summary>
        /// <typeparam name="T">要轉換的強行別</typeparam>
        /// <param name="path">設定檔路徑</param>
        /// <param name="inModel">要被轉換的物件</param>
        /// <param name="outModel">outModel原始資料</param>
        /// <returns>T</returns>
        public T GetTreeMapResult<T>(string path, object inModel, object outModel)
        {
            //從指定路徑取得設定json檔
            StreamReader r = new StreamReader(path);
            string jsonString = r.ReadToEnd();
            r.Dispose();
            var treeMap = JsonTransform.ToModelOrDefault<Dictionary<string, dynamic>>(jsonString);
            _inModel = inModel;

            var jsonModelT = JsonConvert.SerializeObject(treeRecursion(treeMap, DictionaryEx.ToDictionary<object>(outModel)));
            return JsonTransform.ToModelOrDefault<T>(jsonModelT);
        }

        /// <summary>
        /// 樹狀設定方式
        /// </summary>
        /// <param name="treeMapping">樹狀設定檔</param>
        /// <param name="outModel">outModel原始資料</param>
        /// <returns>Dictionary結構的資料</returns>
        private Dictionary<string, object> treeRecursion(Dictionary<string, dynamic> treeMapping, Dictionary<string, object> outModel)
        {
            foreach (var sub in treeMapping)
            {
                if (sub.Value.GetType().Name == "JArray")
                {
                    var tmp = DictionaryEx.ToDictionary<dynamic>(outModel[sub.Key]);
                    foreach (var next in sub.Value)
                    {
                        outModel[sub.Key] = treeRecursion(DictionaryEx.ToDictionary<dynamic>(next), tmp);
                    }
                }
                else
                {
                    EnumTransWay transway = (EnumTransWay)sub.Value["TransWay"];

                    var mapping = new TreeMappingModel(sub.Key, (string)sub.Value["InParameter"], transway);
                    outModel[sub.Key] = new transMethod(ITrans[transway]).Invoke(mapping);
                }
            }

            return outModel;
        }

        private object TwoListToDictionary(TreeMappingModel map)
        {
            var parameterDict = DictionaryEx.ToDictionary<string>(map.InParameter);

            var keyListTmp = getDataHierarchy(parameterDict["KeyList"], _inModel);
            var valueListTmp = getDataHierarchy(parameterDict["ValueList"], _inModel);

            var keyList = JsonTransform.ToModelOrDefault<List<string>>(keyListTmp?.ToString());
            var valueList = JsonTransform.ToModelOrDefault<List<string>>(valueListTmp?.ToString());

            parameterDict = new Dictionary<string, string>();
            for (int i = 0; i < keyList.Count; i++)
            {
                parameterDict.Add(keyList[i], valueList[i]);
            }

            return parameterDict;
        }

        /// <summary>
        /// 設NULL
        /// </summary>
        /// <param name="map">設定物件</param>
        /// <returns>object</returns>
        private object Default(TreeMappingModel map)
        {
            return getDataHierarchy(map.InParameter?.ToString(), _inModel);
        }

        /// <summary>
        /// 將Inkey的資料轉為Oukey的StringToString邏輯
        /// </summary>
        /// <param name="map">設定物件</param>
        /// <returns>NULL</returns>
        private object setNull(TreeMappingModel map)
        {
            return null;
        }

        /// <summary>
        /// 深度model取值
        /// </summary>
        /// <param name="stringHierarchy">物件路徑</param>
        /// <param name="inModelData">要被轉換的Model資訊</param>
        /// <returns>object</returns>
        private object getDataHierarchy(string stringHierarchy, object inModelData)
        {
            if (!string.IsNullOrEmpty(stringHierarchy))
            {
                var hierarchyList = stringHierarchy.Split('.');
                foreach (var h in hierarchyList)
                {
                    var imModelDict = DictionaryEx.ToDictionary<object>(inModelData);

                    if (!imModelDict.ContainsKey(h))
                    {
                        return null;
                    }

                    inModelData = imModelDict[h];

                    if (inModelData == null)
                    {
                        return inModelData;
                    }
                }
            }
            else
            {
                return null;
            }

            return inModelData;
        }
    }

    /// <summary>
    /// 物件映射設定檔Model
    /// </summary>
    public class TreeMappingModel
    {
        /// <summary>
        /// 建構值
        /// </summary>
        public TreeMappingModel() { }

        /// <summary>
        /// 建構值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="inParameter">inParameter</param>
        /// <param name="transWay">transWay</param>
        public TreeMappingModel(string key, string inParameter, EnumTransWay transWay)
        {
            this.OutKey = key;
            this.InParameter = inParameter;
            this.TransWay = transWay;
        }

        /// <summary>
        /// 輸出object屬性名稱
        /// </summary>
        public string OutKey { get; set; }

        /// <summary>
        /// 輸入object對應參數
        /// </summary>
        public object InParameter { get; set; }

        /// <summary>
        /// 映射轉換方式
        /// </summary>
        public EnumTransWay TransWay { get; set; }
    }
}
