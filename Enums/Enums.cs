using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage
{
    public enum TypeMeasurement
    {
        Voltage = 0,
        Current,
        ActivePower,
        ReactivePower,
    }

	public enum State
	{
		OFF = 0,
		ON,
	}
}
