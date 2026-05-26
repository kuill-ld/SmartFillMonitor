using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FreeSql.DataAnnotations;
namespace SmartFillMonitor.Models
{
    [Table(Name = "Users")]
    [Index("Idx_UserName", "UserName", true)]
    public class User
    {
        [Column(IsPrimary = true,IsIdentity = true)]
        public long Id { get; set; }
        [Column(StringLength =50,IsNullable =false)]
        public string UserName { get; set; }
        /// 显示名称
        [Column(StringLength = 50)]
        public string  Display { get; set; }
        /// <summary>
        /// 存储哈希密码
        /// </summary>
        [Column(StringLength = 128, IsNullable = false)]
        public string PasswordHash { get; set; }
        [Column(MapType = typeof(int))]
        public Role Role { get; set; }
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool isDisabled { get; set; }

        public DateTime CretedAt { get; set; }= DateTime.Now;//创建角色时间
        public DateTime? LastLoginTime {  get; set; }//最后登录时间
        /// <summary>
        /// 方便ui绑定，只读
        /// </summary>
        [Column(IsIgnore = true)]
        public string RoleName => Role switch
        {
            Role.Admin => "管理员",
            Role.Engineer => "工程师",
            Role.Operator => "操作员",
            _ => "未知角色"
        };
    }
    public enum Role
    {
        [Description("管理员")]
        Admin =0,
        [Description("工程师")]
        Engineer,
        [Description("操作员")]
        Operator,
       
    }
}
