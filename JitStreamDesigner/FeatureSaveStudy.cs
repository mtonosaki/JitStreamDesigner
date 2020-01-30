// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui.Uwp;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace JitStreamDesigner
{
    public class FeatureSaveStudy : FeatureSimulatorBase
    {
        [EventCatch]
        public async Task Save(EventTokenButton token)
        {
            var fd = new FileSavePicker
            {
                DefaultFileExtension = ".jmt",
                SuggestedFileName = Cold.StudyFilePath != null ? Path.GetFileName(Cold.StudyFilePath) : $"Jit Model {(DateTime.Now.ToString("yyyyMMdd-HHmmss"))}",
            };
            fd.FileTypeChoices.Add("Jit Stream Designer", new List<string>() { ".jmt", ".json", ".txt" });
            var file = await fd.PickSaveFileAsync();
            await WriteAsJson(file);
            LOG.AddMes(LLV.INF, "FeatureSaveStudy-SaveCompletely", file.Name, file.Path);
            ControlUtil.SetTitleText(file.Name);
            Cold.StudyFilePath = file.Path;
            ConfigUtil.Set("LastStudyFilePath", Cold.StudyFilePath);
        }

        public const string SEPARATOR = "====JMTSECTION====";

        /// <summary>
        /// Save data as Json format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task WriteAsJson(StorageFile file)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{{\"FormatVersion\":\"JMT1.0\"}}");
            sb.AppendLine(SEPARATOR);
            sb.AppendLine($"{{\"TemplateList.Count\":{Hot.TemplateList.Count}}}");
            foreach (var temp in Hot.TemplateList)
            {
                sb.AppendLine(SEPARATOR);
                var json = JsonConvert.SerializeObject(temp);
                sb.AppendLine(json);
            }
            sb.AppendLine(SEPARATOR);
            sb.AppendLine($"{{\"ActiveTemplate.ID\":\"{Hot.ActiveTemplate.ID}\"}}");
            sb.AppendLine(SEPARATOR);
            sb.AppendLine($"{{\"Sim.Clock\":{JsonConvert.SerializeObject(Now)}}}");

            await FileIO.WriteTextAsync(file, sb.ToString());

        }
    }
}
