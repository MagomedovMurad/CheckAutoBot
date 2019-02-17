using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CheckAutoBot.Svg
{
    public class SvgBuilder
    {
        private readonly string _autoTemplateV1;
        private readonly string _autoTemplateV2;
        private readonly string _busTemplate;
        private readonly string _motoTemplate;
        private readonly string _truckTemplate;
        private readonly string _schemaTemplate;

        public SvgBuilder()
        {
            _autoTemplateV1 = File.ReadAllText(@"SvgTemplates/AutoTemplateV1Svg.xml");
            _autoTemplateV2 = File.ReadAllText(@"SvgTemplates/AutoTemplateV2Svg.xml");
        }

        public string GenerateDamagePointsSvg(string[] damagePoints, DamagePointsType type)
        {
            var template = GetTemplate(damagePoints[0], type);
            if (template == null)
                return null;

            string style = GetStyle(damagePoints, type);
            if (style == null)
                return null;

            return template.Replace("#damagepoints#", style);
        }

        private string GetTemplate(string damagePoint, DamagePointsType type)
        {
            string template = null;

            if (type == DamagePointsType.Old)
            {
                var id = int.Parse(damagePoint);
                if (id >= 1 && id <= 9)  //Auto v1
                    template = _autoTemplateV1;
                else return null;
            }
            else if (type == DamagePointsType.New)
            {
                var pointIdStr = damagePoint.Substring(1, 2);
                var id = int.Parse(pointIdStr);
                if (id >= 10 && id <= 25)  //Auto v2
                    template = _autoTemplateV2;
                else if (id >= 30 && id <= 43)  //Bus
                    template = _busTemplate;
                else if (id >= 50 && id <= 45)  //Moto
                    template = _motoTemplate;
                else if (id >= 60 && id <= 80)  //Truck
                    template = _motoTemplate;
                else if (id >= 90 && id <= 99)  //Schema
                    template = _schemaTemplate;
                else return null;
            }
            return template;
        }

        private string GetStyle(string[] damagePoints, DamagePointsType type)
        {
            if (type == DamagePointsType.Old)
            {
                var lines = damagePoints.Select(x => $".selected .id{x} {{ display: inline; }}");
                return string.Join(Environment.NewLine, lines);
            }
            else 
            {
                var lines = damagePoints.Select(x => $".selected .id_{x.Substring(1, 2)} {{ fill: {(x.Substring(0, 1) == "1" ? "yellow" : "red")}; }}");
                return string.Join(Environment.NewLine, lines);
            }
        }
    }

    public enum DamagePointsType
    {
        New,
        Old
    }
}
