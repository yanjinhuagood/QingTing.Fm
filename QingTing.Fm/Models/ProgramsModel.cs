﻿using Newtonsoft.Json;
using QingTing.Fm.Utility;
using System;
using System.Collections.ObjectModel;

namespace QingTing.Fm.Models
{
    /// <summary>
    /// 节目实体类
    /// </summary>
    public class ProgramsModel : ViewModelBase
    {
        /// <summary>
        /// 图像地址
        /// </summary>
        [JsonProperty("img_url")]
        public string ImageUrl { get; set; }
        /// <summary>
        /// 节目id
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        /// <summary>
        /// 节目name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        [JsonProperty("sequence")]
        public int Sequence { get; set; }
        /// <summary>
        /// 持续时间
        /// </summary>
        [JsonProperty("duration")]
        public double Duration { get; set; }
        /// <summary>
        /// 节目地址
        /// </summary>

        public string FilePath
        {
            get
            {
                long t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //return $"https://audio.qtfm.cn/audiostream/redirect/239329/{Id}?access_token=&device_id=MOBILESITE&qingting_id=&t={t}&sign=5ee6829dd3d72d212860fd6771d13158";
                return $"https://audio.qtfm.cn/audiostream/redirect/239329/{Id}?access_token=&device_id=MOBILESITE&qingting_id=&t=1753885375531&sign=5ee6829dd3d72d212860fd6771d13158";
            }
        }

        /// <summary>
        /// 节目播放量(点播总量)
        /// </summary>
        [JsonProperty("playcount")]
        public string PlayCount { get; set; }
        /// <summary>
        /// 节目更新时间
        /// </summary>
        [JsonProperty("update_time")]
        public string UpdateTime { get; set; }
        /// <summary>
        /// 共{program_count}个节目
        /// </summary>
        [JsonProperty("program_count")]
        public string ProgramCount { get; set; }
        /// <summary>
        /// channel_ondemand
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        [JsonProperty("desc")]
        public string Desc { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        [JsonProperty("isfree")]
        public bool IsFree { get; set; }

        public bool Charge
        {
            get 
            { 
                return !IsFree; 
            }
        }
        /// <summary>
        /// 主播
        /// </summary>
        [JsonProperty("podcasters")]
        public ObservableCollection<ProgramsModel> Podcasters { get; set; }

        private bool _isPlay;
        /// <summary>
        /// 是否播放
        /// </summary>
        public bool IsPlay
        {
            get { return _isPlay; }
            set
            {
                _isPlay = value;
                OnPropertyChanged(nameof(IsPlay));
            }
        }
    }
}
