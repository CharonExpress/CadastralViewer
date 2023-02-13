using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadastralViewerTests
{
    internal static class XmlDocumentData
    {
        public static string xmlDocumentString = """
                        <NewBuilding>
            	<EntitySpatial CsCode="52.2" Name="МСК-52">
            		<SpatialElement Underground="0">
            			<SpelementUnit TypeUnit="Точка" SuNmb="1">
            				<Ordinate X="518754.05" Y="2150702.94" NumGeopoint="1" DeltaGeopoint="0.10"/>
            			</SpelementUnit>
            			<SpelementUnit TypeUnit="Точка" SuNmb="2">
            				<Ordinate X="518763.23" Y="2150703.58" NumGeopoint="2" DeltaGeopoint="0.10"/>
            			</SpelementUnit>
            			<SpelementUnit TypeUnit="Точка" SuNmb="3">
            				<Ordinate X="518762.79" Y="2150709.44" NumGeopoint="3" DeltaGeopoint="0.10"/>
            			</SpelementUnit>
            			<SpelementUnit TypeUnit="Точка" SuNmb="4">
            				<Ordinate X="518753.61" Y="2150708.80" NumGeopoint="4" DeltaGeopoint="0.10"/>
            			</SpelementUnit>
            			<SpelementUnit TypeUnit="Точка" SuNmb="1">
            				<Ordinate X="518754.05" Y="2150702.94" NumGeopoint="1" DeltaGeopoint="0.10"/>
            			</SpelementUnit>
            		</SpatialElement>
            	</EntitySpatial>
            </NewBuilding>
            """;
        public static System.Xml.XmlDocument XmlDocument
        {
            get
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xmlDocumentString);
                return doc;
            }
        }
    }
}
