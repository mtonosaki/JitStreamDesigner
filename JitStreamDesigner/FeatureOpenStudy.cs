// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace JitStreamDesigner
{
    public class FeatureOpenStudy : FeatureSimulatorBase
    {
        [EventCatch]
        public async Task Open(EventTokenButton token)
        {
            var fd = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            };
            var last = ConfigUtil.Get("LastStudyFilePath", string.Empty);
            fd.FileTypeFilter.Add(".jmt");
            var file = await fd.PickSingleFileAsync();
            await LoadFromJson(file, token);
            LOG.AddMes(LLV.INF, "FeatureOpenStudy-LoadCompletely", file.Name, file.Path);
            ControlUtil.SetTitleText(file.Name);
            Cold.StudyFilePath = file.Path;
            ConfigUtil.Set("LastStudyFilePath", Cold.StudyFilePath);
        }

        public const string SEPARATOR = FeatureSaveStudy.SEPARATOR;

        /// <summary>
        /// Save data as Json format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task LoadFromJson(StorageFile file, EventToken token)
        {
            ClearInstance();
            var templateNo = 0;
            var templateNoMax = 0;
            var alltxt = await FileIO.ReadTextAsync(file);
            alltxt = alltxt.Replace("\r\n", "");
            alltxt = alltxt.Replace("\r", "");
            alltxt = alltxt.Replace("\n", "");
            var txts = alltxt.Split(SEPARATOR);
            for (var i = 1; i < txts.Length; i++)
            {
                var json = txts[i].Trim();
                if (templateNo >= 1 && templateNo <= templateNoMax)
                {
                    RestoreTemplate(json);
                    templateNo++;
                    continue;
                }
                var obj = JsonConvert.DeserializeObject(json);
                if (obj is JObject jo && jo.First is JProperty jp && jp.Value is JValue jv)
                {
                    switch (jp.Name)
                    {
                        case "TemplateList.Count":
                        {
                            templateNoMax = jv.Value<int>();
                            templateNo = 1;
                            break;
                        }
                        case "ActiveTemplate.ID":
                        {
                            var id = jv.Value<string>();
                            Hot.ActiveTemplate = Hot.TemplateList.Where(a => a.ID.Equals(id)).FirstOrDefault();
                            break;
                        }
                        case "Sim.Clock":
                        {
                            var val = jv.Value<DateTime>();
                            Now = val;
                            break;
                        }
                    }
                }
            }
            Finalize(token);
        }

        private void ClearInstance()
        {
            Hot.TemplateList.Clear();
            Hot.ActiveTemplate = null;

            // Rebuild Gui Parts
            var pts =
                from Layer in LAYER.JitObjects
                from Parts in Parts.GetParts(Layer)
                select (Layer, Parts);

            foreach (var lp in pts)
            {
                Parts.Remove(PaneJitParts, lp.Parts, lp.Layer);
            }
        }

        private void RestoreTemplate(string json)
        {
            var tcm = JsonConvert.DeserializeObject<TemplateTipModel>(json);
            ResetJac(tcm);
            tcm.UndoRedoCurrenttPointer = 0;    // Expecting Redo to make instance
            Hot.TemplateList.Add(tcm);
        }

        private void Finalize(EventToken token)
        {
            Hot.ActiveTemplate.UndoRedoCurrenttPointer = 0;
            Token.Link(token, new EventUndoRedoQueueConsumptionTokenTrigger
            {
                Sender = this,
                Remarks = "Template Changed",
            });

            // neet to exec by Token instead of ListChip Selected
            //if (ReferenceEquals(TargetListView.SelectedItem, token.TargetTemplate) == false)
            //{
            //    TargetListView.SelectedItem = token.TargetTemplate; // for if not called TargetListView_SelectionChanged
            //}
            var at = Hot.ActiveTemplate;
            Hot.ActiveTemplate = null;
            Token.Link(token, new EventTokenTemplateChangedTrigger
            {
                TargetTemplate = at,
                Sender = this,
                Remarks = "After Open Study",
            });
        }
    }
}
