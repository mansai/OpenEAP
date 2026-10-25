using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Yitter.IdGenerator;

namespace OpenEAP.Model.Entities
{
    [Serializable]
    public abstract class Entity
    {
        /// <summary>
        /// 唯一主键
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public virtual long Id { get; set; }

        public Entity()
        {
            if (YitIdHelper.IdGenInstance != null)
            {
                Id = YitIdHelper.NextId();
            }
            else
            {
                var options = new IdGeneratorOptions();
                YitIdHelper.SetIdGenerator(options);
                Id = YitIdHelper.NextId();
            }
        }

        public Entity(long id) { this.Id = id; }

        #region 验证方法

        string error;

        /// <summary>
        /// 验证失败提示信息
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsIgnore = true)]
        public string Error { get { return error; } }

        /// <summary>
        /// 非空验证
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool NonEmpty(string val)
        {
            bool isValid = !string.IsNullOrEmpty(val);
            SetError(isValid, "can't be empty");
            return isValid;
        }

        void SetError(bool isValid, string errorString)
        {
            if (isValid)
                error = string.Empty;
            else
                error = errorString;
        }

        #endregion

        /// <summary>
        /// 获取当前用户编号
        /// </summary>
        protected static long? GetUserId()
        {
            return 1;
        }
    }
}