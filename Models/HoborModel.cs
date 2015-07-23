using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hobor.Models
{
    [Serializable]
    public class HoborModel
    {
        public bool IsExist { get; set; }
        public string CountryName { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public double XAxis { get; set; }
        public double YAxis { get; set; }
        public HoborModel() { this.IsExist = false; }
        //const double AllowRange = 0.5;
        public static HoborModel HoborAsker(DBHelper DBHelper,  double XAxis, double YAxis)
        {
            HoborModel result=new HoborModel();
            double AllowRange = GetConfig.GetAllowRange();
            double xAxisAbove = XAxis + AllowRange
                , xAxisBelow = XAxis - AllowRange
                , yAxisAbove = YAxis + AllowRange
                , yAxisBelow = YAxis - AllowRange;

            string commandText = "SELECT * FROM Hobor WHERE XAxis BETWEEN @XAxis1 AND @XAxis2 AND YAxis Between @YXias1 AND @YXias2";
            System.Data.DataTable dt = DBHelper.GetData(commandText, new List<Parameter>()
            {
                new Parameter("@XAxis1",xAxisBelow)
                ,new Parameter("@XAxis2",xAxisAbove)
                ,new Parameter("@YXias1",yAxisBelow)
                ,new Parameter("@YXias2", yAxisAbove)
            });
            if (dt != null && dt.Rows.Count > 0)
            {
                result.IsExist = true;
                result.CountryName = dt.Rows[0]["CountryName"].ToString();
                result.Name = dt.Rows[0]["Name"].ToString();
                result.XAxis = Common.ConvertToDouble(dt.Rows[0]["XAxis"]);
                result.YAxis = Common.ConvertToDouble(dt.Rows[0]["YAxis"]);
                result.Info = dt.Rows[0]["Info"].ToString();
            }
            return result;
        }
    }
}