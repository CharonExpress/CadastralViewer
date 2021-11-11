using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CadastralViewer.Pages
{
    public partial class MapComponent : ComponentBase
    {

        [Inject]
        protected IJSRuntime jsR { get; set; }

        [Inject]
        MudBlazor.ISnackbar Snackbar { get; set; }

        [Inject]
        ILogger<Map> mapLogger { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await jsR.InvokeVoidAsync("Omap.initMap");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FeatureCollection">Preserealizated GeoJSON string</param>
        /// <param name="LayerTitle">Preffered layer title</param>
        /// <returns></returns>
        public async Task LoadVectorLayer(string FeatureCollection, string LayerTitle = "Загруженный слой")
        {
            await jsR.InvokeVoidAsync("Omap.addLayer", FeatureCollection, LayerTitle);
        }

        protected async Task UploadFiles(InputFileChangeEventArgs e)
        {

            if (e.FileCount == 0) { return; }

            var _xmlFiles = e.GetMultipleFiles().Where(f => Path.GetExtension(f.Name) == ".xml");

            if (_xmlFiles == null || _xmlFiles.Count() == 0)
            {
                Snackbar.Add("No xml files were provided", MudBlazor.Severity.Warning);
                mapLogger.LogWarning("No xml files were provided");
                return;
            };

            var xmlFiles = _xmlFiles.ToList();

            System.Xml.XmlDocument xmlDocument;

            for (int iterator = 0; iterator < xmlFiles.Count(); iterator++)
            {
                xmlDocument = new();

                MemoryStream destination = new MemoryStream();
                await xmlFiles[iterator].OpenReadStream().CopyToAsync(destination);

                destination.Seek(0, SeekOrigin.Begin);
                xmlDocument.Load(destination);
                SpatialData spatialData = new();
                try
                {
                    spatialData.AddFeatures(xmlDocument);
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"Error processing file {xmlFiles[iterator].Name}", MudBlazor.Severity.Error);
                    mapLogger.LogError($"Error processing file {xmlFiles[iterator].Name}. \n {ex.Message}");
                }

                await AddFeatureCollection(spatialData).ConfigureAwait(false);
            }
        }

        private async Task AddFeatureCollection(SpatialData spatialData)
        {
            if (spatialData.TitledFeatureCollections.Count == 0) Snackbar.Add($"No data to show", MudBlazor.Severity.Info);

            for (int e = 0; e < spatialData.TitledFeatureCollections.Count; e++)
            {
                using (var stringWriter = new StringWriter())
                {
                    var geoJsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();

                    var fc = spatialData.TitledFeatureCollections[e].Item1;
                    geoJsonSerializer.Serialize(stringWriter, fc);

                    await LoadVectorLayer(stringWriter.ToString(), spatialData.TitledFeatureCollections[e].Item2);
                }
            }
        }
    }
}
