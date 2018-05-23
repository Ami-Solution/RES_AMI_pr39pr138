using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;

namespace AMY_System_Management
{
	public class GraphModel
	{
		public GraphModel()
		{
			this.Title = "Example 2";
			this.Points = new List<DataPoint>
							  {

							  };
		}

		public string Title { get; private set; }

		public IList<DataPoint> Points { get; private set; }
	}
}
