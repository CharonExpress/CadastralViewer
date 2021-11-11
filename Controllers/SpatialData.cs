using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace CadastralViewer
{
    [Route("api/spatial_data")]
    [ApiController]
    public class SpatialDataApi : ControllerBase
    {
        private ILogger<SpatialDataApi> logger;
        public SpatialDataApi(ILogger<SpatialDataApi> Logger)
        {
            logger = Logger;
        }

        [HttpPost]
        [Route("upload")]
        public ContentResult Upload()
        {
            IEnumerable<IFormFile> xmlFiles = Request.Form.Files.Where(f => Path.GetExtension(f.FileName) == ".xml");

            if (xmlFiles.Count() == 0)
            {
                logger.LogError("No files were provided");
                return new ContentResult
                {
                    Content = "No xml files were provided",
                    StatusCode = 204
                };
            }

            SpatialData spatialData_ = new();
            foreach (IFormFile file in xmlFiles.ToList())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(file.OpenReadStream());
                spatialData_.AddFeatures(xmlDoc);
             }

            return new ContentResult
            {
                Content = spatialData_.ToString(),
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        
    }
}