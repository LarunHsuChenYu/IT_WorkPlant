using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace IT_WorkPlant.Models
{
    // เก็บข้อมูลของแต่ละอุปกรณ์
    public class ServerInfo
    {
        public string Name { get; set; }
        public string IP { get; set; }
    }

    // โหลดข้อมูลจากไฟล์ JSON
    public static class ServerList
    {
        public static List<ServerInfo> LoadServers()
        {
            // ระบุ path ไปยัง App_Data\serverlist.json
            string path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/serverlist.json");

            // อ่านไฟล์ JSON
            string json = File.ReadAllText(path);

            // แปลง JSON → List<ServerInfo>
            return new JavaScriptSerializer().Deserialize<List<ServerInfo>>(json);
        }
    }
}
