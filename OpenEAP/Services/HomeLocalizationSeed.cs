using OpenEAP.Helper;
using OpenEAP.Model;
using System.Collections.Generic;
using System.Linq;

namespace OpenEAP.Services
{
    /// <summary>
    /// 首页（串焊检测集控）界面文案的三语种子：仅缺失时补入，绝不覆盖已有值（客户自定义翻译优先）
    /// </summary>
    public static class HomeLocalizationSeed
    {
        private static readonly (string Key, string ZhHans, string ZhHant, string EnUs)[] Entries =
        {
            ("HomeTitle", "串焊检测集控", "串焊檢測集控", "Stringer Inspection Console"),
            ("UserName", "操作员01", "操作員01", "Operator 01"),
            ("Settings", "设置", "設置", "Settings"),
            ("MachineSelect", "机台选择", "機台選擇", "Machine Select"),
            ("SelectedMachines", "已选监控机台", "已選監控機台", "Selected Machines"),
            ("Start", "开始", "開始", "Start"),
            ("Pause", "暂停", "暫停", "Pause"),
            ("Refresh", "刷新", "刷新", "Refresh"),
            ("SysStatus", "系统状态：", "系統狀態：", "System Status: "),
            ("PendingList", "待判定列表", "待判定列表", "Pending"),
            ("ResultList", "已判定列表", "已判定列表", "Results"),
            ("ResultHint", "滚动显示 · 最新在上", "滾動顯示 · 最新在上", "Scrolling · latest first"),
            ("Seq", "序号", "序號", "No."),
            ("Serial", "串号", "串號", "Serial"),
            ("DefectType", "异常类型", "異常類型", "Defect"),
            ("Time", "时间", "時間", "Time"),
            ("Judge", "判定人", "判定人", "Judge"),
            ("Verdict", "评定结果", "評定結果", "Verdict"),
            ("Reason", "原因", "原因", "Reason"),
            ("CurrentJudge", "当前待判定", "當前待判定", "Current"),
            ("CurrentJudgeHint", "状态灯指示异常类型 · 点 OK / NG 判定", "狀態燈指示異常類型 · 點 OK / NG 判定", "Status light indicates defect · click OK / NG"),
            ("Machine", "机台", "機台", "Machine"),
            ("SerialColon", "串号", "串號", "Serial"),
            ("CollectTime", "采集时间", "採集時間", "Collected"),
            ("JudgeColon", "判定人", "判定人", "Judge"),
            ("Appearance", "外观", "外觀", "Appearance"),
            ("El", "EL", "EL", "EL"),
            ("CodeMapTitle", "异常原因编码设置", "異常原因編碼設置", "Defect Code Settings"),
            ("CodeMapSub", "配置编码对应的异常原因，可增加或删除编码。0 为删除键。", "配置編碼對應的異常原因，可增加或刪除編碼。0 為刪除鍵。", "Configure code-to-reason mapping. 0 is delete key."),
            ("Code", "编码", "編碼", "Code"),
            ("Delete", "删除", "刪除", "Delete"),
            ("AddCode", "增加编码", "增加編碼", "Add Code"),
            ("ResetDefault", "恢复默认", "恢復默認", "Reset"),
            ("Cancel", "取消", "取消", "Cancel"),
            ("Save", "保存", "保存", "Save"),
        };

        public static void Ensure()
        {
            var keys = Entries.Select(e => e.Key).Distinct().ToList();
            var existing = DbHelp.fsql.Select<ResourcesEntity>()
                .Where(t => keys.Contains(t.Key))
                .ToList()
                .ToDictionary(t => $"{t.Key}|{t.Culture}");

            foreach (var e in Entries)
            {
                EnsureOne(existing, e.Key, "zh-Hans", e.ZhHans);
                EnsureOne(existing, e.Key, "zh-Hant", e.ZhHant);
                EnsureOne(existing, e.Key, "en-US", e.EnUs);
            }
        }

        private static void EnsureOne(Dictionary<string, ResourcesEntity> existing, string key, string culture, string value)
        {
            var lookup = $"{key}|{culture}";
            if (existing.ContainsKey(lookup)) return; // 已存在：保留客户自定义值，不覆盖
            DbHelp.fsql.Insert(new ResourcesEntity
            {
                ModuleId = 0,
                Culture = culture,
                Key = key,
                Value = value,
                Type = "99",
                IsDeleted = "N",
            }).ExecuteAffrows();
        }
    }
}
