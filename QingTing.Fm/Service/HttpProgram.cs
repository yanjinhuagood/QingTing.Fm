using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QingTing.Fm.Models;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QingTing.Fm.Service
{
    public static class HttpProgram
    {
        /// <summary>
        /// 根据分页获取节目
        /// </summary>
        /// <param name="page"></param>
        public static async Task<ObservableCollection<ProgramsModel>> GetShowList(int page)
        {
            var Programs = new ObservableCollection<ProgramsModel>();
            var json = JObject.Parse(await HttpHelper.GetWebAsync(string.Format("https://i.qingting.fm/wapi/channels/239329/programs/page/{0}/pagesize/10", page)))["data"];
            return Programs = JsonConvert.DeserializeObject<ObservableCollection<ProgramsModel>>(json.ToString());
        }
        /// <summary>
        /// 获取当前界面详情信息
        /// </summary>
        public static async Task<ProgramsModel> GetChannelsAsync(int page = 1)
        {
            string url = "https://webbff.qtfm.cn/www";
            string payload = $@"
{{
    ""query"": ""{{ channelPage(cid:239329, page:{page}, order:\""asc\"", qtId:\""null\"") {{ album seo plist reclist categoryId categoryName collectionKeywords }} }}""
}}";

            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode(); 
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(responseBody)["data"];
                    JObject channelPage = (JObject)json["channelPage"];
                    JObject album = (JObject)channelPage["album"];
                    var channel = JsonConvert.DeserializeObject<ProgramsModel>(album.ToString());
                    if (channel == null) return null;
                    var plist = (JArray)channelPage["plist"];
                    var plists = JsonConvert.DeserializeObject<ObservableCollection<ProgramsModel>>(plist.ToString());
                    if (plists != null)
                        channel.Podcasters = plists;
                    return channel;
                }
                catch (HttpRequestException e)
                {
                }
            }
            return null;
        }
    }
}
