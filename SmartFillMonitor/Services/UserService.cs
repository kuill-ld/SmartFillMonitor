using SmartFillMonitor.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Shell;

namespace SmartFillMonitor.Services
{
    public static class UserService
    {
        private const string StaticSalt = "MySuperSecretSalt_2026!@#"; // 固定盐值
        private static User ?_currentUser;
        /// <summary>
        /// 登入状态改变事件，参数为当前用户对象，如果为null表示未登入
        /// </summary>
        public static event Action<User?>? LoginStateChanged;
        public static User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                LoginStateChanged?.Invoke(_currentUser);
            }
        }
        /// <summary>
        /// 初始化默认用户，只会调用1次
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeAsync()
        {
            try
            {
                bool hasUser = await DbProvider.Fsql.Select<User>().AnyAsync();
                if (!hasUser)
                {
                    var now = DateTime.Now;
                    var defaultUsers = new List<User>
                    {
                         new User()
                         {
                                UserName = "admin",
                                Display = "管理员",
                                PasswordHash = HashPassword("111"), // admin
                                Role = Role.Admin,
                                 CretedAt = now,
                         },
                         new User()
                         {
                                UserName = "Engineer",
                                Display = "工程师",
                                PasswordHash = HashPassword("111"), // admin
                                Role = Role.Engineer,
                                 CretedAt = now,
                         }

                    };
                   var affrows= await DbProvider.Fsql.Insert(defaultUsers).ExecuteAffrowsAsync();
                    if (affrows == 2)
                    {
                        LogService.Info($"默认用户创建成功");
                        
                    }
                    else
                    {
                        LogService.Warn($"默认用户创建失败");
                    }

                }
            }
            catch (Exception ex)
            {
                LogService.Error($"系统初始化用户失败", ex);
            }
        }
        /// <summary>
        /// 创建新用户保存到数据库
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <param name="display"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task CreatUserAysnc(string userName,string password,Role role,string display="")
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("用户名和密码不能为空");
            }
            bool exists = await DbProvider.Fsql.Select<User>().Where(u => u.UserName == userName).AnyAsync();
            if(exists)
            {
                throw new ArgumentException("用户名已存在");
            }

            var user = new User()
            {
                UserName = userName,
                Display = string.IsNullOrEmpty(display) ? userName : display,
                PasswordHash = HashPassword(password),
                Role = role,
                CretedAt = DateTime.Now
            };
            await DbProvider.Fsql.Insert<User>().AppendData(user).ExecuteAffrowsAsync();
            LogService.Info($"新用户创建成功:{user.UserName}");
        }
        public static async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await DbProvider.Fsql.Select<User>().OrderBy(u=>u.UserName).ToListAsync();
            }
            catch (Exception ex)
            {
                LogService.Error($"获取用户列表失败",ex);
                throw;
            }
           
        }
        public static Task LogoutAsync()
        {
            if (CurrentUser != null)
            {
                LogService.Warn($"用户[{CurrentUser.UserName}]已登出");
                CurrentUser = null;
            }
            return Task.CompletedTask;
        
        }
        /// <summary>
        /// 验证用户登入凭证
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> AuthernticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }
            try
            {
                var user = await DbProvider.Fsql.Select<User>().Where(u => u.UserName == username).FirstAsync();
                if (user == null)
                {
                    return false;
                }
                var hash = HashPassword(password);
                bool isValid = string.Equals(user.PasswordHash,hash,StringComparison.Ordinal);
                if(isValid)
                {
                    CurrentUser = user; 
                    LogService.Info($"用户[{username}]登入成功");
                }
                else
                {
                    LogService.Warn($"用户[{username}]登入失败，密码错误");
                }
                return isValid;
            }
            catch (Exception ex)
            {
                LogService.Error($"用户登入失败",ex);
                return false;
            }
        }
        /// <summary>
        /// 对明文密码进行哈希处理，返回哈希字符串
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }
            string raw = password + StaticSalt;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            //优化容量
            var sb = new StringBuilder(bytes.Length*2);
            foreach (var a in bytes)
            {
                sb.Append(a.ToString());
            }
            return sb.ToString();

        }
    }
}
