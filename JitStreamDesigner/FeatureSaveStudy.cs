// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui.Uwp;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

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
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = $"Jit Model {(DateTime.Now.ToString("yyyyMMdd-HHmmss"))}",
            };
            fd.FileTypeChoices.Add("Jit Stream Designer", new List<string>() { ".jmj", ".josn", ".txt" });
            var file = await fd.PickSaveFileAsync();
            await WriteAsJson(file);
            LOG.AddMes(LLV.INF, "FeatureSaveStudy-SaveCompletely", file.Name, file.Path);
        }

        public const string SEPARATOR = ":::/!/:::";

        /// <summary>
        /// Save data as Json format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task WriteAsJson(StorageFile file)
        {
            var sb = new StringBuilder();
            sb.AppendLine("JitStreamDesigner jmt File");
            sb.AppendLine($"{{\"FormatVersion\":1.0}}");
            sb.AppendLine(SEPARATOR);
            sb.AppendLine($"{{ \"TemplateList.Count\":{Hot.TemplateList.Count} }}");
            foreach (var temp in Hot.TemplateList)
            {
                sb.AppendLine(SEPARATOR);
                var json = JsonConvert.SerializeObject(temp);
                sb.AppendLine(json);
            }
            sb.AppendLine(SEPARATOR);
            sb.AppendLine($"{{ \"ActiveTemplate.ID\":{Hot.ActiveTemplate.ID} }}");

            await FileIO.WriteTextAsync(file, sb.ToString());

        }
    }
}
