/*
 * This file is part of the C# client for BioSIM Web API.
 *
 * Copyright (C) 2020-2022 Her Majesty the Queen in right of Canada
 * Authors: Mathieu Fortin and Jean-Francois Lavoie, 
 *          (Canadian Wood Fibre Centre, Canadian Forest Service)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed with the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * Please see the license at http://www.gnu.org/copyleft/lesser.html.
 */
using biosimclient.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclienttest
{
    internal class BioSimFakeLocation : IBioSimPlot
    {
		private double _elevationM;
		private double _latitude;
		private double _longitude;

		internal BioSimFakeLocation(double latitudeDeg, double longitudeDeg, double elevationM)
		{
			this._latitude = latitudeDeg;
			this._longitude = longitudeDeg;
			this._elevationM = elevationM;
		}



		public double GetElevationM() { return _elevationM; }

		public double GetLatitudeDeg() { return _latitude; }

		public double GetLongitudeDeg() { return _longitude; }


		public string toString() { return _latitude + "_" + _longitude + "_" + _elevationM; }

	}
}
