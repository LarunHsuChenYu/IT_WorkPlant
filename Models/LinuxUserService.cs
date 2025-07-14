using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Renci.SshNet;

namespace IT_WorkPlant.Models
{
    public class LinuxUserService
    {
      
        public string UserName { get; set; }
        public string Role { get; set; }  
        public string Env { get; set; }  
        public bool AddDba { get; set; }

        
        private readonly string linuxHost = "192.168.1.48:22";
        private readonly string rootUser = "root";
        private readonly string rootPwd = "*Entprise148#";

        public string CreateUser()
        {
            string result = "";
            using (var ssh = new SshClient(linuxHost, "tiptop", "*enrich2025"))
            {
                try
                {
                    ssh.Connect();

                    
                    string homeDir = Role == "IT"
                        ? (Env == "prod" ? "/u1/usr/tiptop" : "/u1/usr/tiptop_test")
                        : "/u1/usr/topgui";

                  
                    string cmdAdd = $"useradd -g 501 -d {homeDir} -s /bin/ksh {UserName}";
                    var r1 = ssh.RunCommand(cmdAdd);
                    result += r1.Result + r1.Error;

                    
                    string cmdPass = $"echo \"{UserName}:{UserName}\" | chpasswd";
                    var r2 = ssh.RunCommand(cmdPass);
                    result += r2.Result + r2.Error;

                    if (AddDba)
                    {
                        string cmdDba = $"usermod -a -G dba {UserName}";
                        var r3 = ssh.RunCommand(cmdDba);
                        result += r3.Result + r3.Error;
                    }

                    ssh.Disconnect();
                }
                catch (Exception ex)
                {
                    result += "發生錯誤：" + ex.Message;
                }
            }
            return result;
        }
    }
}
