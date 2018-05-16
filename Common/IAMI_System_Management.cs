﻿using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	[ServiceContract]
	public interface IAMI_System_Management
	{
		[OperationContract]
		bool SendDataToDataBase(string agregator_code, Dictionary<string, Dictionary<TypeMeasurement, List<double>>> buffer);
	}
}