using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QingTing.Fm.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace QingTing.Fm.Service
{
    public class HttpProgram
    {
        /// <summary>
        /// 根据分页获取节目
        /// </summary>
        /// <param name="page"></param>
        public async Task<ObservableCollection<ProgramsModel>> GetShowList(int page)
        {
            var Programs = new ObservableCollection<ProgramsModel>();
            var json = JObject.Parse(await HttpHelper.GetWebAsync(string.Format("https://i.qingting.fm/wapi/channels/239329/programs/page/{0}/pagesize/10", page)))["data"];
            return Programs = JsonConvert.DeserializeObject<ObservableCollection<ProgramsModel>>(json.ToString());
        }
        /// <summary>
        /// 获取当前界面详情信息
        /// </summary>
        public ProgramsModel GetChannels()
        {
            var Channels = new ProgramsModel();
            var json = JObject.Parse(HttpHelper.GetWeb("https://i.qingting.fm/wapi/channels/239329"))["data"];
            return Channels = JsonConvert.DeserializeObject<ProgramsModel>(json.ToString());
        }
    }
}
